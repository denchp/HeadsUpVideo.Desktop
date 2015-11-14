using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HeadsUpVideo.Desktop.Models
{
    [DataContract]
    public class OptionsModel
    {
        public OptionsModel()
        {
            SmoothingFactor = 10;
        }

        [DataMember]
        public int SmoothingFactor { get; set; }
        [DataMember]
        public PenModel LastPen { get; set; }
    }
}
