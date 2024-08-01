namespace Video.Helper
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// 暗号化
    /// </summary>
    class CryptographHelper
    {
        /// <summary>
        /// 変換しない場合
        /// </summary>
        /// <param name="input">変換元</param>
        static string Identity(string input) => input;

        /// <summary>
        /// 暗号化関数
        /// </summary>
        Func<string, string> encoder = Identity;

        /// <summary>
        /// 復号関数
        /// </summary>
        Func<string, string> decoder = Identity;

        /// <summary>
        /// 変換関数生成
        /// </summary>
        /// <param name="algorithm">変換アルゴリズム</param>
        static Func<byte[], byte[]> MakeTransformer(ICryptoTransform algorithm)
        {
            return (byte[] source) => {
                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, algorithm, CryptoStreamMode.Write)) {
                    cs.Write(source, 0, source.Length);
                    cs.FlushFinalBlock();
                    return ms.ToArray();
                }
            };
        }

        /// <summary>
        /// エンコーダー生成
        /// </summary>
        /// <param name="transform">変換関数</param>
        static Func<string, string> MakeEncoder(Func<byte[], byte[]> transform)
            => plainText => Convert.ToBase64String(transform(Encoding.UTF8.GetBytes(plainText)));

        /// <summary>
        /// デコーダー生成
        /// </summary>
        /// <param name="transform">変換関数</param>
        static Func<string, string> MakeDecoder(Func<byte[], byte[]> transform)
            // base64をPOSTすると、＋は空白に置き換えられるので、それを元に戻す
            => cryptogram => Encoding.UTF8.GetString(transform(Convert.FromBase64String(cryptogram.Replace(" ", "+"))));

        /// <summary>
        /// アルゴリズムに従ったエンコーダーを設定
        /// </summary>
        /// <param name="algorithm">暗号化アルゴリズムまたはnull</param>
        /// <remarks>
        /// algorithm が null ならば内部エンコーダーにidentity を設定する。そうでなければ algorithm から生成したエンコーダーを設定する
        /// </remarks>
        public CryptographHelper SetEncoder(SymmetricAlgorithm algorithm)
        {
            encoder = algorithm != null ? MakeEncoder(MakeTransformer(algorithm.CreateEncryptor())) : Identity;
            return this;
        }

        /// <summary>
        /// アルゴリズムに従ったデコーダーを設定
        /// </summary>
        /// <param name="algorithm">暗号化アルゴリズムまたはnull</param>
        /// <remarks>
        /// algorithm が null ならば内部デコーダーに identity を設定する。そうでなければ algorithm から生成したデコーダーを設定する
        /// </remarks>
        public CryptographHelper SetDecoder(SymmetricAlgorithm algorithm)
        {
            decoder = algorithm != null ? MakeDecoder(MakeTransformer(algorithm.CreateDecryptor())) : Identity;
            return this;
        }

        /// <summary>
        /// AESの暗号化アルゴリズムを作る
        /// </summary>
        /// <returns>暗号化アルゴリズム</returns>
        static SymmetricAlgorithm CreateAesAlgorithm() => Aes.Create();

        /// <summary>
        /// 内部のエンコーダー、デコーダーを設定する
        /// </summary>
        public CryptographHelper BuildFromConfig()
        {
            var settings = Startup.Configuration.GetSection("AppConfiguration");
            SetDecoder(null);
            SetEncoder(null);

            if (settings != null) {
                const string prefix = "cryptographer_";
                var algorithm = CreateAesAlgorithm();
                var kvpList = settings.GetChildren().Select(x => x.Key)
                    .Where(_ => (_.StartsWith(prefix, true, null) && _.Length > prefix.Length))
                    .Select(_ => new {
                        Key = _.Substring(prefix.Length),
                        Value = settings.GetSection(_).Value,
                    });

                string encodeKey = null;
                string decodeKey = null;

                foreach (var kv in kvpList) {
                    switch (kv.Key.ToLower()) {
                        case "mode":
                            var mode = algorithm.Mode;

                            if (Enum.TryParse(kv.Value, true, out mode)) {
                                algorithm.Mode = mode;
                            } else {
                                Trace.TraceWarning($"unknown Cryptographer:{kv.Key} ignored {kv.Value} used {mode}");
                            }

                            break;
                        case "padding":
                            var padding = algorithm.Padding;

                            if (Enum.TryParse(kv.Value, true, out padding)) {
                                algorithm.Padding = padding;
                            } else {
                                Trace.TraceWarning($"unknown Cryptographer:{kv.Key} ignored {kv.Value} used {padding}");
                            }

                            break;
                        case "keysize":
                            var keySize = algorithm.KeySize;

                            if (int.TryParse(kv.Value, out keySize)) {
                                algorithm.KeySize = keySize;
                            } else {
                                Trace.TraceWarning($"invalid Cryptographer:{kv.Key} ignored {kv.Value} used {keySize}");
                            }

                            break;
                        case "blocksize":
                            var blockSize = algorithm.BlockSize;

                            if (int.TryParse(kv.Value, out blockSize)) {
                                algorithm.BlockSize = blockSize;
                            } else {
                                Trace.TraceWarning($"invalid Cryptographer:{kv.Key} ignored {kv.Value} used {blockSize}");
                            }

                            break;
                        case "encodekey":
                            encodeKey = kv.Value;
                            break;
                        case "decodekey":
                            decodeKey = kv.Value;
                            break;
                        case "key":
                            encodeKey ??= kv.Value;
                            decodeKey ??= kv.Value;
                            break;
                        default:
                            break;
                    }
                }

                encodeKey ??= decodeKey;
                decodeKey ??= encodeKey;

                if (string.IsNullOrEmpty(encodeKey) && string.IsNullOrEmpty(decodeKey)) {
                    Startup.Logger.LogError("Cryptographer:key not assigned");
                    return this;
                }

                var keyByteArraySize = (algorithm.KeySize + 7) / 8;
                var encodeKeyBytes = Encoding.UTF8.GetBytes(encodeKey);
                Array.Resize(ref encodeKeyBytes, keyByteArraySize);
                algorithm.Key = encodeKeyBytes;
                SetEncoder(algorithm);
                var decodeKeyBytes = Encoding.UTF8.GetBytes(decodeKey);
                Array.Resize(ref decodeKeyBytes, keyByteArraySize);
                algorithm.Key = decodeKeyBytes;
                SetDecoder(algorithm);
            }

            return this;
        }

        /// <summary>
        /// web.config のアプリケーション設定から生成
        /// </summary>
        /// <returns>暗号処理</returns>
        public static CryptographHelper CreateFromWebConfig() => new CryptographHelper().BuildFromConfig();

        /// <summary>
        /// 暗号化
        /// </summary>
        /// <param name="plainText">平文</param>
        /// <returns>暗号文</returns>
        public string Encode(string plainText) => encoder(plainText);

        /// <summary>
        /// 復号
        /// </summary>
        /// <param name="cryptogram">暗号文</param>
        /// <returns>平文</returns>
        public string Decode(string cryptogram) => decoder(cryptogram);
    }
}