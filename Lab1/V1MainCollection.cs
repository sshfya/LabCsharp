using System;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Lab1
{
	public class V1MainCollection : System.Collections.ObjectModel.ObservableCollection<V1Data>

	{
		public bool Contains(string key)
		{
			foreach (var item in base.Items)
			{
				if (item.Key == key)
					return true;
			}
			return false;
		}

		public new bool Add(V1Data v1Data)
		{
			if (Contains(v1Data.Key))
			{
				return false;
			}
			base.Add(v1Data);
			return true;
		}

		public V1MainCollection(int nV1DataArray, int nV1DataList)
		{
			int i = 1;
			int k = 2;
			for (; i < nV1DataArray + 1; ++i)
			{
				base.Add(new V1DataArray(i.ToString(), DateTime.Now, k, i, i + 3, Program.F));
				k += 1;
			}
			double[] x = new double[3] { i, i + 1, i + 2 };
			for (; i < nV1DataList + nV1DataArray + 1; ++i)
			{
				base.Add(new V1DataList(i.ToString(), DateTime.Now, x, Program.F));
				x = new double[2] { 2 * i, 2 * i + 1 };
			}
			i++;
			base.Add(new V1DataList(i.ToString(), DateTime.Now));
			i++;
            base.Add(new V1DataArray(i.ToString(), DateTime.Now));
        }

		public string ToLongString(string format)
		{
			string res = " V1MainCollection\n";
			foreach (var item in base.Items)
			{
				res += item.ToLongString(format) + "\n";
			}
			return res;
		}

		public override string ToString()
		{
			string res = " V1MainCollection\n";
			foreach (var item in base.Items)
			{
				res += item.ToString() + "\n";
			}
			return res;
		}



		public V1MainCollection()
		{
			int i = 1;
			int k = 2;

			base.Add(new V1DataArray(i.ToString(), DateTime.Now, k, i, i + 3, Program.F));

			i = 3;
			double[] x = new double[3] { i, i + 1, i + 2 };

			base.Add(new V1DataList(i.ToString(), DateTime.Now, x, Program.F));

		}


		public double Mean
		{
			get
			{
				if (Items.Count == 0)
				{
					return double.NaN;
				}
				return Items.SelectMany(data => data.Select(item => Math.Sqrt(item.Y1 * item.Y1 + item.Y2 * item.Y2))).Average();
			}
		}

		public DataItem? MaxMean
		{
			get
			{
				if (Items.Count == 0)
				{
					return null;
				}
				Func<DataItem, double> F = item => Math.Abs(Math.Sqrt(item.Y1 * item.Y1 + item.Y2 * item.Y2) - this.Mean);
				DataItem def = new DataItem(0, this.Mean, 0);
                return Items.Select(data => data.DefaultIfEmpty(def).MaxBy(F)).MaxBy(F);
			}
		}

        public IEnumerable<double> DoubleX
		{
			get
			{
                if (Items.Count == 0)
                {
                    return null;
                }
				return Items.SelectMany(data => data.Select(item => item.X).Distinct())
				.GroupBy(x => x)
				.Where(group => group.Count() > 1)
				.SelectMany(group => group.AsEnumerable()).Distinct().Order();
            }
		}


	}
}