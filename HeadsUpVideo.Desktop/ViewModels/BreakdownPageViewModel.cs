using HeadsUpVideo.Desktop.Base;
using HeadsUpVideo.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadsUpVideo.Desktop.ViewModels
{
    class BreakdownPageViewModel : VideoPageViewModel
    {
        private BreakdownModel _breakdown;
        public BreakdownModel Breakdown { get { return _breakdown; } set { _breakdown = value; TriggerPropertyChange("Breakdown"); } }

        public Command OpenBreakdownCmd { get; set; }

        public BreakdownPageViewModel()
        {
            OpenBreakdownCmd = new Command() { CanExecuteFunc = obj => true, ExecuteFunc = OpenBreakdown };
            Breakdown = new BreakdownModel() { Instances = new ObservableCollection<BreakdownModel.Instance>() };
            Breakdown.Instances.Add(new BreakdownModel.Instance() { Category = "POTATO!", StartTime = 4.5f, StopTime = 9.5f });

        }

        private async void OpenBreakdown()
        {
            Breakdown = await StorageIO.OpenBreakdown();
        }
    }
}
