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

namespace MDE_API.Application.Services
{
    public class MachineService: IMachineService
    {
        

       
        private readonly IMachineRepository _machineRepository;

        public MachineService(IMachineRepository machineRepository)
        {
            _machineRepository = machineRepository;
        }

        public ObservableCollection<Machine> GetMachinesForUser(int userId)
        {
            return _machineRepository.GetMachinesForUser(userId);
        }

        public Machine GetMachineById(int machineId)
        {
            return _machineRepository.GetMachineById(machineId);
        }
        
    }
}
