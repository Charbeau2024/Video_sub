namespace Video.Controllers
{
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Video.Pages;

    /// <summary>
    /// 再生回数取得API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CountController : ControllerBase
    {
        /// <summary>
        /// 再生回数を取得する
        /// </summary>
        /// <param name="name">取得対象の動画名</param>
        /// <returns>再生回数</returns>
        [HttpGet("{name}")]
        public IActionResult GetPlayCount(string name)
        {
            var target = IndexModel.Media_Data_List.FirstOrDefault(x => x.Type == 0 && x.Name == name);
            var count = target != null ? target.Count : 0;
            return Ok(new { playCount = count });
        }
    }
}
