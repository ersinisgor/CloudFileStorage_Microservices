using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

//builder.Configuration.AddJsonFile("secrets.json", optional: false, reloadOnChange: true);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient("GatewayAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["GatewayApi:BaseUrl"]);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Home/Error";
    });
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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