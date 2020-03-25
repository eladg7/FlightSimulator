using FlightSimulatorApp.JoyStick_files;
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

namespace FlightSimulatorApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string IP_REGEX = @"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$";

        private MainWindowViewModel mainViewModel;
        private BingMapViewModel mapViewModel;
        private JoystickViewModel joystickView;

        //  function for testing text box
        delegate bool TextBoxFunc();

        ISimulatorModel _model = new SimulatorModel();
        public MainWindow()
        {
            mainViewModel = new MainWindowViewModel(_model);
            mapViewModel = new BingMapViewModel(_model);
            joystickView = new JoystickViewModel(_model);


            InitializeComponent();
            DataContext = mainViewModel;

            myMap.SetViewModel(mapViewModel);
            Joystick.SetViewModel(joystickView);

            Closing += OnWindowClosing;
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            _model.Disconnect();

        }

        private void TestTextBox(TextBox textBox, TextBoxFunc TestFunc)
        {
            if (!TestFunc())
            {
                textBox.BorderBrush = Brushes.Red;
                simConnectToServerButton.IsEnabled = false;
            }
            else
            {
                simPortTextBox.BorderBrush = Brushes.Gray;
                simConnectToServerButton.IsEnabled = true;
            }

            // Keep the cursor at the end of the input
            textBox.SelectionStart = textBox.Text.Length;
        }

        private void SimIPTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TestTextBox(simIPTextBox, delegate ()
            {
                string tempText = simIPTextBox.Text;
                return tempText.Length > 0 && Regex.IsMatch(tempText, IP_REGEX);
            });
        }

        private void SimPortTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TestTextBox(simPortTextBox, delegate ()
            {
                string tempText = simPortTextBox.Text;
                return tempText.Length > 0 && int.TryParse(tempText, out _);
            });
        }

        private void SimConnectToServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_model.TryingToConnect)
            {
                if (_model.ConnectedToServer)
                {
                    _model.Disconnect();
                }
                else
                {
                    _model.ConnectToNewServer(simIPTextBox.Text, Convert.ToInt32(simPortTextBox.Text));
                }
            }
            else
            {
                _model.TryingToConnect = false;
            }
        }
    }
}
