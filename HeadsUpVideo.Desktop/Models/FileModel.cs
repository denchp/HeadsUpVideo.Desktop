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
    public class FileModel
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Path { get; set; }
        [DataMember]
        public string ContentType { get; set; }

        [XmlIgnore]
        public IRandomAccessStream Stream { get; set; }
    }
}
