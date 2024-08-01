namespace Video.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Linq;
    using Video.Helper;
    using Video.Pages;

    /// <summary>
    /// メディア情報更新API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateMediaRecordController : ControllerBase
    {
        /// <summary>
        /// 更新内容を取得してCSVに記録する
        /// </summary>
        /// <param name="updates">更新内容</param>
        /// <returns>結果</returns>
        [HttpPost]
        public IActionResult UpdateEditableMedia([FromForm] List<EditableMediaUpdateModel> updates)
        {
            var mediaList = IndexModel.Media_Data_List;
            var newMediaList = new List<MediaData>();

            foreach (var update in updates) {
                var target = mediaList.Where(x => x.Name == update.Name).FirstOrDefault();
                if (target is not null) {
                    target.Title = update.Title;
                    target.Priority = update.Priority;
                    target.IsShow = update.IsShow;
                    newMediaList.Add(target);
                }
            }

            CsvService.ExportCsv(newMediaList);
            IndexModel.Media_Data_List = newMediaList;

            return Ok();
        }
    }

    /// <summary>
    /// アップデート情報
    /// </summary>
    public class EditableMediaUpdateModel
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public int Priority { get; set; }
        public bool IsShow { get; set; }
    }
}
