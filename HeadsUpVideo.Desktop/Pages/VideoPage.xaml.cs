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
    public sealed partial class VideoPage : Page
    {
        private VideoPageViewModel viewModel = new VideoPageViewModel();
        private Thumb sliderThumb;
        private AppBarButton sliderButton;

        public VideoPage()
        {
            this.DataContext = viewModel;
            this.InitializeComponent();

            viewModel.QuickPens.CollectionChanged += QuickPens_CollectionChanged;

            inkCanvas.Initialize(viewModel.CurrentPen);
            viewModel.Initialize(inkCanvas);
            
            Initialize();
        }

        private void Initialize()
        {
            this.Loaded += VideoPage_Loaded;
            
            viewModel.TogglePlayPauseEvent += _viewModel_TogglePlayPause;
            viewModel.FileOpened += _viewModel_FileOpened;
            
            Scrubber.ValueChanged += Scrubber_ValueChanged;
        }

        private void _viewModel_FileOpened(object sender, EventArgs e)
        {
            VideoPlayer.SetSource(viewModel.CurrentFile.Stream, viewModel.CurrentFile.ContentType);
        }

        private void _viewModel_TogglePlayPause(object sender, EventArgs e)
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

        private void QuickPens_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            QuickPens.Children.Clear();
            
            foreach (PenViewModel pen in viewModel.QuickPens)
            {
                QuickPens.Children.Add(new PenButton(pen)
                {
                    Command = viewModel.LoadQuickPenCmd,
                    CommandParameter = pen
                });
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
