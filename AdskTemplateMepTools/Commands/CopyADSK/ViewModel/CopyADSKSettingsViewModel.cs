using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using AdskTemplateMepTools.Annotations;
using AdskTemplateMepTools.Commands.CopyADSK.Commands;
using AdskTemplateMepTools.Commands.CopyADSK.View;
using Microsoft.Win32;

namespace AdskTemplateMepTools.Commands.CopyADSK.ViewModel
{
    public class CopyAdskSettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly CopyAdskSettings _model;
        private RelayCommand _addSchedulesOnList;
        private RelayCommand _deleteSchedules;
        private RelayCommand _loadSchedules;
        private RelayCommand _saveSchedules;
        private string _schedulesPath;

        public CopyAdskSettingsViewModel(CopyAdskSettings model)
        {
            _model = model;
            Schedules = new ObservableCollection<Schedule>();

            Configuration.TryReadKey(nameof(CopyAdsk), "Путь к файлу настроек", out var path);
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Schedules = CopyAdskSettings.CreateDefaultSchedules(SchedulesConfigPath);
                Configuration.WriteKey(nameof(CopyAdsk), "Путь к файлу настроек", SchedulesConfigPath);
            }
            else
            {
                SchedulesConfigPath = path;
                Schedules = CopyAdskSettings.LoadSchedules(SchedulesConfigPath);
            }
        }

        public ObservableCollection<Schedule> Schedules { get; }

        public string SchedulesConfigPath
        {
            get => string.IsNullOrEmpty(_schedulesPath)
                ? Path.Combine(Configuration.GetConfigurationDirectory(), "CopyADSKSettings.json")
                : _schedulesPath;
            set
            {
                _schedulesPath = value;
                Configuration.WriteKey(nameof(CopyAdsk), "Путь к файлу настроек", value);
                OnPropertyChanged(nameof(SchedulesConfigPath));
            }
        }

        public RelayCommand LoadSchedules
        {
            get
            {
                return _loadSchedules ??= new RelayCommand(_ =>
                {
                    var openFileDialog = new OpenFileDialog
                    {
                        InitialDirectory = Path.GetDirectoryName(SchedulesConfigPath) ?? string.Empty,
                        Filter = "Json files (*.json)|*.json"
                    };
                    if (openFileDialog.ShowDialog() != true) return;
                    SchedulesConfigPath = openFileDialog.FileName;
                    Schedules.Clear();
                    foreach (var schedule in CopyAdskSettings.LoadSchedules(SchedulesConfigPath)) Schedules.Add(schedule);
                });
            }
        }

        public RelayCommand SaveSchedules
        {
            get
            {
                return _saveSchedules ??= new RelayCommand(_ =>
                {
                    var saveFileDialog = new SaveFileDialog
                    {
                        FileName = Path.GetFileName(SchedulesConfigPath),
                        InitialDirectory = Path.GetDirectoryName(SchedulesConfigPath) ?? string.Empty,
                        Filter = "Json files (*.json)|*.json"
                    };
                    if (saveFileDialog.ShowDialog() != true) return;
                    SchedulesConfigPath = saveFileDialog.FileName;
                    CopyAdskSettings.SaveSchedules(SchedulesConfigPath, Schedules);
                });
            }
        }

        public RelayCommand AddSchedulesOnList
        {
            get
            {
                return _addSchedulesOnList ??= new RelayCommand(_ =>
                {
                    var projectSchedules = GetProjectSchedulesByMask(Schedules.Select(n=>n.Name).ToList());
                    var schedulePicker = new CopyAdskPickSchedulesView(projectSchedules);
                    schedulePicker.ShowDialog();
                    var selectedItems = schedulePicker.ListBoxSpecificationPicker
                        .SelectedItems
                        .Cast<string>()
                        .ToList();
                    foreach (var item in selectedItems) Schedules.Add(new Schedule(item));
                });
            }
        }

        public RelayCommand DeleteSchedulesFromList
        {
            get
            {
                return _deleteSchedules ??= new RelayCommand(obj =>
                {
                    var list = ((IList) obj)
                        .Cast<Schedule>()
                        .ToList();
                    foreach (var item in list) Schedules.Remove(item);
                });
            }
        }
        private List<string> GetProjectSchedulesByMask(ICollection<string> schedules)
        {
            return _model.GetProjectSchedules()
                .Where(s => !schedules.Contains(s))
                .OrderBy(s => s)
                .ToList();
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}