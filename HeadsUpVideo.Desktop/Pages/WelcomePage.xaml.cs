using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using HeadsUpVideo.Desktop.ViewModels;
using HeadsUpVideo.Desktop.CustomControls;
using System.Linq;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using HeadsUpVideo.Desktop.Models;

namespace HeadsUpVideo.Desktop.Pages
{
    public sealed partial class WelcomePage : Page
    {
        internal WelcomePageViewModel viewModel;

        public WelcomePage()
        {
            this.InitializeComponent();

            viewModel = new WelcomePageViewModel();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            var files = e.Parameter as IEnumerable<FileViewModel>;

            this.DataContext = viewModel;

            if (files != null)
                viewModel.RecentFiles = (ObservableCollection<FileViewModel>)files;

            Initialize();

            base.OnNavigatedTo(e);
        }

        private void Initialize()
        {
            viewModel.RecentFiles.CollectionChanged += RecentFiles_CollectionChanged;
        }

        private void RecentFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            lstRecentFiles.Children.Clear();
            stkRecentFiles.Visibility = Visibility.Collapsed;

            if (viewModel.RecentFiles.Any())
                stkRecentFiles.Visibility = Visibility.Visible;

            foreach (var file in viewModel.RecentFiles)
            {
                var link = new HyperlinkButton()
                {
                    Content = file.Name,
                    Command = viewModel.OpenRecentFileCmd,
                    CommandParameter = file
                };
                lstRecentFiles.Children.Add(link);
            }
        }
    }
}
