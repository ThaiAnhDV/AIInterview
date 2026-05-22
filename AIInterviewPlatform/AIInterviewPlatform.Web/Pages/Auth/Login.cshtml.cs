using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text;

namespace AIInterviewPlatform.Web.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public LoginModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient();

                var baseUrl = _configuration["ApiSettings:BaseUrl"];

                if (string.IsNullOrEmpty(baseUrl))
                {
                    ErrorMessage = "ApiSettings:BaseUrl ch?a ???c c?u hình trong appsettings.json.";
                    return Page();
                }

                client.BaseAddress = new Uri(baseUrl);

                var loginRequest = new
                {
                    email = Input.Email,
                    password = Input.Password
                };

                var json = JsonSerializer.Serialize(loginRequest);
                var body = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/auth/login", body);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = "??ng nh?p th?t b?i. API tr? v?: " + content;
                    return Page();
                }

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.Data == null)
                {
                    ErrorMessage = "Không ??c ???c d? li?u ??ng nh?p t? API. Response: " + content;
                    return Page();
                }

                if (string.IsNullOrEmpty(apiResponse.Data.Token))
                {
                    ErrorMessage = "API login không tr? v? token. Response: " + content;
                    return Page();
                }

                HttpContext.Session.SetString("JWToken", apiResponse.Data.Token);
                HttpContext.Session.SetString("UserEmail", apiResponse.Data.Email ?? string.Empty);
                HttpContext.Session.SetString("UserRole", apiResponse.Data.Role ?? string.Empty);

                var savedToken = HttpContext.Session.GetString("JWToken");

                if (string.IsNullOrEmpty(savedToken))
                {
                    ErrorMessage = "Không l?u ???c token vào Session.";
                    return Page();
                }

                return RedirectToPage("/Profile/Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = "L?i ??ng nh?p: " + ex.Message;
                return Page();
            }
        }
    }

    public class LoginInputModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public long UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
