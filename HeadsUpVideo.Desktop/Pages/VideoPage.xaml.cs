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
using HeadsUpVideo.Desktop.Models;

namespace HeadsUpVideo.Desktop.Pages
{
    public sealed partial class VideoPage : Page
    {
        private VideoPageViewModel viewModel;
        private Thumb sliderThumb;
        private AppBarButton sliderButton;

        public VideoPage()
        {
            this.InitializeComponent();
            Initialize();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var file = e.Parameter as FileModel;

            if (file != null && file.Stream != null)
                VideoPlayer.SetSource(file.Stream, file.ContentType);

            base.OnNavigatedTo(e);
        }

        private void Initialize()
        {
            this.Loaded += VideoPage_Loaded;

            viewModel = new VideoPageViewModel();

            viewModel.Initialize(inkCanvas);
            viewModel.TogglePlayPauseEvent += TogglePlayPause;
            Scrubber.ValueChanged += Scrubber_ValueChanged;

            this.DataContext = viewModel;
        }

        private void TogglePlayPause(object sender, EventArgs e)
        {
            switch (VideoPlayer.CurrentState)
            {
                case MediaElementState.Stopped:
                case MediaElementState.Playing:
                    VideoPlayer.Pause();
                    viewModel.LastVideoPosition = VideoPlayer.Position;
                    sliderButton.Icon = new SymbolIcon(Symbol.Play);
                    break;
                case MediaElementState.Paused:
                    Scrubber.Value = 50;
                    sliderButton.Icon = new SymbolIcon(Symbol.Pause);
                    VideoPlayer.Play();
                    break;
            }
        }

        private void VideoPage_Loaded(object sender, RoutedEventArgs e)
        {
            sliderThumb = MyFindSliderChildOfType<Thumb>(Scrubber);
            sliderThumb.Tapped += Thumb_Tapped;
            sliderThumb.DragCompleted += Thumb_DragCompleted;

            sliderButton = MyFindSliderChildOfType<AppBarButton>(sliderThumb);
        }

        private void Thumb_Tapped(object sender, TappedRoutedEventArgs e)
        {
            viewModel.PlayPauseCmd.Execute(this);
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Scrubber.Value = 50;
            viewModel.LastVideoPosition = VideoPlayer.Position;
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
                if (VideoPlayer.CurrentState == MediaElementState.Playing)
                    viewModel.PlayPauseCmd.Execute(null);

                VideoPlayer.Position = viewModel.LastVideoPosition.Add(new TimeSpan(0, 0, 0, 0, (int)(scrubber.Value - 50) * 75));
            }
        }
    }
}
