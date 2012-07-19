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

           public static readonly Func<T, T, T> Sub, Add, Mul, Div;

           public T x, y, z;

           public Point3() { }

           public Point3(T x, T y, T z) {
               this.x = x;
               this.y = y;
               this.z = z;
           }

            
            static Point3() {
               var firstOperand = Expression.Parameter(typeof(T), "a");
               var secondOperand = Expression.Parameter(typeof(T), "b");
               var body = Expression.Subtract(firstOperand, secondOperand);
               var body_mul = Expression.Multiply(firstOperand, secondOperand);
               var body_add = Expression.Add(firstOperand, secondOperand);
               var body_div = Expression.Divide(firstOperand, secondOperand);
               Sub = Expression.Lambda<Func<T, T, T>>
                     (body, firstOperand, secondOperand).Compile();
               Mul = Expression.Lambda<Func<T, T, T>>
                     (body_mul, firstOperand, secondOperand).Compile();
               Add = Expression.Lambda<Func<T, T, T>>
                     (body_add, firstOperand, secondOperand).Compile();
               Div = Expression.Lambda<Func<T, T, T>>
                     (body_div, firstOperand, secondOperand).Compile();
           }

           public T size() {
               return Add(Add(Mul(this.x, this.x), Mul(this.y, this.y)), Mul(this.z, this.z)); 
           }

           public static T operator *(Point3<T> a, Point3<T> b) {
               return Add((Add(Mul(a.x, b.x), Mul(a.y, b.y))), Mul(a.z, b.z));
           }

           public static Point3<T> operator /(Point3<T> a, T b) {
               return new Point3<T>(Div(a.x, b), Div(a.y, b), Div(a.z, b));
           }

           public static Point3<T> operator ^(Point3<T> a, Point3<T> b) {
               return new Point3<T>(Sub(Mul(a.y, b.z), Mul(b.y, a.z)), Sub((T) (object) 0,Sub(Mul(a.x, b.z), Mul(b.x, a.z))), Sub(Mul(a.x, b.y), Mul(b.x, a.y)));
           }

           public static Point3<T> operator -(Point3<T> a, Point3<T> b) {
               return new Point3<T>(Sub(a.x, b.x), Sub(a.y, b.y), Sub(a.z, b.z));
           }

           public static Point3<T> operator +(Point3<T> a, Point3<T> b) {
               return new Point3<T>(Add(a.x, b.x), Add(a.y, b.y), Add(a.z, b.z));
           }

            public static Point3<T> operator *(Matrix3<T> m, Point3<T> p) {
                return new Point3<T>(m.vectors[0] * p, m.vectors[1] * p, m.vectors[2] * p);
            }
            
            //Compares two points distances off the standard basis 
            public int CompareTo(object p){
                return 0;
            }
        }

        public class Matrix3<T> {
            public Point3<T>[] vectors;

            public Matrix3(){
                vectors = new Point3<T>[3];
            }


        }

        public List<Point3<T>> trace;


        public Unistroke(Unistroke<T> u){  
            this.trace = u.trace;
        }

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
