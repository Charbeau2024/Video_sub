namespace Video.Controllers
{
    using System.IO;
    using System.Text;
    using Microsoft.AspNetCore.Mvc;
    using Video.Helper;

    /// <summary>
    /// パスワード認証API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ValidatePasswordController : ControllerBase
    {
        /// <summary>
        /// パスワード認証を行う
        /// </summary>
        /// <param name="request">リクエストパラメータ</param>
        /// <returns>結果</returns>
        [HttpPost]
        public IActionResult ValidatePassword([FromForm] PasswordRequest request)
        {
            var cryptographHelper = CryptographHelper.CreateFromWebConfig();
            var encryptedPassword = cryptographHelper.Encode(request.Password);
            var filesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");
            var filePath = Path.Combine(filesPath, ".pw");

            if (!System.IO.File.Exists(filePath)) {
                System.IO.File.WriteAllTextAsync(filePath, encryptedPassword, Encoding.UTF8).GetAwaiter().GetResult();
            }

            var storedPassword = System.IO.File.ReadAllTextAsync(filePath, Encoding.UTF8).GetAwaiter().GetResult();

            if (encryptedPassword == storedPassword) {
                return Ok();
            } else {
                return Unauthorized();
            }
        }
    }

    /// <summary>
    /// リクエストパラメータ
    /// </summary>
    public class PasswordRequest
    {
        /// <summary>
        /// パスワード
        /// </summary>
        public required string Password { get; set; }
    }
}
