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
        public int Id { get; set; }
        public string LocalName { get; set; } 
        public bool Online { get; set; }
    }
}
