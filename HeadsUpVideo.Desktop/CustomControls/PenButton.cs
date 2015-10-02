using HeadsUpVideo.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace HeadsUpVideo.Desktop.CustomControls
{
    public class PenButton : AppBarButton
    {
        public PenViewModel PenModel { get; set; }

        public PenButton(PenViewModel model)
        {
            PenModel = model;
            PenModel.PropertyChanged += PenModel_PropertyChanged;

            this.Icon = new SymbolIcon(model.IsHighlighter ? Symbol.Highlight : Symbol.Edit);
            this.Foreground = new SolidColorBrush(model.Color);
            this.Label = model.LineStyle.ToString() + (model.EnableArrow ? " Arrow" : "");
        }

        private void PenModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            
        }
    }
}
