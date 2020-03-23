﻿using FlightSimulatorApp.JoyStick_files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlightSimulatorApp
{
    /// <summary>
    /// Interaction logic for Joystick.xaml
    /// </summary>
    public partial class Joystick : UserControl
    {

        private JoystickViewModel _vm;
        private bool _isMouseDown = false;
        private double _baseWidth = 0;
        private double _baseHeight = 0;
        private double _knobRadius = 0;
        private Storyboard _knobAnimation;
        private Point _positionBaseRelative = new Point(0, 0);


        public Joystick()
        {
            InitializeComponent();
            _knobAnimation = Knob.FindResource("CenterKnob") as Storyboard;
        }

        public void SetViewModel(JoystickViewModel vm)
        {
            _vm = vm;
            DataContext = _vm;

        }

        private void centerKnob_Completed(object sender, EventArgs e)
        {

        }

        private void Knob_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _knobAnimation.Stop();
            (Knob).CaptureMouse();
            _isMouseDown = true;
            _baseHeight = Base.ActualHeight;
            _baseWidth = Base.ActualWidth;
            _knobRadius = KnobBase.ActualHeight / 2;
        }

        private void Knob_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown)
            {
                _positionBaseRelative = e.GetPosition(Base);
                UpdateKnobPosition();
            }

        }

        private void UpdateKnobPosition()
        {
            // calculate distance between two points
            double radiusBase = _baseHeight / 2;
            Point centerKnob = new Point(_baseHeight / 2, _baseHeight / 2);
            double permitedRange = radiusBase - _knobRadius;
            double distanceSquared = Math.Sqrt(Math.Pow((centerKnob.X - _positionBaseRelative.X), 2)
                + Math.Pow((centerKnob.Y - _positionBaseRelative.Y), 2));

            // if the distance of the mouse is bigger than the black base.
            if (distanceSquared > permitedRange)
            {
                // math equation for a point between two points.
                double disMinuspre = distanceSquared - permitedRange;
                _positionBaseRelative.X = (disMinuspre * centerKnob.X + permitedRange * _positionBaseRelative.X)
                    /(permitedRange+disMinuspre);
                _positionBaseRelative.Y = (disMinuspre * centerKnob.Y + permitedRange * _positionBaseRelative.Y)
                    / (permitedRange + disMinuspre);
            }
            
       
            //xaml position transfer
            _positionBaseRelative.X -= _baseWidth / 2;
            _positionBaseRelative.Y -= _baseHeight / 2;

            _vm.VM_knobX = _positionBaseRelative.X;
            _vm.VM_knobY = _positionBaseRelative.Y;

        }

        private void Knob_MouseUp(object sender, MouseButtonEventArgs e)
        {
            (Knob).ReleaseMouseCapture();
            _isMouseDown = false;
            _knobAnimation.Begin();
            _vm.VM_knobX = 0;
            _vm.VM_knobY = 0;

        }
    }
}
