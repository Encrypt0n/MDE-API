using MDE_API.Application.Interfaces;
using MDE_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDE_API.Application.Services
{
    public class VPNClientNotifier: IVPNClientNotifier
    {
        private readonly IEnumerable<IVPNClientConnectedObserver> _observers;

        public VPNClientNotifier(IEnumerable<IVPNClientConnectedObserver> observers)
        {
            _observers = observers;
        }

        public async Task NotifyAsync(VpnClientConnectModel model, string baseName, int machineId)
        {
            foreach (var observer in _observers)
            {
                await observer.OnClientConnectedAsync(model, baseName, machineId);
            }
        }
    }

}
