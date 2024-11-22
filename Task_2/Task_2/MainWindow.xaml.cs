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
using System.Formats.Tar;
using System.IO;
using System.Text.Json;
using System.Windows.Automation;

namespace Task_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class mainPopulation
    {
        public int N { get; set; }
        public int K { get; set; }
        public int M { get; set; }
        public List<int[][]> Population { get; set; }
        public int Population_size { get; set; }
        public int Num_of_crossovers_and_mutated { get; set; }

        public mainPopulation(int n, int k, int m, List<int[][]> population, int population_size, int num_of_crossovers_and_mutated)
        {
            N = n;
            K = k;
            M = m;
            Population = population;
            Population_size = population_size;
            Num_of_crossovers_and_mutated = num_of_crossovers_and_mutated;
        }
    }
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
        public const string RUNSJSON = "../../../runs.json";
        public ViewData? Parameters { get; set; }
        public mainPopulation? loadPopulation { get; set; }
        public bool Stop_button { get; set; }
        public int[,] population {  get; set; }
        public List<int[,]>? fullpopulation {  get; set; }
        public List<int[,]>? loadpopulation { get; set; } = null;
        public int population_size { get; set; }
        public int num_of_crossovers_and_mutated { get; set; }
        public Tournament? loadTournament { get; set; } = null;
        public class RunInfo
        {
            public string Name { get; set; }
            public string FileName { get; set; }
            public RunInfo(string name, string filename)
            {
                Name = name;
                FileName = filename;
            }
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                RunInfo objAsPart = obj as RunInfo;
                if (objAsPart == null) return false;
                else return Equals(objAsPart);
            }
            public bool Equals(RunInfo other)
            {
                if (other == null) return false;
                return (this.Name.Equals(other.Name));
            }
        }
        public MainWindow()
        {
            Parameters = new ViewData(0, 0, 0);
            DataContext = Parameters;
            InitializeComponent();
            InitializeRunsFile();

        }

        private void InitializeRunsFile()
        {
            if (!File.Exists(RUNSJSON))
            {
                try
                {
                    File.WriteAllText(RUNSJSON, "[]");  // Создаем пустой JSON-файл с массивом.
                }
                catch (Exception ex)
                {
                    ShowError($"Initialization of {RUNSJSON} failed", ex);
                }
            }
        }

        private void ShowError(string message, Exception ex)
        {
            MessageBox.Show($"{message}: \n{ex.Message}");
        }

        public static List<int[][]> ConvertToSerializableList(List<int[,]> multiDimArrays)
        {
            var result = new List<int[][]>();

            foreach (var multiDimArray in multiDimArrays)
            {
                int rows = multiDimArray.GetLength(0);
                int cols = multiDimArray.GetLength(1);
                int[][] jaggedArray = new int[rows][];

                for (int i = 0; i < rows; i++)
                {
                    jaggedArray[i] = new int[cols];
                    for (int j = 0; j < cols; j++)
                    {
                        jaggedArray[i][j] = multiDimArray[i, j];
                    }
                }

                result.Add(jaggedArray);
            }

            return result;
        }

        private void Save_button_click(object sender, RoutedEventArgs e)
        {
            Stop_button = true;
            var experiment = CreateExperiment();
            if (experiment == null) return;

            var runs = LoadRunsList();
            if (runs == null || runs.Contains(experiment)) return;

            string state = JsonSerializer.Serialize(new mainPopulation(Parameters.N, Parameters.K, Parameters.M,
                ConvertToSerializableList(fullpopulation), population_size, num_of_crossovers_and_mutated));

            if (!SaveExperimentState(experiment, state)) return;

            runs.Add(experiment);

            if (!SaveRunsList(runs)) return;

            MessageBox.Show("Saved.");
        }
        private bool SaveRunsList(List<RunInfo> runs)
        {
            string tmpFileName = $"tmp_{RUNSJSON}";

            try
            {
                File.WriteAllText(tmpFileName, JsonSerializer.Serialize(runs));
                File.Move(tmpFileName, RUNSJSON, true);
                return true;
            }
            catch (Exception ex)
            {
                ShowError($"Error writing to {RUNSJSON}", ex);
                DeleteFileIfExists(tmpFileName);
                return false;
            }
            finally
            {
                DeleteFileIfExists(tmpFileName);
            }
        }
        private bool SaveExperimentState(RunInfo experiment, string state)
        {
            try
            {
                File.WriteAllText(experiment.FileName, state);
                return true;
            }
            catch (Exception ex)
            {
                ShowError($"Error writing file {experiment.FileName}", ex);
                DeleteFileIfExists(experiment.FileName);
                return false;
            }
        }
        private void DeleteFileIfExists(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
        private List<RunInfo> LoadRunsList()
        {
            try
            {
                string json = File.ReadAllText(RUNSJSON);
                return JsonSerializer.Deserialize<List<RunInfo>>(json);
            }
            catch (Exception ex)
            {
                ShowError($"Error reading {RUNSJSON}", ex);
                return null;
            }
        }

        private RunInfo CreateExperiment()
        {
            string experimentName = ExperimentName.Text;
            var experiment = new RunInfo(experimentName, $"../../../{experimentName}.json");

            if (experiment.Name.Length == 0)
            {
                MessageBox.Show("Experiment name is required.");
                return null;
            }

            return experiment;
        }

        private void Load_button_click(object sender, RoutedEventArgs e)
        {
            var dialogWindow = new ChoseWindow();
            dialogWindow.ShowDialog();

            if (dialogWindow.DialogResult != true) return;

            try
            {
                string fileContent = File.ReadAllText(dialogWindow.RunFileName);
                loadPopulation = JsonSerializer.Deserialize<mainPopulation>(fileContent);
                Parameters.N = loadPopulation.N;
                Parameters.M = loadPopulation.M;
                Parameters.K = loadPopulation.K;
                population_size = loadPopulation.Population_size;
                num_of_crossovers_and_mutated = loadPopulation.Num_of_crossovers_and_mutated;
                loadpopulation = ConvertFromSerializableList(loadPopulation.Population);
                loadTournament = new Tournament(Parameters!.N, Parameters.K, Parameters.M, population_size, num_of_crossovers_and_mutated, loadpopulation);
                loadTournament.TournamentSelectionWithRanking();

                TextBox1.Text = Parameters.N.ToString();
                TextBox2.Text = Parameters.K.ToString();
                TextBox3.Text = Parameters.M.ToString();
                loadpopulation = null;

                population = loadTournament.best_result;
                string[,] show_result = loadTournament.Show(population);
                int best_score = loadTournament.best_score;
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
                DataGrid1.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                ShowError("Error reading data", ex);
            }
        }
        public static List<int[,]> ConvertFromSerializableList(List<int[][]> jaggedArrays)
        {
            var result = new List<int[,]>();

            foreach (var jaggedArray in jaggedArrays)
            {
                int rows = jaggedArray.Length;
                int cols = jaggedArray[0].Length;
                int[,] multiDimArray = new int[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        multiDimArray[i, j] = jaggedArray[i][j];
                    }
                }

                result.Add(multiDimArray);
            }

            return result;
        }
        private async void Run_button_click(object sender, RoutedEventArgs e)
        {
            RunButton.IsEnabled = false;
            population_size = 10;
            num_of_crossovers_and_mutated = 10;
            int steps_of_algorithm = 60;

            BindingExpression be1 = BindingOperations.GetBindingExpression(TextBox1, TextBox.TextProperty);
            be1.UpdateSource();
            BindingExpression be2 = BindingOperations.GetBindingExpression(TextBox2, TextBox.TextProperty);
            be2.UpdateSource();
            BindingExpression be3 = BindingOperations.GetBindingExpression(TextBox3, TextBox.TextProperty);
            be3.UpdateSource();
            Tournament manager;
            if (loadTournament is null)
            {
                manager = new Tournament(Parameters!.N, Parameters.K, Parameters.M, population_size, num_of_crossovers_and_mutated, loadpopulation);
            }
            else
            {
                manager = loadTournament;
                loadTournament = null;
            }
            int k = 1;
            Stop_button = false;
            while ((k <= steps_of_algorithm) & (Stop_button == false))
            {
                manager.Generate_best_schedule();
                fullpopulation = manager.population;
                population = manager.best_result;
                string[,] show_result = manager.Show(population);
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
                await Task.Delay(100).ContinueWith(_ =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        DataGrid1.ItemsSource = dt.DefaultView;
                        TextBlock1.Text = manager.best_score.ToString();
                        TextBlock2.Text = k.ToString();
                    });
                });
                k++;
            }
            RunButton.IsEnabled = true;
        }
        private void Stop_button_click(object sender, RoutedEventArgs e)
        {
            Stop_button = true;
        }
    }
}