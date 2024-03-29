﻿using System;
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
using System.Threading.Tasks;
using System.Threading;

namespace HeadsUpVideo.Desktop.Pages
{
    public sealed partial class VideoPage : Page
    {
        private VideoPageViewModel viewModel;
        private Thumb sliderThumb;
        private TextBlock sliderButton;
        private DispatcherTimer updateTimer;

        public VideoPage()
        {
            this.InitializeComponent();
            Initialize();
            updateTimer = new DispatcherTimer();
            updateTimer.Interval = new TimeSpan(0,0,0,0,250);
            updateTimer.Tick += SetCurrentPositionText;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var file = e.Parameter as FileModel;

            VideoPlayer.MediaOpened += VideoPlayer_MediaOpened;
            if (file != null && file.Stream != null)
                VideoPlayer.SetSource(file.Stream, file.ContentType);

            base.OnNavigatedTo(e);
        }

        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            ClipLength.Text = VideoPlayer.NaturalDuration.TimeSpan.ToString(@"hh\:mm\:ss");
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

        private void SetCurrentPositionText(object sender, object e)
        {
            CurrentPosition.Text = VideoPlayer.Position.ToString(@"hh\:mm\:ss");

            if (VideoPlayer.CurrentState != MediaElementState.Playing)
                updateTimer.Stop();
        }

        private void TogglePlayPause(object sender, EventArgs e)
        {
            switch (VideoPlayer.CurrentState)
            {
                case MediaElementState.Stopped:
                case MediaElementState.Playing:
                    VideoPlayer.Pause();
                    viewModel.LastVideoPosition = VideoPlayer.Position;
                    sliderButton.Text = "\u23F5";
                    break;
                case MediaElementState.Paused:
                    Scrubber.Value = 50;
                    sliderButton.Text = "\u23F8";
                    VideoPlayer.Play();
                    updateTimer.Start();
                    break;
            }
        }

        private void VideoPage_Loaded(object sender, RoutedEventArgs e)
        {
            sliderThumb = MyFindSliderChildOfType<Thumb>(Scrubber);
            sliderThumb.Tapped += Thumb_Tapped;
            sliderThumb.DragCompleted += Thumb_DragCompleted;

            sliderButton = MyFindSliderChildOfType<TextBlock>(sliderThumb);
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
                CurrentPosition.Text = VideoPlayer.Position.ToString(@"hh\:mm\:ss");
                VideoPlayer.Position = viewModel.LastVideoPosition.Add(new TimeSpan(0, 0, 0, 0, (int)(scrubber.Value - 50) * 125));
            }
        }

        private void mainGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Right:
                    viewModel.LastVideoPosition = viewModel.LastVideoPosition.Add(new TimeSpan(0, 1, 0));
                    VideoPlayer.Position = viewModel.LastVideoPosition;
                    break;
                case Windows.System.VirtualKey.Left:
                    viewModel.LastVideoPosition = viewModel.LastVideoPosition.Subtract(new TimeSpan(0, 1, 0));
                    VideoPlayer.Position = viewModel.LastVideoPosition;
                    break;
                case Windows.System.VirtualKey.PageUp:
                    VideoPlayer.PlaybackRate += .2;
                    break;
                case Windows.System.VirtualKey.PageDown:
                    VideoPlayer.PlaybackRate -= .2;
                    break;
                case Windows.System.VirtualKey.End:
                    VideoPlayer.PlaybackRate = 1;
                    break;
            }

            e.Handled = true;
        }
    }
}
