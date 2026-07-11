using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ZoneSync.Core.Data;
using ZoneSync.Core.Entities.Identity;
using ZoneSync.Service.Contracts;
using ZoneSync.Service.Modules.CropModule;
using ZoneSync.Service.Modules.FarmZone;
using ZoneSync.Service.Modules.Identity;
using ZoneSync.Service.Modules.Sensors;
using ZoneSync.Service.Modules.AlertsTasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
});

builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IFarmZoneService, FarmZoneService>();
builder.Services.AddScoped<ICropPlanService, CropPlanService>();
builder.Services.AddScoped<ISensorService, SensorService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<ITaskService, TaskService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "dashboard",
    pattern: "Dashboard/{action=FarmDashboard}/{id?}",
    defaults: new { controller = "Dashboard" })
    .WithStaticAssets();

// Seed demo data on startup in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await ZoneSync.Core.Data.SeedData.InitializeAsync(app.Services);
}

app.Run();
