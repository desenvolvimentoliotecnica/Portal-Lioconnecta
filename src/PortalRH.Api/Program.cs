using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using PortalRH.Api.Data;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Infrastructure;
using PortalRH.Api.Models;
using PortalRH.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IFeedService, FeedService>();
builder.Services.AddScoped<ICommunicationService, CommunicationService>();
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<MicrosoftGraphAuthClient>();
builder.Services.AddScoped<MicrosoftGraphConnectionTester>();
builder.Services.AddScoped<IMicrosoftGraphUserPhotoService, MicrosoftGraphUserPhotoService>();
builder.Services.AddScoped<IMicrosoftGraphCalendarService, MicrosoftGraphCalendarService>();
builder.Services.AddScoped<ILdapConfigurationService, LdapConfigurationService>();
builder.Services.AddScoped<IMicrosoftGraphConfigurationService, MicrosoftGraphConfigurationService>();
builder.Services.AddScoped<IPortalAuthService, PortalAuthService>();
builder.Services.AddScoped<IPortalUserAdminService, PortalUserAdminService>();
builder.Services.AddScoped<IPollService, PollService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAgendaService, AgendaService>();
builder.Services.AddScoped<IMoodSurveyService, MoodSurveyService>();
builder.Services.AddScoped<IMoodSurveyFeedbackService, MoodSurveyFeedbackService>();
builder.Services.AddScoped<IPortalShellService, PortalShellService>();
builder.Services.AddScoped<IPortalPanelsComposer, PortalPanelsComposer>();
builder.Services.AddScoped<IQuickLinkService, QuickLinkService>();
builder.Services.AddScoped<IJourneyService, JourneyService>();
builder.Services.AddScoped<IJourneyWorkspaceService, JourneyWorkspaceService>();
builder.Services.AddScoped<IKpiService, KpiService>();
builder.Services.AddScoped<IHrProfileService, HrProfileService>();
builder.Services.AddScoped<IHrWorkspaceService, HrWorkspaceService>();
builder.Services.AddScoped<ICorporateSystemsService, CorporateSystemsService>();
builder.Services.AddScoped<ILdapDirectoryAuthenticator, LdapDirectoryAuthenticator>();
builder.Services.AddScoped<IPasswordHasher<AdminUser>, PasswordHasher<AdminUser>>();
builder.Services.AddScoped<IPasswordHasher<PortalUser>, PasswordHasher<PortalUser>>();
builder.Services.AddScoped<IPortalUserSeedService, PortalUserSeedService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("LioConnectaLocal", policy =>
    {
        policy
            .SetIsOriginAllowed(static origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return false;
                }

                if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                return uri.Port is 3020 or 4173;
            })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<PortalRhDbContext>(options =>
        options.UseNpgsql(connectionString));
}

var app = builder.Build();
await PortalRhDbInitializer.InitializeAsync(app.Services);

var webRootPath = builder.Environment.WebRootPath;
if (string.IsNullOrWhiteSpace(webRootPath))
{
    webRootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
}

var uploadsRoot = PortalUploadPaths.ResolveUploadsRoot(builder.Configuration, builder.Environment);

Directory.CreateDirectory(webRootPath);
PortalUploadPaths.EnsureFeedUploadsReady(builder.Configuration, builder.Environment);
uploadsRoot = PortalUploadPaths.ResolveUploadsRoot(builder.Configuration, builder.Environment);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("LioConnectaLocal");

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            message = "Erro interno ao processar a requisicao."
        });
    });
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(webRootPath),
    RequestPath = ""
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRoot),
    RequestPath = "/uploads"
});
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
