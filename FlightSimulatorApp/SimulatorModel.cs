using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
        //  Map
        const string LATITUDE_X = "/position/latitude-deg";
        const string LONGTITUDE_Y = "/position/longitude-deg";
        //  Server
        const int PORT = 5402;
        const string IP = "127.0.0.1";
        #endregion

        private Dictionary<string, string> Values;
        private Socket ClientSocket;
        private string dashboard;
        volatile bool stop;

        public SimulatorModel()
        {
            Values = new Dictionary<string, string>();

            if (Connect(IP, PORT))
            {
                stop = false;
                InitalizeValuesDictionary();
            }
        }

        private void InitalizeValuesDictionary()
        {
            Values["throttle"] = GetFromSimulator(THROTTLE);
            Values["rudder"] = GetFromSimulator(RUDDER);
            Values["elevator"] = GetFromSimulator(ELEVATOR);
            //Values["latitude_x"] = GetFromSimulator(LATITUDE_X);
            //Values["longitude_y"] = GetFromSimulator(LONGTITUDE_Y);

            UpdateDashboardThread();
            UpdateMapCoordinatesThread();
        }
        private void UpdateMapCoordinatesThread()
        {
            new Thread(delegate ()
            {
                while (!stop)
                {

                    Latitude_x = GetFromSimulator(LATITUDE_X);
                    Longitude_y = GetFromSimulator(LONGTITUDE_Y);

                    Thread.Sleep(250);
                }
            }).Start();
        }

        private void UpdateDashboardThread()
        {
            new Thread(delegate ()
            {
                while (!stop)
                {

                    Values["indicated-heading-deg"] = GetFromSimulator(HEADING);
                    Values["gps_indicated-vertical-speed"] = GetFromSimulator(VERTICAL_SPEED);
                    Values["gps_indicated-ground-speed-kt"] = GetFromSimulator(GROUND_SPEED);
                    Values["airspeed-indicator_indicated-speed-kt"] = GetFromSimulator(INDICATED_SPEED);
                    Values["gps_indicated-altitude-ft"] = GetFromSimulator(GPS_ALTITUDE);
                    Values["attitude-indicator_internal-roll-deg"] = GetFromSimulator(ROLL);
                    Values["attitude-indicator_internal-pitch-deg"] = GetFromSimulator(PITCH);
                    Values["altimeter_indicated-altitude-ft"] = GetFromSimulator(ALTIMETER_ALTITUDE);

                    Dashboard = "indicated-heading-deg = " + Values["indicated-heading-deg"] + "\n"
                        + "gps_indicated-vertical-speed = " + Values["gps_indicated-vertical-speed"] + "\n"
                        + "gps_indicated-ground-speed-kt = " + Values["gps_indicated-ground-speed-kt"] + "\n"
                        + "airspeed-indicator_indicated-speed-kt = " + Values["airspeed-indicator_indicated-speed-kt"] + "\n"
                        + "gps_indicated-altitude-ft = " + Values["gps_indicated-altitude-ft"] + "\n"
                        + "attitude-indicator_internal-roll-deg = " + Values["attitude-indicator_internal-roll-deg"] + "\n"
                        + "attitude-indicator_internal-pitch-deg = " + Values["attitude-indicator_internal-pitch-deg"] + "\n"
                        + "altimeter_indicated-altitude-ft = " + Values["altimeter_indicated-altitude-ft"];

                    Thread.Sleep(250);
                }
            }).Start();
        }

        #region Properties
        public string Dashboard
        {
            get
            {
                return dashboard;
            }
            set
            {
                if (dashboard != value)
                {
                    dashboard = value;
                    NotifyPropertyChanged("Dashboard");
                }
            }
        }
        public string Indicated_heading_deg
        {
            get
            {
                return this.Values["Indicated_heading_deg"];
            }
            set { }
        }

        public string GPS_indicated_vertical_speed
        {
            get
            {
                return this.Values["GPS_indicated_vertical_speed"];
            }
            set { }
        }

        public string GPS_indicated_ground_speed_kt
        {
            get
            {
                return this.Values["GPS_indicated_ground_speed_kt"];
            }
            set { }
        }

        public string Airspeed_indicator_indicated_speed_kt
        {
            get
            {
                return this.Values["Airspeed_indicator_indicated_speed_kt"];
            }
            set { }
        }

        public string GPS_indicated_altitude_ft
        {
            get
            {
                return this.Values["GPS_indicated_altitude_ft"];
            }
            set { }
        }

        public string Attitude_indicator_internal_roll_deg
        {
            get
            {
                return this.Values["Attitude_indicator_internal_roll_deg"];
            }
            set { }
        }

        public string Attitude_indicator_internal_pitch_deg
        {
            get
            {
                return this.Values["Attitude_indicator_internal_pitch_deg"];
            }
            set { }
        }

        public string Altimeter_indicated_altitude_ft
        {
            get
            {
                return this.Values["Altimeter_indicated_altitude_ft"];
            }
            set { }
        }

        public string Throttle
        {
            get { return Values["throttle"]; }
            set
            {
                if (Values["throttle"] != value)
                {
                    Values["throttle"] = value;
                    SetToSimulator(THROTTLE, value);
                }
            }
        }
        public string Rudder
        {
            get { return Values["rudder"]; }
            set
            {
                if (Values["rudder"] != value)
                {
                    Values["rudder"] = value;
                    SetToSimulator(RUDDER, value);
                }
            }
        }
        public string Elevator
        {
            get { return Values["elevator"]; }
            set
            {
                if (Values["elevator"] != value)
                {
                    Values["elevator"] = value;
                    SetToSimulator(ELEVATOR, value);
                }
            }
        }
        public string Latitude_x
        {
            get { return Values["latitude_x"]; }
            set
            {
                if (value != Values["latitude_x"])
                {
                    Values["latitude_x"] = value;
                }
            }
        }
        public string Longitude_y
        {
            get { return Values["longitude_y"]; }
            set
            {
                if (value != Values["longitude_y"])
                {
                    Values["longitude_y"] = value;
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

        public bool Connect(string ip, int port)
        {
            bool connected = false;
            try
            {
                // Establish the remote endpoint  
                // for the socket. This example  
                // uses given port on the given ip
                IPHostEntry ipHost = Dns.GetHostEntry("localhost");
                IPAddress ipAddr = ipHost.AddressList[1];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr, port);

                // Creation TCP/IP Socket using  
                // Socket Class Costructor 
                this.ClientSocket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    // Connect Socket to the remote  
                    // endpoint using method Connect() 
                    this.ClientSocket.Connect(localEndPoint);

                    // We print EndPoint information  
                    // that we are connected 
                    Console.WriteLine("Socket connected to -> {0} ",
                                  this.ClientSocket.RemoteEndPoint.ToString());
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
            if (ClientSocket != null)
            {
                stop = true;
                ClientSocket.Shutdown(SocketShutdown.Both);
                ClientSocket.Close();
            }
        }

        public void move(double x, double y)
        {
            throw new NotImplementedException();
        }

        public string SendToServer(string message)
        {
            string result;
            try
            {
                byte[] bytes = new byte[1024];
                // Encode the data string into a byte array.
                message += "\n";
                byte[] msg = Encoding.ASCII.GetBytes(message);

                // Send the data through the socket.
                int bytesSent = this.ClientSocket.Send(msg);

                // Receive the response from the remote device.
                int bytesRec = this.ClientSocket.Receive(bytes);
                result = Encoding.ASCII.GetString(bytes, 0, bytesRec);
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
            return result;
        }
    }
}
