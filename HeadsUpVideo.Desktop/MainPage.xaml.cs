using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Input.Inking;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Shapes;
using HeadsUpVideo.Desktop.ViewModels;
using System.ComponentModel;
using HeadsUpVideo.Desktop.CustomControls;

namespace HeadsUpVideo.Desktop
{
    public sealed partial class MainPage : Page
    {
        public MainPageViewModel _viewModel = new MainPageViewModel();

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = _viewModel;
            _viewModel.Initialize(inkCanvas, videoPlayer);
            inkCanvas.Initialize(LineCanvas, _viewModel.CurrentPen);
            videoPlayer.CurrentStateChanged += VideoPlayer_CurrentStateChanged;
            Scrubber.ValueChanged += Scrubber_ValueChanged;

            _viewModel.QuickPens.CollectionChanged += QuickPens_CollectionChanged;

            this.Loaded += MainPage_Loaded;
        }

        private void VideoPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            switch (videoPlayer.CurrentState)
            {
                case MediaElementState.Stopped:
                case MediaElementState.Playing:
                    btnPlay.Icon = new SymbolIcon(Symbol.Play);
                    btnPlay.Label = "Play";
                    break;
                case MediaElementState.Paused:
                    Scrubber.Value = 50;
                    btnPlay.Icon = new SymbolIcon(Symbol.Pause);
                    btnPlay.Label = "Pause";
                    break;
            }
        }

        private void QuickPens_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            QuickPens.Children.Clear();

            foreach (PenViewModel pen in e.NewItems)
            {
                QuickPens.Children.Add(new PenButton()
                {
                    PenModel = pen,
                    Command = _viewModel.LoadQuickPenCmd,
                    CommandParameter = pen
                });
            }
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var thumb = MyFindSliderChildOfType<Thumb>(Scrubber);

            thumb.DragCompleted += Thumb_DragCompleted;

            pnlControls.Visibility = Visibility.Collapsed;
            WelcomeScreen.Visibility = Visibility.Visible;
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Scrubber.Value = 50;
            _viewModel.LastVideoPosition = videoPlayer.Position;
        }

        public static T MyFindSliderChildOfType<T>(DependencyObject root) where T : class
        {
            var MyQueue = new Queue<DependencyObject>();
            MyQueue.Enqueue(root);
            while (MyQueue.Count > 0)
            {
                DependencyObject current = MyQueue.Dequeue();
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(current); i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    var typedChild = child as T;
                    if (typedChild != null)
                    {
                        return typedChild;
                    }
                    MyQueue.Enqueue(child);
                }
            }
            return null;
        }

        private void Scrubber_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var scrubber = sender as Slider;

            if (scrubber.Value != 50)
            {
                if (videoPlayer.CurrentState == MediaElementState.Playing)
                    _viewModel.PlayPauseCmd.Execute(null);
                
                videoPlayer.Position = _viewModel.LastVideoPosition.Add(new TimeSpan(0, 0, 0, 0, (int)(scrubber.Value - 50) * 75));
            }
        }

    }
}
