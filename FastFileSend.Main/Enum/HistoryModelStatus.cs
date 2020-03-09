using System;

namespace FastFileSend.Main.Enum
{
    /// <summary>
    /// Represent HistoryModel possible status.
    /// </summary>
    public enum HistoryModelStatus
    {
        Awaiting = 0,
        Ok = 1,
        Uploading = 2,
        UsingAPI = 3,
        Downloading = 4,
        Archiving = 5,
        Unpacking = 6
    }
}
