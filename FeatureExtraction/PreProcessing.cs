using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace SignalProcessing
{
    public class PreProcessing
    {
        //Loads gestures into GestureSet
        public static void loadGestures(ref GestureSet<double> gestures) {
            gestures = new GestureSet<double>(); 

            XmlDocument doc = new XmlDocument();
            doc.Load("gestures.xml");
            XmlNodeList nl = doc.GetElementsByTagName("gesture");
            foreach(XmlNode n in nl){
                XmlAttributeCollection collection = n.Attributes;
                Gesture<double> g = new Gesture<double>(collection[0].InnerText);
                gestures.gestures.Add(g);
                Unistroke<double> u = new Unistroke<double>();
                foreach(XmlNode k in n.ChildNodes){
                    XmlAttributeCollection points = k.Attributes;
                    u.trace.Add(new Unistroke<double>.Point3<double>(Double.Parse(points[0].InnerText), Double.Parse(points[1].InnerText), Double.Parse(points[2].InnerText)));                                
                }
                g.unistrokes.Add(u);
            }
        }

        //Saves given Unistroke to file as a new gesture with name specified
        public static void saveGesture(ref SignalProcessing.Unistroke<double> u, string name) {
            XmlDocument doc = new XmlDocument();

            doc.Load("gestures.xml");
            XmlNodeList nl = doc.GetElementsByTagName("gesture_set");
            XmlElement gesture_elem= doc.CreateElement("gesture");
            XmlAttribute ident = doc.CreateAttribute("name");
            ident.InnerText = name;
            gesture_elem.Attributes.Append(ident);
            
            for(int i = 0; i < u.trace.Count; i++){
                XmlElement n = doc.CreateElement("point");
                XmlAttribute x = doc.CreateAttribute("x");
                XmlAttribute y = doc.CreateAttribute("y");
                XmlAttribute z = doc.CreateAttribute("z");
                x.InnerText = u[i].x.ToString();
                y.InnerText = u[i].y.ToString();
                z.InnerText = u[i].z.ToString();
                n.Attributes.Append(x);
                n.Attributes.Append(y);
                n.Attributes.Append(z);
                gesture_elem.AppendChild(n);
            }

            nl[0].AppendChild(gesture_elem);
            doc.Save("gestures.xml");
        }
    }
}
