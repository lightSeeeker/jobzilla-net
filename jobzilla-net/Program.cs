using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using jobzilla_net.Infrasture.Persistence;
using jobzilla_net.Core.Entities;
using jobzilla_net.Infrasture;
using jobzilla_net.Infrasture.Seed;
using jobzilla_net.Application.Constants;

var webApplicationOptions = new WebApplicationOptions
{
    EnvironmentName = Environment.GetEnvironmentVariable(CommonText.CurrentEnv) ?? CommonText.CurrentEnv
};
WebApplicationBuilder builder = WebApplication.CreateBuilder(webApplicationOptions);
Environment.SetEnvironmentVariable(CommonText.ASPNETCORE_ENVIRONMENT, CommonText.CurrentEnv);
Environment.SetEnvironmentVariable(CommonText.DOTNET_ENVIRONMENT, CommonText.CurrentEnv);
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{webApplicationOptions.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});
builder.Services.AddInfrastructure(builder.Configuration);

// Per-request display-currency context (salary conversion).
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<jobzilla_net.Services.CurrencyContext>();

// ── External (social) login providers ─────────────────────────────────────
// Each provider is registered only when its credentials are present in config
// (appsettings "Authentication:*" or user-secrets / env vars), so the app runs
// fine before credentials are supplied.
var authBuilder = builder.Services.AddAuthentication();
var authConfig = builder.Configuration.GetSection("Authentication");

var google = authConfig.GetSection("Google");
if (!string.IsNullOrWhiteSpace(google["ClientId"]))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = google["ClientId"]!;
        options.ClientSecret = google["ClientSecret"]!;
    });
}

var facebook = authConfig.GetSection("Facebook");
if (!string.IsNullOrWhiteSpace(facebook["AppId"]))
{
    authBuilder.AddFacebook(options =>
    {
        options.AppId = facebook["AppId"]!;
        options.AppSecret = facebook["AppSecret"]!;
    });
}

var twitter = authConfig.GetSection("Twitter");
if (!string.IsNullOrWhiteSpace(twitter["ApiKey"]))
{
    authBuilder.AddTwitter(options =>
    {
        options.ConsumerKey = twitter["ApiKey"]!;
        options.ConsumerSecret = twitter["ApiSecret"]!;
        options.RetrieveUserDetails = true;
    });
}

var linkedIn = authConfig.GetSection("LinkedIn");
if (!string.IsNullOrWhiteSpace(linkedIn["ClientId"]))
{
    authBuilder.AddLinkedIn(options =>
    {
        options.ClientId = linkedIn["ClientId"]!;
        options.ClientSecret = linkedIn["ClientSecret"]!;
    });
}

var app = builder.Build();

// ── Startup: migrate database and seed identity data ──────────────────────
await DatabaseInitializer.InitializeAsync(app.Services);
await IdentitySeeder.SeedRolesAsync(app.Services);
await jobzilla_net.Infrasture.Seed.ContentPageSeeder.SeedAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Resolve the visitor's display currency once per request (cookie or geo).
app.Use(async (ctx, next) =>
{
    await ctx.RequestServices.GetRequiredService<jobzilla_net.Services.CurrencyContext>().EnsureInitializedAsync();
    await next();
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHub<jobzilla_net.Hubs.ChatHub>("/chathub");

app.Run();
