using System;
using System.Collections.ObjectModel;
using System.Linq;
using DeflexPro.Localization;

namespace DeflexPro.ViewModel
{
    public enum BackcalcSubPage { LayerEditor, RunBackcalc, Results }

    public class BackcalcViewModel : ViewModelBase
    {
        private BackcalcSubPage _currentSubPage = BackcalcSubPage.LayerEditor;
        private StationBackcalcViewModel? _selectedStation;
        private string _applyRangeFrom = string.Empty;
        private string _applyRangeTo = string.Empty;
        private bool _applyToRange;
        private bool _isRunning;
        private string _groupNameInput = string.Empty;
        private double _plateRadius = 150;
        private bool _useGroupMode;
        private string _progressLog = string.Empty;
        private int _selectedMethodIndex;

        public ObservableCollection<StationBackcalcViewModel> Stations { get; } = new();
        public ObservableCollection<StationBackcalcViewModel> ResultRows { get; } = new();

        public string[] Methods { get; } =
        [
            Localizer.Get("MethodBisect", "BISECT iteration"),
            "Simplex (Nelder-Mead)",
            Localizer.Get("MethodHybrid", "Hybrid method")
        ];

        public BackcalcSubPage CurrentSubPage
        {
            get => _currentSubPage;
            set { _currentSubPage = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(IsLayerEditorPage)); RaisePropertyChanged(nameof(IsRunPage)); RaisePropertyChanged(nameof(IsResultsPage)); }
        }

        public bool IsLayerEditorPage => _currentSubPage == BackcalcSubPage.LayerEditor;
        public bool IsRunPage => _currentSubPage == BackcalcSubPage.RunBackcalc;
        public bool IsResultsPage => _currentSubPage == BackcalcSubPage.Results;

