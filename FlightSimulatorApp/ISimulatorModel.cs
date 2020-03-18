using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlightSimulatorApp.Client;

namespace FlightSimulatorApp
{
    interface ISimulatorModel : INotifyPropertyChanged, ITCPClient
    {
        //  properties
        string Indicated_heading_deg { get; set; }
        string GPS_indicated_vertical_speed { get; set; }
        string GPS_indicated_ground_speed_kt { get; set; }
        string Airspeed_indicator_indicated_speed_kt { get; set; }
        string GPS_indicated_altitude_ft { get; set; }
        string Attitude_indicator_internal_roll_deg { get; set; }
        string Attitude_indicator_internal_pitch_deg { get; set; }
        string Altimeter_indicated_altitude_ft { get; set; }
        string Dashboard { get; set; }
        string Throttle { get; set; }
        string Rudder { get; set; }
        string Elevator { get; set; }
        string Latitude_x { get; set; }
        string Longitude_y { get; set; }

        //  server
        string SetToSimulator(string propertyPath, string value);
        string GetFromSimulator(string message);

        //  activate
        void move(double x, double y);
    }
}
