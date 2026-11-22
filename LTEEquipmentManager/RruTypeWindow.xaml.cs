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
using LTEEquipmentManager.Data;     // тут находится фабрика для работы с базой
using LTEEquipmentManager.Services; // тут находится наш сервис RRU
using System.Collections.ObjectModel; // нужен для коллекции, которую покажем в таблице
using LTEEquipmentManager.Data.Entities; // тут находится класс RruType
using System.Linq; // нужен, чтобы найти нужную колонку в таблице

namespace LTEEquipmentManager
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    
    public partial class RruTypeWindow : Window
    {


        // тут будем хранить типы портов для выпадающего списка
        private readonly ObservableCollection<PortType> _portTypes = new();

        // через этот сервис будем получать справочники из базы
        private readonly ReferenceDataService _referenceDataService;
        // тут будем хранить список RRU, который показываем в таблице
        private readonly ObservableCollection<RruType> _rruTypes = new();

        // тут будем хранить порты выбранной RRU
        private readonly ObservableCollection<RruTypePort> _rruPorts = new();
        // Стандарты выбранного RRU-порта
        private readonly ObservableCollection<RruTypePortStandart> _rruPortStandarts = new();
        // Справочник доступных стандартов
        private readonly ObservableCollection<Standart> _standarts = new();


        // тут будем хранить порт, который сейчас добавляем
        private RruTypePort? _newRruPort;

        // тут будем хранить порт, который сейчас изменяем
        private RruTypePort? _editingRruPort;

        // тут будем хранить старые значения порта, если нажмём Cancel
        private RruTypePort? _backupRruPort;
        // тут будем хранить сервис, который получает данные RRU из базы
        private readonly RruTypeService _rruTypeService;
        // Новый стандарт выбранного RRU-порта
        private RruTypePortStandart? _newPortStandart;
        // Стандарт порта, который сейчас редактируем
        private RruTypePortStandart? _editingPortStandart;

        // Старый ID стандарта для кнопки Cancel
        private byte _backupStandartId;





        // загружаем типы портов из базы для выпадающего списка
        // Window chrome only. Existing CRUD and event handlers are kept below.
        private void MinimizeWindow_Click(object sender, RoutedEventArgs e) => SystemCommands.MinimizeWindow(this);
        private void ToggleMaximizeWindow_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized) SystemCommands.RestoreWindow(this);
            else SystemCommands.MaximizeWindow(this);
        }
        private void CloseWindow_Click(object sender, RoutedEventArgs e) => SystemCommands.CloseWindow(this);

        private async Task LoadPortTypesAsync()
        {
            // получаем список типов портов через сервис
            var portTypes = await _referenceDataService.GetPortTypesAsync();

            // очищаем старый список, чтобы не было повторений
            _portTypes.Clear();

            // добавляем полученные типы портов в нашу коллекцию
            foreach (var portType in portTypes)
            {
                _portTypes.Add(portType);
            }
        }



        private void btnStandardEdit_Click(object sender, RoutedEventArgs e)
        {
            // Не разрешаем одновременно добавлять и редактировать
            if (_newPortStandart != null || _editingPortStandart != null)
            {
                MessageBox.Show("Сначала сохрани или отмени изменения!");
                return;
            }

            // Получаем выбранную запись
            if (dgRruPortStandards.SelectedItem is not RruTypePortStandart selectedStandard)
            {
                MessageBox.Show("Сначала выбери стандарт!");
                return;
            }

            // Запоминаем объект и его старое значение
            _editingPortStandart = selectedStandard;
            _backupStandartId = selectedStandard.StandartId;

            // Разрешаем редактировать третью таблицу
            dgRruPortStandards.IsReadOnly = false;

            // Переключаем кнопки
            btnStandardAdd.IsEnabled = false;
            btnStandardEdit.IsEnabled = false;
            btnStandardDelete.IsEnabled = false;
            btnStandardSave.IsEnabled = true;
            btnStandardCancel.IsEnabled = true;

            // Запрещаем переключать порт во время редактирования
            dgRruPorts.IsEnabled = false;

            // Оставляем нужную строку выделенной
            dgRruPortStandards.SelectedItem = selectedStandard;
            dgRruPortStandards.ScrollIntoView(selectedStandard);
        }


        private void btnStandardCancel_Click(object sender, RoutedEventArgs e)
        {
            dgRruPortStandards.CancelEdit(DataGridEditingUnit.Cell);
            dgRruPortStandards.CancelEdit(DataGridEditingUnit.Row);

            // Отмена добавления
            if (_newPortStandart != null)
            {
                _rruPortStandarts.Remove(_newPortStandart);
                _newPortStandart = null;
            }

            // Отмена редактирования
            if (_editingPortStandart != null)
            {
                _editingPortStandart.StandartId = _backupStandartId;
                _editingPortStandart = null;
            }

            dgRruPortStandards.IsReadOnly = true;
            dgRruPortStandards.Items.Refresh();

            btnStandardAdd.IsEnabled = true;
            btnStandardEdit.IsEnabled = true;
            btnStandardDelete.IsEnabled = true;
            btnStandardSave.IsEnabled = false;
            btnStandardCancel.IsEnabled = false;

            dgRruPorts.IsEnabled = true;
        }

      

        private async void btnStandardDelete_Click(object sender, RoutedEventArgs e)
        {
            // Запрещаем удаление во время добавления
            if (_newPortStandart != null)
            {
                MessageBox.Show("Сначала сохрани или отмени новый стандарт!");
                return;
            }

            // Проверяем, выбрана ли строка
            if (dgRruPortStandards.SelectedItem is not RruTypePortStandart selectedStandart)
            {
                MessageBox.Show("Сначала выбери стандарт для удаления!");
                return;
            }

            // Спрашиваем подтверждение
            var result = MessageBox.Show(
                "Удалить выбранный стандарт из этого порта?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                // Блокируем кнопку от повторного нажатия
                btnStandardDelete.IsEnabled = false;

                // Удаляем запись из SQL Server
                await _rruTypeService.DeleteRruPortStandartAsync(selectedStandart.Id);

                // Удаляем строку из интерфейса
                _rruPortStandarts.Remove(selectedStandart);

                MessageBox.Show("Стандарт успешно удалён!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.InnerException?.Message ?? ex.Message,
                    "Ошибка удаления стандарта",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                btnStandardDelete.IsEnabled = true;
            }
        }




        private async void btnStandardRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (_newPortStandart != null)
            {
                MessageBox.Show("Сначала сохрани или отмени новый стандарт!");
                return;
            }

            if (dgRruPorts.SelectedItem is not RruTypePort selectedPort ||
                selectedPort.Id <= 0)
            {
                MessageBox.Show("Сначала выбери сохранённый RRU-порт!");
                return;
            }

            try
            {
                btnStandardRefresh.IsEnabled = false;

                // Получаем стандарты выбранного порта из базы
                var standards = await _rruTypeService
                    .GetRruPortStandartsAsync(selectedPort.Id);

                _rruPortStandarts.Clear();

                foreach (var standard in standards)
                {
                    _rruPortStandarts.Add(standard);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.InnerException?.Message ?? ex.Message,
                    "Ошибка обновления стандартов",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                btnStandardRefresh.IsEnabled = true;
            }
        }



 

        private async void btnPortRefresh_Click(object sender, RoutedEventArgs e)
        {
            // Нельзя обновлять во время добавления или редактирования
            if (_newRruPort != null || _editingRruPort != null)
            {
                MessageBox.Show("Сначала сохрани или отмени изменения порта!");
                return;
            }

            // Проверяем, выбрана ли RRU
            if (dgRruTypes.SelectedItem is not RruType selectedRru)
            {
                MessageBox.Show("Сначала выбери RRU!");
                return;
            }

            try
            {
                btnPortRefresh.IsEnabled = false;

                // Получаем порты выбранной RRU из SQL Server
                var ports = await _rruTypeService.GetRruPortsAsync(selectedRru.Id);

                // Очищаем старый список
                _rruPorts.Clear();

                // Загружаем актуальные данные
                foreach (var port in ports)
                {
                    _rruPorts.Add(port);
                }

                // При обновлении портов очищаем стандарты,
                // чтобы не показывать данные старого выбора
                _rruPortStandarts.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.InnerException?.Message ?? ex.Message,
                    "Ошибка обновления портов",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                btnPortRefresh.IsEnabled = true;
            }
        }




        private async void btnStandardSave_Click(object sender, RoutedEventArgs e)
        {
            var standardToSave = _newPortStandart ?? _editingPortStandart;

            if (standardToSave == null)
                return;

            // Завершаем выбор в ComboBox
            if (!dgRruPortStandards.CommitEdit(DataGridEditingUnit.Cell, true) ||
                !dgRruPortStandards.CommitEdit(DataGridEditingUnit.Row, true))
            {
                MessageBox.Show("Заверши выбор стандарта!");
                return;
            }

            // Проверяем наличие стандарта в справочнике
            if (!_standarts.Any(s => s.Id == standardToSave.StandartId))
            {
                MessageBox.Show("Выбери стандарт!");
                return;
            }

            try
            {
                btnStandardSave.IsEnabled = false;
                btnStandardCancel.IsEnabled = false;

                if (_newPortStandart != null)
                {
                    // Добавляем новую запись
                    await _rruTypeService.AddRruPortStandartAsync(standardToSave);
                }
                else
                {
                    // Обновляем существующую запись
                    await _rruTypeService.UpdateRruPortStandartAsync(standardToSave);
                }

                _newPortStandart = null;
                _editingPortStandart = null;

                dgRruPortStandards.IsReadOnly = true;
                dgRruPortStandards.Items.Refresh();

                btnStandardAdd.IsEnabled = true;
                btnStandardEdit.IsEnabled = true;
                btnStandardDelete.IsEnabled = true;
                dgRruPorts.IsEnabled = true;

                MessageBox.Show("Стандарт успешно сохранён!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.InnerException?.Message ?? ex.Message,
                    "Ошибка сохранения стандарта",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                btnStandardSave.IsEnabled = true;
                btnStandardCancel.IsEnabled = true;
            }
        }





        private void btnPortStandardAdd_Click(object sender, RoutedEventArgs e)
        {
            // Не разрешаем добавлять второй стандарт,
            // пока не сохранён первый
            if (_newPortStandart != null)
                return;

            // Проверяем, выбран ли сохранённый порт
            if (dgRruPorts.SelectedItem is not RruTypePort selectedPort ||
                selectedPort.Id <= 0)
            {
                MessageBox.Show("Сначала выбери сохранённый RRU-порт!");
                return;
            }

            // Создаём новую связь порта со стандартом
            _newPortStandart = new RruTypePortStandart
            {
                RruTypePortId = selectedPort.Id
            };

            // Добавляем новую строку в третью таблицу
            _rruPortStandarts.Add(_newPortStandart);

            // Разрешаем редактирование
            dgRruPortStandards.IsReadOnly = false;

            // Выделяем новую строку
            dgRruPortStandards.SelectedItem = _newPortStandart;
            dgRruPortStandards.ScrollIntoView(_newPortStandart);

            // Переключаем кнопки
            btnStandardAdd.IsEnabled = false;
            btnStandardSave.IsEnabled = true;
            btnStandardCancel.IsEnabled = true;

            // Запрещаем менять порт до сохранения
            dgRruPorts.IsEnabled = false;
        }


        private void btnPortEdit_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, выбран ли порт
            if (dgRruPorts.SelectedItem is not RruTypePort selectedPort)
            {
                MessageBox.Show("Сначала выбери порт для редактирования!");
                return;
            }

            // Запрещаем редактирование во время добавления
            if (_newRruPort != null)
            {
                MessageBox.Show("Сначала закончи добавление нового порта!");
                return;
            }

            // Запрещаем повторный Edit во время редактирования
            if (_editingRruPort != null)
            {
                MessageBox.Show("Сначала сохрани или отмени изменения!");
                return;
            }

            // Запоминаем редактируемый порт
            _editingRruPort = selectedPort;

            // Сохраняем старые значения для Cancel
            _backupRruPort = new RruTypePort()
            {
                PortNumber = selectedPort.PortNumber,
                PortName = selectedPort.PortName,
                PortTypeId = selectedPort.PortTypeId,
                RruTypeId = selectedPort.RruTypeId,

                // Сохраняем состояние передачи и приёма
                Tx = selectedPort.Tx,
                Rx = selectedPort.Rx,

                // Сохраняем мощность порта
                MaxPowerWatt = selectedPort.MaxPowerWatt
            };

            // Разрешаем редактирование таблицы
            dgRruPorts.IsReadOnly = false;

            // Включаем Save и Cancel
            btnPortSave.IsEnabled = true;
            btnPortCancel.IsEnabled = true;

            // Отключаем Add
            btnPortAdd.IsEnabled = false;

            // Запрещаем переключать RRU
            dgRruTypes.IsEnabled = false;

            // Оставляем выбранным редактируемый порт
            dgRruPorts.SelectedItem = selectedPort;

            // Ставим курсор на выбранную строку
            dgRruPorts.ScrollIntoView(selectedPort);
        }
      


private async void btnPortDelete_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, не выполняется ли добавление или редактирование
            if (_newRruPort != null || _editingRruPort != null)
            {
                MessageBox.Show("Сначала сохрани или отмени изменения!");
                return;
            }

            // Получаем выбранный порт
            if (dgRruPorts.SelectedItem is not RruTypePort selectedPort)
            {
                MessageBox.Show("Сначала выбери порт!");
                return;
            }

            // Подтверждение удаления
            var result = MessageBox.Show(
                $"Удалить порт '{selectedPort.PortName}'?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                btnPortDelete.IsEnabled = false;

                // Вызываем метод сервиса
                await _rruTypeService.DeleteRruPortAsync(selectedPort.Id);

                // Убираем удалённый порт из таблицы
                _rruPorts.Remove(selectedPort);

                MessageBox.Show("Порт успешно удалён!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.InnerException?.Message ?? ex.Message,
                    "Ошибка удаления",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                btnPortDelete.IsEnabled = true;
            }
        }

        



        // этот метод запускается, когда нажимаем Add под таблицей портов
        private void btnPortAdd_Click(object sender, RoutedEventArgs e)
        {

            // проверяем, выбрали ли мы RRU в первой таблице
            if (dgRruTypes.SelectedItem is not RruType selectedRru)
            {
                // если RRU не выбрана, показываем сообщение
                MessageBox.Show("Сначала выбери RRU!");
                return;
            }

            // пока добавляем только один порт за раз
            if (_newRruPort != null)
            {
                return;
            }

            // создаём новый порт и привязываем его к выбранной RRU
            _newRruPort = new RruTypePort()
            {
                RruTypeId = selectedRru.Id
            };

            // добавляем новый порт в список, который показывает вторая таблица
            _rruPorts.Add(_newRruPort);

            // разрешаем редактировать таблицу портов
            dgRruPorts.IsReadOnly = false;

            // выделяем новый порт
            dgRruPorts.SelectedItem = _newRruPort;

            // прокручиваем таблицу к новой строке
            dgRruPorts.ScrollIntoView(_newRruPort);

            // пока добавляем порт, запрещаем повторное нажатие Add
            btnPortAdd.IsEnabled = false;

            // включаем кнопки сохранения и отмены
            btnPortSave.IsEnabled = true;
            btnPortCancel.IsEnabled = true;

            // запрещаем менять выбранную RRU, пока добавляем её порт
            dgRruTypes.IsEnabled = false;
        }
        // этот метод срабатывает, когда нажимаем Cancel под портами
        private void btnPortCancel_Click(object sender, RoutedEventArgs e)
        {
            // отменяем редактирование ячейки и строки
            dgRruPorts.CancelEdit(DataGridEditingUnit.Cell);
            dgRruPorts.CancelEdit(DataGridEditingUnit.Row);


            // Если мы редактировали существующий порт
            // и сохранили его старые значения
            if (_editingRruPort != null && _backupRruPort != null)
            {
                // Возвращаем старый номер порта
                _editingRruPort.PortNumber = _backupRruPort.PortNumber;

                // Возвращаем старое название порта
                _editingRruPort.PortName = _backupRruPort.PortName;

                // Возвращаем старый тип порта
                _editingRruPort.PortTypeId = _backupRruPort.PortTypeId;

                // Возвращаем связь с исходной RRU
                _editingRruPort.RruTypeId = _backupRruPort.RruTypeId;
                // Восстанавливаем старые значения TX и RX
                _editingRruPort.Tx = _backupRruPort.Tx;
                _editingRruPort.Rx = _backupRruPort.Rx;

                // Восстанавливаем старую мощность
                _editingRruPort.MaxPowerWatt = _backupRruPort.MaxPowerWatt;

                // Завершаем режим редактирования
                _editingRruPort = null;
                _backupRruPort = null;
            }


            // проверяем, есть ли новый порт
            if (_newRruPort != null)
            {
                // удаляем новый порт из списка на экране
                _rruPorts.Remove(_newRruPort);

                // очищаем переменную нового порта
                _newRruPort = null;
            }

            // снова запрещаем редактировать таблицу
            dgRruPorts.IsReadOnly = true;

            // обновляем отображение таблицы
            dgRruPorts.Items.Refresh();

            // возвращаем кнопки в обычное состояние
            btnPortAdd.IsEnabled = true;
            btnPortSave.IsEnabled = false;
            btnPortCancel.IsEnabled = false;

            // снова разрешаем выбирать RRU
            dgRruTypes.IsEnabled = true;
            // Если кнопка Edit называется btnPortEdit
            btnPortEdit.IsEnabled = true;
        }

        // этот метод запускается, когда нажимаем Save под портами

        private async void btnPortSave_Click(object sender, RoutedEventArgs e)
        {
            // Определяем, какой порт сохраняем:
            // новый или существующий
            RruTypePort? portToSave = _newRruPort ?? _editingRruPort;

            if (portToSave == null)
            {
                return;
            }

            // Завершаем редактирование ячейки и строки
            if (!dgRruPorts.CommitEdit(DataGridEditingUnit.Cell, true) ||
                !dgRruPorts.CommitEdit(DataGridEditingUnit.Row, true))
            {
                MessageBox.Show("Заверши редактирование ячейки!");
                return;
            }

            // Проверяем номер порта
            if (portToSave.PortNumber <= 0)
            {
                MessageBox.Show("Номер порта должен быть больше нуля!");
                return;
            }

            // Проверяем название порта
            if (string.IsNullOrWhiteSpace(portToSave.PortName))
            {
                MessageBox.Show("Напиши название порта!");
                return;
            }

            // Проверяем тип порта
            if (portToSave.PortTypeId <= 0)
            {
                MessageBox.Show("Выбери тип порта из списка!");
                return;
            }

            // Запоминаем режим до сохранения
            bool isNewPort = _newRruPort != null;

            try
            {
                // Блокируем кнопки на время работы с базой
                btnPortSave.IsEnabled = false;
                btnPortCancel.IsEnabled = false;
                dgRruPorts.IsReadOnly = true;

                if (isNewPort)
                {
                  
                    await _rruTypeService.AddRruPortAsync(portToSave);
                }
                else
                {
                  
                    await _rruTypeService.UpdateRruPortAsync(portToSave);
                }

                // Сбрасываем режим добавления и редактирования
                _newRruPort = null;
                _editingRruPort = null;
                _backupRruPort = null;

                // Обновляем отображение таблицы
                dgRruPorts.Items.Refresh();

                // Возвращаем кнопки в обычное состояние
                btnPortAdd.IsEnabled = true;
                btnPortSave.IsEnabled = false;
                btnPortCancel.IsEnabled = false;

                // Снова разрешаем выбирать RRU
                dgRruTypes.IsEnabled = true;

                MessageBox.Show(
                    isNewPort
                        ? "Новый порт успешно добавлен!"
                        : "Изменения порта успешно сохранены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.InnerException?.Message ?? ex.Message,
                    "Ошибка сохранения порта",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                // Разрешаем исправить данные и повторить сохранение
                dgRruPorts.IsReadOnly = false;
                btnPortSave.IsEnabled = true;
                btnPortCancel.IsEnabled = true;
            }
        }


        // этот метод запускается, когда выбираем другую RRU в первой таблице
        private async void dgRruTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
             
            // очищаем порты от предыдущей выбранной RRU
            _rruPorts.Clear();

            // проверяем, выбрана ли какая-нибудь RRU
            if (dgRruTypes.SelectedItem is not RruType selectedRru)
            {
                return; // если ничего не выбрано, дальше не идём
            }

            try
            {
                // получаем порты выбранной RRU из базы
                var ports = await _rruTypeService.GetRruPortsAsync(selectedRru.Id);

                // смотрим, сколько портов база нашла для выбранной RRU
              

                // проверяем, что пользователь не успел выбрать другую RRU
                if (!ReferenceEquals(dgRruTypes.SelectedItem, selectedRru))
                {
                    return;
                }

                // добавляем найденные порты во вторую таблицу
                foreach (var port in ports)
                {
                    _rruPorts.Add(port);
                }
            }
            catch (Exception ex)
            {
                // если база вернула ошибку, показываем её
                MessageBox.Show(
                    ex.InnerException?.Message ?? ex.Message,
                    "Ошибка загрузки портов RRU",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public RruTypeWindow(string connectionString)
        {
            InitializeComponent(); // загружаем наше окно из XAML
            Width = Math.Min(Width, Math.Max(MinWidth, SystemParameters.WorkArea.Width - 32));
            Height = Math.Min(Height, Math.Max(MinHeight, SystemParameters.WorkArea.Height - 32));

            // создаём фабрику с настройками подключения, которые пришли из MainWindow
            LTEEquipmentDbContextFactory factory =
                new LTEEquipmentDbContextFactory(connectionString);

            // создаём сервис RRU и передаём ему фабрику
            _rruTypeService = new RruTypeService(factory);
            // используем ту же фабрику для загрузки справочников
            _referenceDataService = new ReferenceDataService(factory);

            // говорим таблице, откуда брать строки для отображения
            dgRruTypes.ItemsSource = _rruTypes;

            // подключаем список портов ко второй таблице
            dgRruPorts.ItemsSource = _rruPorts;

            // Показываем стандарты выбранного порта в третьей таблице
            dgRruPortStandards.ItemsSource = _rruPortStandarts;
            // Находим колонку Standard в третьей таблице
            var standardColumn = dgRruPortStandards.Columns
                .OfType<DataGridComboBoxColumn>()
                .FirstOrDefault(c => c.Header?.ToString() == "Standard");

            // Передаём колонке список стандартов из SQL Server
            if (standardColumn != null)
            {
                standardColumn.ItemsSource = _standarts;
            }
            // говорим выпадающему списку, откуда брать типы портов





            // ищем колонку Port Type внутри таблицы портов
            var portTypeColumn = dgRruPorts.Columns
                .OfType<DataGridComboBoxColumn>()
                .FirstOrDefault(c => c.Header?.ToString() == "Port Type");

            // если нашли колонку, передаём ей список типов портов
            if (portTypeColumn != null)
            {
                portTypeColumn.ItemsSource = _portTypes;
            }
        }
        // загружаем список RRU из базы и показываем его в таблице
        private async Task LoadRruTypesAsync()
        {
            // получаем все RRU через наш сервис
            var rruList = await _rruTypeService.GetRruTypesAsync();

            // очищаем старые строки, чтобы не было повторений
            _rruTypes.Clear();

            // добавляем полученные RRU в коллекцию
            foreach (var rru in rruList)
            {
                _rruTypes.Add(rru);
            }
        }


        private async void dgRruPorts_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            // Очищаем стандарты предыдущего порта
            _rruPortStandarts.Clear();

            // Проверяем, выбран ли порт
            if (dgRruPorts.SelectedItem is not RruTypePort selectedPort)
            {
                return;
            }

            // У нового порта ещё нет ID в SQL Server
            if (selectedPort.Id <= 0)
            {
                return;
            }

            try
            {
                // Загружаем стандарты выбранного порта
                var standarts = await _rruTypeService
                    .GetRruPortStandartsAsync(selectedPort.Id);

                // Проверяем, что пользователь не выбрал другой порт
                if (!ReferenceEquals(dgRruPorts.SelectedItem, selectedPort))
                {
                    return;
                }

                // Добавляем полученные записи в третью таблицу
                foreach (var standart in standarts)
                {
                    _rruPortStandarts.Add(standart);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.InnerException?.Message ?? ex.Message,
                    "Ошибка загрузки стандартов",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

      

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Загружаем список RRU
                await LoadRruTypesAsync();

                // Загружаем типы портов для второй таблицы
                await LoadPortTypesAsync();

                // Загружаем стандарты для ComboBox третьей таблицы
                await LoadStandartsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.InnerException?.Message ?? ex.Message,
                    "Ошибка загрузки данных RRU",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task LoadStandartsAsync()
        {
            // Получаем справочник стандартов из SQL Server
            var standarts = await _rruTypeService.GetStandartsAsync();

            // Очищаем старые данные
            _standarts.Clear();

            // Добавляем стандарты в выпадающий список
            foreach (var standart in standarts)
            {
                _standarts.Add(standart);
            }
        }


    }
}