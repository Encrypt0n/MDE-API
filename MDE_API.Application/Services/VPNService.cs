using MDE_API.Application.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MDE_API.Application.Interfaces;

namespace MDE_API.Application.Services
{
    public class VPNService: IVPNService
    {
        
            private readonly IVPNRepository _vpnRepository;

            public VPNService(IVPNRepository vpnRepository)
            {
                _vpnRepository = vpnRepository;
            }

            public void SaveClientConnection(string clientName, string description, int userId, string assignedIp)
            {
                _vpnRepository.SaveClientConnection(clientName, description, userId, assignedIp);
            }
        }




       
    
}
