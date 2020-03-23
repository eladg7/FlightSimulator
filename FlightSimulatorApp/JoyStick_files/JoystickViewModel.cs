using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightSimulatorApp.JoyStick_files
{
    public class JoystickViewModel : BaseViewModel
    {
        private double _knobX = 0;
        private double _knobY = 0;
        public JoystickViewModel(ISimulatorModel model)
        {
            Model = model;
        }

        public double VM_knobX
        {
            get
            {
                return _knobX;
            }
            set
            {
                if (value != _knobX)
                {
                    _knobX = value;
                    NotifyPropertyChanged("VM_knobX");
                }
            }
        }

        public double VM_knobY
        {
            get
            {
                return _knobY;
            }
            set
            {
                if (value != _knobY)
                {
                    _knobY = value;
                    NotifyPropertyChanged("VM_knobY");
                }
            }
        }


    }
}
