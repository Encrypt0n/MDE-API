using MDE_API.Domain.Models;

namespace MDE_API.Application.Interfaces
{
    public interface IDatabaseService
    {
        void SaveClientConnection(string clientName, string description, int userId, string assignedIp);
        bool RegisterUser(string username, string password);
        User ValidateUser(string username, string password);
    }
}
