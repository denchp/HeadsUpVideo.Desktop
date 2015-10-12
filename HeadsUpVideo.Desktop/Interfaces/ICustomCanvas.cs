using HeadsUpVideo.Desktop.Models;

namespace HeadsUpVideo.Desktop.Interfaces
{
    public interface ICustomCanvas
    {
        void ClearLines(bool restoreSave = false, bool restoreTemplate = false);
        void CreateSavePoint();
        void Clear();
        void SetPen(PenModel currentPen);
        void Initialize();
    }
}
