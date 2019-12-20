using System;

namespace FastFileSend.Main
{
    public class FileItem
    {
        public FileItem(int id, string name, long size, int cRC32, DateTime creationDate, string url)
        {
            Id = id;
            Name = name;
            Size = size;
            CRC32 = cRC32;
            CreationDate = creationDate;
            Url = url;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public int CRC32 { get; set; }
        public System.DateTime CreationDate { get; set; }
        public string Url { get; set; }
    }
}
