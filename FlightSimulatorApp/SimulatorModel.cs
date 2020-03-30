using FlightSimulatorApp.Client;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FlightSimulatorApp
{


    public delegate void ThreadExitCallBack(string result);
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
        private int _currentPort = 0;
        private string _currentIP = "";

        private class RequestsToServer
        {

            public RequestsToServer(string message, bool update, string path)
            {
                Message = message;
                IsUpdate = update;
                Path = path;

            }
            public string Message
            {
                get; set;
            }

            public bool IsUpdate
            {
                get; set;
            }
            public string Path
            {
                get; set;
            }

        }
        #endregion

        #region Fields
       
        private static readonly Object objInitial = new Object();

        private bool _IsInitialRun = false;

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
        private string _location = "0, 0";
        private int _airPlaneAngle = 0;

        private volatile Dictionary<string, bool>
            _connectionState = new Dictionary<string, bool>()
            {
                ["Trying"] = false,
                ["Connected"] = false,
                ["Shutdown"] = false
            };

        private ManualResetEvent _manualResetWarningEvent = new ManualResetEvent(false);
        private ConcurrentQueue<string> _warningQueue = new ConcurrentQueue<string>();

        private ManualResetEvent _manualResetRequestEvent = new ManualResetEvent(false);
        private ConcurrentQueue<RequestsToServer> _requestGET_ToSim = new ConcurrentQueue<RequestsToServer>();
        private ConcurrentQueue<RequestsToServer> _requestsSET_ToSim = new ConcurrentQueue<RequestsToServer>();



        private string _warningString = "";
        private Dispatcher _dispatcher = null;
        #endregion
        public SimulatorModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _currentIP = ReadSetting("DefaultIP");
            _currentPort = Convert.ToInt32(ReadSetting("DefaultPort"));
            WarningQueueThread();
        }


        private static string ReadSetting(string key)
        {
            string result = "Not Found";
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                result = appSettings[key] ?? "Not Found";
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");
            }

            return result;
        }

        private void InitializeValues()
        {
            ToUpdateCenterMap = false;
            UpdatePlaneLocation();

            GetFromSimulator(THROTTLE);
            GetFromSimulator(RUDDER);
            GetFromSimulator(ELEVATOR);
            GetFromSimulator(AILERON);

            UpdateDashboardThread();
        }

        private void UpdatePlaneLocation()
        {
            GetFromSimulator(LATITUDE_X);
            GetFromSimulator(LONGITUDE_Y);
        }

        public void AddWarningMessage(string message)
        {
            _warningQueue.Enqueue(message);
            _manualResetWarningEvent.Set();
        }

        private void WarningQueueThread()
        {
            new Thread(delegate ()
            {
                while (!IsAppShutDown)
                {

                    if (_warningQueue.IsEmpty && !_manualResetWarningEvent.WaitOne(1000))
                    {
                        Warning = "";
                        continue;

                    }
                    string sWarning;
                    _warningQueue.TryDequeue(out sWarning);
                    if (sWarning==null)
                    {
                        sWarning = "";
                    }
                    Warning = sWarning;
                    Thread.Sleep(2000);


                }
            }).Start();
        }

        private void UpdateDashboardThread()
        {
            new Thread(delegate ()
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
            GetFromSimulator(HEADING);
            GetFromSimulator(VERTICAL_SPEED);
            GetFromSimulator(GROUND_SPEED);
            GetFromSimulator(INDICATED_SPEED);
            GetFromSimulator(GPS_ALTITUDE);
            GetFromSimulator(ROLL);
            GetFromSimulator(PITCH);
            GetFromSimulator(ALTIMETER_ALTITUDE);
        }

        public void SetToSimulator(string propertyPath, string value)
        {
            if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(propertyPath))
            {
                value = "set " + propertyPath + " " + value;
                _requestsSET_ToSim.Enqueue(new RequestsToServer(value, false, propertyPath));
                _manualResetRequestEvent.Set();

            }
            else
            {
                Console.WriteLine("Could not send empty value.");
                AddWarningMessage("ERROR: Requesting value is invalid.");
            }

        }

        public void GetFromSimulator(string propertyPath)
        {
            if (!string.IsNullOrEmpty(propertyPath))
            {
                string message = "get " + propertyPath;
                _requestGET_ToSim.Enqueue(new RequestsToServer(message, true, propertyPath));
                _manualResetRequestEvent.Set();
            }
            else
            {
                Console.WriteLine("Could not get empty value.");
                AddWarningMessage("ERROR: Requesting value is invalid.");
            }

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

        public bool ToUpdateCenterMap
        { get; set; } = false;
        public bool IsInitialRun
        {
            get
            {
                bool val;
                lock (objInitial)
                {
                    val = _IsInitialRun;
                }

                return val;
            }
            set
            {
                lock (objInitial)
                {
                    _IsInitialRun = value;
                }
            }
        }

        public string Aileron
        {
            get => _values["aileron"];
            set
            {
                if (_values["aileron"] == value) return;
                _values["aileron"] = value;
                NotifyPropertyChanged("Aileron");
                NotifyPropertyChanged("Aileron_toString");
                SetToSimulator(AILERON, value);
                UpdatePlaneLocation();// update location after change
            }
        }

        public string Throttle
        {
            get => _values["throttle"];
            set
            {
                if (_values["throttle"] == value) return;
                _values["throttle"] = value;
                NotifyPropertyChanged("Throttle");
                NotifyPropertyChanged("Throttle_toString");
                SetToSimulator(THROTTLE, value);
                UpdatePlaneLocation();// update location after change
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
                NotifyPropertyChanged("Rudder");
                UpdatePlaneLocation();// update location after change

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
                NotifyPropertyChanged("Elevator");
                UpdatePlaneLocation();// update location after change
            }
        }

        public string Latitude_x
        {
            get => _values["latitude_x"];
            set
            {
                string val = GetValidLatitude(value);
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
                if (!ToUpdateCenterMap)// initial loction update after second parameter in given for location 
                {
                    ToUpdateCenterMap = true;
                    IsInitialRun = true;
                }
                else
                {
                    NotifyPropertyChanged("PlaneLocationByString");
                }
            }
        }

        public string Longitude_y
        {
            get => _values["longitude_y"];
            set
            {
                string val = GetValidLongitude(value);
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
                NotifyPropertyChanged("SliderEnable");
            }
        }

        public bool IsTryingToConnect
        {
            get => _connectionState["Trying"];

            set
            {
                if (_connectionState["Trying"] == value) return;

                if (!value) // trying to connect, message clearance
                {
                    while (_warningQueue.TryDequeue(out _)) ; //clear queue
                    AddWarningMessage("To fly, please connect to the simulator.");
                }


                _connectionState["Trying"] = value;
                NotifyPropertyChanged("simConnectButton");
                NotifyPropertyChanged("simConnectEnabled");
            }
        }

        #endregion

        #region LocationValidation
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

        private string GetValidLatitude(string value) // 90 border
        {
            string localLat = value;
            if (!IsLatitudeValid(value))
            {
                AddWarningMessage("ERROR: Plane is out of earth's latitude border.");
                try
                {
                    double dValue = Convert.ToDouble(value);
                    if (dValue < 0)
                    {
                        localLat = "-89";
                    }
                    else
                    {
                        localLat = "89";
                    }
                }
                catch (Exception)
                {
                    localLat = "0";
                }
            }

            return localLat;
        }

        private string GetValidLongitude(string value) //border 180
        {
            string localLong = value;
            if (!IsLongitudeValid(value))
            {
                AddWarningMessage("ERROR: Plane is out of earth's longitude border." +
                    " Recalculating valid longitude");
                try
                {
                    // goes to the other side of globe
                    double dValue = Convert.ToDouble(value);
                    if (dValue < 0)
                    {
                        localLong = "179";
                    }
                    else
                    {
                        localLong = "-179";
                    }
                }
                catch (Exception)
                {
                    localLong = "0";
                }
            }

            return localLong;
        }

        #endregion

        #region Connection

        public void ConnectToNewServer(string ip, int port)
        {
            IsConnectedToServer = false;
            IsTryingToConnect = true;
            ClientThread(ip, port);
        }

        private void ClientThread(string ip, int port)
        {
            new Thread(delegate ()
            {
                //  Try to connect to the server as long as it's not connected 
                //  and the user still wants to try to connect
                while (IsTryingToConnect && !Connect(ip, port))
                {
                    Thread.Sleep(1000);
                }
                if (IsConnectedToServer)
                {
                    SendToServerThread();
                    InitializeValues();
                }

            }).Start();


        }

        public bool Connect(string ip, int port)
        {
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
                    IsConnectedToServer = true;
                    AddWarningMessage("Connected successfully.");
                    AddWarningMessage("FLY ME TO THE MOON (:");
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

            if (!IsConnectedToServer && IsTryingToConnect)
            {
                AddWarningMessage("ERROR: Could not connect to simulator, trying again..." +
                                      " Check IP and port values.");
                AddWarningMessage("Trying to connect to the simulator...");
            }

            return IsConnectedToServer;
        }

        public void Disconnect()
        {
            IsTryingToConnect = false;
            if (_clientSocket != null && _clientSocket.Connected)
            {
                _clientSocket.Shutdown(SocketShutdown.Both);
                _clientSocket.Close();
            }

            IsConnectedToServer = false;
            AddWarningMessage("Disconnected from simulator.");
        }

        private void SetServiceResult(string type, string result)
        {
            this._dispatcher.Invoke(() =>
            {
                result = Regex.Replace(result, @"\t|\n|\r", ""); //  Remove \n

                double d;
                if (!Double.TryParse(result, out d))
                {
                    AddWarningMessage("ERROR: Return value from simulator is invalid, in set action.");
                }
            });
        }
        private void getServiceResult(string type, string result)
        {
            this._dispatcher.Invoke(() =>
            {
                result = Regex.Replace(result, @"\t|\n|\r", ""); //  Remove \n


                double d;
                if (!Double.TryParse(result, out d))
                {
                    result = "0";
                    AddWarningMessage("ERROR: Return value from simulator is invalid, in get action.");
                }
                else
                {
                    updateTypes(type, result);
                }

            });
        }

        private void updateTypes(string type, string result)
        {
            switch (type)
            {
                case HEADING:
                    IndicatedHeadingDeg = result;
                    break;
                case VERTICAL_SPEED:
                    GpsIndicatedVerticalSpeed = result;
                    break;
                case GROUND_SPEED:
                    GpsIndicatedGroundSpeedKt = result;
                    break;
                case INDICATED_SPEED:
                    AirspeedIndicatorIndicatedSpeedKt = result;
                    break;
                case GPS_ALTITUDE:
                    GpsIndicatedAltitudeFt = result;
                    break;
                case ROLL:
                    AttitudeIndicatorInternalRollDeg = result;
                    break;
                case PITCH:
                    AttitudeIndicatorInternalPitchDeg = result;
                    break;
                case ALTIMETER_ALTITUDE:
                    AltimeterIndicatedAltitudeFt = result;
                    break;
                case THROTTLE:
                    Throttle = result;
                    break;
                case RUDDER:
                    Rudder = result;
                    break;
                case ELEVATOR:
                    Elevator = result;
                    break;
                case AILERON:
                    Aileron = result;
                    break;
                case LATITUDE_X:
                    Latitude_x = result;
                    break;
                case LONGITUDE_Y:

                    Longitude_y = result;

                    break;
                default:
                    AddWarningMessage("ERROR: Could not find type to update.");
                    break;
            }
        }

        public void SendToServerThread()
        {

            new Thread(delegate ()
            {
                while (IsConnectedToServer)
                {

                    if ((_requestsSET_ToSim.IsEmpty && _requestGET_ToSim.IsEmpty)
                        || (!_manualResetRequestEvent.WaitOne(100)))
                    {
                        continue;
                    }

                    RequestsToServer request;
                    _requestsSET_ToSim.TryDequeue(out request);
                    if (request == null)
                    {
                        _requestGET_ToSim.TryDequeue(out request);
                    }

                    string message = request.Message;
                    bool sentToServer = false;
                    string result;

                    try
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
                        while (_warningQueue.TryDequeue(out _)) ;
                        AddWarningMessage("ERROR: Simulator is not responding.");
                        result = "0";
                    }

                    if (request.IsUpdate)
                    {
                        getServiceResult(request.Path, result);
                    }
                    else
                    {
                        SetServiceResult(request.Path, result);
                    }
                }

            }).Start();
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