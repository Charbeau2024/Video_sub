namespace Video.Helper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Microsoft.AspNetCore.Hosting;

    public class CsvService
    {
        private readonly IWebHostEnvironment Env;

        public static CsvService Csv_Service { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="env">環境</param>
        public CsvService(IWebHostEnvironment env)
        {
            Env = env;
        }

        /// <summary>
        /// CSV読み込み
        /// </summary>
        /// <param name="csvService"></param>
        /// <returns>メディア情報のリスト</returns>
        public static List<MediaData> ReadCSV(CsvService csvService)
        {
            var retval = new List<MediaData>();
            Csv_Service = csvService;
            var filePath = Path.Combine(csvService.Env.WebRootPath, "files", "media.csv");
            using (var stream = new StreamReader(filePath)) {
                while (!stream.EndOfStream) {
                    var line = stream.ReadLine();

                    if (line is not null) {
                        var array = line.Split(',');
                        var data = new MediaData {
                            Id = int.Parse(array[0]),
                            Name = array[1],
                            Title = array[2],
                            Type = int.Parse(array[3]),
                            Priority = int.Parse(array[4]),
                            PublishDate = DateTime.Parse(array[5]),
                            Count = long.Parse(array[6]),
                            Deleted = array[7] != "0",
                            IsShow = array[8] != "0",
                        };
                        retval.Add(data);
                    }
                }
            }

            return retval;
        }

        /// <summary>
        /// ファイル出力を行なう
        /// </summary>
        /// <param name="lines">1行</param>
        public static void ExportCsv(IEnumerable<MediaData> lines)
        {
            var filePath = Path.Combine(Csv_Service.Env.WebRootPath, "files", "media.csv");
            using (var stream = new StreamWriter(filePath, false, Encoding.UTF8)) {
                foreach (var line in lines) {
                    stream.WriteLine($"{line.Id},{line.Name},{line.Title},{line.Type},{line.Priority},{line.PublishDate},{line.Count},{(line.Deleted ? 1 : 0)},{(line.IsShow ? 1 : 0)}");
                }
            }
        }
    }

    /// <summary>
    /// データ構造
    /// </summary>
    public class MediaData
    {
        /// <summary>
        /// ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// メディアのファイル名（拡張子を除く）
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// タイトル
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 種別
        /// 0: video
        /// 1: audio
        /// 2: image
        /// 3: text
        /// 4. file
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 表示順
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 公開日
        /// </summary>
        public DateTime PublishDate { get; set; }

        /// <summary>
        /// 視聴回数
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// 論理削除フラグ
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// 表示 / 非表示
        /// </summary>
        public bool IsShow { get; set; }
    }
}
