using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using eZ430ChronosNet;
using SignalProcessing;
using Classification;

namespace Accelerometer
{
    public class EZ430Chronos
    {

        public static AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        public static eZ430ChronosNet.Chronos watch = new eZ430ChronosNet.Chronos();
        public static Timer timer = null;
        public static int count = 0, buffered_count = 0 ;
        public static sbyte prev_x = 0, prev_y = 0, prev_z = 0;
        private static Unistroke<double> unistroke;
        private static GestureSet<double> gestures;
        private static GestureSet<int> gestures_2;
        private static uWaveRecognizer<Unistroke<double>, GestureSet<double>, double> uwave;
        private static uWaveRecognizer<Unistroke<int>, GestureSet<int>, int> uwave_2;
        public enum States{
            GESTURE_PRECALL,
            GESTURE_RECORDING,
            IDLE,
            PREEXIT
        }
        public static States current_state = States.IDLE;

        /// <summary>
        /// Connects to Chronos watch and starts new thread to receive data
        /// </summary>
        /// <returns>Succes of operation</returns>
        public static int connect_and_receive() {
            PreProcessing.loadGestures(ref gestures);
            uwave = new uWaveRecognizer<Unistroke<double>,GestureSet<double>, double>();
            uwave.train(gestures);
            string port_name = watch.GetComPortName();
            watch.OpenComPort(port_name);
            watch.StartSimpliciTI();
            timer = new Timer(new TimerCallback(Tick), new Object() , 0, 50);
            autoResetEvent.WaitOne();
            watch.StopSimpiliTI();
            watch.CloseComPort();
            
            return 0;
        }

        public static void Main() {            
            connect_and_receive();
            return;
        }


        /// <summary>
        /// Thread function called in 50 ms intervals to collect data from Chronos watch
        /// </summary>
        /// <param name="stateInfo">Unsed parameter - general Object instance</param>
        public static void Tick(Object stateInfo) {
            uint data;
            watch.GetData(out data);
            sbyte x_axis, y_axis, z_axis;
            x_axis = (sbyte)((data >> 8) & (UInt32)255);
            y_axis = (sbyte)((data >> 16) & (UInt32)255);
            z_axis = (sbyte)((data >> 24) & (UInt32)255);
            if (x_axis == 0 && y_axis == 0 && z_axis == 0) return;
            if (count != 0 && count > 5)
            {
                double x_difference=0, y_difference=0, z_difference=0;
                x_difference = prev_x - x_axis;
                y_difference = prev_y - y_axis;
                z_difference = prev_z - z_axis;
                /*
                if (prev_x <= 255 && prev_x > 200 && x_axis >= 0 && x_axis < 50) x_difference = 255 - prev_x + x_axis;
                else if (prev_x < 50 && prev_x >= 0 && x_axis >= 200 && x_axis <= 255) x_difference = 255 - x_axis + prev_x;
                if (prev_y <= 255 && prev_y > 200 && y_axis >= 0 && y_axis < 50) y_difference = 255 - prev_y + y_axis;
                else if (prev_y < 50 && prev_y >= 0 && y_axis >= 200 && y_axis <= 255) y_difference = 255 - y_axis + prev_y;
                if (prev_z <= 255 && prev_z > 200 && z_axis >= 0 && z_axis < 50) z_difference = 255 - prev_z + z_axis;
                else if (prev_z < 50 && prev_z >= 0 && z_axis >= 200 && z_axis <= 255) z_difference = 255 - z_axis + prev_z;
                */
                double sum =Math.Pow(x_difference, 2) + Math.Pow(y_difference, 2) + Math.Pow(z_difference, 2);
                if (sum > 100 && current_state == States.IDLE)
                {
                    unistroke = new Unistroke<double>();
                    current_state = States.GESTURE_PRECALL;    
                }
                else if (sum < 10 && current_state == States.GESTURE_PRECALL && buffered_count < 6) 
                {   
                    current_state = States.IDLE;                                    
                }
                else if (sum > 10 && current_state == States.GESTURE_PRECALL && buffered_count < 6)
                {
                    buffered_count++;
                    //Console.WriteLine("Add buffered sample");
                    unistroke.trace.Add(new Unistroke<double>.Point3<double>((double) x_axis, (double) y_axis, (double) z_axis));
                }
                else if (buffered_count >= 6 && current_state == States.GESTURE_PRECALL) {                                    
                    unistroke.trace.Add(new Unistroke<double>.Point3<double>((double) x_axis, (double) y_axis, (double) z_axis));
                    Console.WriteLine("Gesture Recording");
                    current_state = States.GESTURE_RECORDING;
                    buffered_count = 0;
                }
                else if (sum < 10 && (current_state == States.GESTURE_RECORDING) )
                {
                    //Console.WriteLine("Gesture Preexit");
                    current_state = States.PREEXIT;
                }
                else if (sum < 10 && current_state == States.PREEXIT)
                {
                    current_state = States.IDLE;
                    Console.WriteLine("Gesture EXIT");
                    //PreProcessing.saveGesture(ref unistroke, "arrow");
                    int res;
                    uwave.classify(unistroke, out res);
                    Console.WriteLine(gestures.gestures[res].gestureName);
                }
                else if (current_state == States.GESTURE_RECORDING)
                {
                    unistroke.trace.Add(new Unistroke<double>.Point3<double>((double)x_axis, (double)y_axis, (double)z_axis));
                }
                prev_x = x_axis;
                prev_y = y_axis;
                prev_z = z_axis;
                //Console.WriteLine(sum);
            }
            //Console.WriteLine("{0}, {1}, {2}", x_axis, y_axis, z_axis);
            if (x_axis != 0 && y_axis != 0 && z_axis != 0) count++;
            //if (count > 300) { timer.Dispose(autoResetEvent); }
        }

    }
}
