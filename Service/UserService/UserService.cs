using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SachidaPaudel.Models;
using SachidaPaudel.Utils;

namespace SachidaPaudel.Service.UserService
{
    public class UserService : IUserService
    {
        private User? _currentUser;
        private bool _isUserLoggedIn;
        private readonly CsvHelper _csvHelper;

        public UserService(CsvHelper csvHelper)
        {
            _csvHelper = csvHelper;
        }

        public void SaveUserData(User user)
        {
            // Save user data using CsvHelper
            _csvHelper.SaveUser(user);
            _currentUser = user;
        }

        public bool RegisterUser(User user)
        {
            // Registration logic (e.g., save user to a database)
            // For simplicity, let's assume registration is always successful
            SaveUserData(user);
            return true;
        }

        public void UpdateLoginStatus(bool status)
        {
            _isUserLoggedIn = status;
        }

        public bool IsUserLoggedIn => _isUserLoggedIn;

        public User RetrieveUserData()
        {
            // Retrieve user data logic (e.g., fetch from a database or local storage)
            if (_currentUser == null)
            {
                throw new InvalidOperationException("No user is currently logged in.");
            }
            return _currentUser;
        }

        public bool AuthenticateUser(string username, string password)
        {
            // Authentication logic (e.g., check username and password against a database)
            var users = _csvHelper.LoadUsers();
            var user = users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                _currentUser = user;
                UpdateLoginStatus(true);
                return true;
            }
            return false;
        }

        public void SignOut()
        {
            // Sign out logic (e.g., clear user session)
            _currentUser = null;
            UpdateLoginStatus(false);
        }
    }
}
