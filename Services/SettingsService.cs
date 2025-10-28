using System;
using B_M.Models;
using B_M.Repositories;

namespace B_M.Services
{
    public class SettingsService
    {
        private readonly SettingsRepository _settingsRepository;

        public SettingsService()
        {
            _settingsRepository = new SettingsRepository();
        }

        public SystemSettings GetSystemSettings()
        {
            return _settingsRepository.GetSettings();
        }

        public bool UpdateSystemSettings(SystemSettings settings)
        {
            return _settingsRepository.UpdateSettings(settings);
        }
    }
}