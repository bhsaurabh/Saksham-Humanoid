using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using W3C.Soap;

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
    /// <summary>
    /// Humanoid contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// DSS contract identifer for Humanoid
        /// </summary>
        [DataMember]
        public const string Identifier = "http://schemas.tempuri.org/2010/08/humanoid.html";
    }

    /// <summary>
    /// Humanoid state
    /// </summary>
    [DataContract]
    public class HumanoidState
    {
    }

    /// <summary>
    /// Humanoid main operations port
    /// </summary>
    [ServicePort]
    public class HumanoidOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Subscribe>
    {
    }

    /******************************************************************************************************/
    
    [DataContract]
    public class HumanoidEntity : SingleShapeEntity
    {
        

        // This class holds a description of each of the joints in the entity.
        class JointDesc
        {
            public string Name;
            public float Min;  // minimum allowable angle
            public float Max;  // maximum allowable angle
            public PhysicsJoint Joint; // Phyics Joint
            public float Target;  // Target joint position
            public float Current;  // Current joint position
            public float Speed;  // Rate of moving toward the target position
            public JointDesc(string name, float min, float max)
            {
                Name = name; Min = min; Max = max;
                Joint = null;
                Current = Target = 0;
                Speed = 30;
            }

            // Returns true if the specified target is within the valid bounds
            public bool ValidTarget(float target)
            {
                return ((target >= Min) && (target <= Max));
            }

            // Returns true if the joint is not yet at the target position
            public bool NeedToMove(float epsilon)
            {
                if (Joint == null) return false;
                return (Math.Abs(Target - Current) > epsilon);
            }

            // Takes one step toward the target position based on the specified time
            public void UpdateCurrent(double time)
            {
                float delta = (float)(time * Speed);
                if (Target > Current)
                    Current = Math.Min(Current + delta, Target);
                else
                    Current = Math.Max(Current - delta, Target);
            }
        }

        // Initialize an array of descriptions for each joint in the arm
        JointDesc[] _joints = new JointDesc[]
        {
            new JointDesc("Lleg1", 40, 140),
            new JointDesc("Lleg2", 40, 140),
            new JointDesc("Lleg3", 40, 140),
            new JointDesc("Lleg4", 40, 140),
            new JointDesc("Lleg5", 40, 140),
            new JointDesc("Lleg6", 40, 140),
            new JointDesc("Rleg6", 40, 140),
            new JointDesc("Rleg1", 40, 140),
            new JointDesc("Rleg2", 40, 140),
            new JointDesc("Rleg3", 40, 140),
            new JointDesc("Rleg4", 40, 140),
            new JointDesc("Rleg5", 40, 140),

            new JointDesc("Rhand1", 40, 140),
            new JointDesc("Rhand2", 40, 140),
            new JointDesc("Rhand3", 40, 140),
            new JointDesc("Rhand4", 40, 140),

            new JointDesc("Lhand1", 40, 140),
            new JointDesc("Lhand2", 40, 140),
            new JointDesc("Lhand3", 40, 140),
            new JointDesc("Lhand4", 40, 140),
        };

        /// <summary>
        /// Default constructor
        /// </summary>
        public HumanoidEntity() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="initialPos"></param>
        public HumanoidEntity(Shape shape, Vector3 initialPos)
            : base(shape, initialPos)
        {
        }

        bool _moveToActive = false;
        const float _epsilon = 0.01f;
        SuccessFailurePort _moveToResponsePort = null;
        double _prevTime = 0;

        public override void Update(FrameUpdate update)
        {
            // The joint member in each joint description needs to be set after all of the joints
            // have been created.  If this has not been done yet, do it now.
            if (_joints[0].Joint == null)
            {
                int index = 0;
                foreach (VisualEntity child in Children)
                {
                    // no joints in the camera
                    //if (child.GetType() == typeof(AttachedCameraEntity))
                       // continue;

                    _joints[index + 0].Joint = (PhysicsJoint)child.ParentJoint;
                   // _joints[index + 1].Joint = (PhysicsJoint)child.Children[0].ParentJoint;
                  //  _joints[index + 2].Joint = (PhysicsJoint)child.Children[0].Children[0].ParentJoint;
                   // _joints[index + 3].Joint = (PhysicsJoint)child.Children[0].Children[0].Children[0].ParentJoint;
                   // _joints[index + 4].Joint = (PhysicsJoint)child.Children[0].Children[0].Children[0].Children[0].ParentJoint;
                   // _joints[index + 5].Joint = (PhysicsJoint)child.Children[0].Children[0].Children[0].Children[0].Children[0].ParentJoint;
                    index += 6;
                }
            }

            base.Update(update);

            // update joints if necessary
            if (_moveToActive)
            {
                bool done = true;
                // Check each joint and update it if necessary.
                for (int index = 0; index < _joints.Length; index++)
                {
                    if (_joints[index].NeedToMove(_epsilon))
                    {
                        done = false;

                        Vector3 normal = _joints[index].Joint.State.Connectors[0].JointAxis;
                        _joints[index].UpdateCurrent(_prevTime);
                        _joints[index].Joint.SetAngularDriveOrientation(
                            Quaternion.FromAxisAngle(1, 0, 0, DegreesToRadians(_joints[index].Current)));
                    }
                }


                if (done)
                {
                    // no joints needed to be updated, the movement is finished
                    _moveToActive = false;
                    _moveToResponsePort.Post(new SuccessResult());
                }
            }
            _prevTime = update.ElapsedTime;
            
        }

        public SuccessFailurePort MoveTo(
            float FRShoulderFB,
            float FRShoulderUD,
            float FRElbow,
            float MRShoulderFB,
            float MRShoulderUD,
            float MRElbow,
            float RRShoulderFB,
            float RRShoulderUD,
            float RRElbow,
            float FLShoulderFB,
            float FLShoulderUD,
            float FLElbow,
           
            float time)
        {
            SuccessFailurePort responsePort = new SuccessFailurePort();

            if (_moveToActive)
            {
                responsePort.Post(new Exception("Previous MoveTo still active."));
                return responsePort;
            }

            // check bounds.  If the target is invalid, post an exception message to the response port with a helpful error.
            


            // set the target values on the joint descriptors
            _joints[0].Target = FRShoulderFB;
            _joints[1].Target = FRShoulderUD;
            _joints[2].Target = FRElbow;
            _joints[3].Target = MRShoulderFB;
            _joints[4].Target = MRShoulderUD;
            _joints[5].Target = MRElbow;
            _joints[6].Target = RRShoulderFB;
            _joints[7].Target = RRShoulderUD;
            _joints[8].Target = RRElbow;
            _joints[9].Target = FLShoulderFB;
            _joints[10].Target = FLShoulderUD;
            _joints[11].Target = FLElbow;
          

            // calculate a speed value for each joint that will cause it to complete its motion in the specified time
            for (int i = 0; i < _joints.Length; i++)
                _joints[i].Speed = Math.Abs(_joints[i].Target - _joints[i].Current) / time;

            // set this flag so that the motion is evaluated in the update method
            _moveToActive = true;

            // keep a pointer to the response port so we can post a result message to it.
            _moveToResponsePort = responsePort;

            return responsePort;
        }

        float DegreesToRadians(float degrees)
        {
            return (float)(degrees * Math.PI / 180);
        }
    }

