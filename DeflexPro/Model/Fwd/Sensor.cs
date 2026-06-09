using System;

namespace DeflexPro.Model
{
    public class Sensor
    {
        public enum Location { Unknown, Normal, Lateral, Forward, Center }

        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public Location LocationType
        {
            get { return GetLocation(); }
        }

        public Sensor(int id, double x, double y)
        {
            this.X = x;
            this.Y = y;
            this.Id = id;
        }

        private Location GetLocation()
        {
            if (X == 0.0 && Y == 0.0)
                return Location.Center;

            if (X >= 0.0 && Y == 0.0)
                return Location.Normal;

            if (X == 0.0 && Y != 0.0)
                return Location.Lateral;

            if (X < 0.0 && Y == 0.0)
                return Location.Forward;

            return Location.Unknown;
        }
    }
}