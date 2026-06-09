using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using DeflexPro.Model;

namespace DeflexPro.ViewModel
{
    public class FwdDetailsViewModel : ViewModelBase
    {
        private Fwd fwdMachine = null; 
        private readonly FwdFileReaderFactory fileReaderFactory = new FwdFileReaderFactory();
        private RelayCommand loadFwdMachine; 
        private RelayCommand importFwdMachine;
        private RelayCommand exportFwdMachine;
        private RelayCommand saveFwdMachine;
        private RelayCommand selectAllDrops;
        private RelayCommand clearDropSelection;
        private string currentFileName;

        private ObservableCollection<DropDetailsViewModel> drops = new ObservableCollection<DropDetailsViewModel>();
        private ObservableCollection<DropDetailsViewModel> selectedDrops = new ObservableCollection<DropDetailsViewModel>();
        private ObservableCollection<SensorDetailsViewModel> sensors = new ObservableCollection<SensorDetailsViewModel>();
        private ObservableCollection<StationViewModel> stations = new ObservableCollection<StationViewModel>();
        private StationViewModel selectedStation;
        private PlotViewModel plot = new PlotViewModel();

        public double PlateRadius
        {
            get { return fwdMachine?.PlateRadius ?? 0d; }
            set
            {
                if (fwdMachine == null) return;
                if (fwdMachine.PlateRadius == value) return;
                fwdMachine.PlateRadius = value;
                RaisePropertyChanged("PlateRadius");
            }
        }

        public ObservableCollection<DropDetailsViewModel> Drops
        {
            get { return drops; }
            set
            {
                if (drops == value) return;
                drops = value;
                RaisePropertyChanged("Drops");
            }
        }

        public ObservableCollection<DropDetailsViewModel> SelectedDrops
        {
            get { return selectedDrops; }
            set
            {
                if (selectedDrops == value) return;
                selectedDrops = value;
                RaisePropertyChanged("SelectedDrops");
            }
        }

        public ObservableCollection<SensorDetailsViewModel> Sensors
        {
            get { return sensors; }
            set
            {
                if (sensors == value) return;
                sensors = value;
                RaisePropertyChanged("Sensors");
            }
        }

        public ObservableCollection<StationViewModel> Stations
        {
            get { return stations; }
            set
            {
                if (stations == value) return;
                stations = value;
                RaisePropertyChanged("Stations");
            }
        }

        public StationViewModel SelectedStation
        {
            get { return selectedStation; }
            set
            {
                if (selectedStation == value) return;
                selectedStation = value;
                getSelectedDrops();
                RaisePropertyChanged("SelectedStation");
            }
        }

        public PlotViewModel Plot
        {
            get { return plot; }
            set
            {
                if (plot == value) return;
                plot = value;
                RaisePropertyChanged("Plot");
            }
        }

        public RelayCommand LoadFwdMachine
        {
            get
            {
                if (loadFwdMachine == null)
                    loadFwdMachine = new RelayCommand(SelectAndLoadFwd);
                return loadFwdMachine;
            }
        }

        public RelayCommand ImportFwdMachine =>
            importFwdMachine ??= new RelayCommand(SelectAndLoadFwd);

        public RelayCommand ExportFwdMachine =>
            exportFwdMachine ??= new RelayCommand(ExportCsv, () => Drops.Count > 0);

        public RelayCommand SaveFwdMachine =>
            saveFwdMachine ??= new RelayCommand(SaveCsv, () => Drops.Count > 0);

        public RelayCommand SelectAllDrops =>
            selectAllDrops ??= new RelayCommand(() => SetDropSelection(true), () => SelectedDrops.Count > 0);

        public RelayCommand ClearDropSelection =>
            clearDropSelection ??= new RelayCommand(() => SetDropSelection(false), () => SelectedDrops.Count > 0);

        public string FormatName => fwdMachine?.FormatName ?? "Nincs fájl";
        public string FileName => string.IsNullOrWhiteSpace(currentFileName) ? "Nincs betöltött mérés" : Path.GetFileName(currentFileName);
        public int DropCount => Drops.Count;
        public int StationCount => Stations.Count;
        public int SensorCount => Sensors.Count;

        public FwdDetailsViewModel()
        { }

