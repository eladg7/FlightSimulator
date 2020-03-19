using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FlightSimulatorApp
{
    /// <summary>
    /// Interaction logic for BingMap.xaml
    /// </summary>
    public partial class BingMap : UserControl
    {
        public BingMap()
        {
            InitializeComponent();
            myMap.Focus();
            //Set map to Aerial mode with labels
            myMap.Mode = new AerialMode(true);

        }
    }
}
