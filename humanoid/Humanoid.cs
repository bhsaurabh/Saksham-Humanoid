using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using W3C.Soap;
using submgr = Microsoft.Dss.Services.SubscriptionManager;

#region Simulation namespace


using Microsoft.Robotics.Simulation;
using Microsoft.Robotics.Simulation.Engine;
using engineproxy = Microsoft.Robotics.Simulation.Engine.Proxy;
using Microsoft.Robotics.Simulation.Physics;
using Microsoft.Robotics.PhysicalModel;

using xna = Microsoft.Xna.Framework;
using xnagrfx = Microsoft.Xna.Framework.Graphics;

#endregion 

namespace Humanoid
{
    [Contract(Contract.Identifier)]
    [DisplayName("Humanoid")]
    [Description("Humanoid service (no description provided)")]
    class HumanoidService : DsspServiceBase
    {

        [Partner("Engine",
            Contract = engineproxy.Contract.Identifier,
            CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate)]
        private engineproxy.SimulationEnginePort _engineStub =
            new engineproxy.SimulationEnginePort();

        

        /// <summary>
        /// Service state
        /// </summary>
        [InitialStatePartner(Optional=true,ServiceUri="humanoid.config.xml")]
        HumanoidState _state = new HumanoidState();
       
        /// <summary>
        /// Main service port
        /// </summary>
        [ServicePort("/Humanoid", AllowMultipleInstances = true)]
        HumanoidOperations _mainPort = new HumanoidOperations();

        [SubscriptionManagerPartner]
        submgr.SubscriptionManagerPort _submgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// Service constructor
        /// </summary>
        public HumanoidService(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
        }
        
        //double xpos = 0;
        ///double ypos = 1;
        //double zpos = 0;

        /// <summary>
        /// Service start
        /// </summary>
        protected override void Start()
        {

            // 
            // Add service specific initialization here
            // 

            base.Start();


            SetupCamera();

            // Add objects (entities) in our simulated world
            PopulateWorld();

            if (_state == null)
            {
                _state = new HumanoidState();
            }

            SaveState(_state);
        }

        private void SetupCamera()
        {
            // Set up initial view
            CameraView view = new CameraView();
            view.EyePosition = new Vector3(0f, 1f, 1f);
            view.LookAtPoint = new Vector3(0f, 1f, 0f);
            SimulationEngine.GlobalInstancePort.Update(view);
        }



        private void PopulateWorld()
        {
            AddSky();
            AddGround();

            HumanoidEntity Humanoid = CreateHumanoid("humanoid", new Vector3(0, 0, 0));
            Humanoid.Rotation = new xna.Vector3(0, 0, 0);
            SimulationEngine.GlobalInstancePort.Insert(Humanoid);
          //  insertrod();

            
        }


        public struct HumanoidShapeDescriptor
        {
            public Shapes ShapeID;
            public string Name;
            public double xPosition;
            public double yPosition;
            public double zPosition;
            public double xSize;
            public double ySize;
            public double zSize;
            public double radius;
            public float xRotation;
            public float yRotation;
            public float zRotation;
            public double mass;
            public string mesh;
            public HumanoidShapeDescriptor(
                Shapes _ShapeID,
                string _Name,
                double _xPosition,
                double _yPosition,
                double _zPosition,
                double _xSize,
                double _ySize,
                double _zSize,
                double _radius,
                float _xRotation,
                float _yRotation,
                float _zRotation,
                double _mass,
                string _mesh)
            {
                ShapeID = _ShapeID;
                Name = _Name;
                xPosition = _xPosition;
                yPosition = _yPosition;
                zPosition = _zPosition;
                xSize = _xSize;
                ySize = _ySize;
                zSize = _zSize;
                radius = _radius;
                xRotation = _xRotation;
                yRotation = _yRotation;
                zRotation = _zRotation;
                mass = _mass;
                mesh = _mesh;
            }
        }







