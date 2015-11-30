using HeadsUpVideo.Desktop.Base;
using HeadsUpVideo.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace HeadsUpVideo.Desktop.CustomControls
{
    public sealed partial class BreakdownTimeline : UserControl, INotifyPropertyChanged
    {
        private BreakdownModel _breakdown;
        public String SelectedCategory { get; set; }
        private List<BreakdownModel.Instance> CurrentInstances = new List<BreakdownModel.Instance>();
        private int CurrentIndex = 0;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<ClipEventArgs> PlayClip;

        public BreakdownModel Breakdown { get { return _breakdown; } set { _breakdown = value; } }
        
        public Command OpenBreakdownCmd { get; set; }
        public Command PlaySelectedClipsCmd { get; set; }

        public BreakdownTimeline()
        {
            Breakdown = new BreakdownModel() { Instances = new ObservableCollection<BreakdownModel.Instance>() };
            Breakdown.Instances.Add(new BreakdownModel.Instance() { Category = "POTATO!", StartTime = 4.5f, StopTime = 9.5f });
            TriggerPropertyChange("Breakdown");

            this.DataContext = this;
            OpenBreakdownCmd = new Command() { CanExecuteFunc = obj => true, ExecuteFunc = OpenBreakdown };
            PlaySelectedClipsCmd = new Command() { CanExecuteFunc = obj => true, ExecuteFunc = PlaySelectedClips };
            this.InitializeComponent();
        }

        private void PlaySelectedClips()
        {
            if (PlayClip != null)
                PlayClip(this, new ClipEventArgs(CurrentInstances[CurrentIndex].StartTime, CurrentInstances[CurrentIndex].StopTime));
        }

        public ClipEventArgs GetNextClip()
        {
            if (LoopCurrentClip.IsChecked != true)
                CurrentIndex++;

            return new ClipEventArgs(CurrentInstances[CurrentIndex].StartTime, CurrentInstances[CurrentIndex].StopTime);
        }


        public void Initialize(BreakdownModel model)
        {
            Breakdown = model;
        }
        
        private async void OpenBreakdown()
        {
            Breakdown = await StorageIO.OpenBreakdown();
            TriggerPropertyChange("Breakdown");
        }

        protected void TriggerPropertyChange(string propertyName)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

      
        private void CategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentInstances = Breakdown.Instances.Where(x => x.Category == CategoryList.SelectedValue.ToString()).ToList();
            CurrentIndex = 0;
        }

        public class ClipEventArgs
        {
            public ClipEventArgs(float start, float end)
            {
                StartTime = start; EndTime = end;
            }
            public float StartTime { get; set; }
            public float EndTime { get; set; }
        }

    }
}
