using MDE_API.Application.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MDE_API.Domain.Models;
using System.Data;
using System.Net.Http.Json;
using System.Net.Http;

namespace MDE_API.Application.Services
{
    public class MachineService: IMachineService
    {
        

       
        private readonly IMachineRepository _machineRepository;

        public MachineService(IMachineRepository machineRepository)
        {
            _machineRepository = machineRepository;
        }

        public ObservableCollection<Machine> GetMachinesForCompany(int companyId)
        {
            return _machineRepository.GetMachinesForCompany(companyId);
        }

        public Machine GetMachineById(int machineId)
        {
            return _machineRepository.GetMachineById(machineId);
        }

        public void UpdateDashboardUrl(int machineId, string dashboardUrl)
        {
            _machineRepository.UpdateDashboardUrl(machineId, dashboardUrl);
        }



    }
}
