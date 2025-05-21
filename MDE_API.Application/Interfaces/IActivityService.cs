using MDE_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDE_API.Application.Interfaces
{
    

    namespace MDE_API.Services
    {
        
            public interface IActivityService
            {
                void LogActivity(UserActivityLog activity);
                ObservableCollection<UserActivityLog> GetActivitiesForMachine(int machineId);
            }
        

    }

}
