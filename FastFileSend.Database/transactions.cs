//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FastFileSend.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class transactions
    {
        public int download_idx { get; set; }
        public int receiver_id { get; set; }
        public int sender_id { get; set; }
        public int file_id { get; set; }
        public int status { get; set; }
        public System.DateTime date { get; set; }
    
        public virtual files files_idx { get; set; }
        public virtual users receiver_useridx { get; set; }
        public virtual users sender_useridx { get; set; }
    }
}