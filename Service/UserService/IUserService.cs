using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SachidaPaudel.Models;

namespace SachidaPaudel.Service.UserService
{
    internal interface IUserService
    {
        void SaveUserData(User user);
        bool RegisterUser(User user);
        void UpdateLoginStatus(bool status);
        bool IsUserLoggedIn { get; }
        User RetrieveUserData();
        bool AuthenticateUser(string username, string password);
        void SignOut();


    }
}