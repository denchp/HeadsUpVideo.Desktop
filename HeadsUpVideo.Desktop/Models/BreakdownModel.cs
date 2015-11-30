using HeadsUpVideo.Desktop.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HeadsUpVideo.Desktop.Models
{
    [XmlRoot(ElementName = "file")]
    public class BreakdownModel : NotifyPropertyChangedBase
    {
        public BreakdownModel() {
          
        }

        public FileModel AssociatedVideo { get; set; }

        [XmlArray(ElementName = "ALL_INSTANCES")]
        public ObservableCollection<Instance> Instances { get; set; }

        [XmlType(TypeName = "instance")]
        public class Instance : NotifyPropertyChangedBase
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
            [XmlElement(ElementName = "flagged")]
            public bool? Flagged { get; set; }
        }
        
        [XmlType(TypeName = "label")]
        public struct Label
        {
            [XmlElement(ElementName = "text")]
            public string Text { get; set; }
        }
    }
}
