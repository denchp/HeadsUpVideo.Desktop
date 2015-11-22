using HeadsUpVideo.Desktop.Base;
using System.Collections.ObjectModel;
using Windows.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using HeadsUpVideo.Desktop.Interfaces;
using HeadsUpVideo.Desktop.Models;
using Windows.UI.Xaml;
using System.IO;

namespace HeadsUpVideo.Desktop.ViewModels
{
    class VideoPageViewModel : NotifyPropertyChangedBase
    {
        public event EventHandler TogglePlayPauseEvent;

        public ICustomCanvas Canvas { get; set; }
        
        public TimeSpan LastVideoPosition { get; set; }
        public Command PlayPauseCmd { get; set; }

        public VideoPageViewModel()
        {
            PlayPauseCmd = new Command() { CanExecuteFunc = obj => true, ExecuteFunc = TogglePlayPause };
        }

        public void Initialize(ICustomCanvas canvas)
        {
            Canvas = canvas;
            Canvas.Initialize();
            Canvas.SetPen(NavigationModel.Options.LastPen);
        }

        private void TogglePlayPause()
        {
            Canvas.ClearLines(false, false);

            if (TogglePlayPauseEvent != null)
                TogglePlayPauseEvent(this, new EventArgs());
        }
    }
}
