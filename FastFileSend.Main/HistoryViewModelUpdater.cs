﻿using AutoMapper;
using FastFileSend.Main;
using FastFileSend.Main.Enum;
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
            DateTime minimumDate = DateTime.MinValue;

            var notFakes = HistoryListViewModel.List.Where(x => !x.Fake).ToList();
            if (notFakes.Count > 0)
            {
                minimumDate = notFakes.Max(x => x.Date);
            }

            minimumDate = minimumDate.ToUniversalTime();

            List<HistoryModel> historyList = await ApiServer.GetHistory(minimumDate).ConfigureAwait(false);
            List<HistoryViewModel> historyViewModels = historyList.Select(x => new HistoryViewModel(x)).ToList();

            foreach (HistoryViewModel model in historyViewModels)
            {
                model.Sending = model.Sender == ApiServer.AccountDetails.Id;
                model.Date = model.Date.ToLocalTime();

                if (model.Status == HistoryModelStatus.Awaiting && !model.Sending)
                {
                    model.Status = HistoryModelStatus.Downloading;
                }

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

                model.Progress = model.Id == ApiServer.AccountDetails.Id ? 0 : 1;

                uiContext.Send(x => HistoryListViewModel.List.Insert(0, model), null);
            }

            // Update statuses
            var statusNoOk = HistoryListViewModel.List.Where(x => x.Status != HistoryModelStatus.Ok).Where(x => !x.Fake).ToList();
            foreach (HistoryViewModel model in statusNoOk)
            {
                if (model.Status == HistoryModelStatus.Downloading)
                {
                    continue;
                }

                HistoryModelStatus modelStatus = await ApiServer.GetFileStatus(model.Id).ConfigureAwait(false);
                if (model.Status != modelStatus)
                {
                    model.Status = modelStatus;
                }
            }

            // Restart timer after update.
            TimerUpdateHistory.Start();
        }
    }
}
