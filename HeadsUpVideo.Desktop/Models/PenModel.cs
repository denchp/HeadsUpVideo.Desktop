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
            Double,
            Text
        }

        private bool enableArrow, isFreehand, isHighlighter;
        private Color color;
        private double size;
        LineType lineStyle;
        private string text;

        public PenModel() { }
        public PenModel(PenModel pen)
        {
            this.enableArrow = pen.EnableArrow;
            this.color = pen.Color;
            this.size = pen.Size;
            this.isFreehand = pen.IsFreehand;
            this.isHighlighter = pen.IsHighlighter;
            this.lineStyle = pen.LineStyle;
        }

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
            set
            {
                lineStyle = value;
                TriggerPropertyChange("LineStyle");
                TriggerPropertyChange("IsDouble");
                TriggerPropertyChange("IsSolid");
                TriggerPropertyChange("IsDashed");
            }
        }
        [DataMember]
        public bool IsFreehand
        {
            get { return isFreehand; }
            set
            {
                isFreehand = value;
                TriggerPropertyChange("IsFreehand");
                TriggerPropertyChange("IsStraight");
            }
        }
        [DataMember]
        public string Text
        {
            get { return text; }
            set { text = value; TriggerPropertyChange("Text"); }
        }

        [IgnoreDataMember]
        public bool IsStraight { get { return !IsFreehand; } }
        [IgnoreDataMember]
        public bool IsSolid { get { return lineStyle == LineType.Solid; } }
        [IgnoreDataMember]
        public bool IsDashed { get { return lineStyle == LineType.Dashed; } }
        [IgnoreDataMember]
        public bool IsDouble { get { return lineStyle == LineType.Double; } }
        [IgnoreDataMember]
        public bool IsNormal { get; set; }
        [IgnoreDataMember]
        public bool IsPass { get; set; }
    }
}
