namespace Video.Pages
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    /// <summary>
    /// 概要ページモデル
    /// </summary>
    public class AboutModel : PageModel
    {
        private readonly IWebHostEnvironment Env;

        /// <summary>
        /// 表示するHTMLソース
        /// </summary>
        public static List<string> Html_Data { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="env"></param>
        public AboutModel(IWebHostEnvironment env)
        {
            Env = env;

            Html_Data = new List<string>();
            var filePath = Path.Combine(Env.WebRootPath, "files", "about.html");
            using (var stream = new StreamReader(filePath)) {
                while (!stream.EndOfStream) {
                    var line = stream.ReadLine();
                    Html_Data.Add(line);
                }
            };
        }

        public void OnGet()
        {  
        }
    }
}
