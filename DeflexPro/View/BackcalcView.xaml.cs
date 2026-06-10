using System.Windows.Controls;

namespace DeflexPro.View
{
    public partial class BackcalcView : UserControl
    {
        public BackcalcView()
        {
            InitializeComponent();
        }

        private void ProgressLogTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
                textBox.ScrollToEnd();
        }
    }
}
