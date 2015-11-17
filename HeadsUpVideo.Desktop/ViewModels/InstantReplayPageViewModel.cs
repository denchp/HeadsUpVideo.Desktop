using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;

namespace HeadsUpVideo.Desktop.ViewModels
{
    class InstantReplayPageViewModel : VideoPageViewModel
    {
        MediaComposition Composition { get; set; }
        public IRandomAccessStream Stream;
        public string SelectedDevice { get; set; }

        public async Task<StorageFile> StartCapture(string cameraId)
        {
            //var profiles = MediaCapture.FindAllVideoProfiles(cameraId);
            MediaCapture capture = new MediaCapture();


            var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(Windows.Devices.Enumeration.DeviceClass.VideoCapture);
            for (var i = 0; i < devices.Count; i++)
            {
                Debug.WriteLine(devices[i]);
                SelectedDevice = devices[0].Id;
            }

            var captureInitSettings = new MediaCaptureInitializationSettings();
            captureInitSettings.StreamingCaptureMode = StreamingCaptureMode.Video;

            var mediaCapture = new MediaCapture();
            await mediaCapture.InitializeAsync(captureInitSettings);
            
            var profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);
            
            var storageFile = await KnownFolders.VideosLibrary.CreateFileAsync("cameraCapture.mp4", CreationCollisionOption.ReplaceExisting);
            Stream = await storageFile.OpenAsync(FileAccessMode.ReadWrite);

            var lowLagRecord = await mediaCapture.PrepareLowLagRecordToStreamAsync(profile, Stream);
            await lowLagRecord.StartAsync();
            
            return storageFile;
        }
    }
}