using System;

namespace DeflexPro.Model
{
    public class Deflection
    {
        public Sensor Sensor { get; private set; }
        /// <summary>Measured deflection in micrometers.</summary>
        public double Measure { get; private set; }

        public Deflection(Sensor sensor, double measure)
        {
            this.Sensor = sensor;
            this.Measure = measure;
        }
    }
}
