using MDE_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MDE_API.Application.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace MDE_API.Application.Services
{
    public class UserService: IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public bool RegisterUser(string username, string password)
        {
            if (_userRepository.UserExists(username))
                return false;

            string hash = BCrypt.Net.BCrypt.HashPassword(password);
            _userRepository.CreateUser(username, hash);
            return true;
        }

        public User ValidateUser(string username, string password)
        {
            var userData = _userRepository.GetUserWithPasswordHash(username);
            if (userData == null) return null;

            if (BCrypt.Net.BCrypt.Verify(password, userData.PasswordHash))
            {
                return new User
                {
                    UserID = userData.UserID,
                    Username = username
                };
            }

            return null;
        }
        
    }

}