        public StationBackcalcViewModel? SelectedStation
        {
            get => _selectedStation;
            set { _selectedStation = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(SelectedStationLabel)); }
        }

        public string SelectedStationLabel => SelectedStation == null
            ? Localizer.Get("NoStationSelected", "No station selected")
            : string.Format(Localizer.Get("SelectedStationFormat", "Selected station: {0}"), SelectedStation.ShortName);

        public bool ApplyToRange
        {
            get => _applyToRange;
            set { _applyToRange = value; RaisePropertyChanged(); }
        }

        public string ApplyRangeFrom
        {
            get => _applyRangeFrom;
            set { _applyRangeFrom = value; RaisePropertyChanged(); }
        }

        public string ApplyRangeTo
        {
            get => _applyRangeTo;
            set { _applyRangeTo = value; RaisePropertyChanged(); }
        }

        public bool IsRunning
        {
            get => _isRunning;
            set { _isRunning = value; RaisePropertyChanged(); RunCommand.NotifyCanExecuteChanged(); }
        }

        public string GroupNameInput
        {
            get => _groupNameInput;
            set { _groupNameInput = value; RaisePropertyChanged(); }
        }

        public double PlateRadius
        {
            get => _plateRadius;
            set { _plateRadius = value; RaisePropertyChanged(); }
        }

        public bool UseGroupMode
        {
            get => _useGroupMode;
            set { _useGroupMode = value; RaisePropertyChanged(); }
        }

        public string ProgressLog
        {
            get => _progressLog;
            set { _progressLog = value; RaisePropertyChanged(); }
        }

        public int SelectedMethodIndex
        {
            get => _selectedMethodIndex;
            set { _selectedMethodIndex = value; RaisePropertyChanged(); }
        }

        public int SelectedCount => Stations.Count(s => s.IsSelected);
        public int ResultCount => ResultRows.Count;

        private RelayCommand? _goLayerEditor;
        private RelayCommand? _goRun;
        private RelayCommand? _goResults;
        private RelayCommand? _selectAllStations;
        private RelayCommand? _clearStationSelection;
        private RelayCommand? _applyLayersCommand;
        private RelayCommand? _addLayerCommand;
        private RelayCommand? _removeLayerCommand;
        private RelayCommand? _runCommand;
        private RelayCommand? _assignGroupCommand;
        private RelayCommand? _clearGroupCommand;
        private RelayCommand? _exportResultsCommand;

        public RelayCommand GoLayerEditor => _goLayerEditor ??= new RelayCommand(() => CurrentSubPage = BackcalcSubPage.LayerEditor);
        public RelayCommand GoRun => _goRun ??= new RelayCommand(() => CurrentSubPage = BackcalcSubPage.RunBackcalc);
        public RelayCommand GoResults => _goResults ??= new RelayCommand(() => CurrentSubPage = BackcalcSubPage.Results, () => ResultRows.Count > 0);

        public RelayCommand SelectAllStations => _selectAllStations ??= new RelayCommand(
            () => { foreach (var s in Stations) s.IsSelected = true; RaisePropertyChanged(nameof(SelectedCount)); },
            () => Stations.Count > 0);

        public RelayCommand ClearStationSelection => _clearStationSelection ??= new RelayCommand(
            () => { foreach (var s in Stations) s.IsSelected = false; RaisePropertyChanged(nameof(SelectedCount)); },
            () => Stations.Count > 0);

        public RelayCommand ApplyLayersCommand => _applyLayersCommand ??= new RelayCommand(ApplyLayers, () => SelectedStation != null);

        public RelayCommand AddLayerCommand => _addLayerCommand ??= new RelayCommand(
            () => SelectedStation?.Layers.Insert(SelectedStation.Layers.Count - 1,
                PavementLayerViewModel.CreateLayer("SZK", 200, 200)),
            () => SelectedStation != null);

        public RelayCommand RemoveLayerCommand => _removeLayerCommand ??= new RelayCommand(
            () =>
            {
                if (SelectedStation == null || SelectedStation.Layers.Count <= 2) return;
                var nonHalf = SelectedStation.Layers.LastOrDefault(l => !l.IsHalfspace);
                if (nonHalf != null) SelectedStation.Layers.Remove(nonHalf);
            },
            () => SelectedStation?.Layers.Count > 2);

        public RelayCommand RunCommand => _runCommand ??= new RelayCommand(RunBackcalc, () => !IsRunning && Stations.Any(s => s.IsSelected));

        public RelayCommand AssignGroupCommand => _assignGroupCommand ??= new RelayCommand(AssignGroup,
            () => !string.IsNullOrWhiteSpace(GroupNameInput) && Stations.Any(s => s.IsSelected));

        public RelayCommand ClearGroupCommand => _clearGroupCommand ??= new RelayCommand(
            () => { foreach (var s in Stations.Where(s => s.IsSelected)) s.GroupName = string.Empty; });

        public RelayCommand ExportResultsCommand => _exportResultsCommand ??= new RelayCommand(
            ExportResults, () => ResultRows.Count > 0);

        public void LoadStations(System.Collections.Generic.IEnumerable<double> distances)
        {
            Stations.Clear();
            ResultRows.Clear();
            foreach (var d in distances)
                Stations.Add(new StationBackcalcViewModel(d));
            if (Stations.Count > 0) SelectedStation = Stations[0];
            RaisePropertyChanged(nameof(SelectedCount));
        }

        private void ApplyLayers()
        {
            if (SelectedStation == null) return;

            var targets = ApplyToRange
                ? GetRangeStations()
                : Stations.Where(s => s.IsSelected && s != SelectedStation);

            foreach (var t in targets)
                t.ApplyLayersFrom(SelectedStation);
        }

        private System.Collections.Generic.IEnumerable<StationBackcalcViewModel> GetRangeStations()
        {
            if (!double.TryParse(ApplyRangeFrom, out var from) || !double.TryParse(ApplyRangeTo, out var to))
                return Enumerable.Empty<StationBackcalcViewModel>();
            return Stations.Where(s => s.Distance >= from && s.Distance <= to && s != SelectedStation);
        }

        private void AssignGroup()
        {
            foreach (var s in Stations.Where(s => s.IsSelected))
                s.GroupName = GroupNameInput;
        }

        private void RunBackcalc()
        {
            IsRunning = true;
            ProgressLog = Localizer.Get("BackcalcStarting", "Starting backcalculation...") + "\n";
            ResultRows.Clear();

            // Placeholder – real backcalculation would be async with actual algorithm
            var selected = UseGroupMode
                ? Stations.Where(s => s.IsSelected).ToList()
                : Stations.Where(s => s.IsSelected).ToList();

            foreach (var station in selected)
            {
                AppendLog(string.Format(
                    Localizer.Get("ProcessingStationFormat", "  Processing station {0}..."),
                    station.ShortName));
                var result = new Model.BackcalcResult
                {
                    StationDistance = station.Distance,
                    DropNumber = 1,
                    LayerModuli = station.Layers.Where(l => !l.IsHalfspace).Select(l => l.SeedModulus).ToArray(),
                    SubgradeModulus = station.Layers.LastOrDefault()?.SeedModulus ?? 80,
                    RMSE = 0,
                    SCI = 0, BCI = 0, BDI = 0
                };
                station.Result = result;
                ResultRows.Add(station);
            }

            AppendLog(string.Format(
                Localizer.Get("ProcessingCompleteFormat", "Done. {0} stations processed."),
                selected.Count));
            IsRunning = false;
            RaisePropertyChanged(nameof(ResultCount));
            GoResults.NotifyCanExecuteChanged();
            CurrentSubPage = BackcalcSubPage.Results;
        }

        private void AppendLog(string line) => ProgressLog += line + "\n";

        private void ExportResults()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Title = Localizer.Get("ExportBackcalcDialogTitle", "Export backcalculation results"),
                Filter = Localizer.Get("CsvFileFilter", "CSV file (*.csv)|*.csv"),
                FileName = "backcalc_results.csv"
            };
            if (dlg.ShowDialog() != true) return;

            var sb = new System.Text.StringBuilder("Station;E1_MPa;E2_MPa;E3_MPa;SubgradeE_MPa;RMSE_pct;SCI;BDI;BCI\n");
            foreach (var r in ResultRows)
            {
                if (r.Result == null) continue;
                var res = r.Result;
                sb.Append(r.Distance).Append(';');
                sb.Append(string.Join(";", res.LayerModuli)).Append(';');
                sb.Append(res.SubgradeModulus).Append(';');
                sb.Append(res.RMSE.ToString("0.00")).Append(';');
                sb.Append(res.SCI).Append(';').Append(res.BDI).Append(';').AppendLine(res.BCI.ToString());
            }
            System.IO.File.WriteAllText(dlg.FileName, sb.ToString(), System.Text.Encoding.UTF8);
        }
    }
}
