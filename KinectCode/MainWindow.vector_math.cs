using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace Kinect
{
    partial class MainWindow
    {

       //vector between two points (x1,y1) and (x2,y2) .
        public System.Windows.Vector points2vector(double x1, double y1, double x2, double y2)
        {
            System.Windows.Vector vect1;
            System.Windows.Vector vect2;
            vect1 = new System.Windows.Vector(x1, y1);
            vect2 = new System.Windows.Vector(x2, y2);
            System.Windows.Vector vect = vect1 - vect2;
            return vect;
       
        }

        public System.Windows.Vector point2vector(double x1, double y1)
        {
            System.Windows.Vector vect;
            
            vect = new System.Windows.Vector(x1, y1);
            
            
            return vect;

        }

        //angle between two vectors .
        public double vectors2angle(System.Windows.Vector vec1,System.Windows.Vector vec2)
        {
            double angle=System.Windows.Vector.AngleBetween(vec1,vec2);
            return angle;
        }

        
       //single kinect vector to system 3d .
        public System.Windows.Media.Media3D.Vector3D kvector3d2system3d(Microsoft.Research.Kinect.Nui.Vector vec)
        {
            System.Windows.Media.Media3D.Vector3D vect = new System.Windows.Media.Media3D.Vector3D(vec.X, vec.Y, vec.Z);
            return vect;

        }

        //two kinect vector to system 3d vector between those two vectors .
        public System.Windows.Media.Media3D.Vector3D vectorbetween23dvectors(Microsoft.Research.Kinect.Nui.Vector vec1, Microsoft.Research.Kinect.Nui.Vector vec2)
        {
            System.Windows.Media.Media3D.Vector3D vect1= new System.Windows.Media.Media3D.Vector3D(vec1.X,vec1.Y,vec1.Z);
            System.Windows.Media.Media3D.Vector3D vect2= new System.Windows.Media.Media3D.Vector3D(vec2.X,vec2.Y,vec2.Z);
            System.Windows.Media.Media3D.Vector3D vect = vect1 - vect2;
            return vect;
       
        }

        // draw line between two points . 
        public Line drawline2D(double x1,double y1,double x2,double y2)
        {
            Line myLine = new Line();
            myLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
            myLine.X1 = x1;
            myLine.X2 = x2;
            myLine.Y1 = y1;
            myLine.Y2 = y2;
            myLine.HorizontalAlignment = HorizontalAlignment.Left;
            myLine.VerticalAlignment = VerticalAlignment.Center;
            myLine.StrokeThickness = 2;
            return myLine;
        }

        /*
        public void drawline3D(double x1, double y1, double z1, double x2, double y2, double z2)
        {
             triangleMesh = new MeshGeometry3D();
            Point3D point0 = new Point3D(x1,y1,z1);
            Point3D point1 = new Point3D(x2,y2,z2);
           
            triangleMesh.Positions.Add(point0);
            triangleMesh.Positions.Add(point1);
            triangleMesh.Positions.Add(point2);
            triangleMesh.TriangleIndices.Add(0);
            triangleMesh.TriangleIndices.Add(2);
            triangleMesh.TriangleIndices.Add(1);
            Vector3D normal = new Vector3D(0, 1, 0);
            triangleMesh.Normals.Add(normal);
            triangleMesh.Normals.Add(normal);
            triangleMesh.Normals.Add(normal);
            Material material = new DiffuseMaterial(
                new SolidColorBrush(Colors.DarkKhaki));
            GeometryModel3D triangleModel = new GeometryModel3D(
                triangleMesh, material);
            ModelVisual3D model = new ModelVisual3D();
            model.Content = triangleModel;
            this.mainViewport.Children.Add(model);
        }
        */

       


    }
}
