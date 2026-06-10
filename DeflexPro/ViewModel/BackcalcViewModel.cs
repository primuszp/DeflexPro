using System;
using System.Collections.ObjectModel;
using System.Linq;
using DeflexPro.Localization;
using DeflexPro.OpenPave;

namespace DeflexPro.ViewModel
{
    public enum BackcalcSubPage { LayerEditor, RunBackcalc, Results }

    public class BackcalcViewModel : ViewModelBase
    {
        private const int MaximumBackcalcIterations = 50;
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
        private string _progressStatus = string.Empty;
        private int _progressCompleted;
        private int _progressTotal;
        private int _overallCompleted;
        private int _overallTotal;
        private string _currentStationName = string.Empty;
        private double _lastRmse;
        private int _selectedMethodIndex;
        private readonly OpenPaveService _openPave = new();

        public ObservableCollection<StationBackcalcViewModel> Stations { get; } = new();
        public ObservableCollection<StationBackcalcViewModel> ResultRows { get; } = new();

        public string[] Methods { get; } = ["OpenPave native LE backcalculation"];

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

        public string ProgressStatus
        {
            get => _progressStatus;
            set { _progressStatus = value; RaisePropertyChanged(); }
        }

        public int ProgressCompleted
        {
            get => _progressCompleted;
            set
            {
                _progressCompleted = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ProgressPercent));
                RaisePropertyChanged(nameof(ProgressCountDisplay));
            }
        }

        public int ProgressTotal
        {
            get => _progressTotal;
            set
            {
                _progressTotal = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ProgressPercent));
                RaisePropertyChanged(nameof(ProgressCountDisplay));
            }
        }

        public double ProgressPercent => ProgressTotal == 0 ? 0 : ProgressCompleted * 100.0 / ProgressTotal;
        public string ProgressCountDisplay => $"{ProgressCompleted} / {ProgressTotal}";

        public int OverallCompleted
        {
            get => _overallCompleted;
            set { _overallCompleted = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(OverallPercent)); RaisePropertyChanged(nameof(OverallCountDisplay)); }
        }

        public int OverallTotal
        {
            get => _overallTotal;
            set { _overallTotal = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(OverallPercent)); RaisePropertyChanged(nameof(OverallCountDisplay)); }
        }

        public double OverallPercent => OverallTotal == 0 ? 0 : OverallCompleted * 100.0 / OverallTotal;
        public string OverallCountDisplay => $"{OverallCompleted} / {OverallTotal}";

        public string CurrentStationName
        {
            get => _currentStationName;
            set { _currentStationName = value; RaisePropertyChanged(); }
        }

        public double LastRmse
        {
            get => _lastRmse;
            set { _lastRmse = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(LastRmseDisplay)); }
        }

        public string LastRmseDisplay => _lastRmse > 0 ? $"{_lastRmse:0.000} %" : "–";

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
            () => { foreach (var s in Stations) s.IsSelected = true; },
            () => Stations.Count > 0);

        public RelayCommand ClearStationSelection => _clearStationSelection ??= new RelayCommand(
            () => { foreach (var s in Stations) s.IsSelected = false; },
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

        public void LoadStations(System.Collections.Generic.IEnumerable<DropDetailsViewModel> drops)
        {
            Stations.Clear();
            ResultRows.Clear();
            ProgressCompleted = 0;
            ProgressTotal = 0;
            ProgressStatus = string.Empty;
            ProgressLog = string.Empty;
            foreach (var group in drops.GroupBy(d => d.Distance))
            {
                var station = new StationBackcalcViewModel(group.Key, group.ToArray());
                station.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName != nameof(StationBackcalcViewModel.IsSelected)) return;
                    RaisePropertyChanged(nameof(SelectedCount));
                    RunCommand.NotifyCanExecuteChanged();
                    AssignGroupCommand.NotifyCanExecuteChanged();
                };
                Stations.Add(station);
            }
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

        private async void RunBackcalc()
        {
            IsRunning = true;
            ProgressLog = Localizer.Get("BackcalcStarting", "Starting backcalculation...") + "\n";
            ResultRows.Clear();

            var selected = Stations.Where(s => s.IsSelected).ToList();
            ProgressCompleted = 0;
            ProgressTotal = MaximumBackcalcIterations;
            OverallCompleted = 0;
            OverallTotal = selected.Count;
            LastRmse = 0;
            CurrentStationName = string.Empty;
            ProgressStatus = Localizer.Get("BackcalcStarting", "Starting backcalculation...");

            try
            {
                foreach (var station in selected)
                {
                    ProgressCompleted = 0;
                    CurrentStationName = station.ShortName;
                    ProgressStatus = string.Format(
                        Localizer.Get("ProcessingStationFormat", "Processing station {0}..."),
                        station.ShortName).Trim();
                    AppendLog(string.Format(
                        Localizer.Get("ProcessingStationFormat", "  Processing station {0}..."),
                        station.ShortName));
                    try
                    {
                        var iterationProgress = new Progress<OpenPaveBackcalculationIteration>(iteration =>
                        {
                            ProgressCompleted = iteration.Iteration;
                            LastRmse = iteration.RmsePercent;
                            ProgressStatus = string.Format(
                                Localizer.Get("BackcalcIterationStatusFormat", "{0} – iter {1}/{2}, RMSE {3:0.000}%"),
                                station.ShortName, iteration.Iteration, MaximumBackcalcIterations, iteration.RmsePercent);
                            AppendLog(string.Format(
                                Localizer.Get("BackcalcIterationLogFormat", "    iter {0:00}: RMSE {1:0.000}% | E = [{2}] MPa"),
                                iteration.Iteration,
                                iteration.RmsePercent,
                                string.Join(", ", iteration.Moduli.Select(x => $"{x:0.0}"))));
                        });
                        var result = await System.Threading.Tasks.Task.Run(() => CalculateStation(station, iterationProgress));
                        station.Result = result;
                        ResultRows.Add(station);
                        LastRmse = result.RMSE;
                        AppendLog($"    ✓ RMSE: {result.RMSE:0.00}%");
                    }
                    catch (Exception exception)
                    {
                        station.Result = null;
                        AppendLog($"    ✗ ERROR: {exception.Message}");
                    }
                    OverallCompleted++;
                }

                ProgressCompleted = ProgressTotal;
                CurrentStationName = string.Empty;
                ProgressStatus = string.Format(
                    Localizer.Get("ProcessingCompleteFormat", "Done. {0} stations processed."),
                    selected.Count);
                AppendLog(string.Format(
                    Localizer.Get("ProcessingCompleteFormat", "Done. {0} stations processed."),
                    selected.Count));
                RaisePropertyChanged(nameof(ResultCount));
                GoResults.NotifyCanExecuteChanged();
            }
            finally
            {
                IsRunning = false;
            }
        }

        private Model.BackcalcResult CalculateStation(
            StationBackcalcViewModel station,
            IProgress<OpenPaveBackcalculationIteration> progress)
        {
            var drop = station.Drops.FirstOrDefault(d => d.IsSelected) ?? station.Drops.FirstOrDefault()
                ?? throw new InvalidOperationException("No measurement drop is available for the station.");
            var measured = drop.Deflections
                .Where(d => d.Sensor.X >= 0 && d.Sensor.Y == 0 && double.IsFinite(d.Measure) && d.Measure > 0)
                .OrderBy(d => d.Sensor.X)
                .Select(d => (d.Sensor.X, MechanicalUnits.MicrometersToMillimeters(d.Measure)))
                .ToArray();
            var layers = station.Layers.Select(l => new OpenPaveLayer(
                l.IsHalfspace ? 0 : l.Thickness,
                l.SeedModulus)).ToArray();
            var load = new OpenPaveLoad(
                0, 0, MechanicalUnits.CentiKilonewtonsToNewtons(drop.PeakForce), 0, PlateRadius);
            var solved = _openPave.Backcalculate(
                layers, load, measured, maximumIterations: MaximumBackcalcIterations, progress: progress);
            var moduli = solved.Moduli.ToArray();

            return new Model.BackcalcResult
            {
                StationDistance = station.Distance,
                DropNumber = drop.ImpNumber,
                LayerModuli = moduli.Take(moduli.Length - 1).ToArray(),
                SubgradeModulus = moduli[^1],
                RMSE = solved.RmsePercent,
                SCI = drop.SCI ?? 0,
                BCI = drop.BCI ?? 0,
                BDI = drop.BDI ?? 0
            };
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

            var sb = new System.Text.StringBuilder("Station_m;E1_MPa;E2_MPa;E3_MPa;SubgradeE_MPa;RMSE_pct;SCI_um;BDI_um;BCI_um\n");
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
