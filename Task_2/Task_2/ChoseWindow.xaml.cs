using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Task_2.MainWindow;
using System.IO;

namespace Task_2
{
    /// <summary>
    /// Логика взаимодействия для ChoseWindow.xaml
    /// </summary>
    public partial class ChoseWindow : Window
    {
        public List<RunInfo> Runs { get; private set; } = new();
        public string RunFileName { get; private set; } = string.Empty;
        public ChoseWindow()
        {
            InitializeComponent();
            InitComboBox();
        }
        private void InitComboBox()
        {
            try
            {
                // Загружаем данные из файла
                string json = File.ReadAllText("../../../runs.json");
                Runs = JsonSerializer.Deserialize<List<RunInfo>>(json) ?? new List<RunInfo>();
            }
            catch (Exception ex)
            {
                // Сообщение об ошибке, если файл не удалось прочитать
                MessageBox.Show($"Failed to load runs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Runs = new List<RunInfo>();
            }

            // Заполняем ComboBox именами экспериментов
            ExpName.ItemsSource = Runs.Select(x => x.Name).ToList();
        }
        private void Button_Click_Load(object sender, RoutedEventArgs e)
        {
            // Проверка, чтобы избежать ошибки при отсутствии выбранного элемента
            if (ExpName.SelectedIndex >= 0 && ExpName.SelectedIndex < Runs.Count)
            {
                RunFileName = Runs[ExpName.SelectedIndex].FileName;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please select a valid experiment.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
