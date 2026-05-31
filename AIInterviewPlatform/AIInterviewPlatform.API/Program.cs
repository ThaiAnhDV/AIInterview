using AIInterviewPlatform.Application.Interfaces;
using AIInterviewPlatform.Application.Interfaces.Repositories;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Application.Services;
using AIInterviewPlatform.Infrastructure.Data;
using AIInterviewPlatform.Infrastructure.Repositories;
using AIInterviewPlatform.Infrastructure.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.Text;

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
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IMilestoneGeneratorService, MilestoneGeneratorService>();
builder.Services.AddHttpClient<IActivityDescriptionService, ActivityDescriptionService>();
builder.Services.AddScoped<IRoadmapApplicationService, RoadmapApplicationService>();

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