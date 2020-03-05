using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastFileSend.WebCore.DataBase
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Password { get; set; }
        public DateTime RegisterDate { get; set; }
        public DateTime LastOnline { get; set; }
    }
}