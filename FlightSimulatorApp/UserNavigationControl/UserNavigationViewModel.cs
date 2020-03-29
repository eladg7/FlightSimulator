using System;

namespace FlightSimulatorApp.UserNavigationControl
{
    public class UserNavigationViewModel : BaseViewModel
    {
        private const double DoubleDiffPermitted = 0.001;
        private double _knobX = 0;
        private double _knobY = 0;
        private double _premittedRange = 1;

        public UserNavigationViewModel(ISimulatorModel model)
        {
            Model = model;
        }

        #region properties

        public bool VM_SliderEnable
        {
            get { return Model.IsConnectedToServer; }
            set { }
        }

        public string VM_Elevator
        {
            get
            {
                double val = Convert.ToDouble(Model.Elevator);
                float roundedValue = (float)Math.Round(val, 3);
                return roundedValue.ToString();
            }
            set { }
        }

        public string VM_Rudder
        {
            get
            {
                double val = Convert.ToDouble(Model.Rudder);
                float roundedValue = (float)Math.Round(val, 3);
                return roundedValue.ToString();
            }
            set { }
        }

        public int VM_JoystickAngle
        {
            get { return Model.AirplaneAngle; }
            set { Model.AirplaneAngle = value; }
        }

        public double VM_premittedRange
        {
            get { return _premittedRange; }
            set { _premittedRange = value; }
        }

        public string VM_Aileron_toString
        {
            get
            {
                double val = Convert.ToDouble(Model.Aileron);
                float roundedValue = (float)Math.Round(val, 3);
                return roundedValue.ToString();
            }
            set { }
        }

        public string VM_Throttle_toString
        {
            get
            {
                double val = Convert.ToDouble(Model.Throttle);
                float roundedValue = (float)Math.Round(val, 3);
                return roundedValue.ToString();
            }
            set { }
        }

        public double VM_Aileron
        {
            get { return Convert.ToDouble(Model.Aileron); }
            set
            {
                if (Math.Abs(value - Convert.ToDouble(Model.Aileron)) > DoubleDiffPermitted)
                {
                    Model.Aileron = value.ToString();
                }
            }
        }

        public double VM_Throttle
        {
            get { return Convert.ToDouble(Model.Throttle); }
            set
            {
                if (Math.Abs(value - Convert.ToDouble(Model.Throttle)) > DoubleDiffPermitted)
                {
                    Model.Throttle = value.ToString();
                }
            }
        }

        public double VM_knobX
        {
            get { return _knobX; }
            set
            {
                if (Math.Abs(value - _knobX) > DoubleDiffPermitted)
                {
                    _knobX = value;
                    Model.Rudder = (value / _premittedRange).ToString();

                    NotifyPropertyChanged("VM_knobX");
                }
            }
        }

        public double VM_knobY
        {
            get { return _knobY; }
            set
            {
                if (Math.Abs(value - _knobY) > DoubleDiffPermitted)
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