using System;
using HeadsUpVideo.Desktop.ViewModels;
using Windows.UI.Xaml.Controls;

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
        }
    }
}
