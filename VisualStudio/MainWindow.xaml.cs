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

namespace VisualStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        enum Panel
        {
            SE, Properties
        }

        ColumnDefinition CloneLayerSEForLayer0;
        ColumnDefinition CloneLayerSEForLayerProperties;
        ColumnDefinition CloneLayerPropertiesForLayer0;
        ColumnDefinition CloneLayerPropertiesForLayerSE;
        public MainWindow()
        {
            InitializeComponent();
            CloneLayerSEForLayer0 = new ColumnDefinition();
            CloneLayerSEForLayerProperties = new ColumnDefinition();
            CloneLayerPropertiesForLayer0 = new ColumnDefinition();
            CloneLayerPropertiesForLayerSE = new ColumnDefinition();

            CloneLayerSEForLayer0.SharedSizeGroup = "PanelSEWidth";
            CloneLayerSEForLayerProperties.SharedSizeGroup = "PanelSEWidth";
            CloneLayerPropertiesForLayer0.SharedSizeGroup = "PanelPropertiesWidth";
            CloneLayerPropertiesForLayerSE.SharedSizeGroup = "PanelPropertiesWidth";
        }

        private bool isPanelDocked(Panel panel)
        {
            switch (panel)
            {
                case Panel.SE:
                    return SEButton.Visibility == Visibility.Collapsed;
                case Panel.Properties:
                    return PropertiesButton.Visibility == Visibility.Collapsed;
                default:
                    return false;
            }
        }

        #region Hovering mouse behavior
        private void SEButton_MouseEnter(object sender, MouseEventArgs e)
        {
            SEPanel.Visibility = Visibility.Visible;
            Grid.SetZIndex(SEPanel, 2);
            Grid.SetZIndex(PropertiesPanel, 1);
        }

        private void SEButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!isPanelDocked(Panel.SE))
            {
                SEPanel.Visibility = Visibility.Collapsed;

            }
        }
        private void PropertiesButton_MouseEnter(object sender, MouseEventArgs e)
        {
            PropertiesPanel.Visibility = Visibility.Visible;
            Grid.SetZIndex(PropertiesPanel, 2);
            Grid.SetZIndex(SEPanel, 1);
        }

        private void PropertiesButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!isPanelDocked(Panel.Properties))
            {
                PropertiesPanel.Visibility = Visibility.Collapsed;

            }
        }

        private void SEPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            SEPanel.Visibility = Visibility.Visible;
            Grid.SetZIndex(SEPanel, 2);
            Grid.SetZIndex(PropertiesPanel, 1);
        }

        private void SEPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!isPanelDocked(Panel.SE))
            {
                SEPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void PropertiesPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            PropertiesPanel.Visibility = Visibility.Visible;
            Grid.SetZIndex(PropertiesPanel, 2);
            Grid.SetZIndex(SEPanel, 1);
        }

        private void PropertiesPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!isPanelDocked(Panel.Properties))
            {
                PropertiesPanel.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Toggling docking behavior
        private void SEPinButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleDocking(Panel.SE);
        }
        private void PropertyPinButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleDocking(Panel.Properties);
        }
        private void SEButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleDocking(Panel.SE);
        }

        private void PropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleDocking(Panel.Properties);
        }

        private void ToggleDocking(Panel panel)
        {
            if (isPanelDocked(panel))
            {
                UndockPanel(panel);
            }
            else
            {
                DockPanel(panel);
            }
        }

        private void DockPanel(Panel panel)
        {
            switch (panel)
            {
                case Panel.SE:
                    SEButton.Visibility = Visibility.Collapsed;
                    SEPanel.Visibility = Visibility.Visible;
                    Layer0Panel.ColumnDefinitions.Add(CloneLayerSEForLayer0);

                    if(isPanelDocked(Panel.Properties))
                    {
                        PropertiesPanel.ColumnDefinitions.Add(CloneLayerSEForLayerProperties);
                    }
                    break;
                case Panel.Properties:
                    PropertiesButton.Visibility = Visibility.Collapsed;
                    PropertiesPanel.Visibility = Visibility.Visible;
                    Layer0Panel.ColumnDefinitions.Add(CloneLayerPropertiesForLayer0);

                    if (isPanelDocked(Panel.SE))
                    {
                        SEPanel.ColumnDefinitions.Add(CloneLayerPropertiesForLayerSE);
                    }
                    break;
            }
        }

        private void UndockPanel(Panel panel)
        {
            switch (panel)
            {
                case Panel.SE:
                    SEButton.Visibility = Visibility.Visible;
                    Layer0Panel.ColumnDefinitions.Remove(CloneLayerSEForLayer0);
                    PropertiesPanel.ColumnDefinitions.Remove(CloneLayerSEForLayerProperties);
                    SEPanel.ColumnDefinitions.Remove(CloneLayerPropertiesForLayerSE);
                    break;
                case Panel.Properties:
                    PropertiesButton.Visibility = Visibility.Visible;
                    Layer0Panel.ColumnDefinitions.Remove(CloneLayerPropertiesForLayer0);
                    SEPanel.ColumnDefinitions.Remove(CloneLayerPropertiesForLayerSE);
                    PropertiesPanel.ColumnDefinitions.Remove(CloneLayerSEForLayerProperties);
                    break;
            }
        }

        #endregion
    }
}