using System;
using System.Windows;
using System.Windows.Controls;
using DeflexPro.ViewModel;

namespace DeflexPro.View
{
    /// <summary>
    /// Interaction logic for BrowseFwdView.xaml
    /// </summary>
    public partial class BrowseFwdView : UserControl
    {
        private FwdDetailsViewModel? fwdViewModel;

        public BrowseFwdView()
        {
            InitializeComponent();
            Loaded += BrowseFwdView_Loaded;
            Unloaded += BrowseFwdView_Unloaded;
        }

        private void BrowseFwdView_Loaded(object sender, RoutedEventArgs e)
        {
            fwdViewModel = DataContext as FwdDetailsViewModel;
            if (fwdViewModel != null)
                fwdViewModel.Plot.SeriesChanged += Plot_SeriesChanged;
        }

        private void BrowseFwdView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (fwdViewModel != null)
                fwdViewModel.Plot.SeriesChanged -= Plot_SeriesChanged;
            fwdViewModel = null;
        }

        private void Plot_SeriesChanged(object? sender, EventArgs e)
        {
            graph.InvalidatePlot(true);
        }
    }
}
