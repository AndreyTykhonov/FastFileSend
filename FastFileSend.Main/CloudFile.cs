using Newtonsoft.Json;
using System;

namespace FastFileSend.Main
{
    public class CloudFile
    {
        public CloudFile(long size, string fileName, int cRC32, DateTime creationDate, string url)
        {
            Size = size;
            FileName = fileName;
            CRC32 = cRC32;
            CreationDate = creationDate;
            Url = url;
        }

        public long Size { get; private set; }
        public string FileName { get; private set; }
        public int CRC32 { get; private set; }
        public DateTime CreationDate { get; private set; }
        public string Url { get; private set; }

        public override string ToString()
        {
            return Url;
        }
    }
}
