//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FastFileSend.Web.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class files
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public files()
        {
            this.transactions1 = new HashSet<transactions>();
        }
    
        public int file_idx { get; set; }
        public string file_name { get; set; }
        public long file_size { get; set; }
        public int file_crc32 { get; set; }
        public System.DateTime file_creationdate { get; set; }
        public string file_url { get; set; }
    
        public virtual transactions transactions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactions> transactions1 { get; set; }
    }
}