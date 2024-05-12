using FluentAssertions.Equivalency;
using FluentAssertions.Common;
using FluentAssertions.Execution;
using FsCheck;
using FsCheck.Xunit;
using System.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using Lab1;

namespace TestsModel
{
    public class TestsModel
    {
        public static void F(double x, ref double y1, ref double y2)
        {
            y1 = x * 2;
            y2 = -x + 1;
        }
        public static DataItem F(double x)
        {
            return new DataItem(x, x * 2, -x + 1);
        }
        [Fact]
        public void DataItemTest()
        {
            DataItem di = new DataItem(0, 1, 2);
            di.ToLongString("{0:F2}").Should().Be("0,00 1,00 2,00");
        }
        [Fact]
        public void SplineDataItemTest()
        {
            SplineDataItem di = new SplineDataItem(0, 1, 2);
            di.ToString("{0:F2}").Should().Be("0,00 1,00 2,00");
        }
        [Fact]
        public void DataArrayTest()
        {
            string key = "Key";
            DateTime dateTime = DateTime.Now;
            V1DataArray c1 = new V1DataArray(key, dateTime);
            using (new AssertionScope())
            {
                c1.Key.Should().Be(key);
                c1.Date.Should().Be(dateTime);
            }
            double[] x = new double[] { 1, 2, 3, };
            double[] y1 = new double[] { 2, 4, 6 };
            double[] y2 = new double[] { 0, -1, -2 };
            V1DataArray c2 = new V1DataArray(key, dateTime, x, F);
            using (new AssertionScope())
            {
                c2.Key.Should().Be(key);
                c2.Date.Should().Be(dateTime);
                c2.X.Should().Equal(x);
                c2.Y[0].Should().Equal(y1);
                c2.Y[1].Should().Equal(y2);
                c2.X.Length.Should().Be(3);
            }
            int n = 2;
            double l = 0;
            double r = 1;
            V1DataArray c3 = new V1DataArray(key, dateTime, n, l, r, F);
            x = new double[] { 0, 1 };
            y1 = new double[] { 0, 2 };
            y2 = new double[] { 1, 0 };
            using (new AssertionScope())
            {
                c3.Key.Should().Be(key);
                c3.Date.Should().Be(dateTime);
                c3.X.Should().Equal(x);
                c3.Y[0].Should().Equal(y1);
                c3.Y[1].Should().Equal(y2);
                c3.X.Length.Should().Be(n);
            }
            string filename = $"{System.Random.Shared.Next()}file.da";
            V1DataArray.Save(filename, c3);
            V1DataArray load = new V1DataArray(filename, DateTime.Now);
            V1DataArray.Load(filename, ref load);
            using (new AssertionScope())
            {
                load.Key.Should().Be(c3.Key);
                load.Date.Should().Be(c3.Date);
                load.X.Should().Equal(c3.X);
                load[0].Should().Equal(c3[0]);
                load[1].Should().Equal(c3[1]);

            }
        }
        [Fact]

        public void DataListTest()
        {
            string key = "Key";
            DateTime dateTime = DateTime.Now;
            V1DataList c1 = new V1DataList(key, dateTime);
            using (new AssertionScope())
            {
                c1.Key.Should().Be(key);
                c1.Date.Should().Be(dateTime);
            }
            double[] x = new double[] { 1, 2, 3, };
            double[] y1 = new double[] { 2, 4, 6 };
            double[] y2 = new double[] { 0, -1, -2 };
            V1DataList c2 = new V1DataList(key, dateTime, x, F);
            using (new AssertionScope())
            {
                c2.Key.Should().Be(key);
                c2.Date.Should().Be(dateTime);
                c2.Collect.Should().HaveCount(3);
                c2.Collect.Should().Equal<DataItem>(new DataItem[3] { new DataItem(1, 2, 0), new DataItem(2, 4, -1), new DataItem(3, 6, -2) }, (x, y) => { return x.ToString() == y.ToString(); });
            }
            V1DataArray list_convertion = (V1DataArray)c2;
        }
        [Fact]
        public void MainCollectionTest()
        {

            V1MainCollection collection = new V1MainCollection(0, 0);
            collection.Add(new V1DataArray("arr1", DateTime.Now, new double[1] { 1 }, F));
            using (new AssertionScope())
            {
                collection.Should().HaveCount(3);
                collection.DoubleX.Should().BeEmpty();
                collection.Mean.Should().Be(2.0);
                collection.MaxMean.ToString().Should().Be(new DataItem(3, 6, -2).ToString());
            }
        }
        [Fact]
        public void SplineDataTest()
        {
            double[] x = new double[] { 1, 2, 3, };
            V1DataArray array = new V1DataArray("key", DateTime.Now, x, F);
            SplineData spline_data = new SplineData(array, 2, 100, 0.001, 21);
            spline_data.CalculateSpline();

            spline_data.ResS.Should().Equal(new SplineDataItem[3] { new SplineDataItem(1, 2, 2), new SplineDataItem(2, 4, 4), new SplineDataItem(3, 6, 6), },
                (x, y) => { return x.ToString() == y.ToString(); });
            using (new AssertionScope())
            {
                double step = 0.1;
                for (int i = 0; i < 21; ++i)
                {
                    spline_data.Small_spline[i].Should().Equal(new double[2] { 1 + i * step, 2 * (1 + i * step) }, (x, y) => { double eps = 1e-5; return Math.Abs(x - y) < eps; });
                }
            }
        }
    }
}