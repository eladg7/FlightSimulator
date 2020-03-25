using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightSimulatorApp.Client
{
    public interface ITCPClient
    {
        string IP { get; set; }

        int Port { get; set; }

        bool ConnectedToServer { get; set; }

        bool TryingToConnect { get; set; }

        bool Connect(string ip, int port);

        bool IsConnected();

        void Disconnect();

        string SendToServer(string message);

        void ConnectToNewServer(string ip, int port);
    }
}