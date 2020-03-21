using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        bool IsInitial = true;
        Location _InitialLocation;
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
        BitmapImage myBitmapImage = new BitmapImage();
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
                return Model.LocationByString;
            }
        }
        
        public Location VM_InitialLocation
        {
            get
            {

                if (IsInitial)
                {
                    string[] str = Regex.Split(VM_LocationByString, ", ");
                    _InitialLocation = new Location()
                    {
                        Altitude = Convert.ToDouble(str[0]),
                        Longitude = Convert.ToDouble(str[1])
                    };
                }
                return _InitialLocation;

            }
        }
    }
}
