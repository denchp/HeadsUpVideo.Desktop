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
        public enum LineType
        {
            Solid,
            Dashed,
            Double
        }

        [DataMember]
        public bool EnableArrow { get; set; }

        [DataMember]
        public Color Color { get; set; }
        [DataMember]
        public double Size { get; set; }
        [DataMember]
        public bool IsHighlighter { get; set; }
        [DataMember]
        public LineType LineStyle { get; set; }
        [DataMember]
        public bool IsFreehand { get; set; }
    }
}
