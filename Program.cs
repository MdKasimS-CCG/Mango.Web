using Mango.Web.Service;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authentication.Cookies;
using DotNetEnv;

// Determine whether the application is running inside a Docker container.
bool isRunningInContainer =
    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

// The HTTP profile does not automatically load .env,
// so load it before creating the WebApplicationBuilder.
if (!isRunningInContainer)
{
    Env.Load();
}

var builder = WebApplication.CreateBuilder(args);

string configurationPrefix = isRunningInContainer
    ? "Docker"
    : "Http";

var webOptions = new WebOptions
{
    AuthAPI =
        builder.Configuration[
            $"{configurationPrefix}:ServiceUrls:AuthAPI"]
        ?? string.Empty,

    ProductAPI =
        builder.Configuration[
            $"{configurationPrefix}:ServiceUrls:ProductAPI"]
        ?? string.Empty,

    ShoppingCartAPI =
        builder.Configuration[
            $"{configurationPrefix}:ServiceUrls:ShoppingCartAPI"]
        ?? string.Empty,

    CouponAPI =
        builder.Configuration[
            $"{configurationPrefix}:ServiceUrls:CouponAPI"]
        ?? string.Empty,

    OrderAPI =
        builder.Configuration[
            $"{configurationPrefix}:ServiceUrls:OrderAPI"]
        ?? string.Empty
};

builder.Services.AddSingleton(
    Microsoft.Extensions.Options.Options.Create(webOptions));

builder.Configuration.AddInMemoryCollection(
    new Dictionary<string, string?>
    {
        ["ServiceUrls:AuthAPI"] =
            webOptions.AuthAPI,

        ["ServiceUrls:ProductAPI"] =
            webOptions.ProductAPI,

        ["ServiceUrls:ShoppingCartAPI"] =
            webOptions.ShoppingCartAPI,

        ["ServiceUrls:CouponAPI"] =
            webOptions.CouponAPI,

        ["ServiceUrls:OrderAPI"] =
            webOptions.OrderAPI
    });

// Add services to the container.
builder.Services.AddControllersWithViews();
//Used to access Cookies/Sessions
builder.Services.AddHttpContextAccessor();

/*This line adds the basic IHttpClientFactory and related services to the application's DI (Dependency Injection) container.
It allows for creation and management of HttpClient instances with best practices (such as socket pooling and lifetime management),
helping developers avoid common pitfalls like socket exhaustion.
*/
builder.Services.AddHttpClient();


/* Registering a typed client for ICouponService with its implementation CouponService.
 This allows for dependency injection of ICouponService wherever needed in the application.
 The HttpClient instance provided to CouponService will be managed by the IHttpClientFactory,
 ensuring proper configuration and lifecycle management. 
 This is particularly useful for services that make HTTP calls, as it helps in reusing HttpClient instances efficiently.
*/

//TODO: Why it worked without this earlier
builder.Services.AddHttpClient<IOrderService, OrderService>();
builder.Services.AddHttpClient<ICartService, CartService>();
builder.Services.AddHttpClient<IProductService, ProductService>();
builder.Services.AddHttpClient<ICouponService, CouponService>();
builder.Services.AddHttpClient<IAuthService, AuthService>();

SD.CouponAPIBase =
    webOptions.CouponAPI;

SD.AuthAPIBase =
    webOptions.AuthAPI;

SD.ProductAPIBase =
    webOptions.ProductAPI;

SD.ShoppingCartAPIBase =
    webOptions.ShoppingCartAPI;

SD.OrderAPIBase =
    webOptions.OrderAPI;


builder.Services.AddScoped<ITokenProvider, TokenProvider>();
builder.Services.AddScoped<IBaseService, BaseService>();
builder.Services.AddScoped<IOrderService,OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromHours(10);
        options.LoginPath = "/Auth/Login";

        //TODO: Add Access Denied Page
        options.AccessDeniedPath = "/Auth/AccessDenied";

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
public class WebOptions
{
    public string AuthAPI { get; set; } = string.Empty;
    public string ProductAPI { get; set; } = string.Empty;
    public string ShoppingCartAPI { get; set; } = string.Empty;
    public string CouponAPI { get; set; } = string.Empty;
    public string OrderAPI { get; set; } = string.Empty;
}
