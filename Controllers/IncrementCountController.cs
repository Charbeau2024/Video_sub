namespace Video.Controller
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using Video.Helper;
    using Video.Pages;

    /// <summary>
    /// 再生回数インクリメントAPI
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class IncrementCountController : ControllerBase
    {
        /// <summary>
        /// 再生回数を1増やす
        /// </summary>
        /// <param name="videoName">対象の動画名</param>
        /// <returns>結果</returns>
        [HttpGet("{videoName}")]
        public IActionResult IncrementPlayCount(string videoName)
        {
            var result = new List<MediaData>();

            foreach (var item in IndexModel.Media_Data_List) {
                if (item.Name == videoName && item.Type == 0) {
                    item.Count++;
                    result.Add(item);
                } else {
                    result.Add(item);
                }
            }

            CsvService.ExportCsv(result);

            return Ok();
        }
    }
}
