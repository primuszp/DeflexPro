using DeflexPro.Model;

namespace DeflexPro.ViewModel
{
    public class PavementLayerViewModel : ViewModelBase
    {
        private readonly PavementLayer _layer;

        public PavementLayerViewModel(PavementLayer layer)
        {
            _layer = layer;
        }

        public string MaterialName
        {
            get => _layer.MaterialName;
            set { _layer.MaterialName = value; RaisePropertyChanged(); }
        }

        public double Thickness
        {
            get => _layer.Thickness;
            set { _layer.Thickness = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(ThicknessDisplay)); }
        }

        public string ThicknessDisplay => _layer.IsHalfspace ? "∞" : $"{_layer.Thickness:0} mm";

        public double SeedModulus
        {
            get => _layer.SeedModulus;
            set { _layer.SeedModulus = value; RaisePropertyChanged(); }
        }

        public double MinModulus
        {
            get => _layer.MinModulus;
            set { _layer.MinModulus = value; RaisePropertyChanged(); }
        }

        public double MaxModulus
        {
            get => _layer.MaxModulus;
            set { _layer.MaxModulus = value; RaisePropertyChanged(); }
        }

        public bool IsFixed
        {
            get => _layer.IsFixed;
            set { _layer.IsFixed = value; RaisePropertyChanged(); }
        }

        public bool IsHalfspace
        {
            get => _layer.IsHalfspace;
            set { _layer.IsHalfspace = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(ThicknessDisplay)); }
        }

        public PavementLayer Model => _layer;

        public static PavementLayerViewModel CreateLayer(string material, double thickness, double seed) =>
            new(new PavementLayer { MaterialName = material, Thickness = thickness, SeedModulus = seed });

        public static PavementLayerViewModel CreateHalfspace(double seed) =>
            new(new PavementLayer { MaterialName = "Félvégtér (altalaj)", IsHalfspace = true, SeedModulus = seed, MinModulus = 20, MaxModulus = 1000 });
    }
}
