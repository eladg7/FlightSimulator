using FlightSimulatorApp.Client;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FlightSimulatorApp
{
    class SimulatorModel : ISimulatorModel
    {
        #region ValuesPath
        //  Dashboard
        const string HEADING = "/instrumentation/heading-indicator/indicated-heading-deg";
        const string VERTICAL_SPEED = "/instrumentation/gps/indicated-vertical-speed";
        const string GROUND_SPEED = "/instrumentation/gps/indicated-ground-speed-kt";
        const string INDICATED_SPEED = "/instrumentation/airspeed-indicator/indicated-speed-kt";
        const string GPS_ALTITUDE = "/instrumentation/gps/indicated-altitude-ft";
        const string ROLL = "/instrumentation/attitude-indicator/internal-roll-deg";
        const string PITCH = "/instrumentation/attitude-indicator/internal-pitch-deg";
        const string ALTIMETER_ALTITUDE = "/instrumentation/altimeter/indicated-altitude-ft";
        //  Joystick
        const string THROTTLE = "/controls/engines/current-engine/throttle";
        const string RUDDER = "/controls/flight/rudder";
        const string ELEVATOR = "/controls/flight/elevator";
        const string AILERON = "/controls/flight/aileron";
        //  Map
        const string LATITUDE_X = "/position/latitude-deg";
        const string LONGITUDE_Y = "/position/longitude-deg";
        //  Server
        int _currentPort = 5402;
        string _currentIP = "127.0.0.1";
        #endregion

        private static readonly Object obj = new Object();

        private bool _IsInitial = true;
        private Dictionary<string, string> _values;
        private Socket _clientSocket;
        private string _dashboard;
        private string _location;
        volatile bool _connected = false;
        private int _airPlaneAngle;
        volatile bool _tryToConnect = false;

        public SimulatorModel()
        {
            _values = new Dictionary<string, string>();

            TryingToConnect = true;
            ConnectToNewServer(_currentIP, _currentPort);

        }

        private void InitalizeValues()
        {
            _IsInitial = true;

            _values["throttle"] = GetFromSimulator(THROTTLE);
            _values["rudder"] = GetFromSimulator(RUDDER);
            _values["elevator"] = GetFromSimulator(ELEVATOR);
            _values["aileron"] = GetFromSimulator(AILERON);
            _values["latitude_x"] = GetFromSimulator(LATITUDE_X);
            _values["longitude_y"] = GetFromSimulator(LONGITUDE_Y);

            _location = _values["latitude_x"] + ", " + _values["longitude_y"];


            UpdateDashboardThread();
            UpdateMapCoordinates();
        }

        private void UpdateMapCoordinates()
        {
            Latitude_x = GetFromSimulator(LATITUDE_X);
            Longitude_y = GetFromSimulator(LONGITUDE_Y);
        }

        private void UpdateDashboardThread()
        {
            new Thread(delegate ()
            {
                while (ConnectedToServer)
                {
                    UpdateDashBoardAndData();
                    Thread.Sleep(250);
                }
            }).Start();
        }

        private void UpdateDashBoardAndData()
        {
            _values["indicated-heading-deg"] = GetFromSimulator(HEADING);
            _values["gps_indicated-vertical-speed"] = GetFromSimulator(VERTICAL_SPEED);
            _values["gps_indicated-ground-speed-kt"] = GetFromSimulator(GROUND_SPEED);
            _values["airspeed-indicator_indicated-speed-kt"] = GetFromSimulator(INDICATED_SPEED);
            _values["gps_indicated-altitude-ft"] = GetFromSimulator(GPS_ALTITUDE);
            _values["attitude-indicator_internal-roll-deg"] = GetFromSimulator(ROLL);
            _values["attitude-indicator_internal-pitch-deg"] = GetFromSimulator(PITCH);
            _values["altimeter_indicated-altitude-ft"] = GetFromSimulator(ALTIMETER_ALTITUDE);

            Dashboard = "indicated-heading-deg = " + _values["indicated-heading-deg"] + "\n"
                + "gps_indicated-vertical-speed = " + _values["gps_indicated-vertical-speed"] + "\n"
                + "gps_indicated-ground-speed-kt = " + _values["gps_indicated-ground-speed-kt"] + "\n"
                + "airspeed-indicator_indicated-speed-kt = " + _values["airspeed-indicator_indicated-speed-kt"] + "\n"
                + "gps_indicated-altitude-ft = " + _values["gps_indicated-altitude-ft"] + "\n"
                + "attitude-indicator_internal-roll-deg = " + _values["attitude-indicator_internal-roll-deg"] + "\n"
                + "attitude-indicator_internal-pitch-deg = " + _values["attitude-indicator_internal-pitch-deg"] + "\n"
                + "altimeter_indicated-altitude-ft = " + _values["altimeter_indicated-altitude-ft"];
        }

        #region Properties
        public int AirplaneAngle
        {
            get
            {
                return _airPlaneAngle;
            }
            set
            {
                if (value != _airPlaneAngle)
                {
                    _airPlaneAngle = value;
                    NotifyPropertyChanged("AirplaneAngle");
                }
            }
        }



        public string Dashboard
        {
            get
            {
                return _dashboard;
            }
            set
            {
                if (_dashboard != value)
                {
                    _dashboard = value;
                    NotifyPropertyChanged("Dashboard");
                }
            }
        }

        //public string Indicated_heading_deg
        //{
        //    get
        //    {
        //        return this._values["Indicated_heading_deg"];
        //    }
        //    set { }
        //}

        //public string GPS_indicated_vertical_speed
        //{
        //    get
        //    {
        //        return this._values["GPS_indicated_vertical_speed"];
        //    }
        //    set { }
        //}

        //public string GPS_indicated_ground_speed_kt
        //{
        //    get
        //    {
        //        return this._values["GPS_indicated_ground_speed_kt"];
        //    }
        //    set { }
        //}

        //public string Airspeed_indicator_indicated_speed_kt
        //{
        //    get
        //    {
        //        return this._values["Airspeed_indicator_indicated_speed_kt"];
        //    }
        //    set { }
        //}

        //public string GPS_indicated_altitude_ft
        //{
        //    get
        //    {
        //        return this._values["GPS_indicated_altitude_ft"];
        //    }
        //    set { }
        //}

        //public string Attitude_indicator_internal_roll_deg
        //{
        //    get
        //    {
        //        return this._values["Attitude_indicator_internal_roll_deg"];
        //    }
        //    set { }
        //}

        //public string Attitude_indicator_internal_pitch_deg
        //{
        //    get
        //    {
        //        return this._values["Attitude_indicator_internal_pitch_deg"];
        //    }
        //    set { }
        //}

        //public string Altimeter_indicated_altitude_ft
        //{
        //    get
        //    {
        //        return this._values["Altimeter_indicated_altitude_ft"];
        //    }
        //    set { }
        //}

        public bool IsInitialRun
        {
            get
            {
                return _IsInitial;
            }
            set
            {
                _IsInitial = value;
            }
        }
        public string Aileron
        {
            get { return _values["aileron"]; }
            set
            {
                if (_values["aileron"] != value)
                {
                    _values["aileron"] = value;
                    SetToSimulator(AILERON, value);
                }
            }
        }
        public string Throttle
        {
            get { return _values["throttle"]; }
            set
            {
                if (_values["throttle"] != value)
                {
                    _values["throttle"] = value;
                    SetToSimulator(THROTTLE, value);
                }
            }
        }

        public string Rudder
        {
            get { return _values["rudder"]; }
            set
            {
                if (_values["rudder"] != value)
                {
                    _values["rudder"] = value;
                    SetToSimulator(RUDDER, value);
                    UpdateMapCoordinates();
                }
            }
        }

        public string Elevator
        {
            get { return _values["elevator"]; }
            set
            {
                string tempVal = (-1 * Convert.ToDouble(value)).ToString();

                if (_values["elevator"] != tempVal)
                {
                    _values["elevator"] = tempVal;
                    SetToSimulator(ELEVATOR, tempVal);
                    UpdateMapCoordinates();
                }
            }
        }

        public string Latitude_x
        {
            get { return _values["latitude_x"]; }
            set
            {
                if (value != _values["latitude_x"])
                {
                    _values["latitude_x"] = value;
                    string str = value + ", " + Longitude_y;
                    LocationByString = str;
                }
            }
        }
        public string LocationByString
        {
            get
            {
                return _location;
            }
            set
            {
                if (_location != value)
                {
                    _location = value;
                    NotifyPropertyChanged("InitialLocation");
                }
            }
        }


        public string Longitude_y
        {
            get { return _values["longitude_y"]; }
            set
            {
                if (value != _values["longitude_y"])
                {
                    _values["longitude_y"] = value;
                    string str = Latitude_x + ", " + value;
                    LocationByString = str;
                }
            }
        }

        public string IP
        {
            get
            {
                return _currentIP;
            }
            set
            {
                if (_currentIP != value)
                {
                    _currentIP = value;
                    NotifyPropertyChanged("IP");
                }
            }
        }

        public int Port
        {
            get
            {
                return _currentPort;
            }
            set
            {
                if (_currentPort != value)
                {
                    _currentPort = value;
                    NotifyPropertyChanged("Port");
                }
            }
        }

        public bool ConnectedToServer
        {
            get
            {
                return _connected;
            }
            set
            {
                if (value != _connected)
                {
                    _connected = value;
                    NotifyPropertyChanged("simConnectButton");
                    NotifyPropertyChanged("simConnectEnabled");
                }
            }
        }

        public bool TryingToConnect
        {
            get
            {
                return _tryToConnect;
            }

            set
            {
                if (_tryToConnect != value)
                {
                    _tryToConnect = value;
                    NotifyPropertyChanged("simConnectButton");
                    NotifyPropertyChanged("simConnectEnabled");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public void ConnectToNewServer(string ip, int port)
        {
            ConnectedToServer = false;
            TryingToConnect = true;
            ClientThread(ip, port);
        }

        private void ClientThread(string ip, int port)
        {
            new Thread(delegate ()
            {
                //  Try to connect to the server as long as it's not connected 
                //  tand the user still wants to try to connect
                while (TryingToConnect && !Connect(ip, port))
                {
                    Thread.Sleep(1000);
                }
                ConnectedToServer = true;
                InitalizeValues();
            }).Start();
        }

        public bool Connect(string ip, int port)
        {
            bool connected = false;
            try
            {
                // Establish the remote endpoint  
                // for the socket. This example  
                // uses given port on the given ip
                IPAddress ipAddr = IPAddress.Parse(ip);
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr, port);

                // Creation TCP/IP Socket using  
                // Socket Class Costructor 
                _clientSocket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    // Connect Socket to the remote  
                    // endpoint using method Connect() 
                    this._clientSocket.Connect(localEndPoint);

                    // We print EndPoint information  
                    // that we are connected 
                    Console.WriteLine("Socket connected to -> {0} ",
                                  this._clientSocket.RemoteEndPoint.ToString());
                    TryingToConnect = false;
                    connected = true;
                }
                // Manage of Socket's Exceptions 
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }

            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }

            return connected;
        }

        public void Disconnect()
        {
            TryingToConnect = false;
            if (_clientSocket != null)
            {
                if (_clientSocket != null && _clientSocket.Connected)
                {
                    _clientSocket.Shutdown(SocketShutdown.Both);
                    _clientSocket.Close();
                }
            }
            ConnectedToServer = false;
        }


        public string SendToServer(string message)
        {
            string result;
            bool sentToServer = false;
            try
            {
                lock (obj)
                {
                    byte[] bytes = new byte[1024];
                    // Encode the data string into a byte array.
                    message += "\n";
                    byte[] msg = Encoding.ASCII.GetBytes(message);
                    int bytesRec = 0;

                    // Send the data through the socket.
                    int bytesSent = this._clientSocket.Send(msg);

                    // Receive the response from the remote device.
                    bytesRec = this._clientSocket.Receive(bytes);

                    result = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    sentToServer = true;
                }
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                result = "ArgumentNullException";
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
                result = "SocketException";
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
                result = "Unexpected exception";
            }
            ConnectedToServer = sentToServer;

            return result;
        }

        public string SetToSimulator(string propertyPath, string value)
        {
            string result = "";
            if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(propertyPath))
            {
                value = "set " + propertyPath + " " + value;
                result = SendToServer(value);
            }
            else
            {
                Console.WriteLine("Could not send empty value.");
            }
            return result;
        }

        public string GetFromSimulator(string message)
        {
            string result = "";
            if (!string.IsNullOrEmpty(message))
            {
                message = "get " + message;
                result = SendToServer(message);
            }
            else
            {
                Console.WriteLine("Could not get empty value.");
            }
            //  Remove \n
            return Regex.Replace(result, @"\t|\n|\r", "");
        }
        public bool IsConnected()
        {
            return ConnectedToServer;
        }
    }
}
