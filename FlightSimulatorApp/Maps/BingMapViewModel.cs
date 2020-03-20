using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightSimulatorApp.Maps
{
    class BingMapViewModel
    {
        private ISimulatorModel model;
        public BingMapViewModel(ISimulatorModel model)
        {
            this.model = model;
            model.PropertyChanged += delegate (Object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("VM_" + e.PropertyName);
            };
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        //properties

        public string VM_airPlainPosition
        {
            get
            {
                string longitude = model.Longitude_y;
                string latitude = model.Latitude_x;
                return latitude + ", " + longitude;

            }
            set { }
        }

    }
}
