using FluentAssertions.Execution;
using System.Collections;

namespace TestVM
{
    public class ViewModelTests
    {
        public class UITest : IUIServices
        {
            public double[][] x;
            public double[][] y;
            public string s;
            public void Info(string message)
            {
                this.s = message;
            }

            public string Load()
            {
                return "array.txt";
            }

            public void Plot(double[][] x, double[][] y)
            {
                this.x = x; this.y = y;
            }

            public string Save()
            {
                return "array.txt";
            }
        }
        UITest UI = new UITest();
        private int DataFromControls = 0;
        private int DataFromFile = 0;
        private int Save = 0;
        private void DataFromControlsCanExecuteChanged(object? sender, EventArgs e)
        {
            DataFromControls += 1;
        }
        private void DataFromFileCanExecuteChanged(object? sender, EventArgs e)
        {
            DataFromFile += 1;
        }
        private void SaveCanExecuteChanged(object? sender, EventArgs e)
        {
            Save += 1;
        }

        [Fact]
        public void ValidationTest()
        {
            ViewModel MVM = new ViewModel(UI);
            MVM.DataFromControlsCommand.CanExecuteChanged += DataFromControlsCanExecuteChanged;
            MVM.DataFromFileCommand.CanExecuteChanged += DataFromFileCanExecuteChanged;
            MVM.SaveCommand.CanExecuteChanged += SaveCanExecuteChanged;
            using (new AssertionScope())
            {
                MVM.Grid = new double[1] { 0 };
                MVM["Grid"].Should().NotBe("");
                MVM.Grid = new double[2] { 1, 0 };
                MVM["Grid"].Should().NotBe("");
                MVM.Grid = new double[2] { 0, 1 };
                MVM["Grid"].Should().Be("");

                MVM.N_grid_nodes = -1;
                MVM["N_grid_nodes"].Should().NotBe("");
                MVM.N_grid_nodes = 2;
                MVM["N_grid_nodes"].Should().NotBe("");
                MVM.N_grid_nodes = 10;
                MVM["N_grid_nodes"].Should().Be("");


                MVM.N_small_grid = -1;
                MVM["N_small_grid"] .Should().NotBe("");
                MVM.N_small_grid = 2;
                MVM["N_small_grid"].Should().NotBe("");
                MVM.N_small_grid = 10;
                MVM["N_small_grid"].Should().Be("");

                MVM.N_smooth_spline = 1;
                MVM["N_smooth_spline"].Should().NotBe("");
                MVM.N_smooth_spline = 2;
                MVM["N_smooth_spline"].Should().Be("");
                MVM.N_smooth_spline = 12;
                MVM["N_smooth_spline"].Should().NotBe("");
                MVM.N_smooth_spline = 5;
                MVM["N_smooth_spline"].Should().Be("");
            }
            DataFromControls.Should().Be(13);
            DataFromFile.Should().Be(13);
            Save.Should().Be(13);
        }
        [Fact]
        public void InfoTest()
        {
            ViewModel MVM = new ViewModel(UI);
            
            UI.s = "";
            MVM.DataFromControls();
            UI.s.Should().NotBe("");
        }
        [Fact]
        public void CalculateSplineTest()
        {
            ViewModel MVM = new ViewModel(UI);
            MVM.Grid = new double[2] { 0, 1 };
            MVM.N_grid_nodes = 3;
            MVM.N_small_grid = 5;
            MVM.Function = "X^3";
            MVM.Uniformity = true;
            MVM.N_smooth_spline = 2;
            MVM.MaxIt = 1000;
            MVM.StopR = 0.001;

            double[][] x = new double[5][] { new double[2] { 0.0, -0.125 }, new double[2] { 0.25, 0.125 }, new double[2] { 0.5, 0.375 }, new double[2] { 0.75, 0.625 }, new double[2] { 1.0, 0.875 } };
            double[][] y = new double[3][] { new double[3] { 0.0, 0.0, -0.125 }, new double[3] { 0.5, 0.125, 0.375 }, new double[3] { 1.0, 1.0, 0.875 } };

            MVM.DataFromControls();
            UI.x.Should().Equal(x, (x, y) => { double eps = 1e-5; return Math.Abs(x[0] - y[0]) < eps && Math.Abs(x[1] - y[1]) < eps; });
            UI.y.Should().Equal(y, (x, y) => { double eps = 1e-5; return Math.Abs(x[0] - y[0]) < eps && Math.Abs(x[1] - y[1]) < eps && Math.Abs(x[2] - y[2]) < eps; });
        }
        [Fact]
        public void SaveLoadTest()
        {
            ViewModel MVM = new ViewModel(UI);
            MVM.Grid = new double[2] { 0, 1 };
            MVM.N_grid_nodes = 3;
            MVM.Function = "X^3";
            MVM.Uniformity = true;

            MVM.Save();

            MVM.N_small_grid = 5;
            MVM.N_smooth_spline = 2;
            MVM.MaxIt = 1000;
            MVM.StopR = 0.001;

            double[][] x = new double[5][] { new double[2] { 0.0, -0.125 }, new double[2] { 0.25, 0.125 }, new double[2] { 0.5, 0.375 }, new double[2] { 0.75, 0.625 }, new double[2] { 1.0, 0.875 } };
            double[][] y = new double[3][] { new double[3] { 0.0, 0.0, -0.125 }, new double[3] { 0.5, 0.125, 0.375 }, new double[3] { 1.0, 1.0, 0.875 } };

            MVM.DataFromFile();
            UI.x.Should().Equal(x, (x, y) => { double eps = 1e-5; return Math.Abs(x[0] - y[0]) < eps && Math.Abs(x[1] - y[1]) < eps; });
            UI.y.Should().Equal(y, (x, y) => { double eps = 1e-5; return Math.Abs(x[0] - y[0]) < eps && Math.Abs(x[1] - y[1]) < eps && Math.Abs(x[2] - y[2]) < eps; });
        }
    }
}