        public void SelectAndLoadFwd()
        {
            var dialog = new OpenFileDialog
            {
                Title = "FWD mérési fájl betöltése",
                Filter = "FWD mérési fájlok - KUAB és Primax (*.fwd)|*.fwd|Minden fájl (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
                LoadFwd(dialog.FileName);
        }

        public void LoadFwd(string fileName)
        {
            currentFileName = fileName;
            try
            {
                Populate(fileReaderFactory.Read(fileName));
            }
            catch (Exception exception)
            {
                ClearMeasurements();
                MessageBox.Show(
                    $"A fájl betöltése sikertelen.{Environment.NewLine}{exception.Message}",
                    "DeflexPro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ExportCsv()
        {
            var dialog = new SaveFileDialog
            {
                Title = "Mérési adatok exportálása",
                Filter = "CSV fájl (*.csv)|*.csv",
                FileName = Path.GetFileNameWithoutExtension(currentFileName) + ".csv"
            };

            if (dialog.ShowDialog() == true)
                WriteCsv(dialog.FileName);
        }

        private void SaveCsv()
        {
            var dialog = new SaveFileDialog
            {
                Title = "Mérési adatok mentése",
                Filter = "CSV fájl (*.csv)|*.csv",
                FileName = Path.GetFileNameWithoutExtension(currentFileName) + "-saved.csv"
            };

            if (dialog.ShowDialog() == true)
                WriteCsv(dialog.FileName);
        }

        private void WriteCsv(string fileName)
        {
            var csv = new StringBuilder("Distance;Impact;PeakForce;AirTemperature;AsphaltTemperature;DateTime");
            csv.AppendLine();

            foreach (var drop in Drops)
            {
                csv.Append(drop.Distance.ToString(CultureInfo.InvariantCulture)).Append(';')
                   .Append(drop.ImpNumber.ToString(CultureInfo.InvariantCulture)).Append(';')
                   .Append(drop.PeakForce.ToString(CultureInfo.InvariantCulture)).Append(';')
                   .Append(drop.AirTemperature.ToString(CultureInfo.InvariantCulture)).Append(';')
                   .Append(drop.AsphaltTemperature.ToString(CultureInfo.InvariantCulture)).Append(';')
                   .AppendLine(drop.DateTime.ToString("O", CultureInfo.InvariantCulture));
            }

            File.WriteAllText(fileName, csv.ToString(), Encoding.UTF8);
        }


        void getSelectedDrops()
        {
            foreach (var drop in SelectedDrops)
                drop.IsSelected = false;

            SelectedDrops.Clear();
            if (SelectedStation == null)
            {
                Plot.FillData(SelectedDrops);
                ExportFwdMachine.NotifyCanExecuteChanged();
                SaveFwdMachine.NotifyCanExecuteChanged();
                return;
            }

            foreach (var item in Drops)
            {
                if (item.Distance == SelectedStation.Value)
                {
                    SelectedDrops.Add(item);
                    item.IsSelected = true;
                }
            }
            SelectAllDrops.NotifyCanExecuteChanged();
            ClearDropSelection.NotifyCanExecuteChanged();
            UpdatePlotSelection();
        }

        private void SetDropSelection(bool isSelected)
        {
            foreach (var drop in SelectedDrops)
                drop.IsSelected = isSelected;
        }

        private void DropSelectionChanged(object? sender, EventArgs e)
        {
            UpdatePlotSelection();
        }

        private void UpdatePlotSelection()
        {
            var plottedDrops = new ObservableCollection<DropDetailsViewModel>(
                SelectedDrops.Where(drop => drop.IsSelected));
            Plot.FillData(plottedDrops);
        }

        private void ClearMeasurements()
        {
            fwdMachine = null;
            Drops.Clear();
            SelectedDrops.Clear();
            Sensors.Clear();
            Stations.Clear();
            Plot.FillData(SelectedDrops);
            ExportFwdMachine.NotifyCanExecuteChanged();
            SaveFwdMachine.NotifyCanExecuteChanged();
            RaisePropertyChanged(nameof(FormatName));
            RaisePropertyChanged(nameof(FileName));
            RaisePropertyChanged(nameof(PlateRadius));
            RaisePropertyChanged(nameof(DropCount));
            RaisePropertyChanged(nameof(StationCount));
            RaisePropertyChanged(nameof(SensorCount));

            if (Stations.Count > 0)
                SelectedStation = Stations[0];
        }

        private void Populate(Fwd measurement)
        {
            Drops.Clear();
            SelectedDrops.Clear();
            Sensors.Clear();
            Stations.Clear();
            fwdMachine = measurement;
            RaisePropertyChanged(nameof(FormatName));
            RaisePropertyChanged(nameof(FileName));
            RaisePropertyChanged(nameof(PlateRadius));

            foreach (Sensor sensor in fwdMachine.Sensors)
                Sensors.Add(new SensorDetailsViewModel(sensor));

            var stationValues = new HashSet<double>();
            foreach (Drop drop in fwdMachine.Drops)
            {
                var dropViewModel = new DropDetailsViewModel(drop);
                dropViewModel.SelectionChanged += DropSelectionChanged;
                if (stationValues.Add(dropViewModel.Distance))
                    Stations.Add(new StationViewModel(dropViewModel.Distance));
                Drops.Add(dropViewModel);
            }

            ExportFwdMachine.NotifyCanExecuteChanged();
            SaveFwdMachine.NotifyCanExecuteChanged();
            RaisePropertyChanged(nameof(DropCount));
            RaisePropertyChanged(nameof(StationCount));
            RaisePropertyChanged(nameof(SensorCount));
        }
    }
}
