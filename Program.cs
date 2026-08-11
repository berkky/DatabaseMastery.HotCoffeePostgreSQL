using DatabaseMastery.HotCoffeePostgreSQL.Authentication;
using DatabaseMastery.HotCoffeePostgreSQL.Configuration;
using DatabaseMastery.HotCoffeePostgreSQL.Context;
using DatabaseMastery.HotCoffeePostgreSQL.Services.AdminAuth;
using DatabaseMastery.HotCoffeePostgreSQL.Services.CategoryServices;
using DatabaseMastery.HotCoffeePostgreSQL.Services.ChartServices;
using DatabaseMastery.HotCoffeePostgreSQL.Services.DashboardServices;
using DatabaseMastery.HotCoffeePostgreSQL.Services.ProductServices;
using DatabaseMastery.HotCoffeePostgreSQL.Services.ReservationServices;
using DatabaseMastery.HotCoffeePostgreSQL.Services.ReviewServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AdminAuthOptions>(
    builder.Configuration.GetSection(AdminAuthOptions.SectionName));

builder.Services.AddScoped<IAdminCredentialValidator, AdminCredentialValidator>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = AuthSchemes.HotCoffeeAdmin;
    options.DefaultSignInScheme = AuthSchemes.HotCoffeeAdmin;
    options.DefaultChallengeScheme = AuthSchemes.HotCoffeeAdmin;
})
.AddCookie(AuthSchemes.HotCoffeeAdmin, options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
        policy.RequireRole(AdminRoles.Admin));
});

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IChartService, ChartService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

var app = builder.Build();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program
{
}
