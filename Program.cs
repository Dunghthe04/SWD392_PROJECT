using SWD392_PROJECT.Coordinators;
using SWD392_PROJECT.Data;
using SWD392_PROJECT.Data.Repositories.Interfaces;
using SWD392_PROJECT.Data.Repositories.Implementations;
using SWD392_PROJECT.Services.Interfaces;
using SWD392_PROJECT.Services.Implementations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

// Add Authentication
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.AccessDeniedPath = "/Auth/AccessDenied";
});

// Add Authorization
builder.Services.AddAuthorization();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IIssueRepository, IssueRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReportIssueService, ReportIssueService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<ViewOrderCoordinator>();
builder.Services.AddScoped<UpdateOrderCoordinator>();
builder.Services.AddScoped<ReportIssueCoordinator>();
builder.Services.AddScoped<IStaffContext, DemoStaffContext>();

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
