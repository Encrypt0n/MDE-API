using MDE_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDE_API.Application.Interfaces
{
    public interface IVPNClientConnectedObserver
    {
        Task OnClientConnectedAsync(VpnClientConnectModel model, string baseName, int machineId);
    }
}
