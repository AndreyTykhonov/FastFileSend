using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FastFileSend.Main.Models
{
    /// <summary>
    /// Represents file name and stream. Using by FileUpload.
    /// </summary>
    public class FileInfo
    {
        public string Name { get; set; }
        public Stream Content { get; set; }
    }
}
