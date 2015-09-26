using HeadsUpVideo.Desktop.Base;
using System.Runtime.Serialization;
using Windows.UI;

namespace HeadsUpVideo.Desktop.Models
{
    [DataContract]
    public class PenModel : NotifyPropertyChangedBase
    {
        public enum LineType
        {
            Solid,
            Dashed,
            Double
        }

        private bool enableArrow, isFreehand, isHighlighter;
        private Color color;
        private double size;
        LineType lineStyle;

        [DataMember]
        public bool EnableArrow
        {
            get { return enableArrow; }
            set { enableArrow = value; TriggerPropertyChange("EnableArrow"); }
        }

        [DataMember]
        public Color Color
        {
            get { return color; }
            set { color = value; TriggerPropertyChange("Color"); }
        }
        [DataMember]
        public double Size
        {
            get { return size; }
            set { size = value; TriggerPropertyChange("Size"); }
        }
        [DataMember]
        public bool IsHighlighter
        {
            get { return isHighlighter; }
            set { isHighlighter = value; TriggerPropertyChange("IsHighlighter"); }
        }
        [DataMember]
        public LineType LineStyle
        {
            get { return lineStyle; }
            set { lineStyle = value; TriggerPropertyChange("LineStyle"); }
        }
        [DataMember]
        public bool IsFreehand
        {
            get { return isFreehand; }
            set { isFreehand = value;
                TriggerPropertyChange("IsFreehand");
                TriggerPropertyChange("IsStraight");
            }
        }
        public bool IsStraight { get { return !IsFreehand; } }

    }
}
