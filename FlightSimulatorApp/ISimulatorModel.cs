using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightSimulatorApp
{
    interface ISimulatorModel : INotifyPropertyChanged
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

        //  connection
        bool connect(string ip, int port);
        void disconnect();
        string sendToSimulator(string message);
        string SetToSimulator(string value);
        string GetFromSimulator(string message);
        
        //  activate
        void move(double x, double y);
    }
}
