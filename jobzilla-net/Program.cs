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

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHub<jobzilla_net.Hubs.ChatHub>("/chathub");

app.Run();