        public class ParentChild
        {
            public string Parent;
            public string Child;
            public string JointName;
            public string Dof;
            public Vector3 ChildConnect;
            public Vector3 ParentConnect;
            public Vector3 ParentNormal;
            public Vector3 ParentAxis;
            public Vector3 ChildNormal;
            public Vector3 ChildAxis;
            public ParentChild(string child, string parent, string jointName, string dof, Vector3 childConnect, Vector3 parentConnect, Vector3 childNormal, Vector3 childAxis, Vector3 parentNormal, Vector3 parentAxis)
            {
                Child = child;
                Parent = parent;
                JointName = jointName;
                Dof = dof;
                ChildConnect = childConnect;
                ParentConnect = parentConnect;
                ParentNormal = parentNormal;
                ParentAxis = parentAxis;
                ChildNormal = childNormal;
                ChildAxis = childAxis;
            }
        }





        

        HumanoidShapeDescriptor[] ShapeDescriptors = new HumanoidShapeDescriptor[]
        {
            
           new HumanoidShapeDescriptor(Shapes.Box,	"Torso",	0    ,	1,   0,		.041f,	    .032f,    .052f,		0,	90,	-180,	-90,	.2f,	"tors.obj"),
           
           new HumanoidShapeDescriptor(Shapes.Box,	"Rhand1",	.08f    ,	.45f,   0,		.041f,	    .032f,    .052f,		0,	-90,	-90,	0,	.06f,	"motorbotom.obj"),
           new HumanoidShapeDescriptor(Shapes.Box,	"Rhand2",	.08f    ,	.35f,   0,		.041f,	    .032f,    .052f,		0,	0,	0,	-90,	.07f,	"rhand2.obj"),
           new HumanoidShapeDescriptor(Shapes.Box,	"Rhand3",	.08f,	.25f,   0,		.041f,	    .032f,    .052f,		0,	180,	0,	0,	.06f,	"motorbotom.obj"),
           new HumanoidShapeDescriptor(Shapes.Box,	"Rhand4",	.08f    ,	.15f,   0,		.041f,	    .032f,    .052f,		0,	-90,	-90,	0,	0.06f,	"Rhandg.obj"),
           new HumanoidShapeDescriptor(Shapes.Box,	"RGrip",	.08f   ,	.05f,   0,		.041f,	    .032f,    .052f,		0,	90,	0,	-90,	.01f,	"grip.obj"),
            
           new HumanoidShapeDescriptor(Shapes.Box,	"Lhand1",	.08f    ,    1.55f,   0,		.041f,	    .032f,    .052f,		0,	-90,	-90,	0,	.06f,	"motorbotom.obj"),
           new HumanoidShapeDescriptor(Shapes.Box,	"Lhand2",	.08f    ,	1.65f,   0,		.041f,	    .032f,    .052f,		0,	0,	0,	90,	.07f,	"rhand2.obj"),
           new HumanoidShapeDescriptor(Shapes.Box,	"Lhand3",	.08f,	1.75f,   0,		.041f,	    .032f,    .052f,		0,	180,	0,	0,	.06f,	"motorbotom.obj"),
           new HumanoidShapeDescriptor(Shapes.Box,	"Lhand4",	.08f    ,	1.85f,   0,		.041f,	    .032f,    .052f,		0,	-90,	-90,	0,	0.06f,	"Rhandg.obj"),
           new HumanoidShapeDescriptor(Shapes.Box,	"LGrip",	.08f    ,	1.95f,   0,		.041f,	    .032f,    .052f,		0,	90,	0,	-90,	.01f,	"grip.obj"),
           
            new HumanoidShapeDescriptor(Shapes.Box,	"Torso1",	0   ,	1,   .05f,		.041f,	    .052f,    .032f,		0,	180,	270,	0,	.06f,	"motorbotom.obj"),
            
            new HumanoidShapeDescriptor(Shapes.Box,	"legu1r",	0    ,	.95f,   0.08f,		.032f, .041f,	  .052f     ,		0,	0,	-90,	-90,	.06f,	"legu1r.obj"),
            new HumanoidShapeDescriptor(Shapes.Box,	"legu1l",	0    ,	1.05f,   0.08f,		.032f, .041f,	  .052f     ,		0,	0,	-90,	-90,	.06f,	"legu1l.obj"),
            
            new HumanoidShapeDescriptor(Shapes.Box,	"legu2r",	0    ,	.95,   .12,		.041f,	    .032f,    .052f,		0,	90,	0,	0,	.015f,	"legu2.obj"),
            new HumanoidShapeDescriptor(Shapes.Box,	"legu2l",	0    ,	1.05,   .12,		.041f,	    .032f,    .052f,		0,	90,	0,	0,	.015f,	"legu2.obj"),
            
            new HumanoidShapeDescriptor(Shapes.Box,	"legu3r",	0    ,	.95,   .2,		.041f,	    .032f,    .052f,		0,	0,	-90,	0,	.120f,	"assembly5u.obj"),
            new HumanoidShapeDescriptor(Shapes.Box,	"legu3l",	0   ,	1.05,   .2,		.041f,	    .032f,    .052f,		0,	0,	-90,	0,	.120f,	"assembly5u.obj"),
            
            new HumanoidShapeDescriptor(Shapes.Box,	"legu4r",	0    ,	.95,   .1,		.041f,	    .032f,    .052f,		0,	0,	-90,	0,	.011f,	"legparallel.obj"),
            new HumanoidShapeDescriptor(Shapes.Box,	"legu4l",	0    ,	1.15,   .2,		.041f,	    .032f,    .052f,		0,	0,	-90,	0,	.011f,	"legparallel.obj"),
            
            
            new HumanoidShapeDescriptor(Shapes.Box,	"legu5r",	0    ,	.95,   .35,		.041f,	    .032f,    .052f,		0,	0,	90,	0,	.058f,	"legknee.obj"),
            new HumanoidShapeDescriptor(Shapes.Box,	"legu5l",	0    ,	1.05,   .35,		.041f,	    .032f,    .052f,		0,	0,	90,	0,	.058f,	"legknee.obj"),
            
            
            new HumanoidShapeDescriptor(Shapes.Box,	"legu6r",	0    ,	.95,   .45,		.041f,	    .032f,    .052f,		0,	180,	-90,	0,	.120f,	"assembly5.obj"),
            new HumanoidShapeDescriptor(Shapes.Box,	"legu6l",	0   ,	1.05,   .45,		.041f,	    .032f,    .052f,		0,	180,	-90,	0,	.120f,	"assembly5.obj"),
            
            new HumanoidShapeDescriptor(Shapes.Box,	"legu7r",	0    ,	.95,   .55,		.041f,	    .032f,    .052f,		0,	-90,	0,	0,	.1f,	"part2.obj"),
            new HumanoidShapeDescriptor(Shapes.Box,	"legu7l",	0    ,	1.05,   .55,		.041f,	    .032f,    .052f,		0,	-90,	0,	0,	.1f,	"part2.obj"),
            
          
        };

        
    


