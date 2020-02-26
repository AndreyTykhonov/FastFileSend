using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FastFileSend.Program
{
    public class FileInfo
    {
        public string Name { get; set; }
        public Stream Content { get; set; }
    }
}
