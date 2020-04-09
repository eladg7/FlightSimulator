using System.Windows.Controls;
using Microsoft.Maps.MapControl.WPF;

namespace FlightSimulatorApp.Maps
{
    /// <summary>
    /// Interaction logic for BingMap.xaml
    /// </summary>
    public partial class BingMap : UserControl
    {
        BingMapViewModel _vm;
        public BingMap()
        {
            InitializeComponent();        }

        public void SetViewModel(BingMapViewModel vm)
        {
            _vm = vm;
            DataContext = _vm;
            //  Focus the camera to the map
            MyMap.Focus();
            //  Set map to Aerial mode with labels
            MyMap.Mode = new AerialMode(true);
        }

    }
}
