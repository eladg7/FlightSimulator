using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlightSimulatorApp.Client;

namespace FlightSimulatorApp
{
    public interface ISimulatorModel : INotifyPropertyChanged, ITCPClient
    {
        //  properties
        string Warning { get; set; }
        bool IsInitialRun { get; set; }
        string Throttle { get; set; }
        string Aileron { get; set; }
        string Rudder { get; set; }
        string Elevator { get; set; }
        string PlaneLocationByString { get; set; }
        int AirplaneAngle { get; set; }
        
        string IndicatedHeadingDeg { get; set; }
        
        string GpsIndicatedVerticalSpeed { get; set; }

        string GpsIndicatedGroundSpeedKt { get; set; }

        string AirspeedIndicatorIndicatedSpeedKt { get; set; }

        string GpsIndicatedAltitudeFt { get; set; }

        string AttitudeIndicatorInternalRollDeg { get; set; }

        string AttitudeIndicatorInternalPitchDeg { get; set; }

        string AltimeterIndicatedAltitudeFt { get; set; }

        bool IsAppShutDown { get; set; }


        //  server
        void SetToSimulator(string propertyPath, string value);
        void GetFromSimulator(string propertyPath);
    }
}