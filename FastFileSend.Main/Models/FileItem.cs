using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastFileSend.Main.Models
{
    /// <summary>
    /// Represents model of file that uploaded to API server.
    /// </summary>
    public class FileItem
    {
        public FileItem(int id, string name, long size, int cRC32, DateTime creationDate, Uri url)
        {
            Id = id;
            Name = name;
            Size = size;
            CRC32 = cRC32;
            CreationDate = creationDate;
            Url = url;
        }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public int CRC32 { get; set; }
        public DateTime CreationDate { get; set; }
        public Uri Url { get; set; }
    }
}
