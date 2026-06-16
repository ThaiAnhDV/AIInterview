using AIInterviewPlatform.Application.Interfaces;
using AIInterviewPlatform.Application.Interfaces.Repositories;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Application.Interfaces.Prompts;
using AIInterviewPlatform.Application.Services;
using AIInterviewPlatform.Application.Common.Validators;
using AIInterviewPlatform.Infrastructure.Data;
using AIInterviewPlatform.Infrastructure.Repositories;
using AIInterviewPlatform.Infrastructure.Services;
using AIInterviewPlatform.Infrastructure.Validators;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.Text;
using AIInterviewPlatform.Infrastructure.Services.TextExtractors;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Domain.Enum;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using AIInterviewPlatform.Infrastructure.AI.Prompts;
using AIInterviewPlatform.Infrastructure.AI.Prompts.Providers;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Database
// =======================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// =======================
// Repository
// =======================
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IRoadmapUnitOfWork, RoadmapUnitOfWork>();

// =======================
// Services
// =======================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IPdfTextExtractor, PdfTextExtractor>();
builder.Services.AddScoped<IDocxTextExtractor, DocxTextExtractor>();
builder.Services.AddScoped<IResumeParserService, ResumeParserService>();
builder.Services.AddScoped<IResumeService, ResumeService>();
builder.Services.AddScoped<ITargetJobService, TargetJobService>();
builder.Services.AddScoped<
    ISkillService,
    SkillService>();
builder.Services.AddScoped<
    ISkillGapAnalysisService,
    SkillGapAnalysisService>();
builder.Services.AddScoped<
    IInterviewService,
    InterviewService>();
builder.Services.AddScoped<
    IInterviewAnswerService,
    InterviewAnswerService>();
builder.Services.AddScoped<
    IAnswerEvaluationService,
    AnswerEvaluationService>();
builder.Services.AddScoped<ILearningRoadmapService, LearningRoadmapService>();
builder.Services.AddHttpClient<ISkillExtractionService, SkillExtractionService>();
builder.Services.AddHttpClient<IJobDescriptionSkillExtractionService, JobDescriptionSkillExtractionService>();
builder.Services.AddScoped<ISkillMatchingService, SkillMatchingService>();
builder.Services.AddScoped<ISkillGapAnalysisOrchestratorService, SkillGapAnalysisOrchestratorService>();
builder.Services.AddScoped<IRecommendationService,RecommendationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IDashboardQueryService, DashboardQueryService>();
builder.Services.AddScoped<IDashboardQueryValidator, DashboardQueryValidator>();
builder.Services.AddScoped<IMilestoneGeneratorService, MilestoneGeneratorService>();
builder.Services.AddScoped<IPromptProvider, EnglishPromptProvider>();
builder.Services.AddScoped<IPromptProvider, VietnamesePromptProvider>();
builder.Services.AddScoped<IPromptProviderFactory, PromptProviderFactory>();
builder.Services.AddHttpClient<IActivityDescriptionService, ActivityDescriptionService>();
builder.Services.AddHttpClient<IInterviewQuestionGeneratorService, InterviewQuestionGeneratorService>();
builder.Services.AddHttpClient<IInterviewEvaluationService, InterviewEvaluationService>();
builder.Services.AddHttpClient<IAssistantChatService, AssistantChatService>();
builder.Services.AddScoped<IInterviewEvaluationApplicationService, InterviewEvaluationApplicationService>();
builder.Services.AddScoped<IRoadmapApplicationService, RoadmapApplicationService>();
builder.Services.AddScoped<IInterviewSessionRepository, InterviewSessionRepository>();
builder.Services.AddScoped<IMockInterviewApplicationService, MockInterviewApplicationService>();
builder.Services.AddHttpClient<IAIConnectionTestService, AIConnectionTestService>();
builder.Services.AddScoped<IAIFunctionValidationService, AIFunctionValidationService>();

// =======================
// Controllers
// =======================
builder.Services.AddControllers();

// =======================
// CORS
// =======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// =======================
// JWT Authentication
// =======================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"];

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey!))
    };
});

// =======================
// Swagger
// =======================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AIInterviewPlatform.API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Input JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await SeedAdminUserAsync(dbContext, configuration);
}

// =======================
// Swagger
// =======================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// =======================
// Middleware
// =======================
app.UseHttpsRedirection();

app.UseCors("AllowAll");

// =======================
// Static Files
// =======================
app.UseStaticFiles();

var uploadsPath = Path.Combine(builder.Environment.WebRootPath, "uploads");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// =======================
// Authentication
// =======================
app.UseAuthentication();

app.UseAuthorization();

// =======================
// Controllers
// =======================
app.MapControllers();

app.Run();

static async Task SeedAdminUserAsync(
    ApplicationDbContext dbContext,
    IConfiguration configuration)
{
    const string adminEmail = "admin@aiinterview.local";
    const string adminDefaultPassword = "Admin@123456";
    const string adminFullName = "Admin";

    var account = await dbContext.AuthenticationAccounts
        .Include(authenticationAccount => authenticationAccount.User)
        .ThenInclude(user => user.UserProfile)
        .FirstOrDefaultAsync(authenticationAccount => authenticationAccount.Email.ToLower() == adminEmail);

    if (account != null)
    {
        if (account.User.UserType != UserType.ADMIN)
        {
            account.User.UserType = UserType.ADMIN;
            account.User.UpdatedAt = DateTime.Now;
        }

        if (!IsBCryptHash(account.PasswordHash))
        {
            account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminDefaultPassword);
            account.UpdatedAt = DateTime.Now;
        }

        await dbContext.SaveChangesAsync();
        return;
    }

    var existingAdmin = await dbContext.Users
        .AnyAsync(user => user.UserType == UserType.ADMIN);

    if (existingAdmin)
    {
        return;
    }

    var user = new User
    {
        UserType = UserType.ADMIN,
        Status = UserStatus.ACTIVE,
        CreatedAt = DateTime.Now
    };

    await dbContext.Users.AddAsync(user);
    await dbContext.SaveChangesAsync();

    var adminAccount = new AuthenticationAccount
    {
        UserId = user.Id,
        Email = adminEmail,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminDefaultPassword),
        IsVerified = true,
        CreatedAt = DateTime.Now
    };

    var profile = new UserProfile
    {
        UserId = user.Id,
        FullName = adminFullName,
        CreatedAt = DateTime.Now
    };

    await dbContext.AuthenticationAccounts.AddAsync(adminAccount);
    await dbContext.UserProfiles.AddAsync(profile);
    await dbContext.SaveChangesAsync();
}

static bool IsBCryptHash(string? passwordHash)
{
    if (string.IsNullOrWhiteSpace(passwordHash))
    {
        return false;
    }

    return passwordHash.StartsWith("$2a$") ||
        passwordHash.StartsWith("$2b$") ||
        passwordHash.StartsWith("$2y$");
}
