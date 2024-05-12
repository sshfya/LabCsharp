using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Lab1
{
    public delegate void FValues(double x, ref double y1, ref double y2);
    public class V1DataArray : V1Data
    {
        public double[] X { get; set; }
        public double[][] Y { get; set; }

        public double[] this[int k]
        {
            get
            {
                return Y[k];
            }
        }

        public V1DataList List
        {
            get
            {
                V1DataList List = new V1DataList(Key, Date);
                for (int i = 0; i < X.Length; ++i)
                {
                    List.Collect.Add(new DataItem(X[i], Y[0][i], Y[1][i]));
                }
                return List;
            }
        }

        public override double MaxDistance
        {
            get
            {
                double max = Double.MinValue;
                double min = Double.MaxValue;
                for (int i = 0; i < X.Length; ++i)
                {
                    if (X[i] > max)
                    {
                        max = X[i];
                    }
                    if (X[i] < min)
                    {
                        min = X[i];
                    }
                }
                return Math.Abs(max - min);
            }
        }

        public V1DataArray(string key, DateTime date) : base(key, date)
        {
            X = new double[0];
            Y = new double[2][];
            Y[0] = new double[0];
            Y[1] = new double[0];
        }
        public V1DataArray(string key, DateTime date, double[] x, FValues F) : base(key, date)
        {
            X = x;
            Y = new double[2][];
            Y[0] = new double[x.Length];
            Y[1] = new double[x.Length];
            for (int i = 0; i < x.Length; ++i)
            {
                F(X[i], ref Y[0][i], ref Y[1][i]);
            }
        }

        public V1DataArray(string key, DateTime date, int nX, double xL, double xR, FValues F) : base(key, date)
        {
            X = new double[nX];
            double step = (xR - xL) / (nX - 1);
            for (int i = 0; i < nX; ++i)
            {
                X[i] = xL + step * i;
            }

            Y = new double[2][];
            Y[0] = new double[nX];
            Y[1] = new double[nX];

            for (int i = 0; i < nX; ++i)
            {
                F(X[i], ref Y[0][i], ref Y[1][i]);
            }
        }

        public override string ToString()
        {
            return "V1DataArray" + " " + Key+ " " + Date + '\n';
        }

        public override string ToLongString(string format)
        {
            string res = "V1DataArray" + " " + Key + " " + Date + '\n';
            for (int i = 0; i < X.Length; ++i)
            {
                res += string.Format(format, X[i]) + " " + string.Format(format, Y[0][i]) + " " + string.Format(format, Y[1][i]) + '\n';
            }
            return res;
        }



        public override double Y1Average
        {
            get
            {
                double s = 0;
                int n = 0;
                for (int i = 0; i < X.Length; ++i)
                {
                    s += Y[0][i];
                    n += 1;
                }
                return s / n;
            }
        }

        public class MyEnumerator : IEnumerator<DataItem>
        {
            private V1DataArray array;

            public MyEnumerator(V1DataArray array)
            {
                this.array = array;
            }

            private int current = -1;
            public object Current
            {
                get
                {
                    return new DataItem(array.X[current], array.Y[0][current], array.Y[1][current]);
                }
            }
            DataItem IEnumerator<DataItem>.Current => (DataItem)Current;

            public bool MoveNext()
            {
                if (current < array.X.Length - 1)
                {
                    current++;
                    return true;
                }
                return false;
            }
            public void Reset()
            {
                current = -1;
            }

            public void Dispose(){}
        }


        public override IEnumerator<DataItem> GetEnumerator()
        {
            return new MyEnumerator(this);
        }
        public static bool Save (string filename, V1DataArray array)
        {
            try
            {
                string Date = JsonSerializer.Serialize(array.Date);
                string X = JsonSerializer.Serialize(array.X);
                string Y = JsonSerializer.Serialize(array.Y);
                File.WriteAllText(filename, array.Key +"\n"+ Date + "\n" + X + "\n" + Y);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public static bool Load(string filename, ref V1DataArray array)
        {
            try
            {
                StreamReader reader = new StreamReader(filename);
                array.Key = reader.ReadLine();
                array.Date = JsonSerializer.Deserialize<DateTime>(reader.ReadLine());
                array.X = JsonSerializer.Deserialize<double[]>(reader.ReadLine());
                array.Y = JsonSerializer.Deserialize<double[][]>(reader.ReadLine());
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public override string ThisString
        {
            get { return this.ToLongString("{0:G}"); }
        }

    }
}