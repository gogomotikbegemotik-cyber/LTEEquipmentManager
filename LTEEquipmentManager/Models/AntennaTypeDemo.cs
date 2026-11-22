using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace LTEEquipmentManager.Models
{
    public class AntennaTypeDemo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;//Это событие будет сообщать WPF, какое именно свойство изменилось.
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);

            return true;
        }
        private int _id;
        private string _manufacturer = "";
        private string _model = "";
        private int _height;
        // --- Красивые и лаконичные свойства для WPF ---
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Manufacturer
        {
            get => _manufacturer;
            set => SetProperty(ref _manufacturer, value);
        }

        public string Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }

        public int Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }
    }

}
