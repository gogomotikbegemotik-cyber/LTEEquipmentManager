using LTEEquipmentManager.Data;
using LTEEquipmentManager.Data.Entities;
using LTEEquipmentManager.Models;
using LTEEquipmentManager.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace LTEEquipmentManager
{
    /// <summary>
    /// Interaction logic for AntennaTypeWindow.xaml
    /// </summary>
    public partial class AntennaTypeWindow : Window
    {
        private ObservableCollection<AntennaType> _antennas =new ObservableCollection<AntennaType>();
        private ObservableCollection<AntennaTypePort> _antennaPorts = new ObservableCollection<AntennaTypePort>();
        private ObservableCollection<AntennaTypePortStandart> _antennaPortStandarts = new ObservableCollection<AntennaTypePortStandart>();
        private readonly ObservableCollection<PortType> _portTypes = new ObservableCollection<PortType>();
        private readonly ObservableCollection<Standart> _standarts = new ObservableCollection<Standart>();
        private AntennaType? _newAntenna=null;//Антенна, которая сейчас добавляется
        private AntennaType? _editingAntenna = null;//Ссылка на существующую антенну, которую редактируем
        private AntennaType? _backupAntenna = null; //Независимая копия антенны её первоначальных значений до редактирования
        private AntennaTypePort? _newAntennaPort; //Порт, который сейчас добавляется
        private AntennaTypePort? _editingAntennaPort;//Порт, который сейчас редактируется
        private AntennaTypePort? _backupAntennaPort = null; //Копия порта до редактирования
        private AntennaTypePortStandart? _newAntennaPortStandart; //Стандарт, который сейчас добавляется
        private AntennaTypePortStandart? _editingAntennaPortStandart;//Стандарт, который сейчас редактируется
        private AntennaTypePortStandart? _backupAntennaPortStandart = null;  //Копия стандарта до редактирования                                                  //
        private readonly AntennaTypeService _antennaTypeService;
        private readonly ReferenceDataService _referenceDataService;
        private bool _isUpdatingAntennas = false;
        private bool _isUpdatingAntennaPorts = false;
        private bool IsEditing => _newAntenna != null || _editingAntenna != null || _newAntennaPort != null || 
                                    _editingAntennaPort != null || _newAntennaPortStandart != null || _editingAntennaPortStandart != null;
        private bool _restoringSelection = false;
        private bool _selectionRestorePending = false;

        public  AntennaTypeWindow(string connectionString)
        {
            InitializeComponent();
            Width = Math.Min(Width, Math.Max(MinWidth, SystemParameters.WorkArea.Width - 32));
            Height = Math.Min(Height, Math.Max(MinHeight, SystemParameters.WorkArea.Height - 32));
            
            LTEEquipmentDbContextFactory factory = new LTEEquipmentDbContextFactory(connectionString);//создаем DBContext фабрику с пришедшей строкой подключения
            colPortType.ItemsSource = _portTypes;//подключаем коллекцию типов портов к ComboBox 
            colStandart.ItemsSource = _standarts;//подключаем коллекцию стандартов к ComboBox
            _antennaTypeService = new AntennaTypeService(factory);//создаем объект antennaTypeService
            _referenceDataService = new ReferenceDataService(factory);// создаем объект ReferenceDataService
            dgAntennaTypes.ItemsSource = _antennas;//привязываем коллекцию антенн к таблице грида
            dgAntennaPorts.ItemsSource = _antennaPorts;//привязываем коллекцию портов антенны к таблице грида
            dgAntennaPortStandarts.ItemsSource = _antennaPortStandarts;//привязываем коллекцию стандартов к таблице грида

        }

        // Управление всеми кнопками и DataGrid
        // Window chrome only. Existing CRUD and event handlers are kept below.
        private void MinimizeWindow_Click(object sender, RoutedEventArgs e) => SystemCommands.MinimizeWindow(this);
        private void ToggleMaximizeWindow_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized) SystemCommands.RestoreWindow(this);
            else SystemCommands.MaximizeWindow(this);
        }
        private void CloseWindow_Click(object sender, RoutedEventArgs e) => SystemCommands.CloseWindow(this);

        private void UpdateUiState()
        {
            
            // Определяем, какая таблица сейчас редактируется
            bool editingAntenna = _newAntenna != null || _editingAntenna != null;
            bool editingPort = _newAntennaPort != null || _editingAntennaPort != null;
            bool editingStandart = _newAntennaPortStandart != null || _editingAntennaPortStandart != null;
                        
            
            // Кнопки антеннм если что то добавляется или редактируется IsEditing=true, !IsEditing = false
            btnAdd.IsEnabled = !IsEditing;
            btnEdit.IsEnabled = !IsEditing;
            btnDelete.IsEnabled = !IsEditing;
            btnRefresh.IsEnabled = !IsEditing;

            btnSave.IsEnabled = editingAntenna;
            btnCancel.IsEnabled = editingAntenna;

            // кнопки портов
            btnPortAdd.IsEnabled = !IsEditing;
            btnPortEdit.IsEnabled = !IsEditing;
            btnPortDelete.IsEnabled = !IsEditing;
            btnPortRefresh.IsEnabled = !IsEditing;

            btnPortSave.IsEnabled = editingPort;
            btnPortCancel.IsEnabled = editingPort;

            // кнопки стандартов
            btnStandartAdd.IsEnabled = !IsEditing;
            btnStandartEdit.IsEnabled = !IsEditing;
            btnStandartDelete.IsEnabled = !IsEditing;
            btnStandartRefresh.IsEnabled = !IsEditing;

            btnStandartSave.IsEnabled = editingStandart;
            btnStandartCancel.IsEnabled = editingStandart;


        }
        //выгрузка справочников портов и стандартов в коллекции
        private async Task LoadReferenceDataAsync()
        {
            List<PortType> portTypes = await _referenceDataService.GetPortTypesAsync();

            _portTypes.Clear();

            foreach (PortType portType in portTypes)
            {
                _portTypes.Add(portType);
            }

            List<Standart> standarts = await _referenceDataService.GetStandartsAsync();

            _standarts.Clear();

            foreach (Standart standart in standarts)
            {
                _standarts.Add(standart);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            grdAntennaMain.IsEnabled = false;

            try 
            {
                //await LoadReferenceDataAsync();//наполняем коллекции справочников портов стандартов
                Task task1 = LoadReferenceDataAsync();
                Task<List<AntennaType>> task2 = _antennaTypeService.GetAntennasAsync();
                //List<AntennaType> antennas = await _antennaTypeService.GetAntennasAsync();

                await Task.WhenAll(task1, task2);

                List<AntennaType> antennas = task2.Result;

                _antennas.Clear();//Очищаем коллекцию

                foreach (AntennaType antenna in antennas) //наполняем коллекцию
            {
                _antennas.Add(antenna);
            }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка загрузки данных:\n{errorMessage}");
            }
            finally
            {
                grdAntennaMain.IsEnabled = true;
            }

        }

        //Ограничиваем редактирование только новой антенны 
        private void dgAntennaTypes_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            AntennaType? antenna = _newAntenna ?? _editingAntenna; //Возьми значение слева, если оно не равно null. В противном случае используй значение справа

            if (antenna == null ||!ReferenceEquals(e.Row.Item, antenna))//e.Row.Item - объект, который отображается в строке
            {
                e.Cancel = true;
            }
        }

        //Ограничиваем редактирование только новым портом или редактируемым портом
        private void dgAntennaPorts_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            AntennaTypePort? port = _newAntennaPort ?? _editingAntennaPort;

            if (port == null || !ReferenceEquals(e.Row.Item, port))
            {
                e.Cancel = true;
            }
        }

        //Разрешаем редактирование только новым стандартом или редактируемым 
        private void dgAntennaPortStandarts_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            AntennaTypePortStandart? standart = _newAntennaPortStandart ?? _editingAntennaPortStandart;

            if (standart == null || !ReferenceEquals(e.Row.Item, standart))
            {
                e.Cancel = true;
            }
        }

        //обрботчик для отображения списка портов при изменении выбранной антенны
        private async void dgAntennaTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //проверяем разрешено ли обновление чтоб не спамить запросами в SQL Server 
            if (_isUpdatingAntennas)
            {
                return;
            }
            // Запрещаем изменение выделения во время Add / Edit Antenna
            AntennaType? editingItem = _newAntenna ?? _editingAntenna;
            // и проверяем при редактирование соответствие выделенной строки и редактируемой, если не соответствует выделяем редактируемую строку
            if (KeepEditingSelection(dgAntennaTypes, editingItem,"антенны")) 
            {
                return;
            }

            _antennaPorts.Clear();//Очищаем коллекцию
            _antennaPortStandarts.Clear();//Очищаем коллекцию
            if (dgAntennaTypes.SelectedItem is not AntennaType antennaSelected) //записывает ссылку в переменную antennaSelected при сооветствие типу AntennaType 
                                                                                //в противном случае return 
            {
                return;
            }

            //если добавляется антенна то не обращаться на SQL сервер за портами их там нет
            if (ReferenceEquals(antennaSelected, _newAntenna))
            {
                return;
            }
            try
            { 
                List<AntennaTypePort> antennaPorts = await _antennaTypeService.GetAntennaPortsAsync(antennaSelected.Id);
            
                if (!ReferenceEquals(dgAntennaTypes.SelectedItem, antennaSelected))//проверяем не успело ли изменится выбранная антенна перед наполнением коллекции
                {
                    return;
                }

                _antennaPorts.Clear();//Очищаем коллекцию
                foreach (AntennaTypePort port in antennaPorts) //наполняем коллекцию
                {
                    _antennaPorts.Add(port);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка загрузки портов антенны:\n{errorMessage}");
            }
        }

        //обрботчик для отображения списка поддерживаемых стандартов при изменении выбранного порта антенны
        private async void dgAntennaPorts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //проверяем разрешено ли обновление чтоб не спамить запросами в SQL Serve
            if (_isUpdatingAntennaPorts)
            {
                return;
            }

            // Запрещаем изменение выделения во время Add / Edit AntennaPort 
            AntennaTypePort? editingItem = _newAntennaPort ?? _editingAntennaPort;
            // и проверяем при редактирование соответствие выделенной строки и редактируемой, если не соответствует выделяем редактируемую строку
            if (KeepEditingSelection(dgAntennaPorts, editingItem, "порта"))
            {
                return;
            }

            _antennaPortStandarts.Clear();//Очищаем коллекцию
            
            
            if (dgAntennaPorts.SelectedItem is not AntennaTypePort portSelected) //записывает ссылку в переменную portSelected при сооветствие типу AntennaTypePort 
                                                                                 //в противном случае return 
            {
                return;
            }

            //если добавляется порт то не обращаться на SQL сервер за стандартами их там нет
            if (ReferenceEquals(portSelected, _newAntennaPort))//если добавляется порт то не обращаться на SQL сервер за стандартами их там нет
            {
                return;
            }

            try 
            { 
                List<AntennaTypePortStandart> portStandarts = await _antennaTypeService.GetAntennaPortStandartsAsync(portSelected.Id);

                if (!ReferenceEquals(dgAntennaPorts.SelectedItem, portSelected))//проверяем не успело ли изменится выбранный порт антенны перед наполнением коллекции
                {
                    return;
                }

                _antennaPortStandarts.Clear();
                foreach (AntennaTypePortStandart standart in portStandarts) //наполняем коллекцию
                {
                    _antennaPortStandarts.Add(standart);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка загрузки стандартов:\n{errorMessage}");
            }
        }

        private void dgAntennaPortStandarts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Запрещаем изменение выделения во время Add / Edit AntennaPortStandart
            AntennaTypePortStandart? editingItem =  _newAntennaPortStandart ?? _editingAntennaPortStandart;
            // и проверяем при редактирование соответствие выделенной строки и редактируемой, если не соответствует выделяем редактируемую строку
            if (KeepEditingSelection(dgAntennaPortStandarts, editingItem, "стандарта"))
            {
                return;
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (IsEditing)
            {
                return;
            }
            if (dgAntennaTypes.SelectedItem is not AntennaType antennaSelected) //записывает ссылку в переменную antennaSelected при сооветствие типу AntennaType 
                                                                                //в противном случае return 
            {
                return;
            }
            
            _editingAntenna = antennaSelected; //передаем ссылку на выбранный объект

                
            _backupAntenna = new AntennaType //создаем объект с backup полей и передаем ссылку на объект в _backupAntenna для случая Cancel
                {
                    Id = antennaSelected.Id,
                    AntennaName = antennaSelected.AntennaName,
                    Manufacturer = antennaSelected.Manufacturer,
                    Polarization = antennaSelected.Polarization,
                    AntennaFamily= antennaSelected.AntennaFamily,
                    Height = antennaSelected.Height
                };
    
            dgAntennaTypes.IsReadOnly = false;
            dgAntennaPorts.IsReadOnly = true;
            dgAntennaPortStandarts.IsReadOnly = true;

            UpdateUiState();//вызываем управления кнопками

        }
        

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //отменяем редактирование внутри DataGrid если что то еще в состоянии редактирования
            dgAntennaTypes.CancelEdit(DataGridEditingUnit.Cell);
            dgAntennaTypes.CancelEdit(DataGridEditingUnit.Row);
            
            if (_newAntenna != null)
            {
                AntennaType antennaToRemove = _newAntenna; //передаем ссылку для безопасного удаления из коллекции
                _newAntenna = null;          //удаляем из переменной ссылку на новый объект указывая что добавление завершено
                _antennas.Remove(antennaToRemove);  //при добавлении удаляем из коллекции новый объект а точнее ссылку на объект
                
            }
            
            if (_editingAntenna != null && _backupAntenna != null)
            {
                // восстановливаем четыре свойства редактируемой антенны из резервной копии.
                _editingAntenna.Id = _backupAntenna.Id;
                _editingAntenna.AntennaName = _backupAntenna.AntennaName;
                _editingAntenna.Manufacturer = _backupAntenna.Manufacturer;
                _editingAntenna.Polarization = _backupAntenna.Polarization;
                _editingAntenna.AntennaFamily = _backupAntenna.AntennaFamily;
                _editingAntenna.Height = _backupAntenna.Height;
            }
            
            //очищаем ссылки
            _editingAntenna = null;
            _backupAntenna = null;

            dgAntennaTypes.Items.Refresh();//Обновляем отображения грида после Cancel

            // Возвращаем таблицы в режим просмотра
            dgAntennaTypes.IsReadOnly = true;
            dgAntennaPorts.IsReadOnly = true;
            dgAntennaPortStandarts.IsReadOnly = true;

            UpdateUiState();//вызываем управления кнопками
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (IsEditing)
            {
                return;
            }

            _newAntenna = new AntennaType();//создаем новый объект антенны и сохраняем ссылку на него в переменную
            _antennas.Add(_newAntenna);

            dgAntennaTypes.IsReadOnly = false;
            dgAntennaTypes.SelectedItem = _newAntenna;  //выделяем новую строку и добавляемый объект
            dgAntennaTypes.ScrollIntoView(_newAntenna); //помещаем в поле видимость объект-строку

            UpdateUiState();//вызываем управления кнопками
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {

            if (_newAntenna == null && _editingAntenna == null)
            {
                return;
            }

            bool cellSaved = dgAntennaTypes.CommitEdit(DataGridEditingUnit.Cell, true);//завершаем редактирование ячейку
            bool rowSaved = dgAntennaTypes.CommitEdit(DataGridEditingUnit.Row, true);//завершаем редактирование строки

            if (!cellSaved || !rowSaved) //проверяем завершилось ли редактирование
            {
                return;
            }

            AntennaType antenna = _newAntenna ?? _editingAntenna!;// либо _newAntenna либо _editingAntenna! будет здесь не null

            if (string.IsNullOrWhiteSpace(antenna.Manufacturer) || string.IsNullOrWhiteSpace(antenna.AntennaName))// проверяем обязательные поля на null и пробелы
            {
                MessageBox.Show("Название антенны и производитель — обязательные поля.");
                return;
            }
            
            // Блокируем интерфейс на время SQL-запроса
            grdAntennaMain.IsEnabled = false;

            dgAntennaTypes.IsReadOnly = true;//блокируем редактирование грида


            //пробуем сохранить в БД
            try
            {
                if (_newAntenna != null)
                {
                    await _antennaTypeService.AddAntennaAsync(_newAntenna);
                }
                else if (_editingAntenna != null)
                {
                    await _antennaTypeService.UpdateAntennaAsync(_editingAntenna);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException?.Message ?? ex.Message;//если подробное описание ошибки есть, то ее занести в переменную, если нет, то общее
                MessageBox.Show($"Ошибка при сохранении антенны:\n{errorMessage}");
                dgAntennaTypes.IsReadOnly = false; //деблокируем редактирование грида
                UpdateUiState();//вызываем управления кнопками
                return;
            }
            finally
            {
                grdAntennaMain.IsEnabled = true;
            }

            dgAntennaTypes.Items.Refresh();//обновляем отображение
            //очищаем ссылки      
            _newAntenna = null; 
            _editingAntenna = null;
            _backupAntenna = null;

            dgAntennaTypes.IsReadOnly = true;
            dgAntennaPorts.IsReadOnly = true;
            dgAntennaPortStandarts.IsReadOnly = true;

            UpdateUiState();//вызываем управления кнопками

        }

        private void btnPortAdd_Click(object sender, RoutedEventArgs e)
        {
            if (IsEditing)
            {
                return;
            }
            if (dgAntennaTypes.SelectedItem is not AntennaType antennaSelected) //записывает ссылку в переменную antennaSelected при сооветствие типу AntennaType 
                                                                                //в противном случае return 
            {
                return;
            }
            dgAntennaTypes.IsEnabled = false;//блокируем грид антенн чтоб не изменилась выбранная антенна
            _newAntennaPort = new AntennaTypePort() { AntennaTypeId= antennaSelected.Id };//создаем новый объект порт антенны и сохраняем ссылку на него в переменную
            _antennaPorts.Add(_newAntennaPort);

            dgAntennaPorts.IsReadOnly = false;
            dgAntennaPorts.SelectedItem = _newAntennaPort;  //выделяем новую строку и добавляемый объект
            dgAntennaPorts.ScrollIntoView(_newAntennaPort); //помещаем в поле видимость объект-строку

            UpdateUiState();//вызываем управления кнопками
        }

        private async void btnPortSave_Click(object sender, RoutedEventArgs e)
        {
            if (_newAntennaPort == null && _editingAntennaPort == null)
            {
                return;
            }

            bool cellSaved = dgAntennaPorts.CommitEdit(DataGridEditingUnit.Cell, true);//завершаем редактирование ячейку
            bool rowSaved = dgAntennaPorts.CommitEdit(DataGridEditingUnit.Row, true);//завершаем редактирование строки

            if (!cellSaved || !rowSaved) //проверяем завершилось ли редактирование
            {
                return;
            }

            AntennaTypePort antennaPort = _newAntennaPort ?? _editingAntennaPort!;
            if (string.IsNullOrWhiteSpace(antennaPort.PortName) || antennaPort.PortNumber <= 0 || antennaPort.PortTypeId <= 0)// проверяем обязательные поля
            {
                MessageBox.Show("Название, номер и тип порта — обязательные поля.");
                return;
            }

            // Блокируем интерфейс на время SQL-запроса
            grdAntennaMain.IsEnabled = false;

            //блокируем редактирование грида
            dgAntennaPorts.IsReadOnly = true;

            //пробуем сохранить в БД
            try
            {
                if (_newAntennaPort != null)
                {
                    await _antennaTypeService.AddAntennaPortAsync(_newAntennaPort);
                }
                else if (_editingAntennaPort != null)
                {
                    await _antennaTypeService.UpdateAntennaPortAsync(_editingAntennaPort);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException?.Message ?? ex.Message;//если подробное описание ошибки есть, то ее занести в переменную, если нет, то общее
                MessageBox.Show($"Ошибка при сохранении порта:\n{errorMessage}");
                UpdateUiState();//вызываем управления кнопками
                dgAntennaPorts.IsReadOnly = false; //деблокируем редактирование грида
                return;
            }
            finally
            { 
                grdAntennaMain.IsEnabled = true; // Деблокируем интерфейс
            }
            // Обновляем грид после Update
            dgAntennaPorts.Items.Refresh();

            // Очищаем ссылки
            _newAntennaPort = null;
            _editingAntennaPort = null;
            _backupAntennaPort = null;

            // Возвращаем DataGrid в режим просмотра
            dgAntennaPorts.IsReadOnly = true;
            dgAntennaTypes.IsEnabled = true;

            UpdateUiState();//вызываем управления кнопками
        }

        private void btnPortCancel_Click(object sender, RoutedEventArgs e)
        {
            //отменяем редактирование внутри DataGrid если что то еще в состоянии редактирования
            dgAntennaPorts.CancelEdit(DataGridEditingUnit.Cell);
            dgAntennaPorts.CancelEdit(DataGridEditingUnit.Row);

            if (_newAntennaPort != null)
            {
                AntennaTypePort portToRemove = _newAntennaPort;
                _newAntennaPort = null;               //удаляем из переменной ссылку на новый объект
                _antennaPorts.Remove(portToRemove);  //при добавлении удаляем из коллекции новый объект а точнее ссылку на объект
                
            }
            else if (_editingAntennaPort != null && _backupAntennaPort != null)
            {
                // Восстанавливаем значения пораметров порта из резервной копии
                _editingAntennaPort.AntennaTypeId = _backupAntennaPort.AntennaTypeId;
                _editingAntennaPort.PortNumber = _backupAntennaPort.PortNumber;
                _editingAntennaPort.PortName = _backupAntennaPort.PortName;
                _editingAntennaPort.PortTypeId = _backupAntennaPort.PortTypeId;
                _editingAntennaPort.TiltGroup = _backupAntennaPort.TiltGroup;
                _editingAntennaPort.MinTilt = _backupAntennaPort.MinTilt;
                _editingAntennaPort.MaxTilt = _backupAntennaPort.MaxTilt;
                _editingAntennaPort.TiltStep = _backupAntennaPort.TiltStep;
                _editingAntennaPort.Polarization = _backupAntennaPort.Polarization;
                _editingAntennaPort.AzimuthOffsetDeg = _backupAntennaPort.AzimuthOffsetDeg;
                _editingAntennaPort.Combiner = _backupAntennaPort.Combiner;
            }


            //очищаем ссылки
            _editingAntennaPort = null;
            _backupAntennaPort = null;

            dgAntennaPorts.Items.Refresh();//Обновляем отображения грида после Cancel

            // Возвращаем таблицы в режим просмотра
            dgAntennaTypes.IsReadOnly = true;
            dgAntennaPorts.IsReadOnly = true;
            dgAntennaPortStandarts.IsReadOnly = true;

            // Восстанавливаем кнопки

            UpdateUiState();//вызываем управления кнопками

            dgAntennaTypes.IsEnabled = true;

        }

        private void btnPortEdit_Click(object sender, RoutedEventArgs e)
        {
            if (IsEditing)
            {
                return;
            }

            if (dgAntennaPorts.SelectedItem is not AntennaTypePort antennaPortSelected) //записывает ссылку в переменную antennaSelected при сооветствие типу AntennaType 
                                                                                //в противном случае return 
            {
                return;
            }
            
            dgAntennaTypes.IsEnabled = false;//блокируем грид антенн чтоб не изменилась редактируемая антенна
            _editingAntennaPort = antennaPortSelected; //передаем ссылку на выбранный объект


            _backupAntennaPort = new AntennaTypePort //создаем объект с backup полей и передаем ссылку на объект в _backupAntenna для случая Cancel
            {
                Id = antennaPortSelected.Id,
                AntennaTypeId = antennaPortSelected.AntennaTypeId,
                PortNumber = antennaPortSelected.PortNumber,
                PortName = antennaPortSelected.PortName,
                PortTypeId = antennaPortSelected.PortTypeId,
                TiltGroup = antennaPortSelected.TiltGroup,
                MinTilt = antennaPortSelected.MinTilt,
                MaxTilt = antennaPortSelected.MaxTilt,
                TiltStep = antennaPortSelected.TiltStep,
                Polarization = antennaPortSelected.Polarization,
                AzimuthOffsetDeg = antennaPortSelected.AzimuthOffsetDeg,
                Combiner = antennaPortSelected.Combiner
            };

            dgAntennaTypes.IsReadOnly = true;
            dgAntennaPorts.IsReadOnly = false;
            dgAntennaPortStandarts.IsReadOnly = true;

            UpdateUiState();//выставляем кнопки

        }

        private async void btnPortDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgAntennaPorts.SelectedItem is not AntennaTypePort antennaPortSelected) //записывает ссылку в переменную antennaPortSelected при сооветствие типу
                                                                                        //в противном случае return 
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                                    "Удалить выбранный порт?",
                                    "Подтверждение удаления",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }
            //блокируем  пока идет запрос SQL
            grdAntennaMain.IsEnabled = false;

            try
            {
                await _antennaTypeService.DeleteAntennaPortAsync(antennaPortSelected.Id);

                _antennaPorts.Remove(antennaPortSelected);//удаляем также и из коллекции
            }
            catch (Exception ex) 
            {
                string errorMessage = ex.InnerException?.Message ?? ex.Message;//если подробное описание ошибки есть, то ее занести в переменную, если нет, то общее
                MessageBox.Show($"Ошибка при удалении порта:\n{errorMessage}");
                return;
            }
            finally
            {
                grdAntennaMain.IsEnabled = true;
                
            }
                       
                        
        }

        private async void btnPortRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (IsEditing)
            {
                return;
            }

            if (dgAntennaTypes.SelectedItem is not AntennaType antennaSelected) //записывает ссылку в переменную antennaSelected при сооветствие типу AntennaType 
                                                                                //в противном случае return 
            {
                return;
            }

            // блокируем до выполнения
            grdAntennaMain.IsEnabled = false;
            
             
            try
            {
                // загружаем данные из SQL Server
                List<AntennaTypePort> antennaPorts = await _antennaTypeService.GetAntennaPortsAsync(antennaSelected.Id);

                // Проверяем, не изменилась ли выбранная антенна
                if (!ReferenceEquals(dgAntennaTypes.SelectedItem, antennaSelected))
                {
                    return;
                }

                //фиксируем перед обновлением Id выбранного порта
                int? antennaPortSelectedId = (dgAntennaPorts.SelectedItem as AntennaTypePort)?.Id;

                // Игнорируем SelectionChanged во время программного обновления портов dgAntennaPorts_SelectionChanged
                _isUpdatingAntennaPorts = true;
                               

                try 
                {
                
                    // очищаем коллекцию
                    _antennaPorts.Clear();

                    // Заполняем новыми объектами
                    foreach (AntennaTypePort port in antennaPorts)
                    {
                        _antennaPorts.Add(port);
                    }

                    // Восстанавливаем выделение
                    AntennaTypePort? antennaPortSelected = _antennaPorts.FirstOrDefault(p => p.Id == antennaPortSelectedId);
                    dgAntennaPorts.SelectedItem = antennaPortSelected;

                    // Если выбранный порт больше не существует, очищаем связанные стандарты
                    if (dgAntennaPorts.SelectedItem == null)
                    {
                        _antennaPortStandarts.Clear();
                    }
                }
                finally 
                { 
                    _isUpdatingAntennaPorts = false; 
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException?.Message ?? ex.Message;

                MessageBox.Show($"Ошибка при обновлении портов:\n{errorMessage}");
            }
            finally
            {
                grdAntennaMain.IsEnabled = true;
            }

            
        }

        private void btnStandartAdd_Click(object sender, RoutedEventArgs e)
        {
            //проверяем не добавляется или редактируется уже, если так то выходим из обработчика кнопки
            if (IsEditing)
            {
                return;
            }

            if (dgAntennaPorts.SelectedItem is not AntennaTypePort antennaPortSelected) //записывает ссылку в переменную  при сооветствие типу  
                                                                                //в противном случае return 
            {
                return;
            }

            
            //блокируем гриды  чтоб не изменилась выбранный порт
            dgAntennaTypes.IsEnabled = false;
            dgAntennaPorts.IsEnabled = false;

            _newAntennaPortStandart = new AntennaTypePortStandart() { AntennaTypePortId = antennaPortSelected.Id };//создаем новый объект и сохраняем ссылку на него в переменную
            _antennaPortStandarts.Add(_newAntennaPortStandart);//добавляем в коллекцию говый стандарт

            dgAntennaPortStandarts.IsReadOnly = false;
            dgAntennaPortStandarts.SelectedItem = _newAntennaPortStandart;  //выделяем новую строку и добавляемый объект
            dgAntennaPortStandarts.ScrollIntoView(_newAntennaPortStandart); //помещаем в поле видимость объект-строку

            UpdateUiState();//вызываем управления кнопками
        }

        private void btnStandartCancel_Click(object sender, RoutedEventArgs e)
        {
            //отменяем редактирование внутри DataGrid если что то еще в состоянии редактирования
            dgAntennaPortStandarts.CancelEdit(DataGridEditingUnit.Cell);
            dgAntennaPortStandarts.CancelEdit(DataGridEditingUnit.Row);

            if (_newAntennaPortStandart != null)
            {
                AntennaTypePortStandart standartToRemove = _newAntennaPortStandart;//передаем ссылку для безопасного удаления
                _newAntennaPortStandart = null;               //удаляем из переменной ссылку на новый объект
                _antennaPortStandarts.Remove(standartToRemove);  //при добавлении удаляем из коллекции новый объект а точнее ссылку на объект
                
            }
            else if (_editingAntennaPortStandart != null && _backupAntennaPortStandart != null)
            {
                // Восстанавливаем значения параметров порта из резервной копии
                _editingAntennaPortStandart.AntennaTypePortId = _backupAntennaPortStandart.AntennaTypePortId;
                _editingAntennaPortStandart.StandartId = _backupAntennaPortStandart.StandartId;
                _editingAntennaPortStandart.Gain = _backupAntennaPortStandart.Gain;
                _editingAntennaPortStandart.Hhpbw = _backupAntennaPortStandart.Hhpbw;
                _editingAntennaPortStandart.Vhpbw = _backupAntennaPortStandart.Vhpbw;
                
            }


            //очищаем ссылки
            _editingAntennaPortStandart = null;
            _backupAntennaPortStandart = null;

            dgAntennaPortStandarts.Items.Refresh();//Обновляем отображения грида после Cancel

            // Возвращаем таблицы в режим просмотра
            dgAntennaTypes.IsReadOnly = true;
            dgAntennaPorts.IsReadOnly = true;
            dgAntennaPortStandarts.IsReadOnly = true;
                       

            dgAntennaTypes.IsEnabled = true;
            dgAntennaPorts.IsEnabled = true;
            UpdateUiState();//вызываем управления кнопками
        }

        private void btnStandartEdit_Click(object sender, RoutedEventArgs e)
        {
            //проверяем не добавляется или редактируется уже, если так то выходим из обработчика кнопки
            if (IsEditing)
            {
                return;
            }

            if (dgAntennaPortStandarts.SelectedItem is not AntennaTypePortStandart antennaPortStandartSelected) //записывает ссылку в переменную antennaSelected при сооветствие типу AntennaType 
                                                                                        //в противном случае return 
            {
                return;
            }

            dgAntennaTypes.IsEnabled = false;//блокируем грид антенн чтоб не изменилась редактируемая антенна
            dgAntennaPorts.IsEnabled = false;//блокируем грид портов чтоб не изменилась редактируемая антенна
            
            _editingAntennaPortStandart = antennaPortStandartSelected; //передаем ссылку на выбранный объект


            _backupAntennaPortStandart = new AntennaTypePortStandart //создаем объект с backup полей и передаем ссылку на объект в _backupAntenna для случая Cancel
            {
                Id = antennaPortStandartSelected.Id,
                AntennaTypePortId = antennaPortStandartSelected.AntennaTypePortId,
                StandartId = antennaPortStandartSelected.StandartId,
                Gain= antennaPortStandartSelected.Gain,
                Vhpbw = antennaPortStandartSelected.Vhpbw,
                Hhpbw = antennaPortStandartSelected.Hhpbw
                
            };

            dgAntennaPortStandarts.IsReadOnly = false;

            UpdateUiState();//вызываем управления кнопками

        }

        private async void btnStandartSave_Click(object sender, RoutedEventArgs e)
        {
            //проверяем если ничего не добавляется или не редактируется, то выходим из обработчика кнопки
            if (_newAntennaPortStandart == null && _editingAntennaPortStandart == null)
            {
                return;
            }

            bool cellSaved = dgAntennaPortStandarts.CommitEdit(DataGridEditingUnit.Cell, true);//завершаем редактирование ячейку
            bool rowSaved = dgAntennaPortStandarts.CommitEdit(DataGridEditingUnit.Row, true);//завершаем редактирование строки

            if (!cellSaved || !rowSaved) //проверяем завершилось ли редактирование
            {
                return;
            }

            AntennaTypePortStandart antennaPortStandart = _newAntennaPortStandart ?? _editingAntennaPortStandart!;

            //проверяем выбран ли стандарт и есть ли он в списке доступных
            if (!_standarts.Any(s => s.Id == antennaPortStandart.StandartId))
            {
                MessageBox.Show("Выберите стандарт.");
                return;
            }

            // проверяем Gain Hhpbw Vhpbw либо должны быть null либо если есть значения определяем диапазон допустимых значений 
            if ((antennaPortStandart.Gain.HasValue && antennaPortStandart.Gain.Value < 0) ||
                (antennaPortStandart.Hhpbw.HasValue && (antennaPortStandart.Hhpbw.Value <= 0 || antennaPortStandart.Hhpbw.Value > 360)) ||
                (antennaPortStandart.Vhpbw.HasValue && (antennaPortStandart.Vhpbw.Value <= 0 || antennaPortStandart.Vhpbw.Value > 360)))
            {
                MessageBox.Show("Gain должен быть >= 0, а HHPBW и VHPBW должны находиться в диапазоне (0; 360].");

                return;
            }

            // Блокируем интерфейс на время SQL-запроса
            grdAntennaMain.IsEnabled = false;
            dgAntennaPortStandarts.IsReadOnly = true;//блокируем редактирование грида

            //пробуем сохранить в БД
            try
            {
                if (_newAntennaPortStandart != null)
                {
                    await _antennaTypeService.AddAntennaPortStandartAsync(_newAntennaPortStandart);
                }
                else if (_editingAntennaPortStandart != null)
                {
                    await _antennaTypeService.UpdateAntennaPortStandartAsync(_editingAntennaPortStandart);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException?.Message ?? ex.Message;//если подробное описание ошибки есть, то ее занести в переменную, если нет, то общее
                MessageBox.Show($"Ошибка при сохранении порта:\n{errorMessage}");
                dgAntennaPortStandarts.IsReadOnly = false; //деблокируем редактирование грида
                UpdateUiState();//вызываем управления кнопками
                return;
            }
            finally
            {
                grdAntennaMain.IsEnabled = true;
            }
            // Обновляем грид после Update
            dgAntennaPortStandarts.Items.Refresh();

            // Очищаем ссылки
            _newAntennaPortStandart = null;
            _editingAntennaPortStandart = null;
            _backupAntennaPortStandart = null;

            // Возвращаем таблицы в режим просмотра
            dgAntennaPortStandarts.IsReadOnly = true;
            dgAntennaTypes.IsEnabled = true;
            dgAntennaPorts.IsEnabled = true;

            UpdateUiState();//вызываем управления кнопками


        }

        private async void btnStandartDelete_Click(object sender, RoutedEventArgs e)
        {
            // проверяем не добавляется или редактируется уже, если так то выходим из обработчика кнопки
            if (_newAntennaPortStandart != null || _editingAntennaPortStandart != null)
            {
                return;
            }

            if (dgAntennaPortStandarts.SelectedItem is not AntennaTypePortStandart antennaPortStandartSelected) //записывает ссылку в переменную antennaPortSelected при сооветствие типу
                                                                                        //в противном случае return 
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                                    "Удалить выбранный стандарт?",
                                    "Подтверждение удаления",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }
            //блокируем  пока идет запрос SQL
            grdAntennaMain.IsEnabled = false;

            try
            {
                await _antennaTypeService.DeleteAntennaPortStandartAsync(antennaPortStandartSelected.Id);

                _antennaPortStandarts.Remove(antennaPortStandartSelected);//удаляем также и из коллекции
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException?.Message ?? ex.Message;//если подробное описание ошибки есть, то ее занести в переменную, если нет, то общее
                MessageBox.Show($"Ошибка при удалении стадарта:\n{errorMessage}");
                return;
            }
            finally
            {
                grdAntennaMain.IsEnabled = true;
               
            }
            
                        
        }

        private async void btnStandartRefresh_Click(object sender, RoutedEventArgs e)
        {
            // проверяем не добавляется или редактируется уже, если так то выходим из обработчика кнопки
            if (IsEditing)
            {
                return;
            }

            //Получаем выбранный порт из dgAntennaPorts.SelectedItem
            if (dgAntennaPorts.SelectedItem is not AntennaTypePort antennaPortSelected) //записывает ссылку в переменную antennaPortSelected при сооветствие типу
                                                                                        //в противном случае return 
            {
                return;
            }

            //блокируем от двойного нажатия
            grdAntennaMain.IsEnabled = false;
            
            //фиксируем перед обновлением Id выбранного стандарта
            int? antennaPortStandartSelectedId = (dgAntennaPortStandarts.SelectedItem as AntennaTypePortStandart)?.Id;

            try
            {
                // загружаем данные из SQL Server коллекцию antennaPortStandarts
                List<AntennaTypePortStandart> antennaPortStandarts = await _antennaTypeService.GetAntennaPortStandartsAsync(antennaPortSelected.Id);

                // Проверяем, не изменилась ли выбранный порт
                if (!ReferenceEquals(dgAntennaPorts.SelectedItem, antennaPortSelected))
                {
                    return;
                }

                // очищаем коллекцию
                _antennaPortStandarts.Clear();

                // Заполняем новыми объектами коллекцию _antennaPortStandarts
                foreach (AntennaTypePortStandart standart in antennaPortStandarts)
                {
                    _antennaPortStandarts.Add(standart);
                }

                // Восстанавливаем выделение ранее выделенного стандарта
                AntennaTypePortStandart? antennaPortStandartSelected = _antennaPortStandarts.FirstOrDefault(p => p.Id == antennaPortStandartSelectedId);
                dgAntennaPortStandarts.SelectedItem = antennaPortStandartSelected;

                // и прокручиваем грид к выбранному стандарту
                if (antennaPortStandartSelected != null)
                {
                    dgAntennaPortStandarts.ScrollIntoView(antennaPortStandartSelected);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException?.Message ?? ex.Message;

                MessageBox.Show($"Ошибка при обновлении стандартов:\n{errorMessage}");
            }
            finally
            {
                grdAntennaMain.IsEnabled = true; 
                
            }
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //проверяем может что то добавляется или редактируется, тогда выход
            if (IsEditing)
            {
                return;
            }

            //Получаем выбранную антенну порт из dgAntenna.SelectedItem
            if (dgAntennaTypes.SelectedItem is not AntennaType antennaSelected)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Удалить антенну {antennaSelected.AntennaName}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // Блокируем взаимодействие на время SQL-запроса
            grdAntennaMain.IsEnabled = false;

            try
            {
                await _antennaTypeService.DeleteAntennaAsync(antennaSelected.Id);

                // Удаляем из коллекции только после успешного SQL DELETE
                _antennas.Remove(antennaSelected);
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException?.Message ?? ex.Message;

                MessageBox.Show($"Ошибка удаления антенны:\n{errorMessage}");
            }
            finally
            {
                //обратно разблокируем то что заблокировали
                grdAntennaMain.IsEnabled = true;
               
            }
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            //проверяем может что то добавляется или редактируется, тогда выход
            if (IsEditing)
            {
                return;
            }


            //блокируем на время выполнения
            grdAntennaMain.IsEnabled = false;
            

            try
            {
                // Загружаем новые данные из БД
                List<AntennaType> antennas = await _antennaTypeService.GetAntennasAsync();
                
                //запоминаем Id выбранной антенны до обновления грида
                short? antennaSelectedId = (dgAntennaTypes.SelectedItem as AntennaType)?.Id;

                // запрещаем обновление для dgAntennaTypes_SelectionChanged
                _isUpdatingAntennas = true;
                
                //Обновляем коллекцию и грид
                try
                {
                    _antennas.Clear();

                    foreach (AntennaType antenna in antennas)
                    {
                        _antennas.Add(antenna);
                    }

                    // Ищем выбранную антенну среди новых объектов
                    AntennaType? newAntennaSelected = _antennas.FirstOrDefault(a => a.Id == antennaSelectedId);

                    // Восстанавливаем выделение
                    dgAntennaTypes.SelectedItem = newAntennaSelected;

                    // Если антенна исчезла из БД, очищаем связанные данные
                    if (newAntennaSelected == null)
                    {
                        _antennaPorts.Clear();
                        _antennaPortStandarts.Clear();
                    }
                }
                finally
                {
                    _isUpdatingAntennas = false;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException?.Message ?? ex.Message;

                MessageBox.Show($"Ошибка обновления антенн:\n{errorMessage}");
            }
            finally
            {
                grdAntennaMain.IsEnabled = true;
            }
        }

        //метод для проверки выделенный объект соответствует редактируемому или нет false - соответствует true - не соответствует
        private bool KeepEditingSelection(DataGrid grid, object? editingItem, string itemName)
        {
            // Не обрабатываем события, возникшие при восстановлении
            if (_restoringSelection || _selectionRestorePending)
            {
                return true;
            }

            // Ничего не редактируется
            if (editingItem == null)
            {
                return false;
            }

            // Выбрана именно редактируемая строка
            if (ReferenceEquals(grid.SelectedItem, editingItem))
            {
                return false;
            }

            // Пользователь попытался выбрать другую строку
            _selectionRestorePending = true;
            
            //запланировали изменить строку на редактируемую
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    _restoringSelection = true;

                    // Восстанавливаем выделение редактируемой строки
                    if (grid.Items.Contains(editingItem))
                    {
                        grid.SelectedItem = editingItem;
                        grid.ScrollIntoView(editingItem);
                    }
                }
                finally
                {
                    _restoringSelection = false;
                    _selectionRestorePending = false;
                }

                MessageBox.Show(
                    $"Завершите редактирование {itemName}. Нажмите Save или Cancel.",
                    "Редактирование не завершено",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

            }), System.Windows.Threading.DispatcherPriority.Background);

            return true;
        }

    }
}
