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

namespace HeadsUpVideo.Desktop.Pages
{
    public sealed partial class WelcomePage : Page
    {
        internal WelcomePageViewModel viewModel = new WelcomePageViewModel();

        public WelcomePage()
        {
            this.DataContext = viewModel;
            this.InitializeComponent();
            
            Initialize();
        }

        private void Initialize()
        {
           
        }
    }
}
