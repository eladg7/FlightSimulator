﻿using FlightSimulatorApp.Maps;
using Microsoft.Maps.MapControl.WPF;
using Microsoft.Maps.MapControl.WPF.Design;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
        BingMapViewModel _vm;
        public BingMap()
        {
            InitializeComponent();

    
            
        }

        public void SetViewModel(BingMapViewModel vm)
        {
            _vm = vm;
            DataContext = _vm;
            MyMap.Focus();
            //Set map to Aerial mode with labels
            MyMap.Mode = new AerialMode(true);
        }

    }
}
