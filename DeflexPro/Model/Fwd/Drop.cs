using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DeflexPro.Model
{
    public class Drop
    {
        public int ImpNumber { get; set; }
        public DateTime DateTime { get; set; }
        /// <summary>Station distance in m.</summary>
        public double Distance { get; set; }
        /// <summary>Peak force in 0.01 kN, as used by the legacy KUAB data model.</summary>
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
