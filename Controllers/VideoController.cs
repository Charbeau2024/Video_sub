namespace Video.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Storage.Blobs.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Video.Helper;

    /// <summary>
    /// 動画再生API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        /// <summary>
        /// 動画を取得してストリーミングで返す
        /// </summary>
        /// <param name="name">動画名</param>
        /// <returns>動画ストリーム</returns>
        [HttpGet("{name}")]
        public async Task<IActionResult> GetVideoAsync(string name)
        {
            new BlobService();
            var blobServiceClient = BlobService.Blob_Service_Client;
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(BlobService.Video_Container_Name);
            var blobClient = blobContainerClient.GetBlobClient($"{name}.mp4");

            if (!await blobClient.ExistsAsync()) {
                return NotFound();
            }

            BlobProperties properties = await blobClient.GetPropertiesAsync();
            var fileLength = properties.ContentLength;

            HttpContext.Request.Headers.TryGetValue("Range", out var rangeHeader);
            Response.Headers.Append("Accept-Ranges", "bytes");

            if (!string.IsNullOrEmpty(rangeHeader)) {
                var from = 0L;
                var to = fileLength - 1;
                var range = rangeHeader.ToString().Replace("bytes=", string.Empty).Split('-');

                if (range.Length > 1) {
                    if (!string.IsNullOrEmpty(range[0])) {
                        from = long.Parse(range[0]);
                    }
                    if (!string.IsNullOrEmpty(range[1])) {
                        to = long.Parse(range[1]);
                    }
                } else if (!string.IsNullOrEmpty(range[0])) {
                    from = long.Parse(range[0]);
                }

                if (from > to || from < 0 || to >= fileLength) {
                    return BadRequest();
                }

                var length = to - from + 1;
                var contentRange = $"bytes {from}-{to}/{fileLength}";

                Response.Headers.Append("Content-Range", contentRange);
                Response.StatusCode = StatusCodes.Status206PartialContent;

                var blobHttpRange = new HttpRange(from, length);
                var blobDownloadInfo = await blobClient.DownloadAsync(blobHttpRange);
                var stream = blobDownloadInfo.Value.Content;

                return File(stream, "video/mp4", enableRangeProcessing: true);
            }

            var fullStream = await blobClient.DownloadAsync();
            return File(fullStream.Value.Content, "video/mp4");
        }
    }
}
