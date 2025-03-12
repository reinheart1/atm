var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession(); // Enable Session

var app = builder.Build();

app.UseRouting();
app.UseSession(); // Activate Session

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=ATM}/{action=Login}/{id?}");
});

app.Run();
