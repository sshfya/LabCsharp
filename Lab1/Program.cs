using Lab1;

namespace Lab1
{
    internal class Program
    {
        public static void F(double x, ref double y1, ref double y2)
        {
            y1 = Math.Sin(x);
            y2 = Math.Tan(x);
        }
        public static DataItem F(double x)
        {
            return new DataItem(x, x*x*x, Math.Tan(x));
        }

        public static void Main(string[] args) 
        {
            string filename = "results.txt";
            V1DataArray v1a = new V1DataArray("key", DateTime.Now, 5, 0, 1, F);
            SplineData sd = new SplineData(v1a, 5, 100, 0.001, 5);
            sd.CalculateSpline();
            Console.WriteLine(sd.ToLongString("{0:f3}"));
            sd.Save(filename, "{0:f3}");
            
        }
    }
}