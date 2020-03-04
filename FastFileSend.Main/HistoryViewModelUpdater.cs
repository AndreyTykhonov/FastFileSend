using AutoMapper;
using FastFileSend.Main;
using FastFileSend.Main.Models;
using FastFileSend.Main.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;

namespace FastFileSend.Main
{
    /// <summary>
    /// Gathering actual HistoryListView from the API server.
    /// </summary>
    class HistoryViewModelUpdater
    {
        HistoryListViewModel HistoryListViewModel { get; set; }
        Api ApiServer { get; set; }
        
        System.Timers.Timer TimerUpdateHistory { get; set; }
        SynchronizationContext uiContext;

        const double HistoryUpdateInterval = 1000;

        public HistoryViewModelUpdater(Api apiServer, HistoryListViewModel historyViewModel)
        {
            ApiServer = apiServer;
            HistoryListViewModel = historyViewModel;

            uiContext = SynchronizationContext.Current;

            TimerUpdateHistory = new System.Timers.Timer(HistoryUpdateInterval);
            TimerUpdateHistory.Elapsed += TimerUpdateHistory_Elapsed;
            TimerUpdateHistory.AutoReset = false;

            TimerUpdateHistory_Elapsed(this, null);
        }

        /// <summary>
        /// Triggers history update.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TimerUpdateHistory_Elapsed(object sender, ElapsedEventArgs e)
        {
            DateTime minimumDate = HistoryListViewModel.List.Max(x => x.Date);
            List<HistoryModel> historyList = await ApiServer.GetHistory(minimumDate);
            List<HistoryViewModel> historyViewModels = historyList.Select(x => x as HistoryViewModel).ToList();

            foreach (HistoryViewModel model in historyViewModels)
            {
                var duplicate = HistoryListViewModel.List.FirstOrDefault(x => x.Id == model.Id);

                if (duplicate != null)
                {
                    if (duplicate.Fake)
                    {
                        DynamicMapper.Map(model, duplicate);
                        //uiContext.Send(x => HistoryViewModel.List.Remove(duplicate), null);
                    }
                    if (duplicate.Status != model.Status)
                    {
                        duplicate.Status = model.Status;
                    }

                    continue;
                }

                uiContext.Send(x => HistoryListViewModel.List.Insert(0, model), null);
            }

            // Restart timer after update.
            TimerUpdateHistory.Start();
        }
    }
}
