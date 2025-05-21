using MDE_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDE_API.Application.Interfaces
{
    public interface IDashboardService
    {
        ObservableCollection<DashboardPage> GetDashboardPages(int machineId);
        void AddDashboardPage(int machineId, string pageName, string pageUrl);
        void DeleteDashboardPage(int pageId);
        string GetFirstDashboardPageUrl(int machineId);

    }
}
