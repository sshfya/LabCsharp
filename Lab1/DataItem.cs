namespace Lab1
{
    public struct DataItem
    {
        public double X { get; set; }
        public double Y1 { get; set; }
        public double Y2 { get; set; }

        public DataItem(double x, double y1, double y2)
        {
            X = x;
            Y1 = y1;
            Y2 = y2;
        }

        public string ToLongString(string format)
        {
            return string.Format(format, X) + " " + string.Format(format, Y1) + " " + string.Format(format, Y2);
        }

        public override string ToString()
        {
            return "DataItem";
        }
        
    }
}