using System.Collections.Generic;

namespace DeflexPro.ViewModel
{
    public class StationViewModel : ViewModelBase
    {
        public double Value { get; private set; }
        public string DisplayName { get; private set; }

        public StationViewModel(double distance)
        {
            this.Value = distance;
            this.DisplayName = distance.ToString("00+00.00");
        }
    }
}