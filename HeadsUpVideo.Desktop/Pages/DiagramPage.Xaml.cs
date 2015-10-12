using System;
using HeadsUpVideo.Desktop.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;

namespace HeadsUpVideo.Desktop.Pages
{
    public sealed partial class DiagramPage : Page
    {
        internal DiagramPageViewModel viewModel = new DiagramPageViewModel();

        public DiagramPage()
        {
            this.DataContext = viewModel;
            this.InitializeComponent();

            this.Initialize();
        }

        private void Initialize()
        {
            viewModel.Initialize(inkCanvas);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var resource = e.Parameter as string;

            viewModel.DiagramBackground = new BitmapImage(new Uri("ms-appx:/" + resource));

            base.OnNavigatedTo(e);
        }
    }
}
