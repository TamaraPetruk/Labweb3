using Lab1.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Налаштування Serilog для запису логів у папку Logs
Log.Logger = new LoggerConfiguration()
    .WriteTo.File(
        "Logs/log-.txt",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Додаємо MVC
builder.Services.AddControllersWithViews();

// Реєстрація сервісу надсилання email
builder.Services.AddScoped<IEmailSender, EmailSender>();

var app = builder.Build();

// Логування всіх HTTP-запитів
app.Use(async (context, next) =>
{
    var requestTime = DateTime.Now;

    var fullUrl =
        $"{context.Request.Scheme}://" +
        $"{context.Request.Host}" +
        $"{context.Request.PathBase}" +
        $"{context.Request.Path}" +
        $"{context.Request.QueryString}";

    var ipAddress =
        context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

    Log.Information(
        "{Time} | URL: {Url} | IP: {IP}",
        requestTime,
        fullUrl,
        ipAddress);

    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Дозволяємо віддавати файли з wwwroot,
// у тому числі файли, які були завантажені під час роботи програми
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}