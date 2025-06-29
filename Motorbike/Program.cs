using Microsoft.EntityFrameworkCore;
using Motorbike.Data;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
    sqlServerOptions => {
        sqlServerOptions.CommandTimeout(60);
        sqlServerOptions.EnableRetryOnFailure();
        // Thêm cấu hình này
        sqlServerOptions.UseRelationalNulls();
    }));

builder.Services.AddControllersWithViews();

// Cấu hình Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Tăng lên 60 phút để đủ thời gian hoàn tất thanh toán
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Thêm vào file Program.cs
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
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
// Thêm middleware này
app.Use(async (context, next) =>
{
    context.Response.Headers["Content-Type"] = "text/html; charset=utf-8";
    await next();
});
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Sử dụng session middleware
app.UseSession();

// Thêm middleware kiểm tra quyền admin cho area Admin
app.UseWhen(context => context.Request.Path.StartsWithSegments("/Admin"), appBuilder =>
{
    appBuilder.Use(async (context, next) =>
    {
        var roleId = context.Session.GetInt32("RoleId");
        if (roleId != 1)
        {
            context.Response.Redirect("/Account/Login");
            return;
        }
        await next();
    });
});

// Cấu hình Area cho Admin
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Thêm route cho thanh toán
app.MapControllerRoute(
    name: "checkout",
    pattern: "checkout",
    defaults: new { controller = "Checkout", action = "Index" });

app.Run();
