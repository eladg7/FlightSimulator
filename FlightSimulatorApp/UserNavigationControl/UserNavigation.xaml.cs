using System.Windows;
using System.Windows.Controls;

namespace FlightSimulatorApp.UserNavigationControl
{
    public partial class UserNavigation : UserControl
    {
        private UserNavigationViewModel _vm;

        public UserNavigation()
        {
            InitializeComponent();
        }

        public void SetViewModel(UserNavigationViewModel vm)
        {
            _vm = vm;
            DataContext = _vm;
            Joystick.SetViewModel(_vm);
        }

        private void AileronSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _vm.VM_Aileron = e.NewValue;
        }

        private void ThrottleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _vm.VM_Throttle = e.NewValue;
        }
    }
}