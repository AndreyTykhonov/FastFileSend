using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FastFileSend.Main;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastFileSend.Tests
{
    [TestClass]
    public class ApiServerTests
    {
        [TestMethod]
        public async Task FullTest()
        {
            // Don't test often
            return;

            ApiServer apiServer = await ApiServer.CreateNewAccount();

            Debug.WriteLine("My id " + apiServer.Id);
            Debug.WriteLine("My password " + apiServer.Password);

            CloudFile cloudFile = new CloudFile(0, "test", 0, DateTime.Now, "http://net.com");

            FileItem uploadedFile = await apiServer.Upload(cloudFile);

            Debug.WriteLine("File id = " + uploadedFile.Id);

            await apiServer.Send(uploadedFile, apiServer.Id);

            List<HistoryItem> myHistory = await apiServer.GetHistory();

            Assert.IsTrue(myHistory.Count == 1, "Wrong history length!");

            HistoryItem sendedHistory = myHistory.First();

            Assert.IsTrue(sendedHistory.Status == 0, "Wrong history status!");

            Assert.IsFalse(sendedHistory.Sender != apiServer.Id, "Wrong sender!");
            Assert.IsFalse(sendedHistory.Receiver != apiServer.Id, "Wrong receiver!");
            Assert.IsFalse(sendedHistory.File.Id != uploadedFile.Id, "Wrong file id!");

            //await apiServer.NotifyDownloadedAsync(uploadedFile);

            /* Status sets with delay
            myHistory = await apiServer.GetHistory();
            sendedHistory = myHistory.First();

            Assert.IsTrue(sendedHistory.Status == 1, "Wrong history status after notified!");
            */
        }
    }
}
