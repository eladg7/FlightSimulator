using FlightSimulatorApp.Maps;
using Microsoft.Maps.MapControl.WPF;
using Microsoft.Maps.MapControl.WPF.Design;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        BingMapViewModel _vm = new BingMapViewModel();
        public BingMap()
        {
            InitializeComponent();
            DataContext = _vm;

            myMap.Focus();
            //Set map to Aerial mode with labels
            myMap.Mode = new AerialMode(true);

        }

        public void SetModel(ISimulatorModel m)
        {
            _vm.Model = m;
        }

      }
}
