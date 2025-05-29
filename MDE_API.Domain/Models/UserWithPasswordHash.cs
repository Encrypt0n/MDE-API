using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDE_API.Domain.Models
{
    public class UserWithPasswordHash
    {
        public int UserID { get; set; }
        public string PasswordHash { get; set; }

        public int Role { get; set; }

        public int CompanyID { get; set; }
    }

}
