using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DeflexPro.Model
{
    public class Drop
    {
        public int ImpNumber { get; set; }
        public DateTime DateTime { get; set; }
        public double Distance { get; set; }
        public double PeakForce { get; set; }
        public double AirTemperature { get; set; }
        public double AsphaltTemperature { get; set; }
        public List<Deflection> Deflections { get; set; }

        public Drop()
        {
            this.Deflections = new List<Deflection>();
        }
    }
}
