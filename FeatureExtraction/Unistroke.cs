using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace SignalProcessing
{
    public class Unistroke<T>
    {
        public class Point3<T> : IComparable{

           private static readonly Func<T, T, T> Sub;

           public T x;
           public T y;
           public T z;

           Point3(T x, T y, T z) {
               this.x = x;
               this.y = y;
               this.z = z;

           }

           static Point3() {
               var firstOperand = Expression.Parameter(typeof(T), "a");
               var secondOperand = Expression.Parameter(typeof(T), "b");
               var body = Expression.Subtract(firstOperand, secondOperand);
               Sub = Expression.Lambda<Func<T, T, T>>
                     (body, firstOperand, secondOperand).Compile();
           }

           public static Point3<T> operator -(Point3<T> a, Point3<T> b) {
               return new Point3<T>(Sub(a.x, b.x), Sub(a.y, b.y), Sub(a.z, b.z));
           }
            
            //Compares two points distances from standard 
            public int CompareTo(object p){
                return 0;
            }
        }

        public List<Point3<T>> trace;

        public Point3<T> this[int i] { 
            get {
              return this.trace[i];
            }

            set {
                this.trace[i] = value; 
            }
        }

        public int Length() {
            return this.trace.Count;
        }

        public static explicit operator Array(Unistroke<T> u){
            return u.trace.ToArray();
        }
        
    }
}
