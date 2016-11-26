using System;
using System.IO;
using System.IO.Compression;

namespace MyWhiteboard.Stroke
{
    public static class ByteArrayCompressionUtility
    {
        public static byte[] Compress(this byte[] inputData)
        {
            if (inputData == null)
            {
                throw new ArgumentNullException(nameof(inputData), "inputData must be non-null");
            }

            using (var compressIntoMs = new MemoryStream())
            {
                using (var gzs = new GZipStream(compressIntoMs,
                    CompressionMode.Compress))
                {
                    gzs.Write(inputData, 0, inputData.Length);
                }
                return compressIntoMs.ToArray();
            }
        }

        public static byte[] Decompress(this byte[] inputData)
        {
            if (inputData == null)
            {
                throw new ArgumentNullException(nameof(inputData), "inputData must be non-null");
            }

            using (var compressedMs = new MemoryStream(inputData))
            {
                using (var decompressedMs = new MemoryStream())
                {
                    using (var gzs = new GZipStream(compressedMs,
                        CompressionMode.Decompress))
                    {
                        gzs.CopyTo(decompressedMs);
                    }
                    return decompressedMs.ToArray();
                }
            }
        }
    }
}