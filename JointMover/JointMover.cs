//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: JointMover.cs $ $Revision: 1 $
//-----------------------------------------------------------------------

// Editted for Saksham - Humanoid robot project
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using submgr = Microsoft.Dss.Services.SubscriptionManager;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

using simtypes = Microsoft.Robotics.Simulation;
using sim = Microsoft.Robotics.Simulation;
using simengine = Microsoft.Robotics.Simulation.Engine;
using Physics = Microsoft.Robotics.Simulation.Physics;
using System.ComponentModel;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Robotics.PhysicalModel;
using Microsoft.Robotics.Simulation.Physics;
using W3C.Soap;
using Microsoft.Ccr.Adapters.WinForms;
using System.IO.Ports;

using System.IO;

namespace ProMRDS.Simulation.JointMover
{
    [DisplayName("(User) Joint Mover")]
    [Description("Allows joints in a sim entity to be manipulated.")]
    [DssCategory(simtypes.PublishedCategories.SimulationService)]
    [Contract(Contract.Identifier)]
    public class JointMoverService : DsspServiceBase
    {
        #region Simulation Variables
        simengine.SimulationEnginePort _simEngine;
        simengine.VisualEntity _entity;
        simengine.SimulationEnginePort _notificationTarget;
        #endregion

        // This port receives events from the user interface
        FromWinformEvents _fromWinformPort = new FromWinformEvents();
        SimulatedBipedMoverUI _simulatedBipedMoverUI = null;

        System.IO.Ports.SerialPort SerialPort1 = new SerialPort();

