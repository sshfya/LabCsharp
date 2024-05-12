using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Lab1
{
    public delegate DataItem FDI (double x);
    
    public class V1DataList : V1Data
    {
        public List<DataItem> Collect { get; set; }

        public V1DataList(string key, DateTime date) : base(key, date)
        {
            Collect = new List<DataItem>();
        }

        public V1DataList(string key, DateTime date, double[] x, FDI F) : base(key, date)
        {
            Collect = new List<DataItem>();
            List<double> usedX = new List<double>();
            for (int i = 0; i < x.Length; i++)
            {
                if (!usedX.Contains(x[i]))
                {
                    usedX.Add(x[i]);
                    Collect.Add(F(x[i]));
                }
            }
        }

        public override double MaxDistance
        {
            get
            {
                double max = Double.MinValue;
                double min = Double.MaxValue;
                foreach (var d in Collect)
                {
                    if (d.X > max)
                    {
                        max = d.X;
                    }
                    if (d.X < min)
                    {
                        min = d.X;
                    }
                }

                return Math.Abs(max - min);
            }
        }

        public static explicit operator V1DataArray(V1DataList source)
        {
            V1DataArray Array = new V1DataArray(source.Key, source.Date);
            int n = source.Collect.Count;
            Array.X = new double[n];
            Array.Y = new double[2][];
            Array.Y[0] = new double[n];
            Array.Y[1] = new double[n];

            for (int i = 0; i < n; ++i)
            {
                Array.X[i] = source.Collect[i].X;
                Array.Y[0][i] = source.Collect[i].Y1;
                Array.Y[1][i] = source.Collect[i].Y2;
            }

            return Array;
        }
        
        public override string ToString()
        {
            return "V1DataList " + Key + " " + Date + " " + Collect.Count + '\n';
        }

        public override string ToLongString(string format)
        {
            string res = "V1DataList " + Key +" " + Date + " " + Collect.Count + '\n';
            foreach (var d in  Collect)
            {
                res += string.Format(format, d.X) + " " + string.Format(format, d.Y1) + " " + string.Format(format, d.Y2) + '\n';
            }
            return res;
        }


        public override double Y1Average
        {
            get
            {
                double s = 0;
                int i = 0;
                foreach (var d in Collect)
                {
                    s += d.Y1;
                    i += 1;
                }
                return s / i;
            }
        }


        public override IEnumerator<DataItem> GetEnumerator()
        {
            return Collect.GetEnumerator();
        }

        public override string ThisString
        {
            get { return this.ToLongString("{0:G}"); }
        }

    }
}