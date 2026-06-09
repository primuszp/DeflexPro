using System;
using System.Windows;
using DeflexPro.Localization;
using DeflexPro.ViewModel;

namespace DeflexPro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var language = new ResourceDictionary
            {
                Source = new Uri(
                    Localizer.IsHungarian
                        ? "Localization/Strings.hu.xaml"
                        : "Localization/Strings.en.xaml",
                    UriKind.Relative)
            };

            Resources.MergedDictionaries.Insert(0, language);
            Resources["Locator"] = new ViewModelLocator();
            base.OnStartup(e);
        }
    }
}
