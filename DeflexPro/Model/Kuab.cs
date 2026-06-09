using System;
using System.Collections.Generic;
using DeflexPro.Model.Events;

namespace DeflexPro.Model
{
    class Kuab : Fwd
    {
        public Kuab()
            : base()
        {
            this.Sensors = new List<Sensor>()
            {
                new Sensor(0,  0.0, 0),
                new Sensor(1,  200, 0),
                new Sensor(2,  300, 0),
                new Sensor(3,  450, 0),
                new Sensor(4,  600, 0),
                new Sensor(5,  900, 0),
                new Sensor(6, 1200, 0)
            };
        }
    }
}