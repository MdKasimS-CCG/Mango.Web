using Mango.Web.Service;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

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
SD.CouponAPIBase = builder.Configuration["ServiceUrls:CouponAPI"];
SD.AuthAPIBase = builder.Configuration["ServiceUrls:AuthAPI"];
SD.ProductAPIBase = builder.Configuration["ServiceUrls:ProductAPI"];
SD.ShoppingCartAPIBase = builder.Configuration["ServiceUrls:ShoppingCartAPI"];
SD.OrderAPIBase = builder.Configuration["ServiceUrls:OrderAPI"];


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
