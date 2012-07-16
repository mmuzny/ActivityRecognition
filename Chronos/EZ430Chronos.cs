using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using eZ430ChronosNet;

namespace Accelerometer
{
    public class EZ430Chronos
    {

        public static AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        public static eZ430ChronosNet.Chronos watch = new eZ430ChronosNet.Chronos();
        public static Timer timer = null;
        public static int count = 0;

        /// <summary>
        /// Connects to Chronos watch and starts new thread to receive data
        /// </summary>
        /// <returns>Succes of operation</returns>
        public int connect_and_receive() {
            string port_name = watch.GetComPortName();
            watch.OpenComPort(port_name);
            watch.StartSimpliciTI();
            timer = new Timer(new TimerCallback(Tick), new Object() , 0, 50);
            autoResetEvent.WaitOne();
            watch.StopSimpiliTI();
            watch.CloseComPort();
            return 0;
        }

        /// <summary>
        /// Thread function called in 50 ms intervals to collect data from Chronos watch
        /// </summary>
        /// <param name="stateInfo">Unsed parameter - general Object instance</param>
        public static void Tick(Object stateInfo) {
            uint data;
            watch.GetData(out data);
            byte x_axis, y_axis, z_axis;
            x_axis = BitConverter.GetBytes((data >> 24) & 0xff)[0];
            y_axis = BitConverter.GetBytes((data >> 16) & 0xff)[0];         
            z_axis = BitConverter.GetBytes((data >> 8) & 0xff)[0];
            Console.WriteLine("#{0}, #{1}, #{2}", x_axis, y_axis, z_axis);
            count++;
            if (count < 100) { timer.Dispose(autoResetEvent); }
        }
       

    }
}
