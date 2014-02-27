//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: jointmoverui.cs $ $Revision: 1 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using physics = Microsoft.Robotics.Simulation.Physics;

namespace ProMRDS.Simulation.JointMover
{
    public partial class SimulatedBipedMoverUI : Form
    {
        FromWinformEvents _fromWinformPort;
        const string _defaultEntityNameText = "Select an Entity";

        public SimulatedBipedMoverUI(FromWinformEvents EventsPort)
        {
            _fromWinformPort = EventsPort;

            InitializeComponent();

            _fromWinformPort.Post(new FromWinformMsg(FromWinformMsg.MsgEnum.Loaded, null, this));
        }

        public void AddEntityName(string name, bool ignoreSameName)
        {
            if (_entityNameComboBox.Items.Contains(name) == false)
            {
                if (_entityNameComboBox.Items.Count == 1 && _entityNameComboBox.Items[0].ToString() == _defaultEntityNameText)
                    _entityNameComboBox.Items.Clear();

                _entityNameComboBox.Items.Add(name);

                if (_entityNameComboBox.Items.Count == 1)
                {
                    _entityNameComboBox.Text = _entityNameComboBox.Items[0].ToString();
                    _entityNameComboBox_SelectionChangeCommitted(null, null);
                }
            }
            else if (ignoreSameName == true)
            {
                _entityNameComboBox_SelectionChangeCommitted(null, null);
            }
        }

        public void ClearSliders()
        {
            this.splitContainer1.Panel2.Controls.Clear();
            this.splitContainer1.Panel2.Invalidate();
        }

        public void AddSliders(Dictionary<string,DOFDesc> joints)
        {
            Stack<Control> tmp = new Stack<Control>();

            const int spacing = -18;
            const int labelWidth = 50;
            int y = 5;
            int x = 5;
            foreach (DOFDesc desc in joints.Values)
            {
                Label lb = new Label();
                lb.Location = new System.Drawing.Point(x, y);
                lb.Size = new System.Drawing.Size(labelWidth, 45);
                lb.Text = desc.Name;

                tmp.Push(lb);

                TrackBar tb = new TrackBar();
                tb.Location = new System.Drawing.Point(x + labelWidth, y);

                tb.Maximum = (int)(150);//desc.Maximum * desc.Scale
                tb.Minimum = (int)(-150);//desc.Minimum * desc.Scale
                tb.Value = (int)(desc.DefaultDriveValue * desc.Scale);
                tb.Name = desc.Name;
                tb.Size = new System.Drawing.Size(300 - labelWidth, 45);
                tb.TabIndex = 0;
                tb.TickFrequency = 3;
                tb.TickStyle = TickStyle.None;
                tb.Scroll += new System.EventHandler(this.trackBar_Scroll);

                tmp.Push(tb);

                Label vlb = new Label();
                vlb.AutoSize = true;
                vlb.Location = new System.Drawing.Point(x + labelWidth + tb.Width + 3, y);
                vlb.Text = desc.DefaultDriveValue.ToString();
                tb.Tag = new object[] { desc, vlb };
                tmp.Push(vlb);
                y += tb.Size.Height + spacing;
            }

            // controls are added in reverse order because they overlap
            while (tmp.Count > 0)
                this.splitContainer1.Panel2.Controls.Add(tmp.Pop());

            this.splitContainer1.Panel2.Invalidate();

        }

        private void trackBar_Scroll(object sender, EventArgs e)
        {
            TrackBar tb = (TrackBar)sender;
         
            DOFDesc  desc = (DOFDesc)((object[])tb.Tag)[0];
           
            Label lb = (Label)((object[])tb.Tag)[1];
            lb.Text = (tb.Value).ToString();
            _fromWinformPort.Post(new FromWinformMsg(FromWinformMsg.MsgEnum.MoveJoint, null, new MoveJoint(tb.Value, desc.Name)));
        }

        private void _suspendBox_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void _entityNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _fromWinformPort.Post(new FromWinformMsg(FromWinformMsg.MsgEnum.RefreshList, null, _entityNameComboBox.SelectedText));
        }

        private void _entityNameComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
                _entityNameComboBox_SelectedIndexChanged(null, null);
        }

        private void _entityNameComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (_entityNameComboBox != null && _entityNameComboBox.SelectedItem != null)
            {
                _fromWinformPort.Post(new FromWinformMsg(FromWinformMsg.MsgEnum.ChangeEntity, null, _entityNameComboBox.SelectedItem.ToString()));
                this.Invalidate(true);
            }
        }

        private void _entityNameComboBox_MouseClick(object sender, MouseEventArgs e)
        {
            _entityNameComboBox_SelectedIndexChanged(null, null);
        }

        internal void ClearNames()
        {
            _entityNameComboBox.Items.Clear();
            _entityNameComboBox.Items.Add(_defaultEntityNameText);
            _entityNameComboBox.Text = _defaultEntityNameText;
            this.Invalidate(true);
        }

        private void _entityNameComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            _fromWinformPort.Post(new FromWinformMsg(FromWinformMsg.MsgEnum.RefreshList, null, _entityNameComboBox.SelectedText));
        }

        private void Start_Click(object sender, EventArgs e)
        {
            _fromWinformPort.Post(new FromWinformMsg(FromWinformMsg.MsgEnum.Start, null, null));
            Start.Enabled = false;
            Stop.Enabled = true;
            Reset.Enabled = true;
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            _fromWinformPort.Post(new FromWinformMsg(FromWinformMsg.MsgEnum.Stop, null, null));
            Start.Enabled = true;
            Stop.Enabled = false;
            Reset.Enabled = false;
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            _fromWinformPort.Post(new FromWinformMsg(FromWinformMsg.MsgEnum.Reset, null, null));
        }
        bool mirror=false;
        private void Mirror_Click(object sender, EventArgs e)
        {
            if (!mirror)
            {
                _fromWinformPort.Post(new FromWinformMsg(FromWinformMsg.MsgEnum.Mirrorstart, null, null));
                mirror = true;
            }
            else
            {
                _fromWinformPort.Post(new FromWinformMsg(FromWinformMsg.MsgEnum.Mirrorstop, null, null));
                mirror = false;
            }
        }

        private void front_stand_Click(object sender, EventArgs e)
        {
            _fromWinformPort.Post(new FromWinformMsg(FromWinformMsg.MsgEnum.Frontstand, null, null));
        }

        private void walk_Click(object sender, EventArgs e)
        {
            _fromWinformPort.Post(new FromWinformMsg(FromWinformMsg.MsgEnum.Walk, null, null));
            
        }

        private void SerialPortControllerThread_DoWork(object sender, DoWorkEventArgs e)
        {
            Console.WriteLine("wassup");
        }

      

     

       

      
    }
}