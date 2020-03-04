using System;
using System.Collections.Generic;
using System.Text;

namespace FastFileSend.Main.Models
{
    /// <summary>
    /// This model used to store user account details.
    /// </summary>
    public class AccountDetails
    {
        public AccountDetails(int id, string password)
        {
            Id = id;
            Password = password;
        }

        public int Id { get; set; }
        public string Password { get; set; }
    }
}
