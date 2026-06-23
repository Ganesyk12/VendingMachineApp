using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using VendingMachineApp.Models;
using VendingMachineApp.Services;
using QuestPDF.Infrastructure;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

QuestPDF.Settings.License = LicenseType.Community;

DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);


// Add DbContext
// Using MySQL
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseMySql(
//         builder.Configuration.GetConnectionString("VendingMachineContext"),
//         new MySqlServerVersion(new Version(8, 0, 40))
//     )
// );

// Using PostgreSQL
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

if (string.IsNullOrEmpty(connectionString))
{
    connectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
                       $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
                       $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
                       $"Username={Environment.GetEnvironmentVariable("DB_USER")};" +
                       $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};" +
                       $"Search Path={Environment.GetEnvironmentVariable("DB_SEARCH_PATH")};" +
                       "Ssl Mode=Prefer;Trust Server Certificate=true";
}

builder.Services.AddDbContext<VendingMachineContext>(options =>
    options.UseNpgsql(
        connectionString,
        x => x.MigrationsHistoryTable("__EFMigrationsHistory", "VendingMachine"))
);

// Add Redis as Message Broker
var redisOptions = new RedisOptions
{
    Host = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost",
    Port = int.Parse(Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379"),
    Password = Environment.GetEnvironmentVariable("REDIS_PASSWORD") ?? "",
    EmailStream = Environment.GetEnvironmentVariable("REDIS_STREAM_EMAIL") ?? "email_queue",
    ConsumerGroup = Environment.GetEnvironmentVariable("REDIS_CONSUMER_GROUP") ?? "email_workers",
    ConsumerName = Environment.GetEnvironmentVariable("REDIS_CONSUMER_NAME") ?? $"worker_{Environment.MachineName}"
};
builder.Services.AddSingleton(redisOptions);
builder.Services.AddSingleton<IRedisService, RedisService>();
builder.Services.AddHostedService<EmailBackgroundService>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.Cookie.Name = "UserAuthCookie";
        options.LoginPath = "/Account/Login";
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
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
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
