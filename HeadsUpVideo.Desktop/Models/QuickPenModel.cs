using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace HeadsUpVideo.Desktop.Models
{
    [DataContract]
    public class QuickPenModel
    {
        [DataMember]
        public Color Color { get; set; }
        [DataMember]
        public double Size { get; set; }
        [DataMember]
        public bool IsHighlighter { get; set; }
    }
}
