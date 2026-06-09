using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DeflexPro.ViewModel;

namespace DeflexPro.View
{
    /// <summary>
    /// Interaction logic for BrowseFwdView.xaml
    /// </summary>
    public partial class BrowseFwdView : UserControl
    {
        FwdDetailsViewModel fwdViewModel = null;

        public BrowseFwdView()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(BrowseFwdView_Loaded);
        }

        void BrowseFwdView_Loaded(object sender, RoutedEventArgs e)
        {
            fwdViewModel = this.DataContext as FwdDetailsViewModel;
            if (fwdViewModel != null)
                fwdViewModel.Plot.SeriesChanged += new EventHandler(Plot_SeriesChanged);
        }

        void Plot_SeriesChanged(object sender, EventArgs e)
        {
            graph.InvalidatePlot(true);
        }
    }
}
