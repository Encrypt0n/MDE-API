using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDE_API.Domain.Models
{
    public class Company
    {
        public int CompanyID { get; set; }
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; }
        public string Subnet { get; set; }
    }

}
