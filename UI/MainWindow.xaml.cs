using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot;
using OxyPlot.Series;
using static System.Runtime.InteropServices.JavaScript.JSType;
using VModel;
using System.Numerics;

namespace UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public class GridConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)value == "") return null;
            string[] LR = ((string)value).Split([',', ';', ' ', ':']).Where(val => val != "").ToArray();    
            double[] doubles = new double[LR.Length];
            try
            {
                for (int i = 0; i < doubles.Length; i++) { 
                   doubles[i] = double.Parse(LR[i].Replace('.', ','));
                }
            }
            catch
            {
                return null;
            }
            //System.Array.Sort(doubles);
            return doubles;
        }
    }
    public class ComboBoxConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            if ((string)((ComboBoxItem)value).Content == "Равномерная")
            { 
                return true;
            }
            return false;
            throw new NotImplementedException("No Such Type");
        }
    }
    public class FuncConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return (string)((ComboBoxItem)value).Content;
        }
    }
    public partial class MainWindow : Window, IUIServices
    {
        public void Plot(double[][] Small_spline, double[][] ResS)
        {
            SplineData.ItemsSource = ResS;
            Values.ItemsSource = Small_spline;

            PlotModel plot = new PlotModel();
            ScatterSeries ScatterSeries = new ScatterSeries();
            LineSeries lineSeries = new LineSeries();
            foreach (var elem in Small_spline)
            {
                lineSeries.Points.Add(new DataPoint(elem[0], elem[1]));
                //lineSeries.LabelFormatString = "{1:0.00}";
            }
            foreach (var elem in ResS)
            {
                ScatterSeries.Points.Add(new ScatterPoint(elem[0], elem[1]));
                //ScatterSeries.LabelFormatString = "{1:0.00}";
            }
            plot.Series.Add(lineSeries);
            plot.Series.Add(ScatterSeries);
            Plotter.Model = plot;
        }
        public void Info(string message)
        {
            MessageBox.Show(message);
        }
        public string Save()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string filename = dialog.FileName;
                return filename;
            }
            return null;
        }
        public string Load()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string filename = dialog.FileName;
                return filename;
            }
            return null;
        }
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new ViewModel(this);

            Binding bounds = new Binding();
            bounds.ValidatesOnDataErrors = true;
            bounds.Mode = BindingMode.OneWayToSource;
            bounds.Path = new PropertyPath("Grid");
            bounds.Converter = new GridConverter();
            bounds.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Bounds.SetBinding(TextBox.TextProperty, bounds);

            Binding n_grid_nodes = new Binding();
            n_grid_nodes.ValidatesOnDataErrors = true;
            n_grid_nodes.Mode = BindingMode.OneWayToSource;
            n_grid_nodes.Path = new PropertyPath("N_grid_nodes");
            n_grid_nodes.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            N_grid_nodes.SetBinding(TextBox.TextProperty, n_grid_nodes);

            Binding uniformity = new Binding();
            uniformity.ValidatesOnDataErrors = true;
            uniformity.Mode = BindingMode.OneWayToSource;
            uniformity.Path = new PropertyPath("Uniformity");
            uniformity.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            uniformity.Converter = new ComboBoxConverter();
            Uniformity.SetBinding(ComboBox.SelectedValueProperty, uniformity);

            Binding func = new Binding();
            func.ValidatesOnDataErrors = true;
            func.Mode = BindingMode.OneWayToSource;
            func.Path = new PropertyPath("Function");
            func.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            func.Converter = new FuncConverter();
            Func.SetBinding(ComboBox.SelectedValueProperty, func);

            Binding n_smooth_spline = new Binding();
            n_smooth_spline.ValidatesOnDataErrors = true;
            n_smooth_spline.Mode = BindingMode.OneWayToSource;
            n_smooth_spline.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            n_smooth_spline.Path = new PropertyPath("N_smooth_spline");
            N_smooth_spline.SetBinding(TextBox.TextProperty, n_smooth_spline);

            Binding n_small_grid = new Binding();
            n_small_grid.ValidatesOnDataErrors = true;
            n_small_grid.Mode = BindingMode.OneWayToSource;
            n_small_grid.Path = new PropertyPath("N_small_grid");
            n_small_grid.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            N_small_grid.SetBinding(TextBox.TextProperty, n_small_grid);

            Binding stop_r = new Binding();
            stop_r.ValidatesOnDataErrors = true;
            stop_r.Mode = BindingMode.OneWayToSource;
            stop_r.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            stop_r.Path = new PropertyPath("StopR");
            StopR.SetBinding(TextBox.TextProperty, stop_r);

            Binding max_it = new Binding();
            max_it.ValidatesOnDataErrors = true;
            max_it.Mode = BindingMode.OneWayToSource;
            max_it.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            max_it.Path = new PropertyPath("MaxIt");
            MaxIt.SetBinding(TextBox.TextProperty, max_it);
        }

    }
}