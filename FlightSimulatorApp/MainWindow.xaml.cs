using FlightSimulatorApp.Maps;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using FlightSimulatorApp.DashboardTableFiles;
using System.Configuration;
using FlightSimulatorApp.UserNavigationControl;

namespace FlightSimulatorApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string IpRegex =
            @"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$";

        private MainWindowViewModel _mainViewModel;
        private BingMapViewModel _mapViewModel;
        private UserNavigationViewModel _userNavigationView;
        private DashboardTableViewModel _dashboardTableViewModel;

        //  function for testing text box
        private delegate bool TextBoxFunc();

        ISimulatorModel _model;

        public MainWindow()
        {
            //    Initiate the model with ip and port from app.config
            _model = new SimulatorModel(ReadSetting("DefaultIP"),
                Convert.ToInt32(ReadSetting("DefaultPort")));
            _mainViewModel = new MainWindowViewModel(_model);
            _mapViewModel = new BingMapViewModel(_model);
            _userNavigationView = new UserNavigationViewModel(_model);
            _dashboardTableViewModel = new DashboardTableViewModel(_model);

            InitializeComponent();
            DataContext = _mainViewModel;

            MyMap.SetViewModel(_mapViewModel);
            UserNavigation.SetViewModel(_userNavigationView);
            DashboardTable.SetViewModel(_dashboardTableViewModel);

            Closing += OnWindowClosing;
        }

        private static string ReadSetting(string key)
        {
            string result = "Not Found";
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                result = appSettings[key] ?? "Not Found";
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");
            }

            return result;
        }


        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            _model.Disconnect();
            _model.IsAppShutDown = true;
        }

        #region connectArea

        private void TestTextBox(TextBox textBox, TextBoxFunc testFunc)
        {
            if (!testFunc())
            {
                textBox.BorderBrush = Brushes.Red;
                SimConnectToServerButton.IsEnabled = false;
            }
            else
            {
                SimPortTextBox.BorderBrush = Brushes.Gray;
                SimConnectToServerButton.IsEnabled = true;
            }

            // Keep the cursor at the end of the input
            textBox.SelectionStart = textBox.Text.Length;
        }

        private void SimIPTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TestTextBox(SimIpTextBox, delegate()
            {
                string tempText = SimIpTextBox.Text;
                return tempText.Length > 0 && Regex.IsMatch(tempText, IpRegex);
            });
        }

        private void SimPortTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TestTextBox(SimPortTextBox, delegate()
            {
                string tempText = SimPortTextBox.Text;
                return tempText.Length > 0 && int.TryParse(tempText, out _);
            });
        }

        private void SimConnectToServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_model.IsTryingToConnect)
            {
                if (_model.IsConnectedToServer)
                {
                    _model.Disconnect();
                }
                else
                {
                    _model.ConnectToNewServer(SimIpTextBox.Text, Convert.ToInt32(SimPortTextBox.Text));
                }
            }
            else
            {
                _model.IsTryingToConnect = false;
            }
        }

        #endregion
    }
}