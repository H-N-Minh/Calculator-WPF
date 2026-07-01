using System.Configuration;
using System.Data;
using System.Windows;

namespace PriorityTaskBoard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        // Bonus: Runtime Theme Toggling
        public static void ChangeTheme(string themeName)
        {
            Current.Resources.MergedDictionaries.Clear();
            Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"Themes/{themeName}Theme.xaml", UriKind.Relative)
            });
        }
    }

}
