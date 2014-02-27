using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.IO.Ports;


namespace Kinect
{
    partial class MainWindow
    {
        // vector of real time position
        // vectors for humanoid . 
        System.Windows.Media.Media3D.Vector3D shoulder_right1;
        System.Windows.Media.Media3D.Vector3D shoulder_right2;
        System.Windows.Media.Media3D.Vector3D shoulder_left1;
        System.Windows.Media.Media3D.Vector3D shoulder_left2;
        System.Windows.Media.Media3D.Vector3D spine;
        System.Windows.Media.Media3D.Vector3D hip_center;
        System.Windows.Media.Media3D.Vector3D hip_right1;
        System.Windows.Media.Media3D.Vector3D hip_right3;
        System.Windows.Media.Media3D.Vector3D hip_left1;
        System.Windows.Media.Media3D.Vector3D hip_left3;
        System.Windows.Media.Media3D.Vector3D knee_left;
        System.Windows.Media.Media3D.Vector3D knee_right;
        System.Windows.Media.Media3D.Vector3D ankel_left;
        System.Windows.Media.Media3D.Vector3D ankel_right;


        //Vectors of refrence position
        System.Windows.Media.Media3D.Vector3D rshoulder_right1;
        System.Windows.Media.Media3D.Vector3D rshoulder_right2;
        System.Windows.Media.Media3D.Vector3D rshoulder_left1;
        System.Windows.Media.Media3D.Vector3D rshoulder_left2;
        System.Windows.Media.Media3D.Vector3D rspine;
        System.Windows.Media.Media3D.Vector3D rhip_center;
        System.Windows.Media.Media3D.Vector3D rhip_right1;
        System.Windows.Media.Media3D.Vector3D rhip_right3;
        System.Windows.Media.Media3D.Vector3D rhip_left1;
        System.Windows.Media.Media3D.Vector3D rhip_left3;
        System.Windows.Media.Media3D.Vector3D rknee_left;
        System.Windows.Media.Media3D.Vector3D rknee_right;
        System.Windows.Media.Media3D.Vector3D rankel_left;
        System.Windows.Media.Media3D.Vector3D rankel_right;



        //vector of kinect 
         System.Windows.Media.Media3D.Vector3D kinecthandleft;
         System.Windows.Media.Media3D.Vector3D kinecthandright;
         System.Windows.Media.Media3D.Vector3D kinectspine;
         System.Windows.Media.Media3D.Vector3D kinecthipcenter;
         System.Windows.Media.Media3D.Vector3D kinectshouldercenter;
         System.Windows.Media.Media3D.Vector3D kinectankleleft;
         System.Windows.Media.Media3D.Vector3D kinectankleright;
         System.Windows.Media.Media3D.Vector3D kinectkneeleft;
         System.Windows.Media.Media3D.Vector3D kinectkneeright;
         System.Windows.Media.Media3D.Vector3D kinectfootleft;
         System.Windows.Media.Media3D.Vector3D kinectfootright;
         System.Windows.Media.Media3D.Vector3D Kinecthead;
         System.Windows.Media.Media3D.Vector3D kinecthipleft;
         System.Windows.Media.Media3D.Vector3D kinecthipright;
         System.Windows.Media.Media3D.Vector3D kinectshoulderleft;
         System.Windows.Media.Media3D.Vector3D kinectshoulderright;
         System.Windows.Media.Media3D.Vector3D kinectwristleft;
         System.Windows.Media.Media3D.Vector3D kinectwristright;
         double angleshoulderleft1 = 0;
         double rangleshoulderleft1 = 0;
         double angleshoulderleft2 = 0;
         double rangleshoulderleft2 = 0;
         double angleshoulderright1 = 0;
         double rangleshoulderright1 = 0;
         double angleshoulderright2 = 0;
         double rangleshoulderright2 = 0;
         double anglespine1 = 0;
         double anglehipcenter1 = 0;
         double ranglespine1 = 0;
         double ranglehipcenter1 = 0;
         double anglehipleft1 = 0;
         double anglehipleft2 = 0;
         double anglehipleft3 = 0;
         double anglehipright1 = 0;
         double anglehipright2 = 0;
         double anglehipright3 = 0;
         double ranglehipleft1 = 0;
         double ranglehipleft2 = 0;
         double ranglehipleft3 = 0;
         double ranglehipright1 = 0;
         double ranglehipright2 = 0;
         double ranglehipright3 = 0;
         double ranglekneeleft = 0;
         double ranglekneeright = 0;
         double anglekneeleft = 0;
         double anglekneeright = 0;
         double rangleankleleft = 0;
         double rangleankleright = 0;
         double angleankleleft = 0;
         double angleankleright = 0;


