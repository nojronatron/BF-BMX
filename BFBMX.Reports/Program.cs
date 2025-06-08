using BFBMX.Reports.Components;
using BFBMX.Reports.Helpers;
using BFBMX.Service.Helpers;

namespace BFBMX.Reports
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IAidStationsDict, AidStationsDict>();
            builder.Services.AddSingleton<IReportServerEnvFactory, ReportServerEnvFactory>();
            builder.Services.AddSingleton<IHttpConfiguration, HttpConfiguration>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
