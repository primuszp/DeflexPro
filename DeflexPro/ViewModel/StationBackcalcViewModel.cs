using System.Collections.ObjectModel;
using System.Linq;
using DeflexPro.Model;

namespace DeflexPro.ViewModel
{
    public class StationBackcalcViewModel : ViewModelBase
    {
        private bool _isSelected;
        private bool _hasResult;
        private string _groupName = string.Empty;
        private BackcalcResult? _result;

        public double Distance { get; }
        public string DisplayName => $"{Distance / 1000:0.000} km  ({Distance:0} m)";
        public string ShortName => $"{Distance:0} m";

        public ObservableCollection<PavementLayerViewModel> Layers { get; } = new();
        public IReadOnlyList<DropDetailsViewModel> Drops { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; RaisePropertyChanged(); }
        }

        public bool HasResult
        {
            get => _hasResult;
            set { _hasResult = value; RaisePropertyChanged(); }
        }

        public string GroupName
        {
            get => _groupName;
            set { _groupName = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(HasGroup)); }
        }

        public bool HasGroup => !string.IsNullOrWhiteSpace(_groupName);

        public BackcalcResult? Result
        {
            get => _result;
            set
            {
                _result = value;
                HasResult = value != null;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ResultSummary));
                RaisePropertyChanged(nameof(RmseDisplay));
            }
        }

        public string ResultSummary
        {
            get
            {
                if (_result == null) return "–";
                var moduli = string.Join(" / ", _result.LayerModuli.Select(m => $"{m:0}"));
                return $"[{moduli}] MPa";
            }
        }

        public string RmseDisplay => _result == null ? "–" : $"{_result.RMSE:0.00}%";

        public StationBackcalcViewModel(double distance, IReadOnlyList<DropDetailsViewModel>? drops = null)
        {
            Distance = distance;
            Drops = drops ?? [];
            InitDefaultLayers();
        }

        private void InitDefaultLayers()
        {
            Layers.Add(PavementLayerViewModel.CreateLayer("AC", 150, 3000));
            Layers.Add(PavementLayerViewModel.CreateLayer("CTB", 200, 800));
            Layers.Add(PavementLayerViewModel.CreateLayer("SZK", 300, 200));
            Layers.Add(PavementLayerViewModel.CreateHalfspace(80));
        }

        public void ApplyLayersFrom(StationBackcalcViewModel source)
        {
            Layers.Clear();
            foreach (var l in source.Layers)
            {
                var clone = new PavementLayerViewModel(l.Model.Clone());
                Layers.Add(clone);
            }
        }
    }
}
