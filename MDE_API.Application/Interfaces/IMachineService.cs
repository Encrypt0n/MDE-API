using MDE_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDE_API.Application.Interfaces
{
    public interface IMachineService
    {
        ObservableCollection<Machine> GetMachinesForCompany(int companyId);
        Machine GetMachineById(int machineId);

        void UpdateDashboardUrl(int machineId, string dashboardUrl);

    }
}
