using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightSimulatorApp
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private ISimulatorModel _model;

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    

        public ISimulatorModel Model
        {
            get
            {
                return _model;
            }
            set
            {
                if (_model == null)
                {
                    _model = value;
                    _model.PropertyChanged += delegate (Object sender, PropertyChangedEventArgs e)
                    {
                        NotifyPropertyChanged("VM_" + e.PropertyName);
                    };
                }
            }
        }
    }
}
