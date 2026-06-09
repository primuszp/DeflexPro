using System;
using System.Collections.Generic;

namespace DeflexPro.Model
{
    public abstract class Fwd
    {
        /// <summary>
        /// Plate Radius [mm]
        /// </summary>
        public double PlateRadius { get; set; }
        public List<Drop> Drops { get; set; }
        public List<Sensor> Sensors { get; set; }
        public string FormatName { get; set; } = string.Empty;
        public string SourceFileName { get; set; } = string.Empty;

        public Fwd()
        {
            this.PlateRadius = 150;
            this.Drops = new List<Drop>();
            this.Sensors = new List<Sensor>();
        }
    }
}
