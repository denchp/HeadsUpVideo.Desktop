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
using System.IO;
using Windows.Media.Capture;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.AccessCache;
using System.Diagnostics;

namespace HeadsUpVideo.Desktop.Pages
{
    public sealed partial class InstantReplayPage : Page
    {
        private InstantReplayPageViewModel viewModel;
        private Thumb sliderThumb;
        private TextBlock sliderButton;
        private DVRRandomAccessStream fileStream;
        private Stream storageFileStream;
        private MediaCapture mediaCapture;
        private StorageFile storageFile;
        private LowLagMediaRecording lowLagRecord;

        public InstantReplayPage()
        {
            this.InitializeComponent();
            Initialize();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private async void Initialize()
        {
            this.Loaded += InstantReplayPage_Loaded;

            viewModel = new InstantReplayPageViewModel();

            viewModel.Initialize(inkCanvas);
            viewModel.TogglePlayPauseEvent += TogglePlayPause;
            Scrubber.ValueChanged += Scrubber_ValueChanged;

            var file = await viewModel.StartCapture(null);
            //var profiles = MediaCapture.FindAllVideoProfiles(cameraId);
            MediaCapture capture = new MediaCapture();


            var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(Windows.Devices.Enumeration.DeviceClass.VideoCapture);

            var captureInitSettings = new MediaCaptureInitializationSettings();
            var profile = MediaEncodingProfile.CreateWmv(VideoEncodingQuality.Auto);

            captureInitSettings.StreamingCaptureMode = StreamingCaptureMode.Video;
            captureInitSettings.VideoDeviceId = devices[0].Id;
          
            mediaCapture = new MediaCapture();
            await mediaCapture.InitializeAsync(captureInitSettings);
            
            storageFile = await KnownFolders.VideosLibrary.CreateFileAsync("InstantReplayCapture.wmv", CreationCollisionOption.GenerateUniqueName);
            StorageApplicationPermissions.FutureAccessList.Add(storageFile);

            storageFileStream = await storageFile.OpenStreamForWriteAsync();

            fileStream = new DVRRandomAccessStream(storageFileStream);
           
            lowLagRecord = await mediaCapture.PrepareLowLagRecordToStreamAsync(profile, fileStream);
            await lowLagRecord.StartAsync();
           
            VideoPlayer.SetSource(fileStream.PlaybackStream, storageFile.ContentType);

            VideoPlayer.PartialMediaFailureDetected += VideoPlayer_PartialMediaFailureDetected;
            VideoPlayer.MediaFailed += VideoPlayer_MediaFailed;
            VideoPlayer.CurrentStateChanged += (s, e) => { Debug.WriteLine("State: " + VideoPlayer.CurrentState);  };
            VideoPlayer.MediaEnded += VideoPlayer_MediaEnded;

            this.DataContext = viewModel;
        }

        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
             VideoPlayer.SetSource(fileStream.PlaybackStream, storageFile.ContentType);
        }

        private void VideoPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
        }

        private void VideoPlayer_PartialMediaFailureDetected(MediaElement sender, PartialMediaFailureDetectedEventArgs args)
        {
            throw new NotImplementedException();
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
                    break;
            }
        }

        private void InstantReplayPage_Loaded(object sender, RoutedEventArgs e)
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

                VideoPlayer.Position = viewModel.LastVideoPosition.Add(new TimeSpan(0, 0, 0, 0, (int)(scrubber.Value - 50) * 125));
            }
        }
    }
}
