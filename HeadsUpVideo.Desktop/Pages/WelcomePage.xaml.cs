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

            Initialize();
        }

        private void Initialize()
        {
            viewModel.RecentFiles.CollectionChanged += RecentFiles_CollectionChanged;
            RecentFiles_CollectionChanged(this, null);
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
                    CommandParameter = file.Path
                };
                lstRecentFiles.Children.Add(link);
            }
        }
    }
}
