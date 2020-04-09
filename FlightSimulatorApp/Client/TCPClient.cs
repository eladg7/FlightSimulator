using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightSimulatorApp.Client
{
    public interface ITCPClient
    {
        #region properties
        string IP { get; set; }

        int Port { get; set; }

        bool IsConnectedToServer { get; set; }

        bool IsTryingToConnect { get; set; }
        #endregion

        //  Disconnect from current server socket
        void Disconnect();

        // The thread that's reponsible for sending data to the server,
        // either get or set
        void SendToServerThread();

        //  Connect to a new server by given ip and port
        void ConnectToNewServer(string ip, int port);
    }
}