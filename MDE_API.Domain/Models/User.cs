﻿namespace MDE_API.Domain.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }

        public int Role { get; set; }

        public int CompanyID { get; set; }
    }
}
