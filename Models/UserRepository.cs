using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using B_M.Models;

namespace B_M.Models
{
    public class UserRepository : IDisposable
    {
        private readonly ApplicationDbContext _context;

        public UserRepository()
        {
            _context = new ApplicationDbContext();
        }

        // Existing methods...
        public User GetUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            return _context.Users
                .Include("UserDetails")
                .FirstOrDefault(u => u.Email == email);
        }

        public User GetUserByUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return null;

            return _context.Users
                .Include("UserDetails")
                .FirstOrDefault(u => u.UserName == username);
        }

        public User GetUserByEmailOrUsername(string emailOrUsername)
        {
            if (string.IsNullOrEmpty(emailOrUsername))
                return null;

            return _context.Users
                .Include("UserDetails")
                .FirstOrDefault(u => u.Email == emailOrUsername || u.UserName == emailOrUsername);
        }

        public User GetUserById(int userId)
        {
            return _context.Users
                .Include("UserDetails")
                .FirstOrDefault(u => u.UserID == userId);
        }

        public bool UsernameExists(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            return _context.Users.Any(u => u.UserName == username);
        }

        public bool EmailExists(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            return _context.Users.Any(u => u.Email == email);
        }

        // NEW: Check if Google email is already linked to any user
        public bool IsGoogleEmailLinked(string googleEmail, int? excludeUserID = null)
        {
            if (string.IsNullOrEmpty(googleEmail))
                return false;

            return _context.Users
                .Any(u => u.Email == googleEmail && 
                          !string.IsNullOrEmpty(u.GoogleId) && 
                          (excludeUserID == null || u.UserID != excludeUserID));
        }

        // NEW: Get user by GoogleId
        public User GetUserByGoogleId(string googleId)
        {
            if (string.IsNullOrEmpty(googleId))
                return null;

            return _context.Users
                .Include("UserDetails")
                .FirstOrDefault(u => u.GoogleId == googleId);
        }

        // NEW: Get all users with Google linked (for admin/debugging)
        public List<User> GetUsersWithGoogleLinked()
        {
            return _context.Users
                .Include("UserDetails")
                .Where(u => !string.IsNullOrEmpty(u.GoogleId))
                .ToList();
        }

        // Get all users for admin management
        public List<User> GetAllUsers()
        {
            return _context.Users
                .Include("UserDetails")
                .OrderByDescending(u => u.CreatedAt)
                .ToList();
        }

        // Delete user and related data
        public bool DeleteUser(int userId)
        {
            try
            {
                var user = _context.Users.Find(userId);
                if (user == null)
                    return false;

                // Delete related UserDetails first (if exists)
                var userDetails = _context.UserDetails.FirstOrDefault(ud => ud.UserID == userId);
                if (userDetails != null)
                {
                    _context.UserDetails.Remove(userDetails);
                }

                // Delete the user
                _context.Users.Remove(user);
                _context.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in DeleteUser: {ex.Message}");
                return false;
            }
        }

        public bool CreateUser(User user, UserDetails userDetails)
        {
            try
            {
                _context.Users.Add(user);
                _context.SaveChanges();

                userDetails.UserID = user.UserID;
                _context.UserDetails.Add(userDetails);
                _context.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateUser(User user)
        {
            try
            {
                _context.Entry(user).State = EntityState.Modified;
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public UserDetails GetUserDetails(int userId)
        {
            return _context.UserDetails.FirstOrDefault(ud => ud.UserID == userId);
        }

        public bool UpdateUserDetails(UserDetails userDetails)
        {
            try
            {
                _context.Entry(userDetails).State = EntityState.Modified;
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}