        static Vector3 X = new Vector3(1, 0, 0);
        static Vector3 Y = new Vector3(0, 1, 0);
        static Vector3 Z = new Vector3(0, 0, 1);
        static Vector3 nX = new Vector3(-1, 0, 0);
        static Vector3 nY = new Vector3(0, -1, 0);
        static Vector3 nZ = new Vector3(0, 0, -1);


        ParentChild[] Relationships = new ParentChild[]
        { 
            
            
            new ParentChild("Torso1",    "Torso",		"Torso1",    "Swing1",   new Vector3(0,.02375f,-.0005f),	        new Vector3(.0003f,-0.0175f,-0.0163f),	Y,	X,		Y,	Z),
            
            
            new ParentChild("legu1l",    "Torso1",		"Torso2L",    "Twist", new Vector3(.017f,.0015f,0.005f),	        new Vector3(0.0005f,-.016f,.02175f),	Y,	nX,		Y,	Z),
            new ParentChild("legu1r",    "Torso1",		"Torso2R",    "Twist", new Vector3(-.017f,.0015f,0.005f),	        new Vector3(0.0005f,-.016f,-.021f),	Y,	nX,		Y,	Z),
         
            new ParentChild("legu2r",    "legu1r",    	"legu1R",    "Twist",   new Vector3(0,0,0),      new Vector3(0,-0.02175f,0.015f),	Z,	Y,		Z,	Y),
            new ParentChild("legu2l",    "legu1l",    	"legu1L",    "Twist",   new Vector3(0,0,0),      new Vector3(0,-0.02175f,0.015f),	Z,	Y,		Z,	Y),
              
              
            new ParentChild("legu3r",    "legu2r",    	"legu2R",    "Swing1",   new Vector3(0,0.015f,0.02175f), new Vector3(0,-0.022f,-0.0385f),	Z,	Y,		nZ,	Y),
            new ParentChild("legu3l",    "legu2l",    	"legu2L",    "Swing1",   new Vector3(0,0.015f,0.02175f), new Vector3(0,-0.022f,-0.0385f),	Z,	Y,		nZ,	Y),
            
            new ParentChild("legu4r",    "legu3r",    	"legu3R",    "Swing1",   new Vector3(-0.04075f,0.031197f,0.007301f),      new Vector3(0.02175f,-0.01f,-0.03425f),	X,	Y,		nX,	Y),
            new ParentChild("legu4l",    "legu3l",    	"legu3L",    "Swing1",   new Vector3(-0.04075f,0.031197f,0.007301f),      new Vector3(0.02175f,-0.01f,-0.03425f),	X,	Y,		nX,	Y),
             
            new ParentChild("legu5r",    "legu4r",    	"legu4R",    "Swing1",   new Vector3(0.004f,0.042f,0.006236f),      new Vector3(-0.00075f,-0.012803f,-0.018699f),	X,	Y,		X,	Y),
            new ParentChild("legu5l",    "legu4l",    	"legu4L",    "Swing1",   new Vector3(0.004f,0.042f,0.006236f),      new Vector3(-0.00075f,-0.012803f,-0.018699f),	X,	Y,		X,	Y),
            
            
            new ParentChild("legu6r",    "legu5r",    	"legu5R",    "Swing1",   new Vector3(0.02125f,0.01042f,-0.03775f),      new Vector3(0.004f,-0.028f,0.007f),	X,	Y,		X,	Y),
            new ParentChild("legu6l",    "legu5l",    	"legu5L",    "Swing1",   new Vector3(0.02125f,0.01042f,-0.03775f),      new Vector3(0.004f,-0.028f,0.007f),	X,	Y,		X,	Y),
            
            new ParentChild("legu7r",    "legu6r",    	"legu6R",    "Swing2",   new Vector3(0,.02f,.04f),      new Vector3(0,-0.016f,0.02175f),	X,	Y,		X,	Y),
            new ParentChild("legu7l",    "legu6l",    	"legu6L",    "Swing2",   new Vector3(0,.02f,.04f),      new Vector3(0,-0.016f,0.02175f),	X,	Y,		X,	Y),
           
              
            
          
            new ParentChild("Rhand1",    "Torso",		"Rhand1",    "Swing1",   new Vector3(-0.02375f,.0005f,-.0015f),	        new Vector3(0.05574f,0.06325f,-0.021f),	X,	Y,		X,	Y),
            new ParentChild("Rhand2",    "Rhand1",		"Rhand2",    "Twist",   new Vector3(0.002f,.042547f,.019962f),	        new Vector3(0.016f,0,0.01975f),	X,	Z,		X,	Z), 
           new ParentChild("Rhand3",    "Rhand2",		"Rhand3",    "Swing2",   new Vector3(-0.0015f,.02375f,.0005f),	        new Vector3(0.016f,-0.02175f,0),	X,	Z,		X,	Z), 
          new ParentChild("Rhand4",    "Rhand3",		"Rhand4",    "Swing1",   new Vector3(0.015067f,.0345f,-.002f),	        new Vector3(0.01775f,-0.016f,0),	X,	Z,		X,	Z), 
           new ParentChild("RGrip",    "Rhand4",		"RGrip",    "Twist",   new Vector3(-0.01775f,.01138f,.0161f),	        new Vector3(0.016f,0,0.01875f),	X,	Z,		X,	Z), 
           
            new ParentChild("Lhand1",    "Torso",		"Lhand1",    "Swing1",   new Vector3(-0.02375f,.0005f,-.0015f),	        new Vector3(-0.05574f,0.06325f,-0.021f),	nX,	Z,		X,	Z),
            new ParentChild("Lhand2",    "Lhand1",		"Lhand2",    "Twist",   new Vector3(-0.002f,-.043547f,.019962f),	        new Vector3(0.014f,0.00f,-0.0185f),	nX,	nZ,		X,	Z), 
           new ParentChild("Lhand3",    "Lhand2",		"Lhand3",    "Swing2",   new Vector3(-0.0015f,.02375f,.0005f),	        new Vector3(-0.016f,0.02175f,0),	nX,	Z,		X,	Z), 
           new ParentChild("Lhand4",    "Lhand3",		"Lhand4",    "Swing1",   new Vector3(0.015067f,.0345f,-.002f),	        new Vector3(0.01775f,-0.016f,0),	X,	Z,		X,	Z), 
           new ParentChild("LGrip",    "Lhand4",		"LGrip",    "Twist",   new Vector3(-0.01775f,.01138f,.0161f),	        new Vector3(0.016f,0,0.01875f),	X,	Z,		X,	Z), 
           };

