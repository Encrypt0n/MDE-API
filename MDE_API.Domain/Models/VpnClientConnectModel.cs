using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDE_API.Domain.Models
{
    public class VpnClientConnectModel
    {
        public string ClientName { get; set; }
        public string Description { get; set; }
        public int CompanyID { get; set; }
        public string AssignedIp { get; set; }
        public List<string> UiBuilderUrls { get; set; }
    }
}
