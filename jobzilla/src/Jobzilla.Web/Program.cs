using Jobzilla.Application;
using Jobzilla.Infrastructure;
using Jobzilla.Infrastructure.Identity;
using Jobzilla.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllersWithViews();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(AppRoles.Admin));
    options.AddPolicy("EmployerOnly", policy => policy.RequireRole(AppRoles.Employer, AppRoles.Admin));
    options.AddPolicy("CandidateOnly", policy => policy.RequireRole(AppRoles.Candidate, AppRoles.Admin));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

await DatabaseInitializer.InitializeAsync(app.Services);
await IdentitySeeder.SeedRolesAsync(app.Services);

app.Run();
