using FastFileSend.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace FastFileSend.UI
{
    class HistoryViewModelUpdater
    {
        HistoryViewModel HistoryViewModel { get; set; }
        ApiServer ApiServer { get; set; }
        
        Timer TimerUpdateHistory { get; set; }

        public HistoryViewModelUpdater(ApiServer apiServer, HistoryViewModel historyViewModel)
        {
            ApiServer = apiServer;
            HistoryViewModel = historyViewModel;

            TimerUpdateHistory = new Timer(1000);
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
                if (HistoryViewModel.List.Any(x => x.Id == model.Id))
                {
                    continue;
                }

                HistoryViewModel.List.Add(model);
            }
        }

        HistoryModel HistoryItemToHistoryModel(HistoryItem historyItem)
        {
            HistoryModel historyModel = new HistoryModel()
            {
                ETA = "",
                Id = historyItem.Id,
                Name = historyItem.File.Name,
                Receiver = historyItem.Receiver,
                Sender = historyItem.Sender,
                Size = historyItem.File.Size,
                StatusText = historyItem.Status == 0 && historyItem.Receiver == ApiServer.Id? "Preparing to download" : "Awaiting remote download",
                Url = historyItem.File.Url
            };

            return historyModel;
        }
    }
}
