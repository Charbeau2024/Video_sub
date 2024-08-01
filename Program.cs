using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Video;
using Video.Helper;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options => {
        options.UseMemberCasing();
    });
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<CsvService>();
var connectionString = builder.Configuration.GetConnectionString("AzureBlobStorage");
builder.Services.AddSingleton(new BlobServiceClient(connectionString));
builder.Logging.AddAzureWebAppDiagnostics();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "/api/{controller}/{id?}");
var startup = new Startup(builder.Environment, app.Logger);
startup.Configure(app, app.Environment);

app.Run();
