# FlightSimulator
Flight Simulator is a WPF application, built in MVVM architecture, that simulates airplane flight using [Flightgear](https://www.flightgear.org/) values.<br />
This application is very easy to use, and responds to every user's commands.<br />
The application connects to a TCP/IP server.<br />

# Edge Cases
In this project, the application handles every edge case: 
 - Bad coordinates (out of the world).
 - Connection loss in the middle of the process.
 - The application will never crash - it will display a warning that says there is something wrong.
 - The server responds in delay (or does not set the value in time).
 - The server does not work at all.
 
 # Added features
 We have added more features in order to make the application run better and handle more cases:
  - The plane image rotates according to the user's joystick movement.
  - Beautiful design.
  - The user can try to connect to a server (and let it keep trying) until connected to a server.
  - The appliction sends a lot of data to the server simultaneously, and read from the server simultaneously as well.
  * used [Bing Maps](https://www.bing.com/maps) in order to make the user enjoy more.
  
  # In order to run the application
  You will need to start a server that sends all the required data to the simulator.
