using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeadsUpVideo.Desktop.ViewModels;

namespace HeadsUpVideo.Desktop.Interfaces
{
    public interface ICustomCanvas
    {
        void ClearLines(bool restoreSave = false, bool restoreTemplate = false);
        void CreateSavePoint();
        void Clear();
        void SetPen(PenViewModel currentPen);
    }
}
