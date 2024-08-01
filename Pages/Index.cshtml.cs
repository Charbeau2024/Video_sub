namespace Video.Pages
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Video.Helper;

    public class IndexModel : PageModel
    {
        /// <summary>
        /// 画像ファイル名をキー、URLをバリューとするディクショナリ
        /// </summary>
        public Dictionary<string, string> Image_Dic { get; set; } = [];

        /// <summary>
        /// 動画ファイル名をキー、URLをバリューとするディクショナリ
        /// </summary>
        public Dictionary<string, string> Video_Dic { get; set; } = [];

        /// <summary>
        /// 音声ファイル名をキー、URLをバリューとするディクショナリ
        /// </summary>
        public Dictionary<string, string> Sound_Dic { get; set; } = [];

        /// <summary>
        /// データファイル名をキー、URLをバリューとするディクショナリ
        /// </summary>
        public Dictionary<string, string> Data_Dic { get; set; } = [];

        /// <summary>
        /// 動画ファイル名をキー、タイトル文字列をバリューとするディクショナリ
        /// </summary>
        public Dictionary<string, string> Title_Dic { get; set; } = [];

        /// <summary>
        /// メディア情報の一覧
        /// </summary>
        public static List<MediaData> Media_Data_List { get; set; } = [];

        public readonly BlobServiceClient Blob_Service_Client;
        public readonly CsvService Csv_Service;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="blobServiceClient"></param>
        /// <param name="csvService"></param>
        public IndexModel(BlobServiceClient blobServiceClient, CsvService csvService)
        {
            Blob_Service_Client = blobServiceClient;
            Csv_Service = csvService;
            Media_Data_List = CsvService.ReadCSV(Csv_Service);
            Title_Dic = Media_Data_List
                .Where(x => x.IsShow == true && x.Deleted == false && x.Type == 0)
                .OrderBy(x => x.Priority)
                .ToDictionary(x => x.Name, x => x.Title);
        }

        [HttpGet]
        public async Task OnGetAsync()
        {
            await GetDicAsync(BlobService.Image_Container_Name);
            await GetDicAsync(BlobService.Video_Container_Name);
            // 音声ファイル、データファイルは未実装
            //await GetDicAsync(BlobService.Sound_Container_Name);
            //await GetDicAsync(BlobService.Data_Container_Name);
        }

        /// <summary>
        /// ファイル名をキー、URLをバリューとするディクショナリを取得
        /// </summary>
        async Task GetDicAsync(string name)
        {
            var containerClient = Blob_Service_Client.GetBlobContainerClient(name);
            var urlList = new List<string>();

            await foreach (var blobItem in containerClient.GetBlobsAsync()) {
                var uri = containerClient.GetBlobClient(blobItem.Name).Uri;
                urlList.Add(uri.ToString());
            }

            switch (name) {
                default:
                    break;
                case BlobService.Image_Container_Name:
                    Image_Dic = urlList.ToDictionary(x => x.Split("/").Last().Split(".").First().Replace("_", string.Empty));
                    break;
                case BlobService.Video_Container_Name:
                    Video_Dic = urlList.ToDictionary(x => x.Split("/").Last().Split(".").First());
                    break;
                case BlobService.Sound_Container_Name:
                    Sound_Dic = urlList.ToDictionary(x => x.Split("/").Last().Split(".").First());
                    break;
                case BlobService.Data_Container_Name:
                    Data_Dic = urlList.ToDictionary(x => x.Split("/").Last().Split(".").First());
                    break;
            }
        }
    }
}
