using FlightSimulatorApp.Maps;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightSimulatorApp
{
    class MainWindowViewModel : BaseViewModel
    {
        public MainWindowViewModel(ISimulatorModel model)
        {
            Model = model;
        }


        public string VM_Dashboard
        {
            get
            {
                return Model.Dashboard;
            }
            set { }

        }

        public string VM_simConnectEnabled
        {
            get
            {
                return (!Model.ConnectedToServer && !Model.TryingToConnect).ToString();
            }
            set { }
        }

        public int VM_simPort
        {
            get
            {
                return Model.Port;
            }
            set
            {
                Model.Port = value;
            }
        }

        public string VM_simIP
        {
            get
            {
                return Model.IP;
            }
            set
            {
                Model.IP = value;
            }
        }

        public string VM_simConnectButton
        {
            get
            {
                string result;
                if (Model.ConnectedToServer)
                {
                    result = "Disconnect";
                }
                else if (Model.TryingToConnect)
                {
                    result = "Cancel";
                }
                else
                {
                    result = "Connect";
                }
                return result;
            }
            set { }
        }
    }


}

