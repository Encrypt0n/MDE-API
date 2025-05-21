using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MDE_API.Application.Interfaces;
using MDE_API.Application.Interfaces.MDE_API.Services;
using MDE_API.Domain.Models;

namespace MDE_API.Application.Services
{


   
        public class ActivityService : IActivityService
        {
            private readonly IActivityRepository _activityRepository;

            public ActivityService(IActivityRepository activityRepository)
            {
                _activityRepository = activityRepository;
            }

            public void LogActivity(UserActivityLog activity)
            {
                _activityRepository.LogActivity(activity);
            }

            public ObservableCollection<UserActivityLog> GetActivitiesForMachine(int machineId)
            {
                return _activityRepository.GetActivitiesForMachine(machineId);
            }
        }
    



}
