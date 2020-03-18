using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightSimulatorApp.Client
{
    interface ITCPClient
    {
        bool Connect(string ip, int port);
        
        void Disconnect();
        
        string SendToServer(string message);
    }
}
