using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;

namespace HeadsUpVideo.Desktop.Models
{
    class DVRRandomAccessStream : IRandomAccessStream
    {

        private Stream internalStream;
        public IRandomAccessStream PlaybackStream;
        public MediaStreamSource StreamSource;
        
        public DVRRandomAccessStream(Stream stream)
        {
            this.internalStream = stream;
            PlaybackStream = new MemoryStream().AsRandomAccessStream();
            IMediaStreamDescriptor descriptor = new Descriptor() { Language = "English", Name = "Instant Replay Stream" };
            StreamSource = new MediaStreamSource(descriptor);
            StreamSource.CanSeek = true;

            StreamSource.SampleRequested += StreamSource_SampleRequested;
        }

        private void StreamSource_SampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
        }

        public DVRRandomAccessStream(byte[] bytes)
        {
            this.internalStream = new MemoryStream(bytes);
        }

        public IInputStream GetInputStreamAt(ulong position)
        {
            this.internalStream.Seek((long)position, SeekOrigin.Begin);

            return this.internalStream.AsInputStream();
        }

        public IOutputStream GetOutputStreamAt(ulong position)
        {
            this.internalStream.Seek((long)position, SeekOrigin.Begin);

            return this.internalStream.AsOutputStream();
        }

        public ulong Size
        {
            get { return (ulong)this.internalStream.Length; }
            set { this.internalStream.SetLength((long)value); }
        }

        public bool CanRead
        {
            get { return true; }
        }

        public bool CanWrite
        {
            get { return true; }
        }

        public IRandomAccessStream CloneStream()
        {
            throw new NotSupportedException();
        }

        public ulong Position
        {
            get { return (ulong)this.internalStream.Position; }
        }

        public void Seek(ulong position)
        {
            this.internalStream.Seek((long)position, 0);
        }

        public void Dispose()
        {
            this.internalStream.Dispose();
        }

        public Windows.Foundation.IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
        {
            var inputStream = this.GetInputStreamAt(0);
            return inputStream.ReadAsync(buffer, count, options);
        }

        public Windows.Foundation.IAsyncOperation<bool> FlushAsync()
        {
            var outputStream = this.GetOutputStreamAt(0);
            return outputStream.FlushAsync();
        }

        public Windows.Foundation.IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
        {
            var outputStream = this.GetOutputStreamAt((ulong)this.internalStream.Length);
            return outputStream.WriteAsync(buffer);
        }

        private class Descriptor : IMediaStreamDescriptor
        {
            public bool IsSelected { get; }

            public string Language { get; set; }

            public string Name { get; set; }
        }
    }
}