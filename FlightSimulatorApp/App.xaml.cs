using FlightSimulatorApp.DashboardTableFiles;
using FlightSimulatorApp.Maps;
using FlightSimulatorApp.UserNavigationControl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FlightSimulatorApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        
        private ISimulatorModel _model;

        private void StartUp_App(object sender, StartupEventArgs e)
        {
            //    Initiate the model with ip and port from app.config
            _model = new SimulatorModel();

            MainWindow mw = new MainWindow(_model);
            mw.Show();
        }       
    }
}
