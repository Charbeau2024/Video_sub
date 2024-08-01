namespace Video
{
    using System.Text;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// <see href="https://docs.microsoft.com/ja-jp/aspnet/core/migration/50-to-60?view=aspnetcore-6.0&tabs=visual-studio#update-net-sdk-version-in-globaljson"/>
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 構成設定
        /// </summary>
        public static IConfiguration Configuration { get; private set; }

        /// <summary>
        /// ログの保存
        /// </summary>
        public static ILogger Logger { get; private set; }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="env">API 環境</param>
        /// <param name="logger">ログ</param>
        public Startup(IWebHostEnvironment env, ILogger logger)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            Logger = logger;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "/api/{controller}/{id?}");
            });
        }
    }
}