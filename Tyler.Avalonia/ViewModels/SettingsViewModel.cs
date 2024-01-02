using Net.Essentials;

using Tyler.Services;

namespace Tyler.ViewModels
{
    public class SettingsViewModel : ViewModel
    {
        readonly SettingsService _settingsService;

        public string? BackgroundColor { get; set; }

        public SettingsViewModel()
        {
            _settingsService = ContainerService.Instance.GetOrCreate<SettingsService>();
            _settingsService.Data.Inject(this);
        }

        public void Save()
        {
            this.Inject(_settingsService.Data);
            _settingsService.SaveWithLock();
        }

        public CommandModel SaveCommand => new CommandModel(Save);
    }
}