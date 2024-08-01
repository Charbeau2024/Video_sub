namespace Video.Controllers
{
    using System;
    using System.IO;
    using Microsoft.AspNetCore.Mvc;
    using Video.Helper;

    /// <summary>
    /// パスワード変更API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ChangePasswordController : ControllerBase
    {
        /// <summary>
        /// パスワードを暗号化して保存する
        /// </summary>
        /// <param name="request">変更するパスワードのリクエスト</param>
        /// <returns>結果</returns>
        [HttpPost]
        public IActionResult SaveEncryptedPassword([FromForm] NewPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Password)) {
                return BadRequest("Password is required.");
            }

            try {
                var cryptographHelper = CryptographHelper.CreateFromWebConfig();
                var encryptedPassword = cryptographHelper.Encode(request.Password);
                var filesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");

                if (!Directory.Exists(filesPath)) {
                    Directory.CreateDirectory(filesPath);
                }

                var filePath = Path.Combine(filesPath, ".pw");
                System.IO.File.WriteAllTextAsync(filePath, encryptedPassword).GetAwaiter().GetResult();

                return Ok();
            } catch (Exception) {
                return StatusCode(500, "Internal server error.");
            }
        }
    }
    /// <summary>
    /// リクエストパラメータ
    /// </summary>
    public class NewPasswordRequest
    {
        /// <summary>
        /// 暗号化されていないパスワード
        /// </summary>
        public required string Password { get; set; }
    }
}
