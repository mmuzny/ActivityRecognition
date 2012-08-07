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
        private static Unistroke<double> unistroke;
        private static GestureSet<double> gestures;
        private static uWaveRecognizer<Unistroke<double>, GestureSet<double>, double> uwave;
        private static ThreeDollarRecognizer<Unistroke<double>, GestureSet<double>, double> dollar;
        private static SVM<Unistroke<double>, RecordSet<double>, double> svm;
        private static double x_offset = 0, y_offset = 0, z_offset = 0;
        public static double prev_x = 0, prev_y = 0, prev_z = 0;
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
            uint data;
            PreProcessing.loadGestures(ref gestures);
            uwave = new uWaveRecognizer<Unistroke<double>,GestureSet<double>, double>();
            //dollar = new ThreeDollarRecognizer<Unistroke<double>,GestureSet<double>, double>();
            uwave.train(gestures);
            //dollar.train(gestures);
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
            sbyte x_data, y_data, z_data;
            double x_axis, y_axis, z_axis;
            x_data = (sbyte)((data >> 8) & (UInt32)255);
            y_data = (sbyte)((data >> 16) & (UInt32)255);
            z_data = (sbyte)((data >> 24) & (UInt32)255);
            x_axis = (double) (x_data*18*0.5 + prev_x*0.5 - x_offset); 
            y_axis = (double) (y_data*18*0.5 + prev_y*0.5 - y_offset); 
            z_axis = (double) (z_data*18*0.5 + prev_z*0.5 - z_offset); 
            //x_axis = (double) (x_data*18*0.5 + prev_x*0.5); 
            //y_axis = (double) (y_data*18*0.5 + prev_y*0.5); 
            //z_axis = (double) (z_data*18*0.5 + prev_z*0.5); 
            
            if (x_axis == 0 && y_axis == 0 && z_axis == 0) return;
            if (count == 1) { x_offset = prev_x; y_offset = prev_y; z_offset = prev_z; }
            if (count != 0 && count > 5)
            {
                double x_difference=0, y_difference=0, z_difference=0;
                x_difference = prev_x - x_axis;
                y_difference = prev_y - y_axis;
                z_difference = prev_z - z_axis;
                double sum =Math.Pow(x_difference, 2) + Math.Pow(y_difference, 2) + Math.Pow(z_difference, 2);
                if (sum > 1100 && current_state == States.IDLE)
                {
                    unistroke = new Unistroke<double>();
                    current_state = States.GESTURE_PRECALL;    
                }
                else if (sum < 300 && current_state == States.GESTURE_PRECALL && buffered_count < 10) 
                {   
                    buffered_count = 0;
                    current_state = States.IDLE;                                    
                }
                else if (sum > 300 && current_state == States.GESTURE_PRECALL && buffered_count < 10)
                {
                    buffered_count++;
                    //Console.WriteLine("Add buffered sample");
                    //Console.WriteLine("{0}, {1}, {2}", x_axis, y_axis, z_axis);
                    unistroke.trace.Add(new Unistroke<double>.Point3<double>((double) x_axis, (double) y_axis, (double) z_axis));
                }
                else if (buffered_count >= 10 && current_state == States.GESTURE_PRECALL) {                                    
                    unistroke.trace.Add(new Unistroke<double>.Point3<double>((double) x_axis, (double) y_axis, (double) z_axis));
                    //Console.WriteLine("Gesture Recording");
                    current_state = States.GESTURE_RECORDING;
                    buffered_count = 0;
                }
                else if (sum < 300 && (current_state == States.GESTURE_RECORDING) )
                {
                    //Console.WriteLine("Gesture Preexit");
                    current_state = States.PREEXIT;
                    unistroke.trace.Add(new Unistroke<double>.Point3<double>((double) x_axis, (double) y_axis, (double) z_axis));
                }
                else if (sum < 300 && current_state == States.PREEXIT)
                {
                    current_state = States.IDLE;
                    Console.WriteLine("Gesture EXIT");
                    //PreProcessing.saveGesture(ref unistroke, "RECTANGLE");
                    int res;
                    uwave.classify(unistroke, out res);
                    //dollar.classify(unistroke, out res);
                    Console.WriteLine("Recognized gesture: "+gestures.gestures[res].gestureName);
                }
                else if (current_state == States.GESTURE_RECORDING)
                {
                    //Console.WriteLine("Recording gesture");
                    unistroke.trace.Add(new Unistroke<double>.Point3<double>((double)x_axis, (double)y_axis, (double)z_axis));
                }
                //Console.WriteLine(sum);
            }
            prev_x = x_axis;
            prev_y = y_axis;
            prev_z = z_axis;
            //Console.WriteLine("{0}, {1}, {2}", x_axis, y_axis, z_axis);
            if (x_axis != 0 && y_axis != 0 && z_axis != 0) count++;
            //if (count > 300) { timer.Dispose(autoResetEvent); }
        }

    }
}
