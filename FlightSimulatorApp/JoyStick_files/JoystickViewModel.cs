﻿using System;
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
        private double _premittedRange = 1;
        private double _throttle = 0;
        public JoystickViewModel(ISimulatorModel model)
        {
            Model = model;
        }

        #region properties
        public int VM_JoystickAngle
        {
            set
            {
                Model.AirplaneAngle = value;
            }
        }

        public double VM_premittedRange
        {
            set
            {
                _premittedRange = value;
            }
        }

        public double VM_Throttle
        {
            get
            {
                return _throttle;
            }
            set
            {
                if (value != _throttle)
                {
                    _throttle = value;
                    Model.Throttle = value.ToString();
                }
            }
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
                    Model.Rudder = (value / _premittedRange).ToString();

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
                    Model.Elevator = (value / _premittedRange).ToString();

                    NotifyPropertyChanged("VM_knobY");
                }
            }
        }
        #endregion

    }
}
