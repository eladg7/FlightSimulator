using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FlightSimulatorApp.Maps
{
    public class BingMapViewModel : BaseViewModel
    {

        Location _InitialLocation;
         BitmapImage myBitmapImage = new BitmapImage();
        public BingMapViewModel(ISimulatorModel model)
        {
            Model = model;
            CreateBitmap();
        }

        private void CreateBitmap()
        {
            myBitmapImage.BeginInit();
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            myBitmapImage.UriSource = new Uri(
            System.IO.Path.Combine(path, "Plane.png"));
            // To save significant application memory, set the DecodePixelWidth or  
            // DecodePixelHeight of the BitmapImage value of the image source to the desired 
            // height or width of the rendered image. If you don't do this, the application will 
            // cache the image as though it were rendered as its normal size rather then just 
            // the size that is displayed.
            // Note: In order to preserve aspect ratio, set DecodePixelWidth
            // or DecodePixelHeight but not both.
            //Define the image display properties
            myBitmapImage.DecodePixelHeight = 150;
            myBitmapImage.EndInit();

        }

        //properties 

        public int VM_AirplaneAngle
        {
            get
            {
                return Model.AirplaneAngle;
            }
            set
            {
                NotifyPropertyChanged("VM_AirplaneAngle");
            }
        }

        public BitmapImage PlaneImage
        {
            get
            {
                return myBitmapImage;
            }
        }

        public string VM_LocationByString
        {
            get
            {
                if (Model.LocationByString == null)
                {
                    return "0, 0";
                }
                return Model.LocationByString;
            }
        }

        public Location VM_InitialLocation
        {
            get
            {
                if (IsModelConnected())
                {
                    if (Model.IsInitialRun) // inital plain location
                    {
                        string[] str = Regex.Split(VM_LocationByString, ", ");
                        _InitialLocation = new Location()
                        {
                            Latitude = Convert.ToDouble(str[0]),
                            Longitude = Convert.ToDouble(str[1])
                        };
                        NotifyPropertyChanged("VM_LocationByString");
                        Model.IsInitialRun = false;
                    }
                }
                else
                {
                    //  Could not parse location - set it to (0, 0)
                    _InitialLocation = _InitialLocation = new Location()
                    {
                        Latitude = 0,
                        Longitude = 0
                    };
                }

                return _InitialLocation;
            }
            set { }

        }


    }
}
