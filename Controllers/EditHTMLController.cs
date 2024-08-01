namespace Video.Controllers
{
    using System;
    using System.IO;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// 概要ページの編集API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EditHTMLController : ControllerBase
    {
        /// <summary>
        /// 編集された概要ページを保存する
        /// </summary>
        /// <param name="request">HTMLソースのリクエスト</param>
        /// <returns>結果</returns>
        [HttpPost]
        public IActionResult SaveHTML([FromForm] HTMLRequest request)
        {
            try {
                var headlineText = request.HeadlineText;
                var hrmlSouorce = request.HTMLSource;
                var content = $"{headlineText}\n{hrmlSouorce}";

                var filesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");

                if (!Directory.Exists(filesPath)) {
                    Directory.CreateDirectory(filesPath);
                }

                var filePath = Path.Combine(filesPath, "about.html");
                System.IO.File.WriteAllTextAsync(filePath, content).GetAwaiter().GetResult();

                return Ok();
            } catch (Exception) {
                return StatusCode(500, "Internal server error.");
            }
        }
    }

    /// <summary>
    /// リクエストパラメータ
    /// </summary>
    public class HTMLRequest
    {
        /// <summary>
        /// 見出しのHTMLソース
        /// </summary>
        public required string HeadlineText { get; set; }

        /// <summary>
        /// 本文のHTMLソース
        /// </summary>
        public required string HTMLSource { get; set; }
    }
}