        double ashoulder_right1 = 0;
        double ashoulder_left1 = 0;
        double ashoulder_left2 = 0;
        double ashoulder_right2 = 0;
        double aspine = 0;
        double ahip_center = 0;
        double ahip_right1 = 0;
        double ahip_right2 = 0;
        double ahip_right3 = 0;
        double ahip_left1 = 0;
        double ahip_left2 = 0;
        double ahip_left3 = 0;
        double akneeleft = 0;
        double akneeright = 0;
        double aankleleft = 0;
        double aankleright = 0;

        public void humanoid(SkeletonFrameReadyEventArgs e)
        {
              using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
              {
                if (skeletonFrame == null)
                    return;

                // Copies the data in a Skeleton array (6 items) 
                skeletonFrame.CopySkeletonDataTo(skeletonData);         // Consider creating a copy of this array in this code

                // Retrieves Skeleton objects with Tracked state
                var trackedSkeletons = skeletonData.Where(s => s.TrackingState == SkeletonTrackingState.Tracked);

                foreach (Skeleton seenSkeleton in trackedSkeletons)
                {
                        foreach (Joint joint in seenSkeleton.Joints)        // 20 such Joints are tracked by the Kinect
                        {

                            var ID = joint.JointType;
                            var scaledJoint = ScaleJoint(joint);
                            if (ID == JointType.HandLeft)
                            {
                                kinecthandleft = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                            }
                            if (ID == JointType.HandRight)
                            {
                                kinecthandright = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                            }
                            if (ID == JointType.ShoulderLeft)
                            {
                                kinectshoulderleft = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);

                            }
                            if (ID == JointType.ShoulderRight)
                            {
                                kinectshoulderright = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                            }
                            if (ID == JointType.Spine)
                            {
                                kinectspine = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                            }
                            if (ID == JointType.HipCenter)
                            {
                                kinecthipcenter = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                            }
                            if (ID == JointType.ShoulderCenter)
                            {
                                kinectshouldercenter = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                            }
                            if (ID == JointType.AnkleLeft)
                            {
                                kinectankleleft = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                            }
                            if (ID == JointType.AnkleRight)
                            {
                                kinectankleright = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                            }
                            if (ID == JointType.KneeLeft)
                            {
                                kinectkneeleft = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                            }
                            if (ID == JointType.KneeRight)
                            {
                                kinectkneeright = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                            }

                            if (ID == JointType.FootLeft)
                            {
                                kinectfootleft = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                            }
                            if (ID == JointType.FootRight)
                            {
                                kinectfootright = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                            }
                            if (ID == JointType.Head)
                            {
                                Kinecthead = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                            }
                            if (ID == JointType.HipLeft)
                            {
                                kinecthipleft = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                            }
                            if (ID == JointType.HipRight)
                            {
                                kinecthipright = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                            }
                            if (ID == JointType.WristLeft)
                            {
                                kinectwristleft = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);

                            }
                            if (ID == JointType.WristRight)
                            {
                                kinectwristright = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);


                            }
                        }
                    }
                }
            
            shoulder_left1 = kinectshoulderleft - kinectwristleft;
            shoulder_right1 = kinectshoulderright - kinectwristright;
            spine = kinectshoulderleft - kinectshoulderright;
            hip_center = kinecthipcenter - Kinecthead;
            hip_right1 = kinecthipright - kinectkneeright;
            hip_left1 = kinecthipleft - kinectkneeleft;
            hip_left3 = kinectankleleft - kinectfootleft;
            hip_right3 = kinectankleright - kinectfootright;
            knee_left = kinectankleleft - kinectkneeleft;
            knee_right = kinectankleright - kinectkneeright;
            ankel_left = kinectankleleft - kinectfootleft;
            ankel_right = kinectankleright - kinectfootright;

            

            angleshoulderleft1 = Math.Acos(shoulder_left1.X / Math.Abs(shoulder_left1.Length));
            angleshoulderleft2 = Math.Acos(shoulder_left1.Z / Math.Abs(shoulder_left1.Length));
            angleshoulderright1 = Math.Acos(shoulder_right1.X / shoulder_right1.Length);
            angleshoulderright2 = Math.Acos(shoulder_right1.Z / shoulder_right1.Length);
            anglespine1 = Math.Acos(spine.X / spine.Length);
            
            anglehipcenter1 = Math.Acos(hip_center.Y / hip_center.Length);
            anglehipleft1 = Math.Acos(hip_left1.X / hip_left1.Length);
            anglehipright1 = Math.Acos(hip_right1.X / hip_right1.Length);
            anglehipleft2 = Math.Acos(hip_left1.Z / hip_left1.Length);
            anglehipright2 = Math.Acos(hip_right1.Z / hip_right1.Length);
            anglehipleft3 = Math.Acos(hip_left3.X / hip_left3.Length);
            anglehipright3 = Math.Acos(hip_right3.X / hip_right3.Length);
            anglekneeleft = Math.Acos(knee_left.Z / knee_left.Length);
            anglekneeright = Math.Acos(knee_right.Z / knee_right.Length);
            angleankleleft = Math.Acos(ankel_left.Z / ankel_left.Length);
            angleankleright = Math.Acos(ankel_right.Z / ankel_right.Length);

            if (i > 0)
            {
                calculate_angle();
            }
        }

        int i = 0;
        private void capture_refrence_Click(object sender, SkeletonFrameReadyEventArgs e)  // was RoutedEventArgs
        {
            i = i + 1;
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame == null)
                    return;

                // Copies the data in a Skeleton array (6 items) 
                skeletonFrame.CopySkeletonDataTo(skeletonData);

                // Retrieves Skeleton objects with Tracked state
                var trackedSkeletons = skeletonData.Where(s => s.TrackingState == SkeletonTrackingState.Tracked);

                foreach (Skeleton seenSkeleton in trackedSkeletons)
                {
                    foreach (Joint joint in seenSkeleton.Joints)
                    {

                        var ID = joint.JointType;
                        var scaledJoint = ScaleJoint(joint);
                        if (ID == JointType.HandLeft)
                        {
                            kinecthandleft = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                        }
                        if (ID == JointType.HandRight)
                        {
                            kinecthandright = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                        }
                        if (ID == JointType.ShoulderLeft)
                        {
                            kinectshoulderleft = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);

                        }
                        if (ID == JointType.ShoulderRight)
                        {
                            kinectshoulderright = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                        }
                        if (ID == JointType.Spine)
                        {
                            kinectspine = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                        }
                        if (ID == JointType.HipCenter)
                        {
                            kinecthipcenter = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                        }
                        if (ID == JointType.ShoulderCenter)
                        {
                            kinectshouldercenter = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                        }
                        if (ID == JointType.AnkleLeft)
                        {
                            kinectankleleft = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                        }
                        if (ID == JointType.AnkleRight)
                        {
                            kinectankleright = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                        }
                        if (ID == JointType.KneeLeft)
                        {
                            kinectkneeleft = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                        }
                        if (ID == JointType.KneeRight)
                        {
                            kinectkneeright = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                        }

                        if (ID == JointType.FootLeft)
                        {
                            kinectfootleft = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                        }
                        if (ID == JointType.FootRight)
                        {
                            kinectfootright = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                        }
                        if (ID == JointType.Head)
                        {
                            Kinecthead = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                        }
                        if (ID == JointType.HipLeft)
                        {
                            kinecthipleft = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                        }
                        if (ID == JointType.HipRight)
                        {
                            kinecthipright = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
                        }
                        if (ID == JointType.WristLeft)
                        {
                            kinectwristleft = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);

                        }
                        if (ID == JointType.WristRight)
                        {
                            kinectwristright = new System.Windows.Media.Media3D.Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);


                        }
                  }
                }
            }

            rshoulder_left1 = kinectshoulderleft - kinectwristleft;
            rshoulder_right1 = kinectshoulderright - kinectwristright;
            rspine = kinectshoulderleft - kinectshoulderright;
            rhip_center = kinecthipcenter - Kinecthead;
            rhip_right1 = kinecthipright - kinectkneeright;
            rhip_left1 = kinecthipleft - kinectkneeleft;
            rhip_left3 = kinectankleleft - kinectfootleft;
            rhip_right3 = kinectankleright - kinectfootright;
            rknee_left = kinectankleleft - kinectkneeleft;
            rknee_right = kinectankleright - kinectkneeright;
            rankel_left = kinectankleleft - kinectfootleft;
            rankel_right = kinectankleright - kinectfootright;

            rangleshoulderleft1 = Math.Acos(rshoulder_left1.X / rshoulder_left1.Length);
            rangleshoulderleft2 = Math.Acos(rshoulder_left1.Z / rshoulder_left1.Length);
            rangleshoulderright1 = Math.Acos(rshoulder_right1.X / rshoulder_right1.Length);
            rangleshoulderright2 = Math.Acos(rshoulder_right1.Z / rshoulder_right1.Length);
            ranglespine1 = Math.Acos(rspine.X / rspine.Length);
            ranglehipcenter1 = Math.Acos(rhip_center.Y / rhip_center.Length);
            ranglehipleft1 = Math.Acos(rhip_left1.X / rhip_left1.Length);
            ranglehipright1 = Math.Acos(rhip_right1.X / rhip_right1.Length);
            ranglehipleft2 = Math.Acos(rhip_left1.Z / rhip_left1.Length);
            ranglehipright2 = Math.Acos(rhip_right1.Z / rhip_right1.Length);
            ranglehipleft3 = Math.Acos(rhip_left3.X / rhip_left3.Length);
            ranglehipright3 = Math.Acos(rhip_right3.X / rhip_right3.Length);
            ranglekneeleft = Math.Acos(rknee_left.Z / rknee_left.Length);
            ranglekneeright = Math.Acos(rknee_right.Z / rknee_right.Length);
            rangleankleleft = Math.Acos(rankel_left.Z / rankel_left.Length);
            rangleankleright = Math.Acos(rankel_right.Z / rankel_right.Length);
            calculate_angle();
         //   spine = kshoulderright - kshoulderleft;
         //   hip_center = kshouldercenter - khipcenter;
        }

        private void calculate_angle()
        {

            ashoulder_right1 = ((rangleshoulderright1 - angleshoulderright1) * 57.27);
            ashoulder_left1 = ((rangleshoulderleft1 - angleshoulderleft1) * 57.27);
            ashoulder_left2 = ((rangleshoulderleft2 - angleshoulderleft2) * 57.27);
            ashoulder_right2 = ((rangleshoulderright2 - angleshoulderright2) * 57.27);
            aspine = ((ranglespine1 - anglespine1) * 57.27);
            ahip_center = ((ranglehipcenter1 - anglehipcenter1) * 57.27);
            ahip_right1 = ((ranglehipright1 - anglehipright1) * 57.27);
            ahip_right2 = ((ranglehipright2 - anglehipright2) * 57.27);
            ahip_right3 = ((ranglehipright3 - anglehipright3) * 57.27);
            ahip_left1 = ((ranglehipleft1 - anglehipleft1) * 57.27);
            ahip_left2 = ((ranglehipleft2 - anglehipleft2) * 57.27);
            ahip_left3 = ((ranglehipleft3 - anglehipleft3) * 57.27);
            akneeleft = ((ranglekneeleft - anglekneeleft) * 57.27);
            akneeright = ((ranglekneeright - anglekneeright) * 57.27);
            aankleleft = ((rangleankleleft - angleankleleft) * 57.27);
            aankleright = ((rangleankleright - angleankleright) * 57.27);
        /*    double aknee_left = System.Windows.Vector.AngleBetween(rknee_left,knee_left);
            double aknee_right = System.Windows.Vector.AngleBetween(rknee_right,knee_right);
            double aankel_left = System.Windows.Vector.AngleBetween(rankel_left,ankel_left);
            double aankel_right = System.Windows.Vector.AngleBetween(rankel_right,ankel_right);
         */   
            anglelefthand.Content = ashoulder_left1;
            anglerighthand.Content = ashoulder_right1;
            anglelefthandZ.Content = ashoulder_left2;
            anglerighthandZ.Content = ashoulder_right2;
            anglespine.Content = aspine;
            anglehipcenter.Content = ahip_center;
            anglehipleft.Content = ahip_left1;
            anglehipright.Content = ahip_right1;
            anglehipleftz.Content = ahip_left2;
            anglehiprightz.Content = ahip_right2;
            anglehipleftr.Content = ahip_left3;
            anglehiprightr.Content = ahip_right3;
            langlekneeleft.Content = akneeleft;
            langlekneeright.Content = akneeright;
            langleankleleft.Content = aankleleft;
            langleankleright.Content = aankleright;
           SerialSend(17, (int)((-ashoulder_right1 + 90) * 512 / 90));
            SerialSend(12, (int)(((-ashoulder_left1) + 90) * 512 / 90));
            SerialSend(16, (int)((-ashoulder_right2 + 90) * 512 / 90));
            SerialSend(11, (int)(((ashoulder_left2) + 90) * 512 / 90));
            SerialSend(7, (int)(((aspine) + 90) * 512 / 90));
            SerialSend(6, (int)(((ahip_center) + 90) * 512 / 90));
      //      SerialSend(32, (int)(((-ahip_left1) + 90) * 512 / 90));
        //    SerialSend(22, (int)(((-ahip_right1) + 90) * 512 / 90));
        }
        
          public void SerialSend(byte id, int angle)
          {
            
                         byte[] buff = new byte[9];
                         byte checksum = 0x00;
                         buff[0] = 0xff;
                         buff[1] = 0xff;
                         buff[2] = id;
                         buff[3] = 0x05;
                         buff[4] = 0x03;
                         buff[5] = 0x1e;
                         buff[6] = (byte)(angle);
                         buff[7] = (byte)(angle / 256);
                         for (byte i = 2; i < 8; i++)
                         {
                             checksum += buff[i];

                         }
                         checksum = (byte)~checksum;
                         buff[8] = checksum;
                        SerialPort1.Write(buff, 0, 9);  

          }
     
        
          public void reset_Click(object sender, RoutedEventArgs e)
          {
           
              SerialSend(254, 512);
              System.Threading.Thread.Sleep(500);
              byte id = 33;
              SerialSend(id, 550);
              id = 23;
              int angle = 1024 - 550;
              SerialSend(id, angle);
        
            
          }
         
    }
}
