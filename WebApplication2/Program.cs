using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using WebApplication2.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
