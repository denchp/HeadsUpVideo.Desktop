using HeadsUpVideo.Desktop.Models;

namespace HeadsUpVideo.Desktop.Interfaces
{
    public interface ICustomCanvas
    {
        int SmoothingFactor { get; set; }

        void ClearLines(bool restoreSave = false, bool restoreTemplate = false);
        void CreateSavePoint();
        void Clear();
        void SetPen(PenModel currentPen);
        void Initialize();
        void ShowDiagramTools(bool showTools);
    }
}
