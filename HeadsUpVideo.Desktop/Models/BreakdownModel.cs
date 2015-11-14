using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HeadsUpVideo.Desktop.Models
{
    [XmlRoot(ElementName = "file")]
    public class BreakdownModel
    {
        [XmlArray(ElementName = "ALL_INSTANCES")]
        public List<Instance> Instances { get; set; }

        [XmlType(TypeName = "instance")]
        public struct Instance
        {
            [XmlElement(ElementName = "ID")]
            public int Id { get; set; }
            [XmlElement(ElementName = "start")]
            public float StartTime { get; set; }
            [XmlElement(ElementName = "end")]
            public float StopTime { get; set; }
            [XmlElement(ElementName = "code")]
            public string Category { get; set; }
            [XmlElement(ElementName = "label")]
            public Label Label { get; set; }
        }

        [XmlType(TypeName = "label")]
        public struct Label
        {
            [XmlElement(ElementName = "text")]
            public string Text { get; set; }
        }
    }
}
