using System;

namespace FlightSimulatorApp.DashboardTableFiles
{
    public class DashboardTableViewModel : BaseViewModel
    {
        public DashboardTableViewModel(ISimulatorModel model)
        {
            Model = model;
        }

        #region properties
        public string VM_Indicated_heading_deg
        {
            get
            {
                var doubleNum = Convert.ToDouble(Model.IndicatedHeadingDeg);
                return doubleNum.ToString("0.###");
            }
            set { }
        }

        public string VM_GPS_indicated_vertical_speed
        {
            get
            {
                var doubleNum = Convert.ToDouble(Model.GpsIndicatedVerticalSpeed);
                return doubleNum.ToString("0.###");
            }
            set { }
        }

        public string VM_GPS_indicated_ground_speed_kt
        {
            get
            {
                var doubleNum = Convert.ToDouble(Model.GpsIndicatedGroundSpeedKt);
                return doubleNum.ToString("0.###");
            }
            set { }
        }

        public string VM_Airspeed_indicator_indicated_speed_kt
        {
            get
            {
                var doubleNum = Convert.ToDouble(Model.AirspeedIndicatorIndicatedSpeedKt);
                return doubleNum.ToString("0.###");
            }
            set { }
        }

        public string VM_GPS_indicated_altitude_ft
        {
            get
            {
                var doubleNum = Convert.ToDouble(Model.GpsIndicatedAltitudeFt);
                return doubleNum.ToString("0.###");
            }
            set { }
        }

        public string VM_Attitude_indicator_internal_roll_deg
        {
            get
            {
                var doubleNum = Convert.ToDouble(Model.AttitudeIndicatorInternalRollDeg);
                return doubleNum.ToString("0.###");
            }
            set { }
        }

        public string VM_Attitude_indicator_internal_pitch_deg
        {
            get
            {
                var doubleNum = Convert.ToDouble(Model.AttitudeIndicatorInternalPitchDeg);
                return doubleNum.ToString("0.###");
            }
            set { }
        }

        public string VM_Altimeter_indicated_altitude_ft
        {
            get
            {
                var doubleNum = Convert.ToDouble(Model.AltimeterIndicatedAltitudeFt);
                return doubleNum.ToString("0.###");
            }
            set { }
        }
        #endregion
    }
}