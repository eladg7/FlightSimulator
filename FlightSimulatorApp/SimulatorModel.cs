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
        private ManualResetEvent _manualResetWarningEvent = new ManualResetEvent(false);
        private bool _IsInitialRun = true;

        private delegate bool LocationValidator(string loc);

        private Dictionary<string, string> _values = new Dictionary<string, string>()
        {
            ["throttle"] = "0",
            ["rudder"] = "0",
            ["elevator"] = "0",
            ["aileron"] = "0",

            ["latitude_x"] = "0",
            ["longitude_y"] = "0",

            ["Indicated_heading_deg"] = "0",
            ["GPS_indicated_vertical_speed"] = "0",
            ["GPS_indicated_ground_speed_kt"] = "0",
            ["Airspeed_indicator_indicated_speed_kt"] = "0",
            ["GPS_indicated_altitude_ft"] = "0",
            ["Attitude_indicator_internal_roll_deg"] = "0",
            ["Attitude_indicator_internal_pitch_deg"] = "0",
            ["Altimeter_indicated_altitude_ft"] = "0"
        };

        private Socket _clientSocket;
        private string _dashboard = "";
        private string _location = "0, 0";
        private int _airPlaneAngle = 0;

        private volatile Dictionary<string, bool>
            _connectionState = new Dictionary<string, bool>()
            {
                ["Trying"] = true,
                ["Connected"] = false,
                ["Shutdown"] = false
            };

        private volatile Queue<string> _warningQueue = new Queue<string>();
        private string _warningString = "";

        public SimulatorModel()
        {
            WarningQueueThread();
            ConnectToNewServer(_currentIP, _currentPort);
        }

        private void InitalizeValues()
        {
            IsInitialRun = true;

            Throttle = GetFromSimulator(THROTTLE);
            Rudder = GetFromSimulator(RUDDER);
            Elevator = GetFromSimulator(ELEVATOR);
            Aileron = GetFromSimulator(AILERON);

            UpdateDashboardThread();
            UpdateMapCoordinates();
        }

        private static bool IsLocationValid(string lat, string lon)
        {
            return IsLongitudeValid(lon) && IsLatitudeValid(lat);
        }

        private static bool IsLongitudeValid(string lon)
        {
            bool valid;
            try
            {
                double longitude = Convert.ToDouble(lon);
                valid = longitude >= -179 && longitude <= 179;
            }
            catch (Exception)
            {
                valid = false;
            }

            return valid;
        }

        private static bool IsLatitudeValid(string lat)
        {
            bool valid;
            try
            {
                double latitude = Convert.ToDouble(lat);
                valid = latitude >= -85 && latitude <= 85;
            }
            catch (Exception)
            {
                valid = false;
            }

            return valid;
        }

        private void WarningQueueThread()
        {
            new Thread(delegate()
            {
                while (!IsAppShutDown)
                {
                    if (_warningQueue.Count != 0)
                    {
                        Warning = _warningQueue.Dequeue();
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        _manualResetWarningEvent.WaitOne();
                    }
                }
            }).Start();
        }

        private void UpdateMapCoordinates()
        {
            string lat = GetFromSimulator(LATITUDE_X);
            string lon = GetFromSimulator(LONGITUDE_Y);
            if (IsInitialRun && !IsLocationValid(lat, lon))
            {
                Latitude_x = "0";
                Longitude_y = "0";
                _warningQueue.Enqueue("ERROR: Invalid Longitude or Latitude. Location set to default (0, 0).");
                _manualResetWarningEvent.Set();
            }
            else
            {
                Latitude_x = lat;
                Longitude_y = lon;
            }
        }

        private void UpdateDashboardThread()
        {
            new Thread(delegate()
            {
                while (IsConnectedToServer)
                {
                    UpdateDashBoardAndData();
                    Thread.Sleep(250);
                }
            }).Start();
        }

        private void UpdateDashBoardAndData()
        {
            IndicatedHeadingDeg = GetFromSimulator(HEADING);
            GpsIndicatedVerticalSpeed = GetFromSimulator(VERTICAL_SPEED);
            GpsIndicatedGroundSpeedKt = GetFromSimulator(GROUND_SPEED);
            AirspeedIndicatorIndicatedSpeedKt = GetFromSimulator(INDICATED_SPEED);
            GpsIndicatedAltitudeFt = GetFromSimulator(GPS_ALTITUDE);
            AttitudeIndicatorInternalRollDeg = GetFromSimulator(ROLL);
            AttitudeIndicatorInternalPitchDeg = GetFromSimulator(PITCH);
            AltimeterIndicatedAltitudeFt = GetFromSimulator(ALTIMETER_ALTITUDE);
        }

        public void SetToSimulator(string propertyPath, string value)
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
                _warningQueue.Enqueue("ERROR: Requesting value is invalid.");
                _manualResetWarningEvent.Set();
            }

            result = Regex.Replace(result, @"\t|\n|\r", ""); //  Remove \n


            double d;
            if (!Double.TryParse(result, out d))
            {
                _warningQueue.Enqueue("ERROR: Return value from simulator is invalid, in set action.");
                _manualResetWarningEvent.Set();
            }
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
                result = "0";
                _warningQueue.Enqueue("ERROR: Requesting value is invalid.");
                _manualResetWarningEvent.Set();
            }

            result = Regex.Replace(result, @"\t|\n|\r", ""); //  Remove \n


            double d;
            if (!Double.TryParse(result, out d))
            {
                result = "0";
                _warningQueue.Enqueue("ERROR: Return value from simulator is invalid, in get action.");
                _manualResetWarningEvent.Set();
            }

            return result;
        }

        #region Properties

        public string Warning
        {
            get { return _warningString; }
            set
            {
                if (value != _warningString)
                {
                    _warningString = value;
                    NotifyPropertyChanged("Warning");
                    NotifyPropertyChanged("WarningColor");
                }
            }
        }

        public int AirplaneAngle
        {
            get => _airPlaneAngle;
            set
            {
                if (value == _airPlaneAngle) return;
                _airPlaneAngle = value;
                NotifyPropertyChanged("AirplaneAngle");
            }
        }


        public string Dashboard
        {
            get => _dashboard;
            set
            {
                if (_dashboard == value) return;
                _dashboard = value;
                NotifyPropertyChanged("Dashboard");
            }
        }

        public string IndicatedHeadingDeg
        {
            get => _values["Indicated_heading_deg"];
            set
            {
                if (value != _values["Indicated_heading_deg"])
                {
                    _values["Indicated_heading_deg"] = value;
                    NotifyPropertyChanged("Indicated_heading_deg");
                }
            }
        }

        public string GpsIndicatedVerticalSpeed
        {
            get => _values["GPS_indicated_vertical_speed"];
            set
            {
                if (_values["GPS_indicated_vertical_speed"] != value)
                {
                    _values["GPS_indicated_vertical_speed"] = value;
                    NotifyPropertyChanged("GPS_indicated_vertical_speed");
                }
            }
        }

        public string GpsIndicatedGroundSpeedKt
        {
            get => _values["GPS_indicated_ground_speed_kt"];
            set
            {
                if (_values["GPS_indicated_ground_speed_kt"] != value)
                {
                    _values["GPS_indicated_ground_speed_kt"] = value;
                    NotifyPropertyChanged("GPS_indicated_ground_speed_kt");
                }
            }
        }

        public string AirspeedIndicatorIndicatedSpeedKt
        {
            get => _values["Airspeed_indicator_indicated_speed_kt"];
            set
            {
                if (_values["Airspeed_indicator_indicated_speed_kt"] != value)
                {
                    _values["Airspeed_indicator_indicated_speed_kt"] = value;
                    NotifyPropertyChanged("Airspeed_indicator_indicated_speed_kt");
                }
            }
        }

        public string GpsIndicatedAltitudeFt
        {
            get => _values["GPS_indicated_altitude_ft"];
            set
            {
                if (_values["GPS_indicated_altitude_ft"] != value)
                {
                    _values["GPS_indicated_altitude_ft"] = value;
                    NotifyPropertyChanged("GPS_indicated_altitude_ft");
                }
            }
        }

        public string AttitudeIndicatorInternalRollDeg
        {
            get => _values["Attitude_indicator_internal_roll_deg"];
            set
            {
                if (_values["Attitude_indicator_internal_roll_deg"] != value)
                {
                    _values["Attitude_indicator_internal_roll_deg"] = value;
                    NotifyPropertyChanged("Attitude_indicator_internal_roll_deg");
                }
            }
        }

        public string AttitudeIndicatorInternalPitchDeg
        {
            get => _values["Attitude_indicator_internal_pitch_deg"];
            set
            {
                if (_values["Attitude_indicator_internal_pitch_deg"] != value)
                {
                    _values["Attitude_indicator_internal_pitch_deg"] = value;
                    NotifyPropertyChanged("Attitude_indicator_internal_pitch_deg");
                }
            }
        }

        public string AltimeterIndicatedAltitudeFt
        {
            get => _values["Altimeter_indicated_altitude_ft"];
            set
            {
                if (_values["Altimeter_indicated_altitude_ft"] != value)
                {
                    _values["Altimeter_indicated_altitude_ft"] = value;
                    NotifyPropertyChanged("Altimeter_indicated_altitude_ft");
                }
            }
        }

        public bool IsInitialRun
        {
            get => _IsInitialRun;
            set => _IsInitialRun = value;
        }

        public string Aileron
        {
            get => _values["aileron"];
            set
            {
                if (_values["aileron"] == value) return;
                _values["aileron"] = value;
                SetToSimulator(AILERON, value);
            }
        }

        public string Throttle
        {
            get => _values["throttle"];
            set
            {
                if (_values["throttle"] == value) return;
                _values["throttle"] = value;
                SetToSimulator(THROTTLE, value);
            }
        }

        public string Rudder
        {
            get => _values["rudder"];
            set
            {
                if (_values["rudder"] == value) return;
                _values["rudder"] = value;
                SetToSimulator(RUDDER, value);
                UpdateMapCoordinates();
            }
        }

        public string Elevator
        {
            get => _values["elevator"];
            set
            {
                string tempVal = (-1 * Convert.ToDouble(value)).ToString();

                if (_values["elevator"] == tempVal) return;
                _values["elevator"] = tempVal;
                SetToSimulator(ELEVATOR, tempVal);
                UpdateMapCoordinates();
            }
        }

        public string Latitude_x
        {
            get => _values["latitude_x"];
            set
            {
                string val = GetValidLocation(value, 85, IsLatitudeValid);
                if (val == _values["latitude_x"]) return;
                _values["latitude_x"] = val;
                PlaneLocationByString = val + ", " + Longitude_y;
            }
        }

        public string PlaneLocationByString
        {
            get => _location;
            set
            {
                if (_location == value) return;
                _location = value;
                NotifyPropertyChanged("PlaneLocationByString");
            }
        }


        public string Longitude_y
        {
            get => _values["longitude_y"];
            set
            {
                string val = GetValidLocation(value, 179, IsLongitudeValid);
                if (val == _values["longitude_y"]) return;
                _values["longitude_y"] = val;
                PlaneLocationByString = Latitude_x + ", " + val;
            }
        }

        public string IP
        {
            get => _currentIP;
            set
            {
                if (_currentIP == value) return;
                _currentIP = value;
                NotifyPropertyChanged("IP");
            }
        }

        public int Port
        {
            get => _currentPort;
            set
            {
                if (_currentPort == value) return;
                _currentPort = value;
                NotifyPropertyChanged("Port");
            }
        }

        public bool IsAppShutDown
        {
            get { return _connectionState["Shutdown"]; }
            set
            {
                _connectionState["Shutdown"] = value;
                _manualResetWarningEvent.Set();
            }
        }

        public bool IsConnectedToServer
        {
            get => _connectionState["Connected"];
            set
            {
                if (value == _connectionState["Connected"]) return;
                _connectionState["Connected"] = value;
                NotifyPropertyChanged("simConnectButton");
                NotifyPropertyChanged("simConnectEnabled");
            }
        }

        public bool IsTryingToConnect
        {
            get => _connectionState["Trying"];

            set
            {
                if (_connectionState["Trying"] == value) return;
                if (!value) // trying to connect message clearance
                {
                    _warningQueue.Clear();
                }

                _connectionState["Trying"] = value;
                NotifyPropertyChanged("simConnectButton");
                NotifyPropertyChanged("simConnectEnabled");
            }
        }

        #endregion

        private string GetValidLocation(string value, int modulo, LocationValidator validator)
        {
            string result;
            if (validator(value))
            {
                result = value;
            }
            else
            {
                double locaDouble;
                try
                {
                    locaDouble = Convert.ToDouble(value);
                    if (locaDouble < 0)
                    {
                        locaDouble = (-1 * locaDouble) % modulo;
                    }
                    else
                    {
                        locaDouble = (locaDouble % modulo) * -1;
                    }
                }
                catch (Exception)
                {
                    locaDouble = 0;
                }

                result = locaDouble.ToString("0.######");
            }

            return result;
        }


        #region Connection

        public void ConnectToNewServer(string ip, int port)
        {
            IsConnectedToServer = false;
            IsTryingToConnect = true;
            ClientThread(ip, port);
        }

        private void ClientThread(string ip, int port)
        {
            new Thread(delegate()
            {
                //  Try to connect to the server as long as it's not connected 
                //  tand the user still wants to try to connect
                while (IsTryingToConnect && !Connect(ip, port))
                {
                    _warningQueue.Enqueue("Trying to connect to the simulator...");
                    _manualResetWarningEvent.Set();
                    Thread.Sleep(1000);
                }

                IsConnectedToServer = true;
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

                    //Configure Tcp Socket :10 seconds time out
                    _clientSocket.ReceiveTimeout = 10000;
                    _clientSocket.SendTimeout = 10000;

                    // We print EndPoint information  
                    // that we are connected 
                    Console.WriteLine("Socket connected to -> {0} ",
                        this._clientSocket.RemoteEndPoint.ToString());
                    IsTryingToConnect = false;
                    connected = true;
                    _warningQueue.Enqueue("Connected successfully.");
                    _warningQueue.Enqueue("FLY ME TO THE MOON (:");

                    _manualResetWarningEvent.Set();
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

            if (!connected)
            {
                _warningQueue.Enqueue("ERROR: Could not connect to simulator, trying again..." +
                                      "\nCheck IP and port values.");
                _manualResetWarningEvent.Set();
            }

            return connected;
        }

        public void Disconnect()
        {
            _warningQueue.Enqueue("Disconnecting from simulator...");
            _manualResetWarningEvent.Set();

            IsTryingToConnect = false;
            if (_clientSocket != null && _clientSocket.Connected)
            {
                _clientSocket.Shutdown(SocketShutdown.Both);
                _clientSocket.Close();
            }

            IsConnectedToServer = false;
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

            IsConnectedToServer = sentToServer;
            if (!IsConnectedToServer)
            {
                _warningQueue.Enqueue("ERROR: Simulator is not responding.");
                _manualResetWarningEvent.Set();
            }

            return result;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}