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

        #region Properties
        public string VM_WarningColor
        {
            get
            {
                if (Model.Warning.Contains("ERROR")){
                    return "Red";
                }
                else
                {
                    return "White";
                }
            }
        }

        public string VM_Warning
        {
            get
            {
                return Model.Warning;
            }
            set { }
        }
  

        public string VM_simConnectEnabled
        {
            get
            {
                return (!Model.IsConnectedToServer && !Model.IsTryingToConnect).ToString();
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
                if (Model.IsConnectedToServer)
                {
                    result = "Disconnect";
                }
                else if (Model.IsTryingToConnect)
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
        #endregion
    }


}

