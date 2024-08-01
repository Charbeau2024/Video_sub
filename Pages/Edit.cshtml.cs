namespace Video.Pages
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Video.Helper;

    /// <summary>
    /// 管理者ページモデル
    /// </summary>
    public class EditModel : PageModel
    {
        /// <summary>
        /// 編集可能なメディア一覧
        /// </summary>
        public MediaData[] Editable_Media_Array { get; set; }

        public static BlobServiceClient Blob_Service_Client;
        public readonly CsvService Csv_Service;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EditModel(BlobServiceClient blobServiceClient, CsvService csvService)
        {
            Blob_Service_Client = blobServiceClient;
            Csv_Service = csvService;
        }

        /// <summary>
        /// 編集可能なメディア一覧を取得
        /// </summary>
        [HttpGet]
        public async Task OnGetAsync()
        {
            // AzureStorageに存在する動画の固有名一覧
            var storedVideoList = await BlobService.GetVideoNameListFromStorageAsync(Blob_Service_Client);
            // MediaDataに登録されていない動画の固有名一覧
            var newMediaList = storedVideoList.Where(x => !IndexModel.Media_Data_List.Select(y => y.Name).Contains(x)).ToList();
            // ファイルに記録されているMediaDataの一覧
            var data = CsvService.ReadCSV(Csv_Service);

            // ストレージに存在するがCSVに記録されていないメディアがあれば追加
            if (newMediaList.Any()) {
                foreach (var item in newMediaList) {
                    data.Add(new MediaData() {
                        Id = data.Max(x => x.Id) + 1,
                        Name = item,
                        Title = string.Empty,
                        Type = 0,
                        Priority = data.Max(x => x.Priority) + 1,
                        PublishDate = DateTime.UtcNow.AddHours(9),
                        Count = 0,
                        Deleted = false,
                        IsShow = false,
                    });
                }
            }

            // 編集可能なメディア一覧をソートして配列化
            Editable_Media_Array = data
                .OrderByDescending(x => x.IsShow)    // isShow が true のものを優先
                .ThenBy(x => x.Priority)    // 次に Priority 順で並べ替え
                .ToArray();
        }
    }
}
