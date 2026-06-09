using System;
using System.Collections.Generic;
using DeflexPro.Model;

namespace DeflexPro.ViewModel
{
    public class LocationViewModel : ViewModelBase
    {
        public List<EnumValueViewModel> Values { get; private set; }
        public EnumValueViewModel Selected { get; set; }

        public LocationViewModel(Sensor.Location current)
        {
            this.Values = new List<EnumValueViewModel>();
            Array enumValues = Enum.GetValues(typeof(Sensor.Location));

            foreach (Enum item in enumValues)
                this.Values.Add(new EnumValueViewModel(item));

            this.Selected = this.Values[(int)current];
        }
    }
}