using ProtoBuf;
using System;

namespace MyWhiteboard.Stroke
{
    [ProtoContract]
    [ProtoInclude(100, typeof(StrokeDescription))]
    public class StrokePoints
    {
        [ProtoMember(1)]
        public Guid Id;
        [ProtoMember(2)]
        public double[] PointXValues;
        [ProtoMember(3)]
        public double[] PointYValues;
        [ProtoMember(4)]
        public float[] PressureValues;
        [ProtoMember(5)]
        public short Order;
        [ProtoMember(6)]
        public short NumberOfPackages;
    }

    [ProtoContract]
    public class StrokeDescription : StrokePoints
    {
        [ProtoMember(5)]
        public bool IsPencil;
        [ProtoMember(6)]
        public byte[] ColorValues;
        [ProtoMember(7)]
        public bool DrawAsHighlighter;
        [ProtoMember(8)]
        public bool FitToCurve;
        [ProtoMember(9)]
        public bool IgnorePressure;
        [ProtoMember(10)]
        public double[] SizeValues;
        [ProtoMember(11)]
        public double Opacity;
    }
}