        string _parentName = "Torso";






        HumanoidEntity CreateHumanoid(string name, Vector3 initialPosition)
        {
            Dictionary<string, VisualEntity> humanoidShapes = new Dictionary<string, VisualEntity>();
            string prefix = name + "_";

            foreach (HumanoidShapeDescriptor desc in ShapeDescriptors)
            {
                Shape newShape = null;
                
                     
                        newShape = new BoxShape(new BoxShapeProperties(
                            desc.Name + " Shape", (float)desc.mass, new Pose(), new Vector3((float)desc.xSize, (float)desc.ySize, (float)desc.zSize)));
                      
               // newShape = new ConvexMeshShape(new ConvexMeshShapeProperties(desc.Name + "Shape", desc.mesh));

                    
                SingleShapeEntity shapeEntity = null;
                
                if (desc.Name == _parentName)
                {
                    shapeEntity = new HumanoidEntity(newShape, new Vector3(
                        (float)desc.xPosition + initialPosition.X,
                        (float)desc.yPosition + initialPosition.Y,
                        (float)desc.zPosition + initialPosition.Z));
                }
                else
                {
                    shapeEntity = new SegmentEntity(newShape, new Vector3(
                        (float)desc.xPosition + initialPosition.X,
                        (float)desc.yPosition + initialPosition.Y,
                        (float)desc.zPosition + initialPosition.Z));
                }
                
                shapeEntity.State.Name = prefix + desc.Name;
                shapeEntity.State.Pose.Orientation = UIMath.EulerToQuaternion(new xna.Vector3((float)desc.xRotation, (float)desc.yRotation, (float)desc.zRotation));
                if (!string.IsNullOrEmpty(desc.mesh))
                    shapeEntity.State.Assets.Mesh = desc.mesh;
                shapeEntity.MeshScale = new Vector3(.001f, .001f, .001f);
                shapeEntity.MeshRotation = new Vector3(desc.xRotation,desc.yRotation, desc.zRotation);
                shapeEntity.Flags = VisualEntityProperties.DoCompletePhysicsShapeUpdate;

             //  shapeEntity.State.Flags |= EntitySimulationModifiers.Kinematic;
                humanoidShapes.Add(shapeEntity.State.Name, shapeEntity);
            }
            
            // now set up the Parent/Child relationships
            foreach (ParentChild rel in Relationships)
            {
                string Dof = rel.Dof;
                JointAngularProperties angular = new JointAngularProperties();

                if (Dof=="Twist")
                {
                    angular.TwistMode = JointDOFMode.Free;
                    angular.TwistDrive = new JointDriveProperties(JointDriveMode.Position, new SpringProperties(500000, 1000, 0), 1000000);
                   // angular.TwistDrive.Mode = JointDriveMode.Velocity;
                }

                if (Dof=="Swing1")
                {
                    angular.Swing1Mode = JointDOFMode.Free;
                    angular.SwingDrive = new JointDriveProperties(JointDriveMode.Position, new SpringProperties(500000, 1000, 0), 1000000);
                    //angular.SwingDrive.Mode = JointDriveMode.Velocity;
                }

                if (Dof == "Swing2")
                {
                    angular.Swing2Mode = JointDOFMode.Free;
                    angular.SwingDrive = new JointDriveProperties(JointDriveMode.Position, new SpringProperties(500000, 1000, 0), 1000000);
                   // angular.SwingDrive.Mode = JointDriveMode.Velocity;
                }

                EntityJointConnector[] connectors = new EntityJointConnector[]
                {
                    new EntityJointConnector(humanoidShapes[prefix + rel.Child], rel.ChildNormal, rel.ChildAxis, rel.ChildConnect),
                    new EntityJointConnector(humanoidShapes[prefix + rel.Parent], rel.ParentNormal, rel.ParentAxis, rel.ParentConnect)
                };

                SegmentEntity child = (SegmentEntity)humanoidShapes[prefix + rel.Child];
                child.CustomJoint = new Joint();
                child.CustomJoint.State = new JointProperties(angular, connectors);
                child.CustomJoint.State.Name = rel.JointName;
                child.Flags = VisualEntityProperties.DoCompletePhysicsShapeUpdate;
             
              
                
                humanoidShapes[prefix + rel.Parent].InsertEntityGlobal(humanoidShapes[prefix + rel.Child]);
            }
            
            
            HumanoidEntity retValue = (HumanoidEntity)humanoidShapes[prefix + _parentName];




           // retValue.State.MassDensity.CenterOfMass = new Pose(new Vector3(0.05f,1.05f,0), new Quaternion(0,0,0,1));
           retValue.State.Flags = EntitySimulationModifiers.Kinematic;
            retValue.State.Name = name;
            
            return retValue;
        }




