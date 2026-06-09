using System;
using System.Collections.Generic;
using System.Linq;
using DeflexPro.Model;
using DeflexPro.Localization;

namespace DeflexPro.ViewModel
{
    public class DropDetailsViewModel : ViewModelBase
    {
        private readonly Drop drop;
        private bool isSelected;

        public DropDetailsViewModel(Drop drop)
        {
            this.drop = drop;
        }

        public event EventHandler? SelectionChanged;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected == value) return;
                isSelected = value;
                RaisePropertyChanged();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int ImpNumber 
        {
            get { return drop.ImpNumber; }
            set
            {
                if (drop.ImpNumber == value) return;
                drop.ImpNumber = value;
                RaisePropertyChanged("ImpNumber");
            }
        }

        public string DropLabel => string.Format(
            Localizer.Get("DropNumberFormat", "Drop #{0}"),
            ImpNumber);

        public DateTime DateTime 
        {
            get { return drop.DateTime; }
            set
            {
                if (drop.DateTime == value) return;
                drop.DateTime = value;
                RaisePropertyChanged("DateTime");
            }
        }

        public double Distance
        {
            get { return drop.Distance; }
            set
            {
                if (drop.Distance == value) return;
                drop.Distance = value;
                RaisePropertyChanged("Distance");
            }
        }

        public double PeakForce
        {
            get { return drop.PeakForce; }
            set
            {
                if (drop.PeakForce == value) return;
                drop.PeakForce = value;
                RaisePropertyChanged("PeakForce");
            }
        }

        public double AirTemperature
        {
            get { return drop.AirTemperature; }
            set
            {
                if (drop.AirTemperature == value) return;
                drop.AirTemperature = value;
                RaisePropertyChanged("AirTemperature");
            }
        }

        public double AsphaltTemperature
        {
            get { return drop.AsphaltTemperature; }
            set
            {
                if (drop.AsphaltTemperature == value) return;
                drop.AsphaltTemperature = value;
                RaisePropertyChanged("AsphaltTemperature");
            }
        }

        public List<Deflection> Deflections
        {
            get { return drop.Deflections; }
            set
            {
                if (drop.Deflections == value) return;
                drop.Deflections = value;
                RaisePropertyChanged("Deflections");
            }
        }

        // Deflection at sensor nearest to given radial offset (mm)
        private double? DeflAt(double offsetMm)
        {
            if (drop.Deflections == null || drop.Deflections.Count == 0) return null;
            var ordered = drop.Deflections
                .Where(d => d.Sensor.X >= 0 && !double.IsNaN(d.Measure) && d.Measure != 0)
                .OrderBy(d => Math.Abs(d.Sensor.X - offsetMm))
                .FirstOrDefault();
            return ordered?.Sensor.X is double x && Math.Abs(x - offsetMm) < 80 ? ordered.Measure : null;
        }

        public double? D0 => DeflAt(0);
        public double? D300 => DeflAt(300);
        public double? D600 => DeflAt(600);
        public double? D900 => DeflAt(900);

        // Standard deflection basin indices (μm)
        public double? SCI => (D0.HasValue && D300.HasValue) ? D0 - D300 : null;
        public double? BDI => (D300.HasValue && D600.HasValue) ? D300 - D600 : null;
        public double? BCI => (D600.HasValue && D900.HasValue) ? D600 - D900 : null;

        public string SCIDisplay => SCI.HasValue ? $"{SCI.Value:0.0} μm" : "–";
        public string BDIDisplay => BDI.HasValue ? $"{BDI.Value:0.0} μm" : "–";
        public string BCIDisplay => BCI.HasValue ? $"{BCI.Value:0.0} μm" : "–";
        public string D0Display  => D0.HasValue  ? $"{D0.Value:0.0} μm"  : "–";

        // All sensor deflections as display-ready list
        public List<DeflectionDisplayItem> DeflectionRows =>
            drop.Deflections?
                .Where(d => !double.IsNaN(d.Measure))
                .OrderBy(d => d.Sensor.X)
                .Select(d => new DeflectionDisplayItem(d.Sensor.X, d.Measure))
                .ToList() ?? [];
    }

    public class DeflectionDisplayItem
    {
        public string Label { get; }
        public string Value { get; }
        public double Raw { get; }

        public DeflectionDisplayItem(double offsetMm, double valueMicron)
        {
            Label = $"D{offsetMm:0}";
            Value = $"{valueMicron:0.0} μm";
            Raw = valueMicron;
        }
    }
}
