using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDE_API.Application.Interfaces
{
    public interface IVPNService
    {
        int SaveClientConnection(string clientName, string description, int companyId, string assignedIp, List<string> uibuilderUrls);
        Task AddCloudflareDnsRecordAsync(string baseName);
    }
}
