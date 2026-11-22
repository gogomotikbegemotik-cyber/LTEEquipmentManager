using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LTEEquipmentManager.Services;

namespace LTEEquipmentManager
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            // Fit the reference composition to the available desktop, including high DPI.
            Width = Math.Min(Width, Math.Max(MinWidth, SystemParameters.WorkArea.Width - 32));
            Height = Math.Min(Height, Math.Max(MinHeight, SystemParameters.WorkArea.Height - 32));
        }

        // Visual window chrome only; connection handling below is unchanged.
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

        private readonly SqlConnectionService _connectionService = new SqlConnectionService();
        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            string server = txtServer.Text;
            string database = txtDatabase.Text;
            string username = txtLogin.Text;
            string password = txtPassword.Password;
            txtStatus.Foreground = Brushes.Red;
            //Проверка на заполняемость полей 
            if (string.IsNullOrWhiteSpace(server))
            {
                txtStatus.Text = "Server name is required";
                return;
            }
            else if (string.IsNullOrWhiteSpace(database))
            {
                txtStatus.Text = "Database name is required";
                return;
            }
            else if (string.IsNullOrWhiteSpace(username))
            {
                txtStatus.Text = "Login is required";
                return;
            }
            else if (string.IsNullOrWhiteSpace(password))
            {
                txtStatus.Text = "Password is required";
                return;
            }
            else
            {
                txtStatus.Text = $"Server: {server}, Database: {database}";
            }
            
            btnConnect.IsEnabled = false;
            btnConnect.Content = "Connecting...";
            string connectionString=SqlConnectionService.BuildConnectionString(server, database, username, password);

            try
            {
                bool connected = await _connectionService.TestConnectionAsync(connectionString);
                
                if (connected)
                {
                    txtStatus.Foreground = Brushes.Green;
                    txtStatus.Text = "Connection successful";                   // Выводим сообщение об успешном подключении
                    MainWindow mainWindow = new MainWindow(connectionString);   //создаем новый объект класса MainWindow и передаем строку подключения
                    Application.Current.MainWindow = mainWindow;                //назначаем новое главное окно
                    mainWindow.Show();                                          //показываем новое главное окно
                    this.Close();                                               //закрываем текущее окно авторизации

                }
                else
                {
                    txtStatus.Foreground = Brushes.Red;
                    txtStatus.Text = "Connection failed";// Выводим сообщение об ошибке подключения
                }
                
            }
            finally
            {
                btnConnect.IsEnabled = true;
                btnConnect.Content = "Connect";
            }
                        
        }
    }
}
