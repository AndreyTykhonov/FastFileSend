using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FastFileSend.Main
{
    public static class ZipFileExtensions
    {
        public static async Task SaveAsync(this ZipFile zip, string path)
        {
            await Task.Run(() => zip.Save(path)).ConfigureAwait(false);
        }

        public static async Task ExtractAllAsync(this ZipFile zip, string path)
        {
            await Task.Run(() => zip.ExtractAll(path)).ConfigureAwait(false);
        }
    }
}
