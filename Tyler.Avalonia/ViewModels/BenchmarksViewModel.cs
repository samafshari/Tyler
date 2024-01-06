using Net.Essentials;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tyler.ViewModels
{
    public class BenchmarksViewModel : TinyViewModel
    {
        public ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();

        public BenchmarksViewModel()
        {
            RaisePropertyChangeOnUI = true;
            BenchmarkService.Instance.LogFunc = s =>
            {
                RunOnUIAction(() => Logs.Insert(0, s?.ToString() ?? ""));
            };
        }

        public CommandModel RefreshCommand => new CommandModel(BenchmarkService.Instance.PrintStatus);
    }
}
