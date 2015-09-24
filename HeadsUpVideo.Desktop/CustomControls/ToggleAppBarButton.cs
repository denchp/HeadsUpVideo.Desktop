using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace HeadsUpVideo.Desktop.CustomControls
{

    public class ToggleAppBarButton : AppBarButton
    {
        public ToggleAppBarButton()
        {
            this.DefaultStyleKey = typeof(AppBarButton);
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool),
                typeof(ToggleAppBarButton),
                new PropertyMetadata(false, new PropertyChangedCallback(OnSelectedChanged)));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty);  }
            set { SetValue(IsSelectedProperty, value); }
        }

        private static void OnSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = d as ToggleAppBarButton;

            if ((bool)e.NewValue)
            {
                button.Background = new SolidColorBrush(Colors.Gray);
            }
            else
            {
                button.Background = null;
            }
        }
    }
}
