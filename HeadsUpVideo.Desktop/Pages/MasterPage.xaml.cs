using Windows.UI.Xaml.Controls;
using HeadsUpVideo.Desktop.ViewModels;

namespace HeadsUpVideo.Desktop.Pages
{
    public sealed partial class MasterPage : Page
    {
        public MasterPageViewModel viewModel = new MasterPageViewModel();

        public MasterPage()
        {
            this.DataContext = viewModel;
            this.InitializeComponent();
            
            Initialize();
        }

        private void Initialize()
        {
            this.Loaded += MasterPage_Loaded;
            viewModel.NavigateTo += ViewModel_NavigateTo;
        }

        private void ViewModel_NavigateTo(object sender, System.Type e)
        {
            frameBody.Navigate(e);
        }

        private void MasterPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            viewModel.Initialize();
        }
    }
}
