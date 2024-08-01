namespace Video.Controllers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs.Models;
    using Microsoft.AspNetCore.Mvc;
    using Video.Helper;
    using Video.Pages;

    /// <summary>
    /// 動画ファイルとサムネイル画像をAzureStorageにアップロードするAPI
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        /// <summary>
        /// 動画ファイルとサムネイル画像をアップロード
        /// </summary>
        /// <returns>結果</returns>
        [HttpPost]
        public async Task<IActionResult> UploadAsync()
        {
            var baseFileName = Request.Form["baseFileName"].FirstOrDefault();
            var videoFile = Request.Form.Files.FirstOrDefault(f => f.Name == "videoFile");
            var thumbFile = Request.Form.Files.FirstOrDefault(f => f.Name == "thumbFile");

            if (string.IsNullOrEmpty(baseFileName) || videoFile == null || thumbFile == null) {
                return BadRequest("Please provide a base file name and select both video and thumbnail files.");
            }

            var videoExt = Path.GetExtension(videoFile.FileName);
            var thumbExt = Path.GetExtension(thumbFile.FileName);

            var blobServiceClient = EditModel.Blob_Service_Client;
            var videoBlobClient = blobServiceClient.GetBlobContainerClient("video");
            var thumbBlobClient = blobServiceClient.GetBlobContainerClient("image");

            // Upload video file
            var videoBlob = videoBlobClient.GetBlobClient(baseFileName + videoExt);
            using (var stream = videoFile.OpenReadStream()) {
                var videoHeaders = new BlobHttpHeaders { ContentType = videoFile.ContentType };
                await videoBlob.UploadAsync(stream, videoHeaders);
            }

            // Upload thumbnail image file
            var thumbBlob = thumbBlobClient.GetBlobClient(baseFileName + thumbExt);
            using (var stream = thumbFile.OpenReadStream()) {
                var thumbHeaders = new BlobHttpHeaders { ContentType = thumbFile.ContentType };
                await thumbBlob.UploadAsync(stream, thumbHeaders);
            }

            var mediaList = IndexModel.Media_Data_List;
            var newMedia = new MediaData() {
                Id = mediaList.Max(x => x.Id) + 1,
                Name = baseFileName,
                Title = string.Empty,
                Type = 0,
                Priority = mediaList.Max(x => x.Priority) + 1,
                PublishDate = DateTime.UtcNow.AddHours(9),
                Count = 0,
                Deleted = false,
                IsShow = false,
            };
            mediaList.Add(newMedia);

            CsvService.ExportCsv(mediaList);
            IndexModel.Media_Data_List = mediaList;

            return Ok();
        }
    }
}
