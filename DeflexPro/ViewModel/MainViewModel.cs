using System.Collections.Generic;

namespace DeflexPro.ViewModel
{
    public enum MainPage { Dashboard, Backcalculation }

    public class MainViewModel : ViewModelBase
    {
        private MainPage _currentPage = MainPage.Dashboard;

        public FwdDetailsViewModel FwdDetails { get; }
        public BackcalcViewModel Backcalc { get; }

        public MainPage CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsDashboard));
                RaisePropertyChanged(nameof(IsBackcalc));
            }
        }

        public bool IsDashboard => _currentPage == MainPage.Dashboard;
        public bool IsBackcalc => _currentPage == MainPage.Backcalculation;

        private RelayCommand? _goDashboard;
        private RelayCommand? _goBackcalc;

        public RelayCommand GoDashboard => _goDashboard ??= new RelayCommand(() => CurrentPage = MainPage.Dashboard);

        public RelayCommand GoBackcalc => _goBackcalc ??= new RelayCommand(
            () =>
            {
                SyncStationsToBackcalc();
                CurrentPage = MainPage.Backcalculation;
            },
            () => FwdDetails.Stations.Count > 0);

        public MainViewModel()
        {
            FwdDetails = new FwdDetailsViewModel();
            Backcalc = new BackcalcViewModel();

            FwdDetails.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(FwdDetails.StationCount))
                    GoBackcalc.NotifyCanExecuteChanged();
            };
        }

        private void SyncStationsToBackcalc()
        {
            Backcalc.PlateRadius = FwdDetails.PlateRadius;
            Backcalc.LoadStations(FwdDetails.Drops);
        }
    }
}
