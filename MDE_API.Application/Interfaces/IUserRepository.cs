using MDE_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDE_API.Application.Interfaces
{
    public interface IUserRepository
    {
        bool UserExists(string username);
        void CreateUser(string username, string passwordHash, int companyId);
        UserWithPasswordHash GetUserWithPasswordHash(string username);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
    }

}
