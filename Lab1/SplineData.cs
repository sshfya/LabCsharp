using System;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json;
using System.Runtime.InteropServices;
using System.Numerics;

namespace Lab1
{
	public class SplineData
	{
		public V1DataArray Array { get; set; }
		public int M { get; set; }
		public double[] S { get; set; }
        public int MaxIt { get; set; }
        public int CountIt { get; set; }
		public double MinR { get; set; }
        public double StopR { get; set; }
        public int Code { get; set; }
        public List<SplineDataItem> ResS { get; set; }

        public int Num_small_grid { get; set; }
        public List<double[]> Small_spline { get; set; }

        public SplineData(V1DataArray a, int m, int max, double stopr, int n_small_grid)
		{
			Array = a;
			M = m;
			MaxIt = max;
			S = new double[a.X.Length];
			ResS = new List<SplineDataItem>();
            StopR = stopr;
            Num_small_grid = n_small_grid;
        }
        [DllImport("\\..\\..\\..\\..\\Lab1\\x64\\Debug\\MKLdll.dll",
        CallingConvention = CallingConvention.Cdecl)]
        public static extern void MakeApproximation(double[] X, double[] Y, int M,
                                            int N, double[] spline, int maxiter, double stopr,
                                            int n_small_grid, double[] values,
                                            ref int numiter, ref int errorcode, ref double minr);
        public void CalculateSpline()
        {
            int CountIt = 0;
            int code = 0;
            double MinR = 0;
            double[] Values = new double[Num_small_grid];

            MakeApproximation(Array.X, Array.Y[0], M, Array.X.Length, S, MaxIt, StopR,
                Num_small_grid, Values,
                ref CountIt, ref code, ref MinR);
            
            this.CountIt = CountIt;
            this.Code = code;
            this.MinR = MinR;
            for (int i = 0; i < Array.X.Length; i++) {
                ResS.Add(new SplineDataItem(Array.X[i], Array.Y[0][i], S[i]));
            }
            double h = (Array.X[Array.X.Length - 1] - Array.X[0]) / (Num_small_grid - 1);
            Small_spline = new List<double[]>();
            for (int i = 0; i < Num_small_grid; i++)
            {
                Small_spline.Add(new double[2] { Array.X[0] + h * i, Values[i] });
            }

        }
        public string ToLongString(string format)
		{
            string res = Array.ToLongString(format) + '\n';
            for (int i = 0; i < Array.X.Length; i++)
            {
                res += string.Format(format, Array.X[i]) + ' ';
                res += string.Format(format, Array.Y[0][i]) + ' ';
                res += string.Format(format, S[i]) + '\n';
            }
            res += "Минимальное значение невязки = " + string.Format(format, MinR) + '\n';
            res += "Код остановки = " + Code + '\n';
            res += "Количество итераций = " + CountIt + '\n';
            return res;
        }

        public void Save(string filename, string format)
		{
			string str = this.ToLongString(format);
            File.WriteAllText(filename, str);
        }

        

    }
}

