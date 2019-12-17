﻿using System;

namespace FastFileSend.UI
{
    [Flags]
    public enum HistoryModelStatus
    {
        Awaiting = 0,
        Ok = 1,
        Uploading = 2,
        UsingAPI = 3,
        Downloading = 4
    }
}
