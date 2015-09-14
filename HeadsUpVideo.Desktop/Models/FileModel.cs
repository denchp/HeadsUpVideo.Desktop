using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HeadsUpVideo.Desktop.Models
{
    [DataContract]
    public class FileModel
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Path { get; set; }
    }
}
