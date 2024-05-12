namespace VModel
{
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Windows.Input;
    using System.Collections;
    using Lab1;
    using static System.Runtime.InteropServices.JavaScript.JSType;
    using System.Net.WebSockets;

    public interface IUIServices
    {
        void Info(string message);
        void Plot(double[][] x, double[][] y);
        string Save();
        string Load();
    }

    public class RelayCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool> canexecute;

        public event EventHandler? CanExecuteChanged;


        public RelayCommand(Action execute, Func<bool> canexecute)
        {
            this.execute = execute;
            this.canexecute = canexecute;
        }


        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());

        }


        public bool CanExecute(object? parameter)
        {

            return this.canexecute();
        }

        public void Execute(object? parameter)
        {
            this.execute();

        }
    }


    public class ViewModel: INotifyDataErrorInfo
    {
        public static void XCubed(double x, ref double y1, ref double y2)
        {
            y1 = x * x * x;
            y2 = Math.Tan(x);
        }
        public static void XSin(double x, ref double y1, ref double y2)
        {
            y1 = Math.Sin(x);
            y2 = Math.Tan(x);
        }
        private double[] grid;
        public double[] Grid {
            get
            {
                return grid;
            }

            set { 
                grid = value;
                Validate(nameof(Grid));
            }

        }
        private int n_grid_nodes;
        public int N_grid_nodes
        {
            get
            {
                return n_grid_nodes;
            }

            set
            {
                n_grid_nodes = value;
                Validate(nameof(N_grid_nodes));
            }

        }
        public bool Uniformity { get; set; }   
        public string Function { get; set; }
        private int n_smooth_spline;
        public int N_smooth_spline
        {
            get
            {
                return n_smooth_spline;
            }

            set
            {
                n_smooth_spline = value;
                Validate(nameof(N_smooth_spline));
            }

        }
        private int n_small_grid;
        public int N_small_grid
        {
            get
            {
                return n_small_grid;
            }

            set
            {
                n_small_grid = value;
                Validate(nameof(N_small_grid));
            }

        }
        public double StopR { get; set; }
        public int MaxIt { get; set; }

        public V1DataArray Data;
        public SplineData Spline;
        IUIServices View;
        public RelayCommand DataFromControlsCommand { get; private set; }
        public RelayCommand DataFromFileCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public ViewModel(IUIServices view)
        {
            DataFromControlsCommand = new RelayCommand(DataFromControls, DataFromControlsCanExecute);
            DataFromFileCommand = new RelayCommand(DataFromFile, () => true);
            SaveCommand = new RelayCommand(Save, SaveCanExecute);
            Grid = null;
            N_grid_nodes = 0;
            Uniformity = true;
            Function = "X^3";
            N_smooth_spline = 0;
            N_small_grid = 0;
            StopR = 0.001;
            MaxIt = 100;
            View = view;
        }

        public string this[string propertyName]
        {
            get
            {
                string error = string.Empty;
                switch (propertyName)
                {
                    case "N_grid_nodes":
                        if (N_grid_nodes < 3 )
                        {
                            error = "Число узллов сетки должно быть больше 2";
                        }
                        break;
                    case "N_small_grid":
                        if (N_small_grid < 3)
                        {
                            error = "Число узллов равномерной сетки должно быть больше 2";
                        }
                        break;
                    case "Grid":
                        if (Grid == null || Grid.Length < 2 || Grid[0] > Grid[Grid.Length - 1])
                        {
                            error = "Левая граница отрезка должны быть меньше правой";
                        }
                        break;
                    case "N_smooth_spline":
                        if ((N_smooth_spline < 2) || (N_smooth_spline > N_grid_nodes))
                        {
                            error = "Число узллов сшлаживающего сплайна должно быть больше 2 и не больше числа узлов сетки";
                        }
                        break;
                }
                return error;
            }
        }
        Dictionary<string, string> Errors = new Dictionary<string, string>();

        public bool HasErrors => Errors.Count > 0;

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public IEnumerable GetErrors(string? propertyName)
        {
            if (Errors.ContainsKey(propertyName))
            {
                return Errors[propertyName];

            }
            else
            {
                return Enumerable.Empty<string>();
            }

        }

        public void Validate(string propertyName)
        {
            string result = this[propertyName];

            if (result != "")
            {
                if (Errors.ContainsKey(propertyName))
                    Errors[propertyName] = result;
                else
                    Errors.Add(propertyName, result);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
            else
            {
                Errors.Remove(propertyName);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }

            DataFromControlsCommand.RaiseCanExecuteChanged(); 
            DataFromFileCommand.RaiseCanExecuteChanged(); 
            SaveCommand.RaiseCanExecuteChanged(); 
        }

        public string Error
        {
            get { return Error; }
        }
        public void DataFromControls()
        {
            Random rnd = new Random((int)DateTime.Now.Ticks);
            try
            {
                FValues F = ViewModel.XCubed;
                if (Function == "X^3")
                {
                    F = ViewModel.XCubed;
                }
                else if (Function == "Sin(X)")
                {
                    F = ViewModel.XSin;
                }
                if (Uniformity)
                {
                    Data = new V1DataArray($"{rnd.Next()}", DateTime.Now, N_grid_nodes, Grid[0], Grid[Grid.Length - 1], F);
                } 
                else
                {
                    Data = new V1DataArray($"{rnd.Next()}", DateTime.Now, Grid, F);
                }
                Spline = new SplineData(Data, N_smooth_spline, MaxIt, StopR, N_small_grid);
                Spline.CalculateSpline();
                Plot();
            }
            catch (Exception ex)
            {
                if (Spline != null)
                    View.Info(ex.Message + $"Error Code:{Spline.Code}");
                else
                    View.Info("Error: " + ex.Message);
            }
        }
        public void DataFromFile()
        {
            string filename = View.Load();
            if (filename == null) return;
            try
            {
                Load(filename);
            }
            catch (Exception ee)
            {
                View.Info(ee.Message);
                return;
            }
            Random rnd = new Random((int)DateTime.Now.Ticks);

            Spline = new SplineData(Data, N_smooth_spline, MaxIt, StopR, N_small_grid);
            Spline.CalculateSpline();
            Plot();
        }
        public void Load(string filename)
        {
            Random rnd = new Random((int)DateTime.Now.Ticks);
            Data = new V1DataArray($"{rnd.Next()}", DateTime.Now);
            if (!V1DataArray.Load(filename, ref Data)) throw new Exception("Unable to read data");
        }
        public void Save()
        {
            string filename = View.Save();
            if (filename == null) return;
            FValues F = ViewModel.XCubed;
            if (Function == "X^3")
            {
                F = ViewModel.XCubed;
            }
            else if (Function == "Sin(X)")
            {
                F = ViewModel.XSin;
            }

            Random rnd = new Random((int)DateTime.Now.Ticks);
            if (Uniformity)
            {
                Data = new V1DataArray($"{rnd.Next()}", DateTime.Now, N_grid_nodes, Grid[0], Grid[Grid.Length - 1], F);
            }
            else
            {
                Data = new V1DataArray($"{rnd.Next()}", DateTime.Now, Grid, F);
            }
            try
            {
                V1DataArray.Save(filename, Data);
            }
            catch (Exception e)
            {
                View.Info(e.Message);
            }
        }

        private bool DataFromControlsCanExecute()
        {
            bool res = true;
            string[] ItemsToValidate = new string[4] { "Grid", "N_grid_nodes", "N_smooth_spline", "N_small_grid" };
            foreach (string propertyName in ItemsToValidate)
            {
                if (Errors.ContainsKey(propertyName))
                {
                    res = false;
                }
            }
            return res;
        }

        public bool SaveCanExecute()
        {
            bool res = true;
            string[] ItemsToValidate = new string[2] { "Grid", "N_grid_nodes" };
            foreach (string propertyName in ItemsToValidate)
            {
                if (Errors.ContainsKey(propertyName))
                {
                    res = false;
                }
            }
            return res;
        }
        private void Plot()
        {
            double[][] ResS = new double[Spline.ResS.Count][];
            double[][] Small_spline = new double[Spline.Small_spline.Count][];
            int i = 0;
            foreach (var point in Spline.ResS)
            {
                ResS[i] = new double[3] { point.X, point.Y, point.Ys };
                ++i;
            }
            i = 0;
            foreach (var point in Spline.Small_spline)
            {
                Small_spline[i] = point;
                ++i;
            }
            View.Plot(Small_spline, ResS);
        }
    }
}
