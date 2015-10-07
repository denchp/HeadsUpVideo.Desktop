using Windows.UI.Xaml.Controls;
using HeadsUpVideo.Desktop.ViewModels;

namespace HeadsUpVideo.Desktop.Pages
{
    public sealed partial class MasterPage : Page
    {
        public MasterPageViewModel viewModel;

        public MasterPage()
        {
            this.InitializeComponent();
            
            Initialize();
        }

        private void Initialize()
        {
            viewModel = new MasterPageViewModel();
            viewModel.Initialize();

            this.DataContext = viewModel;
            this.Loaded += MasterPage_Loaded;
        }

        private void MasterPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            NavigationModel.Initialize(frameBody);
        }
    }
}
