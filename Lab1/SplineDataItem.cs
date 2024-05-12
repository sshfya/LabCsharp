using System;
namespace Lab1
{
    public struct SplineDataItem
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Ys { get; set; }

        public SplineDataItem(double x, double y, double ys)
        {
            X = x;
            Y = y;
            Ys = ys;
        }

        public string ToString(string format)
        {
            return string.Format(format, X) + ' ' + string.Format(format, Y) + ' ' + string.Format(format, Ys);
        }

        public override string ToString()
        {
            return "SplineDataItem";
        }
    }
}