using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AIInterviewPlatform.Web.Pages.Profile
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        public ProfileViewModel Profile { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                ErrorMessage = "Không có JWToken trong Session. Login ch?a l?u token.";
                return Page();
            }

            try
            {
                var client = CreateAuthorizedClient(token);

                var response = await client.GetAsync("/api/profile/me");
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = "Cannot load profile.";
                    return Page();
                }

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProfileViewModel>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.Data != null)
                {
                    Profile = apiResponse.Data;
                }

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var client = CreateAuthorizedClient(token);

                var requestBody = new
                {
                    fullName = Profile.FullName,
                    phone = Profile.Phone,
                    educationLevel = Profile.EducationLevel,
                    careerGoal = Profile.CareerGoal
                };

                var json = JsonSerializer.Serialize(requestBody);
                var body = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync("/api/profile/me", body);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = "Update profile failed.";
                    return Page();
                }

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProfileViewModel>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.Data != null)
                {
                    Profile = apiResponse.Data;
                }

                SuccessMessage = "Profile updated successfully.";
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
        }

        private HttpClient CreateAuthorizedClient(string token)
        {
            var client = _httpClientFactory.CreateClient();

            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            client.BaseAddress = new Uri(baseUrl!);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return client;
        }
    }

    public class ProfileViewModel
    {
        public long UserId { get; set; }
        public string Email { get; set; } = string.Empty;

        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? EducationLevel { get; set; }
        public string? CareerGoal { get; set; }

        public string UserType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}