/*******************************************************************************************************************************************/
    /*
    [DataContract]
    public class HumanoidEntity : SimplifiedConvexMeshEnvironmentEntity
    {


        // This class holds a description of each of the joints in the entity.
        class JointDesc
        {
            public string Name;
            public float Min;  // minimum allowable angle
            public float Max;  // maximum allowable angle
            public PhysicsJoint Joint; // Phyics Joint
            public float Target;  // Target joint position
            public float Current;  // Current joint position
            public float Speed;  // Rate of moving toward the target position
            public JointDesc(string name, float min, float max)
            {
                Name = name; Min = min; Max = max;
                Joint = null;
                Current = Target = 0;
                Speed = 30;
            }

            // Returns true if the specified target is within the valid bounds
            public bool ValidTarget(float target)
            {
                return ((target >= Min) && (target <= Max));
            }

            // Returns true if the joint is not yet at the target position
            public bool NeedToMove(float epsilon)
            {
                if (Joint == null) return false;
                return (Math.Abs(Target - Current) > epsilon);
            }

            // Takes one step toward the target position based on the specified time
            public void UpdateCurrent(double time)
            {
                float delta = (float)(time * Speed);
                if (Target > Current)
                    Current = Math.Min(Current + delta, Target);
                else
                    Current = Math.Max(Current - delta, Target);
            }
        }

        // Initialize an array of descriptions for each joint in the arm
        JointDesc[] _joints = new JointDesc[]
        {
            new JointDesc("Lleg1", 40, 140),
            new JointDesc("Lleg2", 40, 140),
            new JointDesc("Lleg3", 40, 140),
            new JointDesc("Lleg4", 40, 140),
            new JointDesc("Lleg5", 40, 140),
            new JointDesc("Lleg6", 40, 140),
            new JointDesc("Rleg6", 40, 140),
            new JointDesc("Rleg1", 40, 140),
            new JointDesc("Rleg2", 40, 140),
            new JointDesc("Rleg3", 40, 140),
            new JointDesc("Rleg4", 40, 140),
            new JointDesc("Rleg5", 40, 140),

            new JointDesc("Rhand1", 40, 140),
            new JointDesc("Rhand2", 40, 140),
            new JointDesc("Rhand3", 40, 140),
            new JointDesc("Rhand4", 40, 140),

            new JointDesc("Lhand1", 40, 140),
            new JointDesc("Lhand2", 40, 140),
            new JointDesc("Lhand3", 40, 140),
            new JointDesc("Lhand4", 40, 140),
        };

        /// <summary>
        /// Default constructor
        /// </summary>
        public HumanoidEntity() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="initialPos"></param>
        public HumanoidEntity(Shape shape, Vector3 initialPos, string meshresources)
            : base( initialPos, meshresources,shape)
        {
        }

        bool _moveToActive = false;
        const float _epsilon = 0.01f;
        SuccessFailurePort _moveToResponsePort = null;
        double _prevTime = 0;

        public override void Update(FrameUpdate update)
        {
            // The joint member in each joint description needs to be set after all of the joints
            // have been created.  If this has not been done yet, do it now.
            if (_joints[0].Joint == null)
            {
                int index = 0;
                foreach (VisualEntity child in Children)
                {
                    // no joints in the camera
                    //if (child.GetType() == typeof(AttachedCameraEntity))
                    // continue;

                    _joints[index + 0].Joint = (PhysicsJoint)child.ParentJoint;
                    // _joints[index + 1].Joint = (PhysicsJoint)child.Children[0].ParentJoint;
                    //  _joints[index + 2].Joint = (PhysicsJoint)child.Children[0].Children[0].ParentJoint;
                    // _joints[index + 3].Joint = (PhysicsJoint)child.Children[0].Children[0].Children[0].ParentJoint;
                    // _joints[index + 4].Joint = (PhysicsJoint)child.Children[0].Children[0].Children[0].Children[0].ParentJoint;
                    // _joints[index + 5].Joint = (PhysicsJoint)child.Children[0].Children[0].Children[0].Children[0].Children[0].ParentJoint;
                    index += 6;
                }
            }

            base.Update(update);

            // update joints if necessary
            if (_moveToActive)
            {
                bool done = true;
                // Check each joint and update it if necessary.
                for (int index = 0; index < _joints.Length; index++)
                {
                    if (_joints[index].NeedToMove(_epsilon))
                    {
                        done = false;

                        Vector3 normal = _joints[index].Joint.State.Connectors[0].JointAxis;
                        _joints[index].UpdateCurrent(_prevTime);
                        _joints[index].Joint.SetAngularDriveOrientation(
                            Quaternion.FromAxisAngle(1, 0, 0, DegreesToRadians(_joints[index].Current)));
                    }
                }


                if (done)
                {
                    // no joints needed to be updated, the movement is finished
                    _moveToActive = false;
                    _moveToResponsePort.Post(new SuccessResult());
                }
            }
            _prevTime = update.ElapsedTime;

        }

        public SuccessFailurePort MoveTo(
            float FRShoulderFB,
            float FRShoulderUD,
            float FRElbow,
            float MRShoulderFB,
            float MRShoulderUD,
            float MRElbow,
            float RRShoulderFB,
            float RRShoulderUD,
            float RRElbow,
            float FLShoulderFB,
            float FLShoulderUD,
            float FLElbow,

            float time)
        {
            SuccessFailurePort responsePort = new SuccessFailurePort();

            if (_moveToActive)
            {
                responsePort.Post(new Exception("Previous MoveTo still active."));
                return responsePort;
            }

            // check bounds.  If the target is invalid, post an exception message to the response port with a helpful error.



            // set the target values on the joint descriptors
            _joints[0].Target = FRShoulderFB;
            _joints[1].Target = FRShoulderUD;
            _joints[2].Target = FRElbow;
            _joints[3].Target = MRShoulderFB;
            _joints[4].Target = MRShoulderUD;
            _joints[5].Target = MRElbow;
            _joints[6].Target = RRShoulderFB;
            _joints[7].Target = RRShoulderUD;
            _joints[8].Target = RRElbow;
            _joints[9].Target = FLShoulderFB;
            _joints[10].Target = FLShoulderUD;
            _joints[11].Target = FLElbow;


            // calculate a speed value for each joint that will cause it to complete its motion in the specified time
            for (int i = 0; i < _joints.Length; i++)
                _joints[i].Speed = Math.Abs(_joints[i].Target - _joints[i].Current) / time;

            // set this flag so that the motion is evaluated in the update method
            _moveToActive = true;

            // keep a pointer to the response port so we can post a result message to it.
            _moveToResponsePort = responsePort;

            return responsePort;
        }

        float DegreesToRadians(float degrees)
        {
            return (float)(degrees * Math.PI / 180);
        }
    }


    [DataContract]
    public class SegmentEntity : SimplifiedConvexMeshEnvironmentEntity
    {
        private Joint _customJoint;

        [DataMember]
        public Joint CustomJoint
        {
            get { return _customJoint; }
            set { _customJoint = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SegmentEntity() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="initialPos"></param>
        public SegmentEntity(Shape shape, Vector3 initialPos, string meshresources)
            : base(initialPos, meshresources,shape)
        {
        }

        public override void Initialize(Microsoft.Xna.Framework.Graphics.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            base.Initialize(device, physicsEngine);

            // update the parent joint to match our custom joint parameters
            if (_customJoint != null)
            {
                if (ParentJoint != null)
                    PhysicsEngine.DeleteJoint((PhysicsJoint)ParentJoint);

                // restore the entity pointers in _customJoint after deserialization if necessary
                if (_customJoint.State.Connectors[0].Entity == null)
                    _customJoint.State.Connectors[0].Entity = FindConnectedEntity(_customJoint.State.Connectors[0].EntityName, this);

                if (_customJoint.State.Connectors[1].Entity == null)
                    _customJoint.State.Connectors[1].Entity = FindConnectedEntity(_customJoint.State.Connectors[1].EntityName, this);

                ParentJoint = _customJoint;
                PhysicsEngine.InsertJoint((PhysicsJoint)ParentJoint);
            }
        }

        VisualEntity FindConnectedEntity(string name, VisualEntity me)
        {
            // find the parent at the top of the hierarchy
            while (me.Parent != null)
                me = me.Parent;

            // now traverse the hierarchy looking for the name
            return FindConnectedEntityHelper(name, me);
        }

        VisualEntity FindConnectedEntityHelper(string name, VisualEntity me)
        {
            if (me.State.Name == name)
                return me;

            foreach (VisualEntity child in me.Children)
            {
                VisualEntity result = FindConnectedEntityHelper(name, child);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Override the base PreSerialize method so that we can properly serialize joints
        /// </summary>
        public override void PreSerialize()
        {
            base.PreSerialize();
            PrepareJointsForSerialization();
        }
    }
    /******************************************************************************************************************************/


    
    [DataContract]
    public class SegmentEntity : SingleShapeEntity
    {
        private Joint _customJoint;

        [DataMember]
        public Joint CustomJoint
        {
            get { return _customJoint; }
            set { _customJoint = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SegmentEntity() { }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="initialPos"></param>
        public SegmentEntity(Shape shape, Vector3 initialPos)
            : base(shape, initialPos)
        {
        }

        public override void Initialize(Microsoft.Xna.Framework.Graphics.GraphicsDevice device, PhysicsEngine physicsEngine)
        {
            base.Initialize(device, physicsEngine);

            // update the parent joint to match our custom joint parameters
            if (_customJoint != null)
            {
                if (ParentJoint != null)
                    PhysicsEngine.DeleteJoint((PhysicsJoint)ParentJoint);

                // restore the entity pointers in _customJoint after deserialization if necessary
                if (_customJoint.State.Connectors[0].Entity == null)
                    _customJoint.State.Connectors[0].Entity = FindConnectedEntity(_customJoint.State.Connectors[0].EntityName, this);

                if (_customJoint.State.Connectors[1].Entity == null)
                    _customJoint.State.Connectors[1].Entity = FindConnectedEntity(_customJoint.State.Connectors[1].EntityName, this);

                ParentJoint = _customJoint;
                PhysicsEngine.InsertJoint((PhysicsJoint)ParentJoint);
            }
        }

        VisualEntity FindConnectedEntity(string name, VisualEntity me)
        {
            // find the parent at the top of the hierarchy
            while (me.Parent != null)
                me = me.Parent;

            // now traverse the hierarchy looking for the name
            return FindConnectedEntityHelper(name, me);
        }

        VisualEntity FindConnectedEntityHelper(string name, VisualEntity me)
        {
            if (me.State.Name == name)
                return me;

            foreach (VisualEntity child in me.Children)
            {
                VisualEntity result = FindConnectedEntityHelper(name, child);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Override the base PreSerialize method so that we can properly serialize joints
        /// </summary>
        public override void PreSerialize()
        {
            base.PreSerialize();
            PrepareJointsForSerialization();
        }
    }
    

    /******************************************************************************************************/
    /// <summary>
    /// Humanoid get operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<HumanoidState, Fault>>
    {
        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        public Get()
        {
        }

        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        /// <param name="body">the request message body</param>
        public Get(GetRequestType body)
            : base(body)
        {
        }

        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        /// <param name="body">the request message body</param>
        /// <param name="responsePort">the response port for the request</param>
        public Get(GetRequestType body, PortSet<HumanoidState, Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }

    /// <summary>
    /// Humanoid subscribe operation
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        public Subscribe()
        {
        }

        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        /// <param name="body">the request message body</param>
        public Subscribe(SubscribeRequestType body)
            : base(body)
        {
        }

        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        /// <param name="body">the request message body</param>
        /// <param name="responsePort">the response port for the request</param>
        public Subscribe(SubscribeRequestType body, PortSet<SubscribeResponseType, Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }
}


