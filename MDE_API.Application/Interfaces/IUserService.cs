using MDE_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDE_API.Application.Interfaces
{
    public interface IUserService
    {
        bool RegisterUser(string username, string password);
        User ValidateUser(string username, string password);

    }
}