        [SubscriptionManagerPartner]
        [Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        submgr.SubscriptionManagerPort _subMgrPort = new submgr.SubscriptionManagerPort();
        string _subMgrUri = string.Empty;

        [InitialStatePartner(Optional = true)]
        private JointMoverState _state = new JointMoverState();

        [ServicePort("/SimulatedJointMover", AllowMultipleInstances = true)]
        private JointMoverOperations _mainPort = new JointMoverOperations();
        public JointMoverService(DsspServiceCreationPort creationPort) :
            base(creationPort)
        {
        }

        protected override void Start()
        {
            _simEngine = simengine.SimulationEngine.GlobalInstancePort;
            _notificationTarget = new simengine.SimulationEnginePort();
            
            if (_state == null)
                CreateDefaultState();

            // PartnerType.Service is the entity instance name. 
            _simEngine.Subscribe(ServiceInfo.PartnerList, _notificationTarget);
            

            // don't start listening to DSSP operations, other than drop, until notification of entity
            Activate(new Interleave(
                new TeardownReceiverGroup
                (
                    Arbiter.Receive<simengine.InsertSimulationEntity>(false, _notificationTarget, InsertEntityNotificationHandlerFirstTime),
                    Arbiter.Receive<DsspDefaultDrop>(false, _mainPort, DefaultDropHandler)
                ),
                new ExclusiveReceiverGroup
                (
                    Arbiter.Receive<FromWinformMsg>(true, _fromWinformPort, OnWinformMessageHandler)
                ),
                new ConcurrentReceiverGroup()
            ));

            // Create the user interface form
            WinFormsServicePort.Post(new Microsoft.Ccr.Adapters.WinForms.RunForm(CreateForm));
            increment incr = new increment();
            base.SendNotification(_subMgrPort, incr);
            Activate(Arbiter.Receive(false, TimeoutPort(5000), dateTime => SpawnIterator(RefreshListIterator)));
        
        }

        // Create the UI form
        System.Windows.Forms.Form CreateForm()
        {
            return new SimulatedBipedMoverUI(_fromWinformPort);
        }

        bool mirror = false;
        // process messages from the UI Form
        void OnWinformMessageHandler(FromWinformMsg msg)
        {
            switch (msg.Command)
            {
                case FromWinformMsg.MsgEnum.Start:
                    // the windows form is ready to go
                    Console.WriteLine("Start serial port");
                    StartSerial();
                    break;
                case FromWinformMsg.MsgEnum.Mirrorstart:
                    // the windows form is ready to go
                    mirror = true;
                    break;
                case FromWinformMsg.MsgEnum.Mirrorstop:
                    // the windows form is ready to go
                    mirror = false;
                    break;
                case FromWinformMsg.MsgEnum.Stop:
                    // the windows form is ready to go
                    Console.WriteLine("STOPPED serial port");
                    try
                    {
                        SerialPort1.Close();
                    }
                    catch
                    {
                    }
                    comportready = false;
                    break;
                case FromWinformMsg.MsgEnum.Reset:
                    // the windows form is ready to go
                    Reset();

                    break;
                case FromWinformMsg.MsgEnum.Frontstand:
                    // the windows form is ready to go
                    FrontStand();

                    break;
                case FromWinformMsg.MsgEnum.Walk:
                    // the windows form is ready to go
                    walk = !walk;
                    Walk();

                    break;
               
                case FromWinformMsg.MsgEnum.Loaded:
                    // the windows form is ready to go
                    _simulatedBipedMoverUI = (SimulatedBipedMoverUI)msg.Object;
                    break;

                case FromWinformMsg.MsgEnum.MoveJoint:
                    MoveJoint((MoveJoint)msg.Object);
                        if(comportready)
                        SendToJoint((MoveJoint)msg.Object);

                    break;

                case FromWinformMsg.MsgEnum.Suspend:
                    Task<simengine.VisualEntity, bool> deferredTask =
                        new Task<simengine.VisualEntity, bool>(_entity, (bool)msg.Object, SuspendBipedInternal);
                    _entity.DeferredTaskQueue.Post(deferredTask);
                    break;

                case FromWinformMsg.MsgEnum.ChangeEntity:
                    {
                        string entityName = (string)msg.Object;
                        if (string.IsNullOrEmpty(entityName) == false)
                        {
                            if (_entity == null || _entity.State.Name != entityName || _state.Joints==null || _state.Joints.Count == 0)
                            {
                                DeleteEntityInternal();
                                simengine.EntitySubscribeRequestType req = new Microsoft.Robotics.Simulation.Engine.EntitySubscribeRequestType();
                                req.Name = entityName;
                                _simEngine.Subscribe(req, _notificationTarget);
                            }
                        }
                        break;
                    }

                case FromWinformMsg.MsgEnum.RefreshList:
                    {
                        SpawnIterator(RefreshListIterator);
                        break;
                    }
            }
        }
        bool comportready=false;
        bool walk = false;
                public void StartSerial()
        {
            SerialPort1.PortName = "COM23";
            SerialPort1.BaudRate = 1000000;
            
            try
            {

                SerialPort1.Open();
            }
            catch
            {
                Console.WriteLine("unable to open COM23");
            }

            comportready = true;
           
        }
                public void Walk()
                {
                    
                    byte id;
                    int angle;
                    int n;
                    int velocity;
                    bool status=true;
                    TextReader tr = new StreamReader("walk.txt");
                    string read = tr.ReadToEnd();
                    tr.Close();

                    string[] integerStrings = read.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    int[] data = new int[integerStrings.Length];
                    
                    for (n = 0; n < integerStrings.Length; n++)
                    {
                        data[n] = (int.Parse(integerStrings[n]));
                       }
                   byte  count = 15;
                    
                   // while (count>0)
                    {
                        count--;
                        for (n = 0; n < integerStrings.Length;)
                        {
                            for (byte i = 0; i < 4; i++)
                            {
                                id = (byte)data[n];
                                angle = data[n + 1];
                                velocity = data[n + 2];
                                SerialSend(id, angle, velocity);
                                System.Threading.Thread.Sleep(1);
                            }
                            System.Threading.Thread.Sleep(data[n + 3]);
                            //status=ReadStatus(id);
                            if (status)
                            {
                                n = n + 4;
                            }
                            

                        }
                    }
                  
                }
                public void FrontStand()
                {
                    byte id;
                    int angle;
                    id = (byte)Dynamixel.Lhand2;
                    angle = -25;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(75);
                    id = (byte)Dynamixel.Rhand2;
                    angle = -angle;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(100);

                    id = (byte)Dynamixel.Lhand1;
                    angle = 90;
                    SerialSend(id, angle, 250);
                    id = (byte)Dynamixel.Rhand1;
                    System.Threading.Thread.Sleep(75);
                    angle = -angle;
                    SerialSend(id, angle, 250);

                    System.Threading.Thread.Sleep(50);

                    id = (byte)Dynamixel.Torso2R;                 //t2r
                    angle = 66;
                    SerialSend(id, angle, 75);
                    System.Threading.Thread.Sleep(200);

                    id = (byte)Dynamixel.Lhand1;
                    angle = 119;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(10);
                    id = (byte)Dynamixel.Rhand1;
                    angle = -angle;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(200);

                    id = (byte)Dynamixel.Torso2R;                 //t2r
                    angle = 100;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(200);

                    id = (byte)Dynamixel.Lhand1;
                    angle = 133;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(10);
                    id = (byte)Dynamixel.Rhand1;
                    angle = -angle;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(200);
                    //
                    id = (byte)Dynamixel.Legu3L;
                    angle = 18;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(75);
                    id = (byte)Dynamixel.Legu3R;
                    angle = -18;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(100);

                    id = (byte)Dynamixel.Legu5L;
                    angle = -56;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(75);
                    id = (byte)Dynamixel.Legu5R;
                    angle = 56;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(100);

                    id = (byte)Dynamixel.Legu3L;
                    angle = 56;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(75);
                    id = (byte)Dynamixel.Legu3R;
                    angle = -56;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(100);

                    id = (byte)Dynamixel.Legu5L;
                    angle = -3;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(75);
                    id = (byte)Dynamixel.Legu5R;
                    angle = 3;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(100);
                    //
                    id = (byte)Dynamixel.Legu4L;
                    angle = -33;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(75);
                    id = (byte)Dynamixel.Legu4R;
                    angle = 33;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(100);

                    id = (byte)Dynamixel.Legu5L;
                    angle = 29;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(75);
                    id = (byte)Dynamixel.Legu5R;
                    angle = -29;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(100);

                    id = (byte)Dynamixel.Legu4L;
                    angle = -40;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(75);
                    id = (byte)Dynamixel.Legu4R;
                    angle = 40;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(100);

                    id = (byte)Dynamixel.Legu5L;
                    angle = 49;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(75);
                    id = (byte)Dynamixel.Legu5R;
                    angle = -49;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(100);
                    //pulling back now
                    id = (byte)Dynamixel.Lhand1;
                    angle = 0;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(75);
                    id = (byte)Dynamixel.Rhand1;
                    angle = -angle;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(1000);

                    id = (byte)Dynamixel.Torso2R;                 //t2r
                    angle = -40;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(1200);


                    id = (byte)Dynamixel.Legu5L;                 //t2r
                    angle = 0;
                    SerialSend(id, angle, 65);

                    id = (byte)Dynamixel.Legu5R;                 //t2r
                    angle = 0;
                    SerialSend(id, angle, 65);
                    System.Threading.Thread.Sleep(20);

                    id = (byte)Dynamixel.Legu4L;                 //t2r
                    angle = -7;
                    SerialSend(id, angle, 45);

                    id = (byte)Dynamixel.Legu4R;                 //t2r
                    angle = 7;
                    SerialSend(id, angle, 45);
                    System.Threading.Thread.Sleep(10);

                    id = (byte)Dynamixel.Legu3L;                 //t2r
                    angle = 0;
                    SerialSend(id, angle, 75);

                    id = (byte)Dynamixel.Legu3R;                 //t2r
                    angle = 0;
                    SerialSend(id, angle, 75);
                    System.Threading.Thread.Sleep(10);

                    id = (byte)Dynamixel.Torso2R;                 //t2r
                    angle = 0;
                    SerialSend(id, angle, 30);

                    System.Threading.Thread.Sleep(10);

                    id = (byte)Dynamixel.Lhand2;
                    angle = 0;
                    SerialSend(id, angle, 150);
                    System.Threading.Thread.Sleep(10);
                    id = (byte)Dynamixel.Rhand2;
                    angle = -angle;
                    SerialSend(id, angle, 150);
                    
                }
         public void Reset()
        {
            
            SerialSend(254, 0,100);
            ControlJoint((byte)Dynamixel.Legu4L, -7, 100);
            ControlJoint((byte)Dynamixel.Legu4R, 7, 100);
            ControlJoint((byte)Dynamixel.Torso1, 7, 100);
            ControlJoint((byte)Dynamixel.Legu2L, -5, 100);
            ControlJoint((byte)Dynamixel.Legu2R, 10, 100);
            
        }
         public bool ControlJoint(byte id, int angle, int velocity)
         {
             //do
             {
             System.Threading.Thread.Sleep(2);
             SerialSend(id, angle, velocity);
             }//while(!ReadStatus());
             
             return true;
         }
               public void SendToJoint(MoveJoint parameter)
        {
            byte id;
            int angle;
            
          
                       
            switch(parameter.Name)
            {
                case "Lhand1": id=11;
                    SerialSend(id, (int)parameter.Angle,0);
                    id = 16;
                    angle =  - (int)parameter.Angle;
                    if(mirror)
                    SerialSend(id, angle,0);
                             break;
                case "Lhand2": id = 12;
                             SerialSend(id, (int)parameter.Angle,0);
                             id = 17;
                             angle =  - (int)parameter.Angle;
                             if (mirror) 
                                 SerialSend(id, angle,0);
                             break;
                case "Lhand3": id = 13;
                             SerialSend(id, (int)parameter.Angle,0);
                             id = 18;
                             angle =  - (int)parameter.Angle;
                             if (mirror) 
                                 SerialSend(id, angle,0);
                             break;
                case "Lhand4": id = 14;
                             SerialSend(id, (int)parameter.Angle,0);
                             id = 19;
                             angle =  - (int)parameter.Angle;
                             if (mirror) 
                                 SerialSend(id, angle,0);
                             break;
                
                case "legu1L": id = 31;
                            SerialSend(id, (int)parameter.Angle,0);
                            id = 21;
                            angle =  - (int)parameter.Angle;
                            if (mirror)
                                SerialSend(id, angle,0);
                            break;
                case "legu2L": id = 32;
                            SerialSend(id, (int)parameter.Angle,0);
                            id = 22;
                            angle =  - (int)parameter.Angle;
                            angle =  (int)parameter.Angle;
                            if (mirror) 
                            SerialSend(id, angle,0);
                            id = 36;                                    //for one leg stand
                            //SerialSend(id, (int)parameter.Angle,0);
                            id = 26;
                            angle = (int)parameter.Angle;
                            //SerialSend(id, angle,0);
                            break;
                case "legu3L": id = 33;
                            SerialSend(id, (int)parameter.Angle,0);
                            id = 23;
                            angle =  - (int)parameter.Angle;
                            if (mirror) 
                                SerialSend(id, angle,0);
                            break;
                case "legu4L": id = 34;
                            SerialSend(id, (int)parameter.Angle,0);
                            id = 24;
                            angle = - (int)parameter.Angle;
                            if (mirror) 
                                SerialSend(id, angle,0);
                            break;
                case "legu5L": id = 35;
                            SerialSend(id, (int)parameter.Angle,0);
                            id = 25;
                            angle =  - (int)parameter.Angle;
                            if (mirror) 
                                SerialSend(id, angle,0);
                            break;
                case "legu6L": id = 36;
                            SerialSend(id, (int)parameter.Angle,0);
                            id = 26;
                            angle =  - (int)parameter.Angle;
                            if (mirror) 
                                SerialSend(id, angle,0);
                            break;
                case "legu1R": id = 21;
                            angle =  - (int)parameter.Angle;
                            SerialSend(id, angle,0);
                            break;
                case "legu2R": id = 22;
                            angle = - (int)parameter.Angle;
                            SerialSend(id, angle,0);
                            break;
                case "legu3R": id = 23;
                            angle =  - (int)parameter.Angle;
                            SerialSend(id, angle,0);
                            break;
                case "legu4R": id = 24;
                            angle =  - (int)parameter.Angle;
                            SerialSend(id, angle,0);
                            break;
                case "legu5R": id = 25;
                            angle =  - (int)parameter.Angle;
                            SerialSend(id, angle,0);
                            break;
                case "legu6R": id = 26;
                            angle =  - (int)parameter.Angle;
                            SerialSend(id, angle,0);
                            break;
                case "Torso1": id = 7;
                            SerialSend(id, (int)parameter.Angle,0);
                            break;
                case "Torso2R": id=6;
                            SerialSend(id, (int)parameter.Angle,0);
                            break;
                case "Torso2L": id = 6;
                            SerialSend(id, (int)parameter.Angle,0);
                            break;
                case "Rhand1": id=16;
                            angle =  - (int)parameter.Angle;
                            SerialSend(id, angle,0);
                            break;
                case "Rhand2": id = 17;
                            angle =  - (int)parameter.Angle;
                            SerialSend(id, angle,0);
                            break;
                case "Rhand3": id = 18;
                            angle =  - (int)parameter.Angle;
                            SerialSend(id, angle,0);
                            break;
                case "Rhand4": id = 19;
                            angle = - (int)parameter.Angle;
                            SerialSend(id, angle,0);
                            break;

            }
        }
               
        
        public bool SerialSend(byte id, int angle, int velocity)
        {
            
            angle = (int)((angle) * 3.41 + 512);
           
            byte[] buff = new byte[11];
            byte checksum = 0x00;
            buff[0] = 0xff;
            buff[1] = 0xff;
            buff[2] = id;
            buff[3] = 0x07;                         //length
            buff[4] = 0x03;                         //write data
            buff[5] = 0x1e;                         //goal position 
            buff[6] = (byte)(angle);
            buff[7] = (byte)(angle / 256);
            buff[8] = (byte)velocity;
            buff[9] = (byte)(velocity / 256);


            for (byte i = 2; i < 10; i++)
            {
                checksum += buff[i];
                //Console.Write(buff[i].ToString("x"));
               // Console.Write(" ");

            }
            Console.WriteLine();
            Console.WriteLine(id);
            checksum = (byte)~checksum;
            buff[10] = checksum;
            try
            {
                SerialPort1.DiscardInBuffer();
                SerialPort1.Write(buff, 0, 11);
                
            }
            catch
            {
                Console.WriteLine("Some problem encountered while writing to serial port");
                return false;
            }
            return true;
        }
     
        public int SerialRead(byte id, byte parameter)
        {
            int angle;
            byte[] buff = new byte[11];
            byte checksum = 0x00;
            buff[0] = 0xff;
            buff[1] = 0xff;
            buff[2] = id;
            buff[3] = 0x04;                         //length
            buff[4] = 0x02;                         //read data
            buff[5] = parameter;                         //present position 
            buff[6] = 0x02;                         //no.of bytes to read
            for (byte i = 2; i < 7; i++)
            {
                checksum += buff[i];

            }
            checksum = (byte)~checksum;
            buff[7] = checksum;
            SerialPort1.DiscardInBuffer();
            SerialPort1.Write(buff, 0, 8);
            System.Threading.Thread.Sleep(10);
            checksum = 0;
            SerialPort1.Read(buff,0,8);
            System.Threading.Thread.Sleep(1);
            for (byte i = 2; i < 7; i++)
            {
                checksum += buff[i];
               
                

            }
            Console.WriteLine(checksum.ToString("x"));
            checksum = (byte)~checksum;
           
            if (checksum == buff[7])
            {
                angle = (int)(256 * buff[6] + buff[5]);
                
                angle = (int)((angle - 512) / 3.41);
                
                return angle;
            }
            else
                return 200;

            
        }
        public bool ReadStatus(byte id)
        {
            byte[] buff = new byte[10];
            byte checksum = 0x00;
            
            bool seqfound = false;
            System.Threading.Thread.Sleep(10);
            try
            {
                SerialPort1.Read(buff, 0, 6);
            }
            catch
            {
                return false;
            }
            Console.WriteLine("inside readstatus");
            for (byte i = 0; i < 6; i++)
            {
                Console.Write(buff[i]);
                Console.Write(" ");
            }
            
                if (buff[0] == buff[1])
                   seqfound = true;
            if (seqfound)
            {
                Console.WriteLine("sequence found");
                
                for (byte i = 2; i < 5; i++)
                {
                    checksum += buff[i];
                    


                }
                
                checksum = (byte)~checksum;
                //if (checksum == buff[5])
                {
                    //if (buff[4] == 0 && buff[2]==id)
                        Console.WriteLine("currectly received");
                        return true;
                }
            }
            Console.WriteLine("sequence not found");
            return false;
            

            
        }
        IEnumerator<ITask> RefreshListIterator()
        {
            var getOrFault = _simEngine.Get();
            yield return getOrFault.Choice();
            Fault ex = (Fault)getOrFault;
            if (ex != null)
            {
                LogError(ex);
                yield break;
            }

            var simState = (sim.SimulationState)getOrFault;
            WinFormsServicePort.FormInvoke(() =>
            {
                foreach (var entity in simState.Entities)
                {
                    if (entity is simengine.GlobalJointEntity)
                    {
                        _simulatedBipedMoverUI.AddEntityName(entity.State.Name, _state.Joints==null || _state.Joints.Count == 0);
                    }
                }
            });

        }

        void MoveJoint(MoveJoint move)
        {
            DOFDesc dof = _state.Joints[move.Name];
            JointDesc desc = dof.Description;
            Vector3 targetVelocity = new Vector3(0, 0, 0);
            
            
            // code for simultaneous motion of both halfs
            string name = null;
            switch(desc.Name)
            {
                case "Lhand1": name = "Rhand1";
                 
                    break;
                case "Lhand2": name = "Rhand2";
                   
                    break;
                case "Lhand3": name = "Rhand3";
                  
                    break;
                case "Lhand4": name = "Rhand4";
                   
                    break;
                case "Rhand1": name = "Rhand1";
                   
                    break;
                case "Rhand2": name = "Rhand2";
                    
                    break;
                case "Rhand3": name = "Rhand3";
                    
                    break;
                case "Rhand4": name = "Rhand4";
                    
                    break;
                case "legu1L": name = "legu1R";

                    break;
                case "legu2L": name = "legu2R";

                    break;
                case "legu3L": name = "legu3R";

                    break;                    
                case "legu4L": name = "legu4R";

                    break;
                case "legu5L": name = "legu5R";

                    break;
                case "legu6L": name = "legu6R";

                    break;
                case "legu1R": name = "legu1R";

                    break;
                case "legu2R": name = "legu2R";

                    break;
                case "legu3R": name = "legu3R";

                    break;
                case "legu4R": name = "legu4R";

                    break;
                case "legu5R": name = "legu5R";

                    break;
                case "legu6R": name = "legu6R";

                    break;
                case "Torso2R": name = "Torso2L";

                    break;
                case "Torso2L": name = "Torso2R";

                    break;
                case "RGrip": name = "RGrip";

                    break;
                case "LGrip": name = "RGrip";

                    break;
                case "Torso1": name = "Torso1";

                    break;

            }
           
            DOFDesc dof1 = _state.Joints[name];
            JointDesc desc1 = dof1.Description;

            switch (dof.Type)
            {
                case DOFType.Twist:
                    desc.TwistAngle = (float)move.Angle;
                    desc1.TwistAngle = (float)move.Angle;
                    targetVelocity = new Vector3((float)-move.Angle, 0, 0);
                    break;
                case DOFType.Swing1:
                    desc.Swing1Angle = (float)move.Angle;
                    desc1.Swing1Angle = (float)move.Angle;
                    targetVelocity = new Vector3(0, (float)-move.Angle, 0);
                    break;
                case DOFType.Swing2:
                    desc.Swing2Angle = (float)move.Angle;
                    desc1.Swing2Angle = (float)move.Angle;
                    targetVelocity = new Vector3(0, 0, (float)-move.Angle);
                    break;
                case DOFType.X:
                    desc.X = (float)move.Angle;
                    targetVelocity = new Vector3((float)-move.Angle, 0, 0);
                    break;
                case DOFType.Y:
                    desc.Y = (float)move.Angle;
                    targetVelocity = new Vector3(0, (float)-move.Angle, 0);
                    break;
                case DOFType.Z:
                    desc.Z = (float)move.Angle;
                    targetVelocity = new Vector3(0, 0, (float)-move.Angle);
                    break;
            }

            PhysicsJoint thisJoint = null;
            PhysicsJoint thisJoint1 = null;
            if (desc.Joint != null)
            {
                thisJoint = (PhysicsJoint)desc.Joint;
                thisJoint1 = (PhysicsJoint)desc1.Joint;
            }
            else
            {
                thisJoint = (PhysicsJoint)desc.JointEntity.ParentJoint;
                thisJoint1 = (PhysicsJoint)desc1.JointEntity.ParentJoint;
            }
           
            if (IsVelocityDrive(thisJoint.State, dof.Type))
            {
                _entity.DeferredTaskQueue.Post(new Task(() => SetDriveInternal(thisJoint, targetVelocity)));
               if(mirror)
                _entity.DeferredTaskQueue.Post(new Task(() => SetDriveInternal(thisJoint1, targetVelocity)));
            }
            else
            {
                Task<Physics.PhysicsJoint, Quaternion, Vector3> deferredTask =
                    new Task<Physics.PhysicsJoint, Quaternion, Vector3>(thisJoint, desc.JointOrientation, desc.JointPosition, SetDriveInternal);
                _entity.DeferredTaskQueue.Post(deferredTask);

                Task<Physics.PhysicsJoint, Quaternion, Vector3> deferredTask1 =
                    new Task<Physics.PhysicsJoint, Quaternion, Vector3>(thisJoint1, desc1.JointOrientation, desc1.JointPosition, SetDriveInternal);
                _entity.DeferredTaskQueue.Post(deferredTask1);
            }
        }

        void SetDriveInternal(Physics.PhysicsJoint joint, Quaternion orientation, Vector3 position)
        {
            if (joint.State.Angular != null)
                joint.SetAngularDriveOrientation(orientation);
            if (joint.State.Linear != null)
                joint.SetLinearDrivePosition(position);
        }
        void SetDriveInternal(Physics.PhysicsJoint joint, Vector3 targetVelocity)
        {
            if (joint.State.Angular != null)
                joint.SetAngularDriveVelocity(targetVelocity);
            if (joint.State.Linear != null)
                joint.SetAngularDriveVelocity(targetVelocity);
        }


        void SuspendBipedInternal(simengine.VisualEntity entity, bool suspend)
        {
            if (suspend)
            {
                entity.PhysicsEntity.IsKinematic = true;
                entity.State.Flags |= Microsoft.Robotics.Simulation.Physics.EntitySimulationModifiers.Kinematic;
                MoveBipedPose(new Vector3(0, 0.3f, 0), entity);
            }
            else
            {
                entity.State.Flags &= ~Microsoft.Robotics.Simulation.Physics.EntitySimulationModifiers.Kinematic;
                MoveBipedPose(new Vector3(0, -0.28f, 0), entity);
                Activate(Arbiter.Receive(false, TimeoutPort(200), SetDynamic));
            }
        }

        void SetDynamic(DateTime now)
        {
            _entity.PhysicsEntity.IsKinematic = false;
        }

        void MoveBipedPose(Vector3 offset, simengine.VisualEntity entity)
        {
            entity.PhysicsEntity.SetPose(new Pose(offset + entity.State.Pose.Position, entity.State.Pose.Orientation));
            foreach (simengine.VisualEntity child in entity.Children)
                MoveBipedPose(offset, child);
        }

        void CreateDefaultState()
        {
            if (_state == null)
            {
                _state = new JointMoverState();
                _state.Joints = new Dictionary<string, DOFDesc>();
            }
        }

        void InsertEntityNotificationHandlerFirstTime(simengine.InsertSimulationEntity ins)
        {
            InsertEntityNotificationHandler(ins);

            base.Start();

            // Listen on the main port for requests and call the appropriate handler.
            MainPortInterleave.CombineWith(
                new Interleave(
                    new TeardownReceiverGroup(),
                    new ExclusiveReceiverGroup(
                        Arbiter.Receive<simengine.InsertSimulationEntity>(true, _notificationTarget, InsertEntityNotificationHandler),
                        Arbiter.Receive<simengine.DeleteSimulationEntity>(true, _notificationTarget, DeleteEntityNotificationHandler),
                        Arbiter.Receive<FromWinformMsg>(true, _fromWinformPort, OnWinformMessageHandler)
                    ),
                    new ConcurrentReceiverGroup()
                )
            );
        }

        void InsertEntityNotificationHandler(simengine.InsertSimulationEntity ins)
        {
            _entity = (simengine.VisualEntity)ins.Body;
            _entity.ServiceContract = Contract.Identifier;

            // traverse the entities associated with this entity and build a list of joints
            _state.Joints.Clear();
            Dictionary<string, simengine.VisualEntity> entities = new Dictionary<string, simengine.VisualEntity>();
            FindJointsAndEntities(_entity, entities);

            // add a slider for each joint to the UI
            WinFormsServicePort.FormInvoke(
                delegate()
                {
                    _simulatedBipedMoverUI.AddEntityName(_entity.State.Name, _state.Joints == null || _state.Joints.Count == 0);
                    _simulatedBipedMoverUI.AddSliders(_state.Joints);
                }
            );
        }

        private void FindJointsAndEntities(simengine.VisualEntity thisEntity, Dictionary<string, simengine.VisualEntity> visited)
        {

            if (thisEntity == null)
                return;

            if (visited.ContainsKey(thisEntity.State.Name))
                return;
            else
                visited.Add(thisEntity.State.Name, thisEntity);

            // process the children first
            if (thisEntity is simengine.VisualEntity)
            {
                foreach (simengine.VisualEntity Child in ((simengine.VisualEntity)thisEntity).Children)
                    FindJointsAndEntities(Child, visited);
            }


            if (thisEntity.ParentJoint != null)
                AddJoint(thisEntity.ParentJoint, thisEntity);

            // search this entity for other entities or joints
            Type thisEntityType = thisEntity.GetType();
            System.Reflection.FieldInfo[] fields = thisEntityType.GetFields(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);
            foreach (FieldInfo field in fields)
            {
                // search for joints
                if ((field.FieldType == typeof(Joint)) || (field.FieldType.IsSubclassOf(typeof(Joint))))
                {
                    Joint thisJoint = (Joint)field.GetValue(thisEntity);
                    if (thisJoint != null)
                        AddJoint(thisJoint, thisEntity);
                }
                else if (field.FieldType == typeof(List<Joint>))
                {
                    List<Joint> jointList = (List<Joint>)field.GetValue(thisEntity);
                    if (jointList != null)
                    {
                        foreach (Joint thisJoint in jointList)
                            AddJoint(thisJoint, thisEntity);
                    }
                }
                else if (field.FieldType == typeof(Joint[]))
                {
                    Joint[] jointArray = (Joint[])field.GetValue(thisEntity);
                    if (jointArray != null)
                    {
                        foreach (Joint thisJoint in jointArray)
                            AddJoint(thisJoint, thisEntity);
                    }
                }

                // search for entities
                if ((field.FieldType == typeof(simtypes.Entity)) || (field.FieldType.IsSubclassOf(typeof(simtypes.Entity))))
                    FindJointsAndEntities((simengine.VisualEntity)field.GetValue(thisEntity), visited);
                else if (field.FieldType.IsGenericType)
                {
                    Type[] parms = field.FieldType.GetGenericArguments();
                    if (parms.Length == 1)
                    {
                        if ((parms[0] == typeof(simtypes.Entity)) || (parms[0].IsSubclassOf(typeof(simtypes.Entity))))
                            foreach (simengine.VisualEntity someEntity in (System.Collections.ICollection)field.GetValue(thisEntity))
                                FindJointsAndEntities(someEntity, visited);
                    }
                }
            }
        }

        void AddJoint(Joint joint, simengine.VisualEntity entity)
        {
            if (!(joint is PhysicsJoint))
                return;

            if (!_state.Joints.ContainsKey(joint.State.Name))
            {
                JointDesc description = new JointDesc(joint.State.Name, entity, joint as PhysicsJoint);
                
                string[] DOFNames = joint.State.Name.Split(';');
                int which = 0;
                if (joint.State.Angular != null)
                {
                    if (joint.State.Angular.TwistMode != JointDOFMode.Locked && IsNotUselessDrive(joint.State.Angular.TwistDrive))
                    {
                        AddDOF(DOFNames, which++, description, DOFType.Twist);
                        Console.WriteLine(joint.State.Name + "Twist");
                    }
                    if (joint.State.Angular.Swing1Mode != JointDOFMode.Locked && IsNotUselessDrive(joint.State.Angular.SwingDrive))
                    {
                        AddDOF(DOFNames, which++, description, DOFType.Swing1);
                        Console.WriteLine(joint.State.Name + "Swing1");
                    }
                    if (joint.State.Angular.Swing2Mode != JointDOFMode.Locked )
                    {
                        AddDOF(DOFNames, which++, description, DOFType.Swing2);
                        Console.WriteLine(joint.State.Name + "Swing2");
                    }
                }
                if (joint.State.Linear != null)
                {
                    if (joint.State.Linear.XMotionMode != JointDOFMode.Locked && IsNotUselessDrive(joint.State.Linear.XDrive))
                        AddDOF(DOFNames, which++, description, DOFType.X);
                    if (joint.State.Linear.YMotionMode != JointDOFMode.Locked && IsNotUselessDrive(joint.State.Linear.YDrive))
                        AddDOF(DOFNames, which++, description, DOFType.Y);
                    if (joint.State.Linear.ZMotionMode != JointDOFMode.Locked && IsNotUselessDrive(joint.State.Linear.ZDrive))
                        AddDOF(DOFNames, which++, description, DOFType.Z);
                }
            }
        }

        private bool IsNotUselessDrive(JointDriveProperties jointDriveProperties)
        {
            if (jointDriveProperties==null || (jointDriveProperties.Mode == JointDriveMode.Position &&
                jointDriveProperties.Spring!=null && jointDriveProperties.Spring.SpringCoefficient == 0))
                return false;
            else
                return true;
        }


        void AddDOF(string[] DOFNames, int which, JointDesc desc, DOFType type)
        {
            bool isVelocityDrive = false;
            float defaultDriveValue = 0.0f;

            float min = -180, max = 180;
            if (which < DOFNames.Length)
            {
                string[] subNames = DOFNames[which].Split('|');
                try
                {
                    if (subNames.Length > 1)
                        min = Single.Parse(subNames[1]);
                    if (subNames.Length > 2)
                        max = Single.Parse(subNames[2]);
                }
                catch
                {
                }
                if (IsVelocityDrive(desc.Joint.State, type))
                {
                    float angularVelocityLength = 0; 
                    if (Math.Abs(desc.Joint.State.Angular.DriveTargetVelocity.X) > Math.Abs(angularVelocityLength))
                        angularVelocityLength = desc.Joint.State.Angular.DriveTargetVelocity.X;

                    if (Math.Abs(desc.Joint.State.Angular.DriveTargetVelocity.Y) > Math.Abs(angularVelocityLength))
                        angularVelocityLength = desc.Joint.State.Angular.DriveTargetVelocity.Y;

                    if (Math.Abs(desc.Joint.State.Angular.DriveTargetVelocity.Z) > Math.Abs(angularVelocityLength))
                        angularVelocityLength = desc.Joint.State.Angular.DriveTargetVelocity.Z;

                    if (Math.Abs(angularVelocityLength) < float.Epsilon)
                    {
                        min = -2.0f;
                        max = 2.0f;
                    }
                    else
                    {
                        min = Math.Min(-angularVelocityLength, angularVelocityLength);
                        max = Math.Max(-angularVelocityLength, angularVelocityLength);
                        defaultDriveValue = -angularVelocityLength;
                    }
                    isVelocityDrive = true;
                }
                _state.Joints.Add(subNames[0], new DOFDesc(subNames[0], desc, type, min, max, isVelocityDrive, defaultDriveValue));
            }
            else
            {
                string name = DOFNames[0];
                switch (which)
                {
                    case 0: name += " Twist"; break;
                    case 1: name += " Swing1"; break;
                    case 2: name += " Swing2"; break;
                    case 3: name += " X"; min = -2; max = 2; break;
                    case 4: name += " Y"; min = -2; max = 2; break;
                    case 5: name += " Z"; min = -2; max = 2; break;
                }
                _state.Joints.Add(name, new DOFDesc(name, desc, type, min, max, isVelocityDrive, defaultDriveValue));
            }
        }

        private bool IsVelocityDrive(JointProperties jointProperties, DOFType dof)
        {
            switch(dof)
            {
                case DOFType.Twist:
                    if (jointProperties.Angular != null && jointProperties.Angular.TwistDrive != null &&
                       jointProperties.Angular.TwistDrive.Mode == JointDriveMode.Velocity)
                        return true;
                    break;

                case DOFType.Swing1:
                case DOFType.Swing2:
                    if (jointProperties.Angular != null && jointProperties.Angular.SwingDrive != null &&
                        jointProperties.Angular.SwingDrive.Mode == JointDriveMode.Velocity)
                        return true;
                    break;

                case DOFType.X:
                    if (jointProperties.Linear != null && jointProperties.Linear.XDrive != null &&
                            jointProperties.Linear.XDrive.Mode == JointDriveMode.Velocity)
                        return true;
                    break;

                case DOFType.Y:
                    if (jointProperties.Linear != null && jointProperties.Linear.YDrive != null &&
                                jointProperties.Linear.YDrive.Mode == JointDriveMode.Velocity)
                        return true;
                    break;

                case DOFType.Z:
                    if (jointProperties.Linear != null && jointProperties.Linear.ZDrive != null &&
                                jointProperties.Linear.ZDrive.Mode == JointDriveMode.Velocity)
                        return true;
                    break;
            }

            return false;
        }

        void DeleteEntityNotificationHandler(simengine.DeleteSimulationEntity del)
        {
            DeleteEntityInternal();
        }

        void DeleteEntityInternal()
        {
            _entity = null;
            // add a slider for each joint to the UI
            WinFormsServicePort.FormInvoke(
                delegate()
                {
                    _simulatedBipedMoverUI.ClearNames();
                    _simulatedBipedMoverUI.ClearSliders();
                    Activate(Arbiter.Receive(false, TimeoutPort(5000), dateTime => SpawnIterator(RefreshListIterator)));
                }
            );
        }


        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> SubscribeHandler(Subscribe subscribe)
        {
            SubscribeRequestType request = subscribe.Body;
            Console.WriteLine("adding subscribers",request.Subscriber);
            SubscribeHelper(_subMgrPort, request, subscribe.ResponsePort);
            yield break;
        }


        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> incrementHandler(increment increment)
        {
            Console.WriteLine("incremented jointmover");
            //base.SendNotification(_subMgrPort, increment);
            increment.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }


        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> GetHandler(HttpGet get)
        {
            get.ResponsePort.Post(new HttpResponseType(_state));
            yield break;
        }

        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> GetHandler(Get get)
        {
            get.ResponsePort.Post(_state);
            yield break;
        }
       
    }
}
