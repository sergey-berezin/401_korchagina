using GenTournament;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Task_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class ViewData
    {
        public int N { get; set; }
        public int K { get; set; }
        public int M { get; set; }

        public ViewData(int n, int k, int m)
        {
            N = n;
            K = k;
            M = m;
        }
    }
    public partial class MainWindow : Window
    {
        public ViewData? Parameters { get; set; }
        public bool Stop_button { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            Parameters = new ViewData(0, 0, 0);
            DataContext = Parameters;
        }
        private async void Run_button_click(object sender, RoutedEventArgs e)
        {
            RunButton.IsEnabled = false;
            int population_size = 1000;
            int num_of_crossovers_and_mutated = 1000;
            int steps_of_algorithm = 1000;

            BindingExpression be1 = BindingOperations.GetBindingExpression(TextBox1, TextBox.TextProperty);
            be1.UpdateSource();
            BindingExpression be2 = BindingOperations.GetBindingExpression(TextBox2, TextBox.TextProperty);
            be2.UpdateSource();
            BindingExpression be3 = BindingOperations.GetBindingExpression(TextBox3, TextBox.TextProperty);
            be3.UpdateSource();

            Tournament manager = new Tournament(Parameters!.N, Parameters.K, Parameters.M, population_size, num_of_crossovers_and_mutated);

            int k = 1;
            Stop_button = false;
            while ((k <= steps_of_algorithm) & (Stop_button == false))
            {
                manager.Generate_best_schedule();
                int[,] result = manager.best_result;
                string[,] show_result = manager.Show(result);
                int best_score = manager.best_score;
                DataTable dt = new DataTable();
                dt.TableName = "Tournament";
                int columns = Parameters.M;
                int rows = Parameters.K;
                var table = show_result;
                for (int j = 0; j <= columns; j++)
                    dt.Columns.Add(j.ToString(), typeof(string));

                for (int i = 0; i < rows; i++)
                {
                    DataRow dr = dt.NewRow();
                    int id = i + 1;
                    dr[0] = id.ToString();
                    for (int j = 1; j <= columns; j++)
                        dr[j] = table[i, j - 1];
                    
                    dt.Rows.Add(dr);
                }
                k++;
                if (k % 10 == 0)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        DataGrid1.ItemsSource = dt.DefaultView;
                        TextBlock1.Text = manager.best_score.ToString();
                        TextBlock2.Text = k.ToString();
                    });
                }
            }
            RunButton.IsEnabled = true;
        }
        private void Stop_button_click(object sender, RoutedEventArgs e)
        {
            Stop_button = true;
        }
    }
}