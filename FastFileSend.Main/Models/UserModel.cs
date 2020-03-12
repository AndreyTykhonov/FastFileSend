using System;
using System.Collections.Generic;
using System.Text;

namespace FastFileSend.Main.Models
{
    /// <summary>
    /// Represents model of User.
    /// </summary>
    public class UserModel
    {
        public virtual int Id { get; set; }
        public virtual string LocalName { get; set; } 
        public virtual bool Online { get; set; }
    }
}
