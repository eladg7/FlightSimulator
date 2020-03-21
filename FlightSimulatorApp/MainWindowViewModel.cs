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
    }


}

