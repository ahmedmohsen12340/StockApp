using StockApp.ConfigurationOptions;
using Services;
using ServicesContract;
using Models;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using Repository;
using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using StockApp.HelperExtentions;
using StockApp.CustomMiddleWares;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddAServices(builder.Configuration);

builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider service, LoggerConfiguration configuration) =>
{
    configuration
    .ReadFrom.Configuration(context.Configuration) //read configuration settings from built-in IConfiguration (appsetting.json)
    .ReadFrom.Services(service); //read services from built-in IServiceProvider 
});

builder.Services.AddOutputCache();
var app = builder.Build();


if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseExceptionHandlingMiddleware();
}

app.UseHsts(); //stands for:  Http Strict Transport Security
app.UseHttpsRedirection();

if (!builder.Environment.IsEnvironment("Test"))
{
    Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotitva");
}

app.UseHttpLogging();
app.UseStaticFiles();
app.UseRouting(); //Identifing action methods based route
app.UseOutputCache();
app.UseAuthentication(); //reading Identity Cookie
app.UseAuthorization(); //Validate access permission of the user
app.MapControllers(); //Execute the Filter pipeline ( actions + filters )

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller=Home}/{action=Index}"
    );
});

app.Run();

//to make the code accessible for auto-generated class programs
public partial class Program { }