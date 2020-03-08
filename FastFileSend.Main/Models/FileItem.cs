using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace FastFileSend.Main.Models
{
    /// <summary>
    /// Represents model of file that uploaded to API server.
    /// </summary>
    public class FileItem
    {
        public FileItem(int id, string name, long size, int cRC32, DateTime creationDate, List<Uri> url)
        {
            Id = id;
            Name = name;
            Size = size;
            CRC32 = cRC32;
            CreationDate = creationDate;
            Url = url;
        }

        public FileItem()
        {

        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public int CRC32 { get; set; }
        [JsonIgnore]
        public DateTime CreationDate { get; set; }

        // To store in DB.
        [JsonIgnore]
        [NotMapped]
        public List<Uri> Url
        {
            get
            {
                return UriListStr.Split(',').Select(x => new Uri(x)).ToList();
            }
            set
            {
                if (value is null)
                {
                    return;
                }

                UriListStr = string.Join(",", value.Select(x => x.AbsoluteUri));
            }
        }

        private string UriListStr;
    }
}
