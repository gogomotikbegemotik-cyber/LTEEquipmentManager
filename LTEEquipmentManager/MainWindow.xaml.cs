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

namespace LTEEquipmentManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string _connectionString;
        public MainWindow(string connectionString)
        {
            InitializeComponent();

            _connectionString = connectionString;
        }

        // Window chrome only. The two existing navigation handlers below are unchanged.
        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void ToggleMaximizeWindow_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                SystemCommands.RestoreWindow(this);
            else
                SystemCommands.MaximizeWindow(this);
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void btnAntennaType_Click(object sender, RoutedEventArgs e)
        {
            AntennaTypeWindow antennaWindow = new AntennaTypeWindow(_connectionString); //создаем новый объект класса MainWindow и передаем строку подключения
            antennaWindow.Owner = this;     //определяем что владельцем AntennaTypeWindow является текущее окно MainWindow
            antennaWindow.ShowDialog();     //показываем новое окно блокируя действия в других окнах
        }

        private void btnRRUType_Click(object sender, RoutedEventArgs e)
        {
            // создаём окно RRU и передаём ему настройки подключения к базе
            RruTypeWindow rruWindow = new RruTypeWindow(_connectionString);

            // указываем, что окно RRU открывается из главного окна
            rruWindow.Owner = this;

            // показываем окно RRU
            rruWindow.ShowDialog();
        }
    }
}