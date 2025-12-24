using aspnet_realestate.Mapping;
using aspnet_realestate.Models;
using aspnet_realestate.Repositories;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using AspNetCoreHero.ToastNotification;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using System.Reflection;
using Microsoft.AspNetCore.Identity;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("sqlCon"));
});

builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    // Kullan?c? ve ?ifre Kurallar? (Hocan?n Ayarlar?)
    options.User.RequireUniqueEmail = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;

    // Hesap Kilitleme Ayarlar?
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);
    options.Lockout.MaxFailedAccessAttempts = 3;
})
.AddDefaultTokenProviders()
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.Cookie.Name = "EmlakIdentityCookie";
    opt.LoginPath = new PathString("/Home/Login");
    opt.LogoutPath = new PathString("/Home/Logout");
    opt.AccessDeniedPath = new PathString("/Home/AccessDenied");
    opt.ExpireTimeSpan = TimeSpan.FromDays(15); // 15 gün kals?n
    opt.SlidingExpiration = true; // Kullan?c? siteyi kulland?kça süre uzas?n
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<AmenitiesGroupRepository>();
builder.Services.AddScoped<AmenitiesRepository>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<CategoryFieldsRepository>();
builder.Services.AddScoped<PropertyRepository>();
builder.Services.AddScoped<MessageRepository>();
builder.Services.AddScoped<SettingRepository>();
builder.Services.AddScoped<MessageRepository>();
builder.Services.AddAutoMapper(cfg => { }, typeof(MapProfile));

builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 10;
    config.IsDismissable = true;
    config.Position = NotyfPosition.BottomRight;
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
    name: "public",
    pattern: "{action=Index}/{id?}",
    defaults: new { controller = "Public" });

app.Run();
