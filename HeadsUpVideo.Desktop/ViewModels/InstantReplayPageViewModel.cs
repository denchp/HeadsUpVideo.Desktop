using System;
using System.Diagnostics;
using System.IO;
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
        public IRandomAccessStream Stream = new InMemoryRandomAccessStream();
        public string SelectedDevice { get; set; }

        public async Task<FileStream> StartCapture(string cameraId)
        {
            return null;
        }
    }
}