using FastFileSend.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;

namespace FastFileSend.UI
{
    class HistoryViewModelUpdater
    {
        HistoryViewModel HistoryViewModel { get; set; }
        ApiServer ApiServer { get; set; }
        
        System.Timers.Timer TimerUpdateHistory { get; set; }
        SynchronizationContext uiContext;

        public HistoryViewModelUpdater(ApiServer apiServer, HistoryViewModel historyViewModel)
        {
            ApiServer = apiServer;
            HistoryViewModel = historyViewModel;

            uiContext = SynchronizationContext.Current;

            TimerUpdateHistory = new System.Timers.Timer(1000);
            TimerUpdateHistory.Elapsed += TimerUpdateHistory_Elapsed;
            TimerUpdateHistory.Start();

            TimerUpdateHistory_Elapsed(this, null);
        }

        private async void TimerUpdateHistory_Elapsed(object sender, ElapsedEventArgs e)
        {
            List<HistoryItem> historyList = await ApiServer.GetHistory();
            var historyModel = historyList.Select(x => HistoryItemToHistoryModel(x));

            foreach (HistoryModel model in historyModel)
            {
                var duplicate = HistoryViewModel.List.ToArray().FirstOrDefault(x => x.Id == model.Id);

                if (duplicate != null)
                {
                    if (duplicate.Fake)
                    {
                        uiContext.Send(x => HistoryViewModel.List.Remove(duplicate), null);
                    }
                    else
                    {
                        continue;
                    }
                }

                uiContext.Send(x => HistoryViewModel.List.Insert(0, model), null);
            }
        }

        HistoryModel HistoryItemToHistoryModel(HistoryItem historyItem)
        {
            bool imReceiver = historyItem.Receiver == ApiServer.Id;
            bool imAwaitingDownload = historyItem.Status == 0;
            bool timeToDownload = imReceiver && imAwaitingDownload;

            HistoryModel historyModel = new HistoryModel()
            {
                ETA = "",
                Id = historyItem.Id,
                Name = historyItem.File.Name,
                Receiver = historyItem.Receiver,
                Sender = historyItem.Sender,
                Size = historyItem.File.Size,
                StatusText = timeToDownload ? "Preparing to download" : "Awaiting remote download",
                Url = historyItem.File.Url,
                Status = historyItem.Status,
                Progress = timeToDownload ? 0 : 100
            };

            if (historyItem.Status == 1)
            {
                historyModel.StatusText = "OK";
            }

            return historyModel;
        }
    }
}
