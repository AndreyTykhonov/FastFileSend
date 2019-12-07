using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FastFileSend.Main;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastFileSend.Tests
{
    [TestClass]
    public class FileUploadTest
    {
        [TestMethod]
        public async Task TestFexUpload()
        {
            IFileUploader uploader = new FexFileUploader();
            CloudFile cloudFile = await uploader.UploadAsync("test.png");
            Assert.IsTrue(cloudFile.CRC32 == 0x7b7a659e, "Wrong file crc32!");
            Assert.IsTrue(cloudFile.FileName == "test.png", "Wrong file nane!");
            Assert.IsTrue(cloudFile.Size == 54827, "Wrong file size!");

            Assert.IsFalse(string.IsNullOrEmpty(cloudFile.Url), "Wrong download url!");

            Debug.WriteLine(cloudFile);
        }
    }
}
