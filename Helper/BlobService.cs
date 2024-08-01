namespace Video.Helper
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// AzureStorage の操作を扱うクラス
    /// </summary>
    public class BlobService
    {
        /// <summary>
        /// ブロブサービスクライアント
        /// </summary>
        public static BlobServiceClient Blob_Service_Client { get; set; }

        // コンテナ名
        public const string Video_Container_Name = "video";
        public const string Sound_Container_Name = "sound";
        public const string Image_Container_Name = "image";
        public const string Data_Container_Name = "data";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BlobService()
        {
            if (Blob_Service_Client == null) {
                Blob_Service_Client = new BlobServiceClient(Startup.Configuration.GetConnectionString("AzureBlobStorage"));
            }
        }

        /// <summary>
        /// 動画名の一覧を取得する
        /// </summary>
        /// <param name="serviceCrient"></param>
        /// <returns>動画名一覧</returns>
        public static async Task<List<string>> GetVideoNameListFromStorageAsync(BlobServiceClient serviceCrient)
        {
            var blobContainerClient = serviceCrient.GetBlobContainerClient(Video_Container_Name);
            var blobs = blobContainerClient.GetBlobsAsync();

            var blobNames = new List<string>();
            await foreach (var blobItem in blobs) {
                blobNames.Add(blobItem.Name.Split("/").Last().Split(".").First());
            }
            return blobNames;
        }
    }
}
