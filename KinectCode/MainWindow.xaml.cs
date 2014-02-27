using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;           
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;     
using System.IO.Ports;
//using Kinect.Toolbox;     // TODO: Check this!!



namespace Kinect
{
 
    public partial class MainWindow : Window
    {
       // public System.Speech.Synthesis.SpeechSynthesizer synthesizer;
       // Kinect.Toolbox.VoiceCommander voiceCommander;
       // Camera cam;
       // Runtime _nui = new Runtime();       
        KinectSensor sensor;                                    //= KinectSensor.KinectSensors[0];   
        public WriteableBitmap OutputImage { get; set; }        // better than initalising & creating a new Bitmap 30 times a sec (as 30fps = frame rate)

        private Skeleton[] skeletonData = new Skeleton[6];      // (can track 6 skeletons - 2 tracked + 4 positiononly
        System.IO.Ports.SerialPort SerialPort1 = new SerialPort();
        SkeletonFrameReadyEventArgs args_e;
        public MainWindow()
        {
            InitializeComponent();
            /*
            voiceCommander = new VoiceCommander("record", "stop", "how are you?", "move back","move forward", "what is your name?","who are you?","hello");
            voiceCommander.OrderDetected += voiceCommander_OrderDetected;
            voiceCommander.Start();
            */
            try
            {
                SerialPort1.PortName = "COM24";          // edit PORT Number accordingly as per connection
                SerialPort1.BaudRate = 1000000;
                SerialPort1.Open();
            }
            catch (Exception f)
            {
                MessageBox.Show("Could not open Serial Port.:" + f.Message);
            }
        }

    
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            sensor = KinectSensor.KinectSensors[0];     // Get the Kinect attached to the system (can have a max of 4 kinects...we'll use the 1st one
           try
           {
               /*_nui.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseColor);
               _nui.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color); */
               
               sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(sensor_SkeletonFrameReady);   // listen to SkeletonTracking events
               sensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(sensor_ColorFrameReady);   // For display on WritableBitmap
               // Check & verify the above 2 lines of code
               // Now enable all he required streams
               sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
               sensor.DepthStream.Enable();
               sensor.SkeletonStream.Enable();
               
           }
            catch (Exception ex)
           {
             System.Diagnostics.Debug.WriteLine(ex.Message);
           }
           

           sensor.Start();
           sensor.ElevationAngle -= 5;
        
         
        }

        //Write on a WritableBitmap
        void sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            
            using (ColorImageFrame colorVideoFrame = e.OpenColorImageFrame())
            {
                // Could be null if we opened it too late
                if (colorVideoFrame == null)
                    return;
                // var image = e.ImageFrame.Image;
                byte[] pixeldata = new byte[colorVideoFrame.PixelDataLength];
                img.Source = BitmapSource.Create(colorVideoFrame.Width, colorVideoFrame.Height, 96, 96, PixelFormats.Bgr32, null,
                    pixeldata, colorVideoFrame.Width * colorVideoFrame.BytesPerPixel);
              
            }
        }

        void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
           
            canvas.Children.Clear();        // clear all existing Joints/JointNames displayed
            humanoid(e);        // send Skeleton data to MainWindow.Humanoid.cs
            
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                // as skeletonFrame could have opened late, when the data is no longer present
                if (skeletonFrame == null)
                    return;

                // Copies the data in a Skeleton array (6 items) 
                skeletonFrame.CopySkeletonDataTo(skeletonData);

                // Retrieves Skeleton objects with Tracked state
                var trackedSkeletons = skeletonData.Where(s => s.TrackingState == SkeletonTrackingState.Tracked);

                foreach(Skeleton seenSkeleton in trackedSkeletons)
                {
                    foreach (Joint joint in seenSkeleton.Joints)
                    {
                        DrawPoint(joint, Colors.Red);
                        WriteNames(joint);
                    }

                }

            }
            args_e = e;
            /*foreach (SkeletonData user in e.SkeletonFrame.Skeletons)
            {
              
                if (user.TrackingState == SkeletonTrackingState.Tracked)
                {
                    foreach (Joint joint in user.Joints)
                    {
                        
                        DrawPoint(joint, Colors.Red);
                        WriteNames(joint);
                    }
                }
            }*/
         }

        private void WriteNames(Joint joint)
        {
            var scaledJoint = ScaleJoint(joint);
            Label l = new Label();
            l.Margin = new Thickness(scaledJoint.Position.X, scaledJoint.Position.Y, 0, 0);
            l.Content=joint.JointType.ToString();       // SB: ID property has been renamed to JointType
            canvas.Children.Add(l);

        }

        private void DrawPoint(Joint joint, Color color)        // to draw a Tracked Skeleton's Joint on Bitmap
        {
            var scaledJoint = ScaleJoint(joint);

            Ellipse ellipse = new Ellipse
            {
                Fill = new SolidColorBrush(color),
                Width = 15,
                Height = 15,
                Opacity = 1,
                Margin = new Thickness(scaledJoint.Position.X, scaledJoint.Position.Y, 0, 0)
            };

            canvas.Children.Add(ellipse);
        }

        private Joint ScaleJoint(Joint joint)
        {
            // scales the Joint for a 640*480 display
            return new Joint()
            {
                JointType = joint.JointType, // as Joint.ID is deprecated
                // was Position = new Microsoft.Research.Nui.Vector
                Position = new Microsoft.Kinect.SkeletonPoint
                {
                    X = ScalePosition(640, joint.Position.X),
                    Y = ScalePosition(480, -joint.Position.Y),
                    Z = joint.Position.Z,
                    //W = joint.Position.W
                },
                TrackingState = joint.TrackingState
            };
        }

        private float ScalePosition(int size, float position)
        {
            float scaledPosition = (((size / 2) * position) + (size / 2));

            if (scaledPosition > size)
            {
                return size;
            }

            if (scaledPosition < 0)
            {
                return 0;
            }

            return scaledPosition;
        }

        #region Motor Control
        // For Kinect's Tilt Motor
        private void motorUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sensor.ElevationAngle += 5;
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Failed to move Kinect motor.");
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Elevation angle is out of range.");
            }
        }

        private void motorDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sensor.ElevationAngle -= 5;
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Failed to move Kinect motor.");
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Elevation angle is out of range.");
            }
        }

        #endregion

     



    }
}
