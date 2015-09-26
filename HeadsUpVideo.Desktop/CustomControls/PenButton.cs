using HeadsUpVideo.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace HeadsUpVideo.Desktop.CustomControls
{
    public class PenButton : AppBarButton
    {
        public PenViewModel PenModel { get; set; }

        public PenButton()
        {
            PenModel.PropertyChanged += PenModel_PropertyChanged;
        }

        private void PenModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            
        }
    }
}
