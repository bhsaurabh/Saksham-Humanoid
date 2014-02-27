//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: JointMoverTypes.cs $ $Revision: 1 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Ccr.Adapters.WinForms;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.Core.DsspHttp;
using System;
using System.Collections.Generic;

using W3C.Soap;
using diffdrive = Microsoft.Robotics.Services.Drive.Proxy;
using simtypes = Microsoft.Robotics.Simulation;
using simengine = Microsoft.Robotics.Simulation.Engine;
using Physics = Microsoft.Robotics.Simulation.Physics;
using Microsoft.Robotics.PhysicalModel;
using Microsoft.Robotics.Simulation.Physics;
using submgr = Microsoft.Dss.Services.SubscriptionManager;



namespace ProMRDS.Simulation.JointMover
{
    
    public static class Contract
    {
        public const string Identifier = "http://www.microsoft.com/contracts/2008/08/jointmover.user.html";
    }

    /// <summary>
    /// The JointMover State
    /// </summary>
    [DataContract()]
    public class JointMoverState
    {
        Dictionary<string, DOFDesc> _joints;

        [DataMember]
        public Dictionary<string, DOFDesc> Joints
        {
            get { return _joints; }
            set { _joints = value; }
        }
        private int _tick;
        [DataMember]
        public int tick
        {
            get {return _tick;}
            set {_tick = value ;}
        }
    }

    [DataContract]
    public enum DOFType
    {
        Twist = 0,
        Swing1 = 1,
        Swing2 = 2,
        X = 3,
        Y = 4,
        Z = 5
    }
    [DataContract]
    public enum Dynamixel
    {
        Lhand1 = 11,
        Lhand2 = 12,
        Lhand3 = 13,
        Lhand4 = 14,
        Rhand1 = 16,
        Rhand2 = 17,
        Rhand3 = 18,
        Rhand4 = 19,
        Legu1L=31,
        Legu2L=32,
        Legu3L=33,
        Legu4L=34,
        Legu5L=35,
        Legu6L=36,
        Legu1R = 21,
        Legu2R = 22,
        Legu3R = 23,
        Legu4R = 24,
        Legu5R = 25,
        Legu6R = 26,
        Torso2R=6,
        Torso1=7,
        Broadcast=254
    }
    [DataContract]
    public class DOFDesc
    {
        [DataMember]
        public string Name;
        [DataMember]
        public JointDesc Description;
        [DataMember]
        public DOFType Type;
        [DataMember]
        public float Minimum;
        [DataMember]
        public float Maximum;
        [DataMember]
        public float Scale;
        [DataMember]
        public bool IsVelocityDrive;
        [DataMember]
        public float DefaultDriveValue;

        public DOFDesc() { }

        public DOFDesc(string name, JointDesc description, DOFType type, float min, float max, bool isVelocityDrive,
            float defaultDriveValue)
        {
            Name = name;
            Description = description;
            Type = type;
            Minimum = min;
            Maximum = max;
            Scale = 100f / (max - min);
            IsVelocityDrive = isVelocityDrive;
            DefaultDriveValue = defaultDriveValue;
        }
    }

    [DataContract]
    public class JointDesc
    {
        [DataMember]
        public string Name;
        public simengine.VisualEntity JointEntity;
        [DataMember]
        public float Swing1Angle;
        [DataMember]
        public float Swing2Angle;
        [DataMember]
        public float TwistAngle;
        [DataMember]
        public float X;
        [DataMember]
        public float Y;
        [DataMember]
        public float Z;
        
        public PhysicsJoint Joint;

        public JointDesc() { }

        public JointDesc(string name, simengine.VisualEntity jointEntity, PhysicsJoint joint)
        {
            Name = name;
            JointEntity = jointEntity;
            Swing1Angle = 0;
            Swing2Angle = 0;
            TwistAngle = 0;
            X = Y = Z = 0;
            Joint = joint;
        }
        public Quaternion JointOrientation
        {
            get
            {
                Quaternion twist = Quaternion.FromAxisAngle(1, 0, 0, (float)(TwistAngle * Math.PI / 180));
                Quaternion swing1 = Quaternion.FromAxisAngle(0, 1, 0, (float)(Swing1Angle * Math.PI / 180));
                Quaternion swing2 = Quaternion.FromAxisAngle(0, 0, 1, (float)(Swing2Angle * Math.PI / 180));
                return twist * swing1 * swing2;
            }
        }

        public Vector3 JointPosition
        {
            get
            {
                return new Vector3(X, Y, Z);
            }
        }
    }
    
    
    /// <summary>
    /// JointMover Main Operations Port
    /// </summary>
    [ServicePort()]
    public class JointMoverOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, HttpGet, Subscribe, increment>
    {
    }

[DataContract]
    public class increment:Update<IncrementRequestType, PortSet<DefaultUpdateResponseType,Fault>>
    {
        public increment()
            : base(new IncrementRequestType())
        {
        }
    }
    [DataContract]
    public class IncrementRequestType
{
     

}

    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
        public Subscribe()
        {
        }
        public Subscribe(SubscribeRequestType body)
            : base(body)
        {
        }
        public Subscribe(SubscribeRequestType body, PortSet<SubscribeResponseType, Fault> responseport)
            : base(body, responseport)
        {
        }
    }
   
    /// <summary>
    /// JointMover Get Operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<JointMoverState, Fault>>
    {
        /// <summary>
        /// JointMover Get Operation
        /// </summary>
        public Get()
        {
        }
        /// <summary>
        /// JointMover Get Operation
        /// </summary>
        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body)
            : base(body)
        {
        }
        /// <summary>
        /// JointMover Get Operation
        /// </summary>
        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body, Microsoft.Ccr.Core.PortSet<JointMoverState, W3C.Soap.Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }

    #region WinForms communication

    public class FromWinformEvents : Port<FromWinformMsg>
    {
    }
   
    public class MoveJoint
    {
        public double Angle;
        public string Name;
        public MoveJoint(double angle, string name)
        {
            Angle = angle;
            Name = name;
        }
    }

    public class FromWinformMsg
    {
        public enum MsgEnum
        {
            Loaded,
            Reset,
            MoveJoint,
            Suspend,
            ChangeEntity,
            RefreshList,
            Start,
            Stop,
            Mirrorstart,
            Mirrorstop,
            Frontstand,
            test,
            Walk

        }

        private string[] _parameters;
        public string[] Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        private MsgEnum _command;
        public MsgEnum Command
        {
            get { return _command; }
            set { _command = value; }
        }

        private object _object;
        public object Object
        {
            get { return _object; }
            set { _object = value; }
        }

        public FromWinformMsg(MsgEnum command, string[] parameters)
        {
            _command = command;
            _parameters = parameters;
        }
        public FromWinformMsg(MsgEnum command, string[] parameters, object objectParam)
        {
            _command = command;
            _parameters = parameters;
            _object = objectParam;
        }
    }
    #endregion
}
