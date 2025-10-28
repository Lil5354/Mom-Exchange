// File: Models/UserRepository.cs
using System;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;


namespace B_M.Models
{
    public class UserRepository : IDisposable
    {
        private readonly ApplicationDbContext _context;

        public UserRepository()
        {
            _context = new ApplicationDbContext();
        }

        public bool CreateUser(User user, UserDetails userDetails)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.Users.Add(user);
                    _context.SaveChanges(); 

                    userDetails.UserID = user.UserID;

                    _context.UserDetails.Add(userDetails);
                    _context.SaveChanges();

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"ERROR in CreateUser: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                    
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"INNER EXCEPTION: {ex.InnerException.Message}");
                        System.Diagnostics.Debug.WriteLine($"INNER STACK TRACE: {ex.InnerException.StackTrace}");
                    }
                    
                    return false;
                }
            }
        }

        public bool EmailExists(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }

        public bool UsernameExists(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;
            return _context.Users.Any(u => u.UserName == username);
        }

        public User GetUserByEmail(string email)
        {
            return _context.Users
                .Include("UserDetails")
                .FirstOrDefault(u => u.Email == email);
        }

        public User GetUserByUsername(string username)
        {
            return _context.Users
                .Include("UserDetails")
                .FirstOrDefault(u => u.UserName == username);
        }

        public User GetUserByEmailOrUsername(string emailOrUsername)
        {
            return _context.Users
                .Include("UserDetails")
                .FirstOrDefault(u => u.Email == emailOrUsername || u.UserName == emailOrUsername);
        }

        public UserDetails GetUserDetails(int userId)
        {
            return _context.UserDetails.Find(userId);
        }

        public bool UpdateUser(User user)
        {
            try
            {
                _context.Entry(user).State = EntityState.Modified;
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in UpdateUser: {ex.Message}");
                return false;
            }
        }

        public bool UpdateUserDetails(UserDetails userDetails)
        {
            try
            {
                _context.Entry(userDetails).State = EntityState.Modified;
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in UpdateUserDetails: {ex.Message}");
                return false;
            }
        }


        public List<User> GetAllUsers()
        {
            return _context.Users
                .Include("UserDetails")
                .OrderByDescending(u => u.CreatedAt)
                .ToList();
        }

        public User GetUserById(int userId)
        {
            return _context.Users
                .Include("UserDetails")
                .FirstOrDefault(u => u.UserID == userId);
        }

        public List<User> GetUsersByRole(byte role)
        {
            return _context.Users
                .Include("UserDetails")
                .Where(u => u.Role == role)
                .OrderByDescending(u => u.CreatedAt)
                .ToList();
        }

        public List<User> GetActiveUsers()
        {
            return _context.Users
                .Include("UserDetails")
                .Where(u => u.IsActive)
                .OrderByDescending(u => u.CreatedAt)
                .ToList();
        }

        public List<User> GetRecentUsers(int count = 10)
        {
            return _context.Users
                .Include("UserDetails")
                .OrderByDescending(u => u.CreatedAt)
                .Take(count)
                .ToList();
        }

        public int GetUserCount()
        {
            return _context.Users.Count();
        }

        public int GetActiveUserCount()
        {
            return _context.Users.Count(u => u.IsActive);
        }

        public int GetUserCountByRole(byte role)
        {
            return _context.Users.Count(u => u.Role == role);
        }

        public int GetNewUsersThisMonth()
        {
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            return _context.Users.Count(u => u.CreatedAt >= startOfMonth);
        }

        public bool DeleteUser(int userId)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Tìm user và user details
                    var user = _context.Users.Find(userId);
                    if (user == null)
                    {
                        return false;
                    }

                    var userDetails = _context.UserDetails.Find(userId);
                    
                    // Xóa user details trước (foreign key constraint)
                    if (userDetails != null)
                    {
                        _context.UserDetails.Remove(userDetails);
                    }

                    // Xóa user
                    _context.Users.Remove(user);
                    
                    _context.SaveChanges();
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"ERROR in DeleteUser: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                    
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"INNER EXCEPTION: {ex.InnerException.Message}");
                        System.Diagnostics.Debug.WriteLine($"INNER STACK TRACE: {ex.InnerException.StackTrace}");
                    }
                    
                    return false;
                }
            }
        }

        // Admin CRUD Methods
        public User GetUserForAdminEdit(int userId)
        {
            return _context.Users
                .Include("UserDetails")
                .FirstOrDefault(u => u.UserID == userId);
        }
        
        public bool UpdateUserProfile(User user, UserDetails userDetails)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.Entry(user).State = EntityState.Modified;
                    _context.Entry(userDetails).State = EntityState.Modified;
                    _context.SaveChanges();
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"ERROR in UpdateUserProfile: {ex.Message}");
                    return false;
                }
            }
        }
        
        public bool UpdateUserRole(int userId, byte newRole)
        {
            try
            {
                var user = _context.Users.Find(userId);
                if (user == null) return false;
                
                user.Role = newRole;
                _context.Entry(user).State = EntityState.Modified;
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in UpdateUserRole: {ex.Message}");
                return false;
            }
        }
        
        public bool UpdateUserStatus(int userId, bool isActive)
        {
            try
            {
                var user = _context.Users.Find(userId);
                if (user == null) return false;
                
                user.IsActive = isActive;
                _context.Entry(user).State = EntityState.Modified;
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in UpdateUserStatus: {ex.Message}");
                return false;
            }
        }
        
        public bool UsernameExistsExcludingUser(string username, int excludeUserId)
        {
            if (string.IsNullOrEmpty(username))
                return false;
            return _context.Users.Any(u => u.UserName == username && u.UserID != excludeUserId);
        }
        
        public bool EmailExistsExcludingUser(string email, int excludeUserId)
        {
            return _context.Users.Any(u => u.Email == email && u.UserID != excludeUserId);
        }

        // Batch operations for Excel import
        public bool CreateUserBatch(List<User> users, List<UserDetails> userDetailsList)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    for (int i = 0; i < users.Count; i++)
                    {
                        _context.Users.Add(users[i]);
                        _context.SaveChanges(); // Get UserID

                        userDetailsList[i].UserID = users[i].UserID;
                        _context.UserDetails.Add(userDetailsList[i]);
                    }

                    _context.SaveChanges();
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"ERROR in CreateUserBatch: {ex.Message}");
                    return false;
                }
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
