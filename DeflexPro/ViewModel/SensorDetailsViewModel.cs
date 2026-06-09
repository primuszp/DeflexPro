using DeflexPro.Model;

namespace DeflexPro.ViewModel
{
    public class SensorDetailsViewModel : ViewModelBase
    {
        private readonly Sensor sensor;

        public SensorDetailsViewModel(Sensor sensor)
        {
            this.sensor = sensor;
        }

        public int Id
        {
            get { return sensor.Id; }
            set
            {
                if (sensor.Id == value) return;
                sensor.Id = value;
                RaisePropertyChanged("Id");
            }
        }

        public double X
        {
            get { return sensor.X; }
            set
            {
                if (sensor.X == value) return;
                sensor.X = value;
                RaisePropertyChanged("X");
            }
        }

        public double Y
        {
            get { return sensor.Y; }
            set
            {
                if (sensor.Y == value) return;
                sensor.Y = value;
                RaisePropertyChanged("Y");
            }
        }

        public LocationViewModel Location
        {
            get { return new LocationViewModel(sensor.LocationType); }
        }
    }
}
