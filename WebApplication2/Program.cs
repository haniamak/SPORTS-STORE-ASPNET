using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using WebApplication2.Data;
using WebApplication2.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();
builder.Services.AddSession();
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ShopContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.BuildServiceProvider().GetService<ShopContext>()?.SeedData();

builder.Services.AddScoped(typeof(IDapperRepository<>), typeof(DapperRepository<>));

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/Login/Login";
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
})
.AddGoogle(options =>
 {
     IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");

     options.ClientId = googleAuthNSection["ClientId"];
     options.ClientSecret = googleAuthNSection["ClientSecret"];
     options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
     options.CallbackPath = "/signin-google";
     options.CorrelationCookie.SameSite = SameSiteMode.Strict;
     options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

 });



var app = builder.Build();

app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles(); // Enables serving files from wwwroot
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Data/images")),
    RequestPath = "/images"
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
