using HeadsUpVideo.Desktop.Base;
using HeadsUpVideo.Desktop.Interfaces;
using Windows.UI.Xaml.Media.Imaging;

namespace HeadsUpVideo.Desktop.ViewModels
{
    class DiagramPageViewModel : NotifyPropertyChangedBase
    {
        public BitmapImage DiagramBackground { get; set; }
        public ICustomCanvas Canvas { get; set; }

        public void Initialize(ICustomCanvas canvas)
        {
            Canvas = canvas;
            Canvas.Initialize();
            Canvas.ShowDiagramTools(true);

            Canvas.SetPen(NavigationModel.Options.LastPen);
            Canvas.SmoothingFactor = NavigationModel.Options.SmoothingFactor;
        }
    }
}
