using System;
using System.Linq;
using B_M.Models;

namespace B_M.Repositories
{
    public class SettingsRepository : IDisposable
    {
        private readonly ApplicationDbContext _context;

        public SettingsRepository()
        {
            _context = new ApplicationDbContext();
        }

        public SystemSettings GetSettings()
        {
            return _context.SystemSettings.FirstOrDefault() ?? new SystemSettings();
        }

        public bool UpdateSettings(SystemSettings settings)
        {
            try
            {
                var existing = _context.SystemSettings.FirstOrDefault();
                if (existing == null)
                {
                    _context.SystemSettings.Add(settings);
                }
                else
                {
                    _context.Entry(existing).CurrentValues.SetValues(settings);
                }
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