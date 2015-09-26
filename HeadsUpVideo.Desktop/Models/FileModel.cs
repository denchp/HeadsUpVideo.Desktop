using HeadsUpVideo.Desktop.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage.Streams;

namespace HeadsUpVideo.Desktop.Models
{
    [DataContract]
    public class FileModel : NotifyPropertyChangedBase
    {
        private string _name, _path, _contentType;
        private IRandomAccessStream _stream;

        [DataMember]
        public string Name
        {
            get { return _name; }
            set { _name = value; TriggerPropertyChange("Name"); }
        }

        [DataMember]
        public string Path
        {
            get { return _path; }
            set { _path = value; TriggerPropertyChange("Path"); }
        }

        [DataMember]
        public string ContentType
        {
            get { return _contentType; }
            set { _contentType = value; TriggerPropertyChange("ContentType"); }
        }

        [XmlIgnore]
        public IRandomAccessStream Stream
        {
            get { return _stream; }
            set { _stream = value; TriggerPropertyChange("Stream"); }
        }
    }
}
