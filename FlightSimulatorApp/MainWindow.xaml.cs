using FlightSimulatorApp.JoyStick_files;
using FlightSimulatorApp.Maps;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlightSimulatorApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel mainViewModel;
        private BingMapViewModel mapViewModel;
        private JoystickViewModel joystickView;

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

    }
}
