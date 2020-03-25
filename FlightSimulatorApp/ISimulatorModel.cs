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
        bool IsInitialRun { get; set; }
        string Dashboard { get; set; }
        string Throttle { get; set; }
        string Aileron { get; set; }
        string Rudder { get; set; }
        string Elevator { get; set; }
        string LocationByString { get; set; }
        int AirplaneAngle { get; set; }

        //  server
        string SetToSimulator(string propertyPath, string value);
        string GetFromSimulator(string message);

    }
}
