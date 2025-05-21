using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;
using MDE_API.Domain.Models;
using MDE_API.Application.Services;
using MDE_API.Application.Interfaces;
using System.Data;

namespace MDE_API.Application.Services
{
    public class DashboardService: IDashboardService
    {


        private readonly IDashboardRepository _dashboardRepository;


        public DashboardService(IDashboardRepository dashboardRepository)
            {
                _dashboardRepository = dashboardRepository;
            }

            public ObservableCollection<DashboardPage> GetDashboardPages(int machineId)
            {
                return _dashboardRepository.GetDashboardPages(machineId);
            }

            public void AddDashboardPage(int machineId, string pageName, string pageUrl)
            {
                _dashboardRepository.AddDashboardPage(machineId, pageName, pageUrl);
            }

            public void DeleteDashboardPage(int pageId)
            {
                _dashboardRepository.DeleteDashboardPage(pageId);
            }

            public string GetFirstDashboardPageUrl(int machineId)
            {
                return _dashboardRepository.GetFirstDashboardPageUrl(machineId);
            }
        }


        
    
}
