using HeadsUpVideo.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadsUpVideo.Desktop.ViewModels
{
    public class PenViewModel : PenModel
    {
        public bool IsSolid { get { return this.LineStyle == LineType.Solid; } }
        public bool IsDashed { get { return this.LineStyle == LineType.Dashed; } }
        public bool IsDouble { get { return this.LineStyle == LineType.Double; } }

        public PenViewModel()
        {
            base.PropertyChanged += PenViewModel_PropertyChanged;
        }

        private void PenViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ("IsSolid, IsDashed, IsDouble".Contains(e.PropertyName))
                return;

            TriggerPropertyChange("IsSolid");
            TriggerPropertyChange("IsDouble");
            TriggerPropertyChange("IsDashed");
        }
    }
}