        void insertrod()
        {
            Shape rod = new CapsuleShape(new CapsuleShapeProperties(1000,new Pose(),.008f,3));
            SingleShapeEntity hangrod = new SingleShapeEntity(rod, new Vector3(0, 1, .05f));
            hangrod.State.Name = "hangrod";
            hangrod.Rotation = new Microsoft.Xna.Framework.Vector3(0, 0, 90);
            //hangrod.State.Pose.Orientation = new Quaternion(0, 0, 1, 1);
            hangrod.State.Flags = EntitySimulationModifiers.Kinematic;
            SimulationEngine.GlobalInstancePort.Insert(hangrod);
        }

        void AddSky()
        {
            // Add a sky using a static texture. We will use the sky texture
            // to do per pixel lighting on each simulation visual entity
            SkyDomeEntity sky = new SkyDomeEntity("skydome.dds", "sky_diff.dds");
            SimulationEngine.GlobalInstancePort.Insert(sky);

            // Add a directional light to simulate the sun.
            LightSourceEntity sun = new LightSourceEntity();
            sun.State.Name = "Sun";
            sun.Type = LightSourceEntityType.Directional;
            sun.Color = new Vector4(0.8f, 0.8f, 0.8f, 1);
            sun.Direction = new Vector3(0.5f, -.75f, 0.5f);
            SimulationEngine.GlobalInstancePort.Insert(sun);
        }

        private void AddGround()
        {
            HeightFieldEntity ground = new HeightFieldEntity("Simple Ground ", "03RamieSc.dds", new MaterialProperties("ground", .2f, .99f, .99f));
            SimulationEngine.GlobalInstancePort.Insert(ground);
        }



        /// <summary>
        /// Handles Subscribe messages
        /// </summary>
        /// <param name="subscribe">the subscribe request</param>
        [ServiceHandler]
        public void SubscribeHandler(Subscribe subscribe)
        {
            SubscribeHelper(_submgrPort, subscribe.Body, subscribe.ResponsePort);
        }
    }
}


/*
 <ServiceRecordType>
      <dssp:Contract>http://www.microsoft.com/contracts/2008/08/jointmover.user.html</dssp:Contract>
      <dssp:PartnerList>
        <dssp:Partner>
          <dssp:Service>http://localhost/humanoid</dssp:Service>
          <dssp:PartnerList />
          <dssp:Name>simulation:Entity</dssp:Name>
        </dssp:Partner>
      </dssp:PartnerList>
      <Name>this:JointMover</Name>
    </ServiceRecordType>
*/
