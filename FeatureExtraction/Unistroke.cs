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
           public static readonly Func<T, T, bool> Greater;


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
               var body_gt = Expression.GreaterThan(firstOperand, secondOperand);

               Sub = Expression.Lambda<Func<T, T, T>>
                     (body, firstOperand, secondOperand).Compile();
               Mul = Expression.Lambda<Func<T, T, T>>
                     (body_mul, firstOperand, secondOperand).Compile();
               Add = Expression.Lambda<Func<T, T, T>>
                     (body_add, firstOperand, secondOperand).Compile();
               Div = Expression.Lambda<Func<T, T, T>>
                     (body_div, firstOperand, secondOperand).Compile();
               Greater = Expression.Lambda<Func<T, T, bool>>
                     (body_gt, firstOperand, secondOperand).Compile();

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
            
            //Compares two points distances off the standard basis -- TODO
            public int CompareTo(object p){
                return 0;
            }
        }

        public class Matrix3<T> {
            public Point3<T>[] vectors;

            public Matrix3(){
                vectors = new Point3<T>[3];
            }

            public Matrix3(double angle, Point3<T> axis) {                 
                vectors = new Point3<T>[3];
                double axis_x = (double) (object) axis.x, axis_y = (double) (object) axis.y, axis_z = (double) (object) axis.z;
                vectors[0].x =(T) (object) (Math.Cos(angle) + Math.Pow(axis_x,2) * (1 - Math.Cos(angle)));
                vectors[0].y =(T) (object) (axis_x*axis_y*(1-Math.Cos(angle)) - axis_z*Math.Sin(angle));
                vectors[0].z =(T) (object) (axis_x*axis_z*(1-Math.Cos(angle)) + axis_y*Math.Sin(angle));
                vectors[1].x =(T) (object) (axis_y*axis_x*(1-Math.Cos(angle)) + axis_z * (Math.Sin(angle)));
                vectors[1].y =(T) (object) (axis_y*axis_y*(1-Math.Cos(angle)) + Math.Cos(angle));
                vectors[1].z =(T) (object) (axis_y*axis_z*(1-Math.Cos(angle)) - axis_x*Math.Sin(angle));
                vectors[2].x =(T) (object) (axis_z*axis_x*(1-Math.Cos(angle)) - axis_y* (Math.Sin(angle)));
                vectors[2].y =(T) (object) (axis_y*axis_z*(1-Math.Cos(angle)) + axis_x*Math.Sin(angle));
                vectors[2].z =(T) (object) (axis_z*axis_z*(1-Math.Cos(angle)) + Math.Cos(angle));
            }

            public Matrix3(double angle_x, double angle_y, double angle_z) { 
                vectors = new Point3<T>[3];
                for (int i = 0; i < 3; i++) { vectors[i] = new Point3<T>(); }
                vectors[0].x =(T) (object) (Math.Cos(angle_y) * Math.Cos(angle_z));
                vectors[0].y =(T) (object) (-Math.Cos(angle_x) * Math.Sin(angle_z) + Math.Sin(angle_x) * Math.Sin(angle_x) * Math.Cos(angle_z));
                vectors[0].z =(T) (object) (Math.Sin(angle_x) * Math.Sin(angle_z) + Math.Cos(angle_x) * Math.Sin(angle_x) * Math.Sin(angle_z));
                vectors[1].x =(T) (object) (Math.Cos(angle_y) * Math.Sin(angle_z));
                vectors[1].y =(T) (object) (Math.Cos(angle_x) * Math.Cos(angle_z) + Math.Sin(angle_x) * Math.Sin(angle_x) * Math.Sin(angle_z));
                vectors[1].z =(T) (object) (-Math.Sin(angle_x) * Math.Cos(angle_z) + Math.Cos(angle_x) * Math.Sin(angle_x) * Math.Sin(angle_z));
                vectors[2].x =(T) (object) (-Math.Sin(angle_y));
                vectors[2].y =(T) (object) (Math.Sin(angle_x) * Math.Cos(angle_x));
                vectors[2].z =(T) (object) (Math.Cos(angle_x) * Math.Cos(angle_y));
            }

        }

        public List<Point3<T>> trace;


        public Unistroke(Unistroke<T> u){  
            this.trace = u.trace;
        }

        public Unistroke() {
            this.trace = new List<Point3<T>>();
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
