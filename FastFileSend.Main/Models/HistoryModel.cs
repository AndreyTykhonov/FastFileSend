using FastFileSend.Main.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FastFileSend.Main.Models
{
    /// <summary>
    /// Represents model of History item.
    /// </summary>
    public class HistoryModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public FileItem File { get; set; }
        public DateTime Date { get; set; }
        public virtual HistoryModelStatus Status { get; set; }
        public int Sender { get; set; }
        public int Receiver { get; set; }
        public virtual long Size { get; set; }
    }
}
