using System;
using System.Collections.Generic;
using System.Collections;
using log4net;
using System.Reflection;
using System.IO;
using System.Drawing;
using MissionPlanner.Utilities;
using SharpDX.DirectInput;
using MissionPlanner.Mavlink;
using MissionPlanner.NvExt;


namespace MissionPlanner.CamJoystick
{
    public static class Extensions
    {
        public static CamJoystickState CurrentCamJoystickState(this SharpDX.DirectInput.Joystick Camjoystick)
        {
            return new CamJoystickState(Camjoystick.GetCurrentState());
        }
    }

    /* Camera Joystick Button Function Class */
    public class CamJoystickButtonFunc
    {
        /* button state */
        public enum ButtonState
        {
            BtnPressed,
            BtnReleased,
        }

        public float nv_cam_cmd;
        public float[] nv_params = new float[6];
        public ButtonState state = ButtonState.BtnReleased;
        public delegate void ButtonFuncCallbck();
        public ButtonFuncCallbck btn_cmd_func = null;        
        /* CamJoystick Btn State Machine functions */
        public void CamJoystickBtntStateMachine(ushort val)
        {
            switch (state)
            {
                case ButtonState.BtnReleased:
                    {
                        if (val == 2000)
                        {
                            state = ButtonState.BtnPressed;

                            /* Send Digicam Command using nv_cam_cmd & nv_params */
                            if (!MainV2.comPort.BaseStream.IsOpen)
                                return;

                            /* execute the button transmission function */
                            btn_cmd_func?.Invoke();
                        }
                    }
                    break;

                case ButtonState.BtnPressed:
                    {
                        if (val == 1000)
                            state = ButtonState.BtnReleased;
                    }
                    break;
            }
        }

        /* Digicam Command Send Function */
        public void CamJoystickBtnSendDigiCam()
        {
            if (nv_params[0] == 0)
                nv_params[0] = 1;
            else
                nv_params[0] = 0;
            
            MainV2.comPort.doCommand(MainV2.comPort.MAV.sysid, MainV2.comPort.MAV.compid,MAVLink.MAV_CMD.DO_DIGICAM_CONTROL,
                                    (float)nv_cam_cmd, nv_params[0], nv_params[1], nv_params[2], nv_params[3], nv_params[4], nv_params[5],false);
        }

        /* Digicam Command Send Function */
        public void CamJoystickBtnSendDigiCamSysMode()
        {      
            MainV2.comPort.doCommand(MainV2.comPort.MAV.sysid, MainV2.comPort.MAV.compid,MAVLink.MAV_CMD.DO_DIGICAM_CONTROL,
                                    (float)nv_cam_cmd, nv_params[0], nv_params[1], nv_params[2], nv_params[3], nv_params[4], nv_params[5], false);
        }

        /* Mount Control Command Send Function */
        public void CamJoystickBtnSendMountControl()
        {
            if (MainV2.Camjoystick.systemMode == NvMavExtCmds.SystemMode.Retract)
                MainV2.comPort.doCommand(MainV2.comPort.MAV.sysid, MainV2.comPort.MAV.compid,MAVLink.MAV_CMD.DO_MOUNT_CONTROL, 0, 0, 0, 0, 0, 0, (float)MAVLink.MAV_MOUNT_MODE.NEUTRAL, false);
            else
                MainV2.comPort.doCommand(MainV2.comPort.MAV.sysid, MainV2.comPort.MAV.compid,MAVLink.MAV_CMD.DO_MOUNT_CONTROL, 0, 0, 0, 0, 0, 0, (float)MAVLink.MAV_MOUNT_MODE.RETRACT, false);
        }

        /* Local Position Command Send Function */
        public void CamJoystickBtnSendLocalPosition()
        {
            /* read local position x & y from settings */
            float pitch = 0, roll = 0;
            if (Settings.Instance["local_position_pitch"] != null)
            {
                if (!float.TryParse(Settings.Instance["local_position_pitch"], out pitch))
                    return;
            }
            if (Settings.Instance["local_position_roll"] != null)
            {
                if (!float.TryParse(Settings.Instance["local_position_roll"], out roll))
                    return;
            }
            MainV2.comPort.doCommand(MainV2.comPort.MAV.sysid, MainV2.comPort.MAV.compid,MAVLink.MAV_CMD.DO_DIGICAM_CONTROL,
                                    (float)NvMavExtCmds.Cmd.SetSystemMode,
                                    (float)NvMavExtCmds.SetSystemModeArgs.LocalPosition,
                                     pitch, roll, 0, 0, 0, false);
        }

        /* Global Position Command Send Function */
        public void CamJoystickBtnSendGlobalPosition()
        {
            /* read global position x & y from settings */
            float elevation = 0, azimuth = 0;
            if (Settings.Instance["global_position_elevation"] != null)
            {
                if (!float.TryParse(Settings.Instance["global_position_elevation"], out elevation))
                    return;
            }
            if (Settings.Instance["global_position_azimuth"] != null)
            {
                if (!float.TryParse(Settings.Instance["global_position_azimuth"], out azimuth))
                    return;
            }
            MainV2.comPort.doCommand(MainV2.comPort.MAV.sysid, MainV2.comPort.MAV.compid,MAVLink.MAV_CMD.DO_DIGICAM_CONTROL,
                                    (float)NvMavExtCmds.Cmd.SetSystemMode,
                                    (float)NvMavExtCmds.SetSystemModeArgs.GlobalPosition,
                                    elevation, azimuth, 0, 0, 0, false);
        }

        /* Track Command Send Function*/
        public void CamJoystickTrack()
        {
            switch (MainV2.Camjoystick.trackingMode)
            {
                case NvMavExtCmds.TrackingMode.Idle:
                case NvMavExtCmds.TrackingMode.TrackOnPos1:
                case NvMavExtCmds.TrackingMode.TrackOnPos2:        
                    MainV2.comPort.doCommand(MainV2.comPort.MAV.sysid, MainV2.comPort.MAV.compid,MAVLink.MAV_CMD.DO_DIGICAM_CONTROL,
                        (float)NvMavExtCmds.Cmd.SetSystemMode, (float)NvMavExtCmds.SetSystemModeArgs.Tracking,
                        0, 0, (float)NvMavExtCmds.TrackingMode.Enabled, 0, 0, false);
                    break;
                case NvMavExtCmds.TrackingMode.Enabled:
                    MainV2.comPort.doCommand(MainV2.comPort.MAV.sysid, MainV2.comPort.MAV.compid,MAVLink.MAV_CMD.DO_DIGICAM_CONTROL,
                        (float)NvMavExtCmds.Cmd.SetSystemMode, (float)NvMavExtCmds.SetSystemModeArgs.Tracking,
                        0, 0, (float)NvMavExtCmds.TrackingMode.Track, 0, 0, false);
                    break;
                case NvMavExtCmds.TrackingMode.Track:

                    break;
                case NvMavExtCmds.TrackingMode.Retrack:
                    MainV2.comPort.doCommand(MainV2.comPort.MAV.sysid, MainV2.comPort.MAV.compid,MAVLink.MAV_CMD.DO_DIGICAM_CONTROL,
                        (float)NvMavExtCmds.Cmd.SetSystemMode, (float)NvMavExtCmds.SetSystemModeArgs.Tracking,
                        0, 0, (float)NvMavExtCmds.TrackingMode.Track, 0, 0, false);
                    break;
                default:
                    break;
            }
        }

        /* Re-Track Command Send Function*/
        public void CamJoystickReTrack()
        {
            switch (MainV2.Camjoystick.trackingMode)
            {
                case NvMavExtCmds.TrackingMode.Track:
                case NvMavExtCmds.TrackingMode.TrackOnPos1:
                case NvMavExtCmds.TrackingMode.TrackOnPos2:
                    MainV2.comPort.doCommand(MainV2.comPort.MAV.sysid, MainV2.comPort.MAV.compid,MAVLink.MAV_CMD.DO_DIGICAM_CONTROL,
                        (float)NvMavExtCmds.Cmd.SetSystemMode, (float)NvMavExtCmds.SetSystemModeArgs.Tracking,
                        0, 0, (float)NvMavExtCmds.TrackingMode.Retrack, 0, 0, false);
                    break;
                default:
                    break;
            }
        }

    }

    /* Camera Joystick State Class */
    public class CamJoystickState
    {
        internal JoystickState baseJoystickState;

        public CamJoystickState(JoystickState state)
        {
            baseJoystickState = state;
        }

        public int[] GetPointOfView()
        {
            return baseJoystickState.PointOfViewControllers;
        }

        public bool[] GetButtons()
        {
            return baseJoystickState.Buttons;
        }

        public int AZ
        {
            get { return baseJoystickState.AccelerationZ; }
        }

        public int AY
        {
            get { return baseJoystickState.AccelerationY; }
        }

        public int AX
        {
            get { return baseJoystickState.AccelerationX; }
        }

        public int ARz
        {
            get { return baseJoystickState.AngularAccelerationZ; }
        }

        public int ARy
        {
            get { return baseJoystickState.AngularAccelerationY; }
        }

        public int ARx
        {
            get { return baseJoystickState.AngularAccelerationX; }
        }

        public int FRx
        {
            get { return baseJoystickState.TorqueX; }
        }

        public int FRy
        {
            get { return baseJoystickState.TorqueY; }
        }

        public int FRz
        {
            get { return baseJoystickState.TorqueZ; }
        }

        public int FX
        {
            get { return baseJoystickState.ForceX; }
        }

        public int FY
        {
            get { return baseJoystickState.ForceY; }
        }

        public int FZ
        {
            get { return baseJoystickState.ForceZ; }
        }

        public int Rx
        {
            get { return baseJoystickState.RotationX; }
        }

        public int Ry
        {
            get { return baseJoystickState.RotationY; }
        }

        public int Rz
        {
            get { return baseJoystickState.RotationZ; }
        }

        public int VRx
        {
            get { return baseJoystickState.AngularVelocityX; }
        }

        public int VRy
        {
            get { return baseJoystickState.AngularVelocityY; }
        }

        public int VRz
        {
            get { return baseJoystickState.AngularVelocityZ; }
        }

        public int VX
        {
            get { return baseJoystickState.VelocityX; }
        }

        public int VY
        {
            get { return baseJoystickState.VelocityY; }
        }

        public int VZ
        {
            get { return baseJoystickState.VelocityZ; }
        }

        public int X
        {
            get { return baseJoystickState.X; }
        }

        public int Y
        {
            get { return baseJoystickState.Y; }
        }

        public int Z
        {
            get { return baseJoystickState.Z; }
        }
    }

    /* Camera Joystick Class */
    public class CamJoystick : IDisposable
    {
        public struct JoyChannel
        {
            public int channel;
            public joystickaxis axis;
            public bool reverse;
            public int expo;
        }

        public enum joystickaxis
        {
            None,
            X,
            Y,
            Z,
            Rx,
            Ry,
            Rz,
            HatUpDown,
            HatLeftRight,
            btn1,
            btn2,
            btn3,
            btn4,
            btn5,
            btn6,
            btn7,
            btn8,
            btn9,
            btn10,
            btn11,
            btn12,
            btn13,
            btn14,
            btn15,
            btn16,
            btn17,
            btn18,
            btn19,
            btn20,
        }

        private const int NUM_OF_BUTTON_FUNCS = 24;
        private const int NUM_OF_JOY_CHANNELS = 48;
        private const int NUM_OF_DATA_CHANNELS = 28;

        public CamJoystickButtonFunc[] btn_func_array = new CamJoystickButtonFunc[NUM_OF_BUTTON_FUNCS];
        public static JoyChannel[] JoyChannels = new JoyChannel[NUM_OF_JOY_CHANNELS]; // we are base 1
        public ushort[] ch_data = new ushort[NUM_OF_DATA_CHANNELS];
        public static bool[] CamJoyButtons = { false };
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        SharpDX.DirectInput.Joystick Camjoystick;
        CamJoystickState state;
        static DirectInput directInput = new DirectInput();
        public bool enabled = false;
        public string name;
        public static CamJoystick self;
        string joystickconfigaxis = "Camjoystickaxis.xml";
        int hat1 = 65535 / 2;        // set to default midpoint
        int hat2 = 65535 / 2;        // set to default midpoint
        public float GndCrsAlt = 0;
        public NvMavExtCmds.SystemMode systemMode = NvMavExtCmds.SystemMode.Observation;
        public NvMavExtCmds.TrackingMode trackingMode = NvMavExtCmds.TrackingMode.Idle;
        public double joystick_gain;
        public int joystick_DZ;

        public void Dispose()
        {
            Dispose(true);
        }

        virtual protected void Dispose(bool disposing)
        {
            try
            {
                //not sure if this is a problem from the finalizer?
                if (disposing && Camjoystick != null && Camjoystick.Properties != null)
                    Camjoystick.Unacquire();
            }
            catch
            {
            }

            try
            {
                if (disposing && Camjoystick != null)
                    Camjoystick.Dispose();
            }
            catch
            {
            }

            //tell gc not to call finalize, this object will be GC'd quicker now.
            GC.SuppressFinalize(this);
        }

        /* Constructor */
        public CamJoystick()
        {
            self = this;

            loadconfig("Camjoystickaxis" + MainV2.comPort.MAV.cs.firmware + ".xml");
            
            joystick_gain = Settings.Instance.GetDouble("joystickGain");
            joystick_DZ = Settings.Instance.GetInt32("joystickDZ");
            joystick_gain /= 100.0;

            for (int i = 0; i < btn_func_array.Length; i++)
                btn_func_array[i] = new CamJoystickButtonFunc();

            btn_func_array[0].nv_cam_cmd = (float)NvMavExtCmds.Cmd.DoNUC;
            btn_func_array[0].btn_cmd_func = btn_func_array[0].CamJoystickBtnSendDigiCam;

            btn_func_array[1].nv_cam_cmd = (float)NvMavExtCmds.Cmd.SetIrPolarity;
            btn_func_array[1].btn_cmd_func = btn_func_array[1].CamJoystickBtnSendDigiCam;

            btn_func_array[2].nv_cam_cmd = (float)NvMavExtCmds.Cmd.SetSensor;
            btn_func_array[2].btn_cmd_func = btn_func_array[2].CamJoystickBtnSendDigiCam;

            btn_func_array[3].nv_cam_cmd = (float)NvMavExtCmds.Cmd.SetRecordState;
            btn_func_array[3].btn_cmd_func = btn_func_array[3].CamJoystickBtnSendDigiCam;

            btn_func_array[4].nv_cam_cmd = (float)NvMavExtCmds.Cmd.TakeSnapShot;
            btn_func_array[4].btn_cmd_func = btn_func_array[4].CamJoystickBtnSendDigiCam;

            btn_func_array[5].btn_cmd_func = btn_func_array[5].CamJoystickTrack;
            btn_func_array[6].btn_cmd_func = btn_func_array[6].CamJoystickReTrack;

            btn_func_array[7].nv_cam_cmd = (float)NvMavExtCmds.Cmd.SetSingleYawMode;
            btn_func_array[7].btn_cmd_func = btn_func_array[7].CamJoystickBtnSendDigiCam;

            btn_func_array[8].nv_cam_cmd = (float)NvMavExtCmds.Cmd.SetSystemMode;      
            btn_func_array[8].nv_params[0] = (float)NvMavExtCmds.SetSystemModeArgs.Hold;
            btn_func_array[8].btn_cmd_func = btn_func_array[8].CamJoystickBtnSendDigiCamSysMode;

            btn_func_array[9].btn_cmd_func = btn_func_array[9].CamJoystickBtnSendMountControl;

            btn_func_array[10].nv_cam_cmd = (float)NvMavExtCmds.Cmd.SetFollowMode;
            btn_func_array[10].btn_cmd_func = btn_func_array[10].CamJoystickBtnSendDigiCam;

            btn_func_array[11].btn_cmd_func = btn_func_array[11].CamJoystickBtnSendLocalPosition;
            btn_func_array[12].btn_cmd_func = btn_func_array[12].CamJoystickBtnSendGlobalPosition;

            btn_func_array[13].nv_cam_cmd = (float)NvMavExtCmds.Cmd.SetSystemMode;
            btn_func_array[13].nv_params[0] = (float)NvMavExtCmds.SetSystemModeArgs.Pilot;
            btn_func_array[13].btn_cmd_func = btn_func_array[13].CamJoystickBtnSendDigiCamSysMode;

            btn_func_array[14].nv_cam_cmd = (float)NvMavExtCmds.Cmd.SetSystemMode;
            btn_func_array[14].nv_params[0] = (float)NvMavExtCmds.SetSystemModeArgs.Stow;
            btn_func_array[14].btn_cmd_func = btn_func_array[14].CamJoystickBtnSendDigiCamSysMode;

            btn_func_array[15].nv_cam_cmd = (float)NvMavExtCmds.Cmd.SetSystemMode;
            btn_func_array[15].nv_params[0] = (float)NvMavExtCmds.SetSystemModeArgs.GRR;
            btn_func_array[15].btn_cmd_func = btn_func_array[15].CamJoystickBtnSendDigiCamSysMode;

            btn_func_array[16].nv_cam_cmd = (float)NvMavExtCmds.Cmd.SetSystemMode;
            btn_func_array[16].nv_params[0] = (float)NvMavExtCmds.SetSystemModeArgs.Observation;
            btn_func_array[16].btn_cmd_func = btn_func_array[16].CamJoystickBtnSendDigiCamSysMode;

            // empty
            btn_func_array[17].btn_cmd_func = null;

            btn_func_array[18].nv_cam_cmd = (float)NvMavExtCmds.Cmd.SetIrPolarity;
            btn_func_array[18].nv_params[0] = (float)NvMavExtCmds.SetPolarityArgs.WhiteHot;
            btn_func_array[18].btn_cmd_func = btn_func_array[18].CamJoystickBtnSendDigiCamSysMode;

            btn_func_array[19].nv_cam_cmd = (float)NvMavExtCmds.Cmd.SetIrPolarity;
            btn_func_array[19].nv_params[0] = (float)NvMavExtCmds.SetPolarityArgs.BlackHot;
            btn_func_array[19].btn_cmd_func = btn_func_array[19].CamJoystickBtnSendDigiCamSysMode;

            btn_func_array[20].nv_cam_cmd = (float)NvMavExtCmds.Cmd.SetSensor;
            btn_func_array[20].nv_params[0] = (float)NvMavExtCmds.SetSensorArgs.DaySensor;
            btn_func_array[20].btn_cmd_func = btn_func_array[20].CamJoystickBtnSendDigiCamSysMode;

            btn_func_array[21].nv_cam_cmd = (float)NvMavExtCmds.Cmd.SetSensor;
            btn_func_array[21].nv_params[0] = (float)NvMavExtCmds.SetSensorArgs.IrSensor;
            btn_func_array[21].btn_cmd_func = btn_func_array[21].CamJoystickBtnSendDigiCamSysMode;

            btn_func_array[22].nv_cam_cmd = (float)NvMavExtCmds.Cmd.SetSystemMode;
            btn_func_array[22].nv_params[0] = (float)NvMavExtCmds.SetSystemModeArgs.Nadir;
            btn_func_array[22].btn_cmd_func = btn_func_array[22].CamJoystickBtnSendDigiCamSysMode;

            btn_func_array[23].nv_cam_cmd = (float)NvMavExtCmds.Cmd.MultipleGCSControl;
            btn_func_array[23].nv_params[0] = (float)NvMavExtCmds.MultipleGCSControlArgs.MultipleGCSEnableSecondaryControl;
            btn_func_array[23].nv_params[1] = (float)NvMavExtCmds.DisArgs.Toggle;
            btn_func_array[23].btn_cmd_func = btn_func_array[23].CamJoystickBtnSendDigiCamSysMode;
        }

        public void loadconfig(string joystickconfigaxis = "Camjoystickaxis.xml")
        {
            log.Info("Loading Camjoystick config files " + joystickconfigaxis);

            // save for later
            this.joystickconfigaxis = Settings.GetUserDataDirectory() + joystickconfigaxis;

            // load config
            if (File.Exists(this.joystickconfigaxis))
            {
                try
                {
                    System.Xml.Serialization.XmlSerializer reader =
                        new System.Xml.Serialization.XmlSerializer(typeof(JoyChannel[]),
                            new Type[] { typeof(JoyChannel) });

                    using (StreamReader sr = new StreamReader(this.joystickconfigaxis))
                    {
                        JoyChannels = (JoyChannel[])reader.Deserialize(sr);
                    }
                }
                catch
                {
                }
            }
        }

        public void saveconfig()
        {
            log.Info("Saving Camjoystick config files " + joystickconfigaxis);

            // save config
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(JoyChannel[]), new Type[] { typeof(JoyChannel) });

            using (StreamWriter sw = new StreamWriter(joystickconfigaxis))
            {
                writer.Serialize(sw, JoyChannels);
            }
        }

        public static IList<DeviceInstance> getDevices()
        {
            return directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);
        }

        public static SharpDX.DirectInput.Joystick getJoyStickByName(string name)
        {
            var joysticklist = directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);
            int i = 1;

            foreach (DeviceInstance device in joysticklist)
            {

                if (device.ProductName == name)
                {
                    return new SharpDX.DirectInput.Joystick(directInput, device.InstanceGuid);
                }
                i++;
            }

            return null;
        }

        public SharpDX.DirectInput.Joystick AcquireJoystick(string name)
        {
            Camjoystick = getJoyStickByName(name);

            if (Camjoystick == null)
                return null;

            Camjoystick.Acquire();

            System.Threading.Thread.Sleep(500);

            Camjoystick.Poll();

            return Camjoystick;
        }

        public bool start(string name)
        {
            self.name = name;

            Camjoystick = AcquireJoystick(name);

            if (Camjoystick == null)
                return false;

            enabled = true;

            System.Threading.Thread tCam = new System.Threading.Thread(new System.Threading.ThreadStart(mainloop))
            {
                Name = "CamJoystick loop",
                Priority = System.Threading.ThreadPriority.AboveNormal,
                IsBackground = true
            };
            tCam.Start();

            return true;
        }

        public static joystickaxis getMovingAxis(string name, int threshold)
        {
            self.name = name;

            var Camjoystick = new CamJoystick().AcquireJoystick(name);

            if (Camjoystick == null)
                return 0;// joystickaxis.ARx;


            System.Threading.Thread.Sleep(50);

            var obj = Camjoystick.CurrentCamJoystickState();
            Hashtable values = new Hashtable();

            // get the state of the joystick before.
            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                values[property.Name] = int.Parse(property.GetValue(obj, null).ToString());
            }

            CustomMessageBox.Show("Please move the Camjoystick axis you want assigned to this function after clicking ok");

            DateTime start = DateTime.Now;

            while (start.AddSeconds(10) > DateTime.Now)
            {
                Camjoystick.Poll();
                System.Threading.Thread.Sleep(50);
                var nextstate = Camjoystick.CurrentCamJoystickState();
                int[] hat1 = nextstate.GetPointOfView();

                type = nextstate.GetType();
                properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    //Console.WriteLine("Name: " + property.Name + ", Value: " + property.GetValue(obj, null));

                    log.InfoFormat("test name {0} old {1} new {2} ", property.Name, values[property.Name],
                        int.Parse(property.GetValue(nextstate, null).ToString()));
                    log.InfoFormat("{0}  {1} {2}", property.Name, (int)values[property.Name],
                        (int.Parse(property.GetValue(nextstate, null).ToString()) + threshold));
                    if ((int)values[property.Name] >
                        (int.Parse(property.GetValue(nextstate, null).ToString()) + threshold) ||
                        (int)values[property.Name] <
                        (int.Parse(property.GetValue(nextstate, null).ToString()) - threshold))
                    {
                        log.Info(property.Name);
                        Camjoystick.Unacquire();
                        return (joystickaxis)Enum.Parse(typeof(joystickaxis), property.Name);
                    }
                }

                if (hat1[0] != -1)
                {
                    int angle = hat1[0] / 100;

                    //0 = down = 18000
                    //0 = up = 0

                    // 0
                    if (angle > 270 || angle < 90)
                    {
                        Camjoystick.Unacquire();
                        return joystickaxis.HatUpDown;
                    }
                    // 180
                    if (angle > 90 && angle < 270)
                    {
                        Camjoystick.Unacquire();
                        return joystickaxis.HatUpDown;
                    }
                    // 90
                    if (angle > 0 && angle < 180)
                    {
                        Camjoystick.Unacquire();
                        return joystickaxis.HatLeftRight;
                    }
                    // 270
                    if (angle > 180 && angle < 360)
                    {
                        Camjoystick.Unacquire();
                        return joystickaxis.HatLeftRight;
                    }
                }
            }

            CustomMessageBox.Show("No valid option was detected");
            return joystickaxis.None;
        }

        public static int getPressedButton(string name)
        {
            self.name = name;

            var Camjoystick = getJoyStickByName(name);

            if (Camjoystick == null)
                return -1;

            Camjoystick.Acquire();

            System.Threading.Thread.Sleep(500);

            Camjoystick.Poll();

            var obj = Camjoystick.CurrentCamJoystickState();

            var buttonsbefore = obj.GetButtons();

            CustomMessageBox.Show(
                "Please press the Camjoystick button you want assigned to this function after clicking ok");

            DateTime start = DateTime.Now;

            while (start.AddSeconds(10) > DateTime.Now)
            {
                Camjoystick.Poll();
                var nextstate = Camjoystick.CurrentCamJoystickState();

                var buttons = nextstate.GetButtons();

                for (int a = 0; a < Camjoystick.Capabilities.ButtonCount; a++)
                {
                    if (buttons[a] != buttonsbefore[a])
                        return a + 1;
                }
            }

            CustomMessageBox.Show("No valid option was detected");
            return -1;
        }

        public void setReverse(int channel, bool reverse)
        {
            JoyChannels[channel].reverse = reverse;
        }

        public void setAxis(int channel, joystickaxis axis)
        {
            JoyChannels[channel].axis = axis;
        }

        public JoyChannel getChannel(int channel)
        {
            return JoyChannels[channel];
        }

        public int getHatSwitchDirection()
        {
            return (state.GetPointOfView())[0];
        }

        public int getNumberPOV()
        {
            return Camjoystick.Capabilities.PovCount;
        }

        void mainloop()
        {
            while (enabled)
            {
                try
                {
                    System.Threading.Thread.Sleep(80);
                    //joystick stuff
                    Camjoystick.Poll();
                    state = Camjoystick.CurrentCamJoystickState();
                    if (getNumberPOV() > 0)
                    {
                        int pov = getHatSwitchDirection();

                        if (pov != -1)
                        {
                            int angle = pov / 100;

                            //0 = down = 18000
                            //0 = up = 0

                            // 0
                            if (angle > 270 || angle < 90)
                                hat1 += 2500;
                            // 180
                            if (angle > 90 && angle < 270)
                                hat1 -= 2500;
                            // 90
                            if (angle > 0 && angle < 180)
                                hat2 += 2500;
                            // 270
                            if (angle > 180 && angle < 360)
                                hat2 -= 2500;
                        }
                        else
                        {
                            hat1 = 32767;
                            hat2 = 32767;
                        }
                    }

                    /* read the joystick values to a local array */
                    for (int i = 0; i < MainV2.Camjoystick.ch_data.Length ; i++)
                    {
                        if (getJoystickAxis(i) != CamJoystick.joystickaxis.None)
                            MainV2.Camjoystick.ch_data[i] = pickchannel(i, JoyChannels[i].axis, JoyChannels[i].reverse, 0);
                    }

                    /* apply deadzone and gain */
                    int center = 512;
                    int roll = (ch_data[0] - center);
                    int pitch = (ch_data[1] - center);

                    int roll_no_dz = (ch_data[0]);
                    int pitch_no_dz = (ch_data[1]);


                    if ( roll > joystick_DZ )
                    {
                        roll -= joystick_DZ;
                        roll = (int)(roll * joystick_gain);
                    }
                    else if (roll < -joystick_DZ)
                    {
                        roll += joystick_DZ;
                        roll = (int)(roll * joystick_gain);
                    }
                    else
                        roll = 0;

                    if (pitch > joystick_DZ)
                    {
                        pitch -= joystick_DZ;
                        pitch = (int)(pitch * joystick_gain);
                    }
                    else if (pitch < -joystick_DZ)
                    {
                        pitch += joystick_DZ;
                        pitch = (int)(pitch * joystick_gain);
                    }
                    else
                        pitch = 0;

                    ch_data[0] = (ushort)(roll + center);
                    ch_data[1] = (ushort)(pitch + center);

                    if (ch_data[0] < 0)
                        ch_data[0] = 0;
                    else if (ch_data[0] > 1023)
                        ch_data[0] = 1023;

                    if (ch_data[1] < 0)
                        ch_data[1] = 0;
                    else if (ch_data[1] > 1023)
                        ch_data[1] = 1023;

                    // check if zoom is mapped to analog channel
                    if (getJoystickAxis(21) != CamJoystick.joystickaxis.None)
                    {
                        ushort zoom_in = 0, zoom_out=0;

                        if (ch_data[21] > joystick_DZ + 512)
                            zoom_in = 2000;
                        else if (ch_data[21] < 512 - joystick_DZ)
                            zoom_out = 2000;

                        /* send the gimbal command every iteration */
                        SendGimbalCommand(ch_data[0], ch_data[1], zoom_in, zoom_out);
                    }
                    else
                        /* send the gimbal command every iteration */
                        SendGimbalCommand(ch_data[0], ch_data[1], ch_data[2], ch_data[3]);

                    /* execute all button functions */
                    for (int i = 0; i < btn_func_array.Length; i++)
                    {
                        int ch = i + 4;
                        if (getJoystickAxis(ch) != CamJoystick.joystickaxis.None)
                            btn_func_array[i].CamJoystickBtntStateMachine(MainV2.Camjoystick.ch_data[ch]);
                    }
                }
                catch (SharpDX.SharpDXException ex)
                {
                    log.Error(ex);
                    uint errno = (uint)ex.HResult;
                    if (errno == 0x8007001E)
                    {
                        MainV2.instance.Invoke((System.Action)
                            delegate { CustomMessageBox.Show("Lost Camera Joystick", "Lost Camera Joystick"); });
                    }
                    return;
                }
                catch (Exception ex)
                {
                    log.Info("Joystick thread error " + ex.ToString());
                } // so we cant fall out
            }
        }

        public void UnAcquireJoyStick()
        {
            if (Camjoystick == null)
                return;
            Camjoystick.Unacquire();
        }

        public joystickaxis getJoystickAxis(int channel)
        {
            try
            {
                return JoyChannels[channel].axis;
            }
            catch
            {
                return joystickaxis.None;
            }
        }

        public ushort getRawValueForChannel(int channel)
        {
            if (Camjoystick == null)
                return 0;

            Camjoystick.Poll();

            state = Camjoystick.CurrentCamJoystickState();

            ushort ans = pickchannel(channel, JoyChannels[channel].axis, false, 0);
            log.DebugFormat("{0} = {1} = {2}", channel, ans, state.X);
            return ans;
        }

        ushort pickchannel(int chan, joystickaxis axis, bool rev, int expo)
        {
            int min, max, trim = 0;

            if (axis == joystickaxis.None)
                return 0;

            min = 0;
            max = 1023;
            trim = 512;

            int range = Math.Abs(max - min);
            int working = 0;

            CamJoyButtons = state.GetButtons();
            bool btn_pressed = false;

            switch (axis)
            {
                case joystickaxis.None:
                    working = ushort.MaxValue / 2;
                    break;
                case joystickaxis.Rx:
                    working = state.Rx;
                    break;
                case joystickaxis.Ry:
                    working = state.Ry;
                    break;
                case joystickaxis.Rz:
                    working = state.Rz;
                    break;
                case joystickaxis.X:
                    working = state.X;
                    break;
                case joystickaxis.Y:
                    working = state.Y;
                    break;
                case joystickaxis.Z:
                    working = state.Z;
                    break;
                case joystickaxis.HatUpDown:
                    hat1 = (int)Constrain(hat1, 0, 65535);
                    working = hat1;
                    break;
                case joystickaxis.HatLeftRight:
                    hat2 = (int)Constrain(hat2, 0, 65535);
                    working = hat2;
                    break;
                case joystickaxis.btn1:
                    working = 1000 + (1000 * Convert.ToInt16(CamJoyButtons[0]));
                    btn_pressed = true;
                    break;
                case joystickaxis.btn2:
                    working = 1000 + (1000 * Convert.ToInt16(CamJoyButtons[1]));
                    btn_pressed = true;
                    break;
                case joystickaxis.btn3:
                    working = 1000 + (1000 * Convert.ToInt16(CamJoyButtons[2]));
                    btn_pressed = true;
                    break;
                case joystickaxis.btn4:
                    working = 1000 + (1000 * Convert.ToInt16(CamJoyButtons[3]));
                    btn_pressed = true;
                    break;
                case joystickaxis.btn5:
                    working = 1000 + (1000 * Convert.ToInt16(CamJoyButtons[4]));
                    btn_pressed = true;
                    break;
                case joystickaxis.btn6:
                    working = 1000 + (1000 * Convert.ToInt16(CamJoyButtons[5]));
                    btn_pressed = true;
                    break;
                case joystickaxis.btn7:
                    working = 1000 + (1000 * Convert.ToInt16(CamJoyButtons[6]));
                    btn_pressed = true;
                    break;
                case joystickaxis.btn8:
                    working = 1000 + (1000 * Convert.ToInt16(CamJoyButtons[7]));
                    btn_pressed = true;
                    break;
                case joystickaxis.btn9:
                    working = 1000 + (1000 * Convert.ToInt16(CamJoyButtons[8]));
                    btn_pressed = true;
                    break;
                case joystickaxis.btn10:
                    working = 1000 + (1000 * Convert.ToInt16(CamJoyButtons[9]));
                    btn_pressed = true;
                    break;
                case joystickaxis.btn11:
                    working = 1000 + (1000 * Convert.ToInt16(CamJoyButtons[10]));
                    btn_pressed = true;
                    break;
                case joystickaxis.btn12:
                    working = 1000 + (1000 * Convert.ToInt16(CamJoyButtons[11]));
                    btn_pressed = true;
                    break;
                case joystickaxis.btn13:
                    working = 1000 + (1000 * Convert.ToInt16(CamJoyButtons[12]));
                    btn_pressed = true;
                    break;
                case joystickaxis.btn14:
                    working = 1000 + (1000 * Convert.ToInt16(CamJoyButtons[13]));
                    btn_pressed = true;
                    break;
                case joystickaxis.btn15:
                    working = 1000 + (1000 * Convert.ToInt16(CamJoyButtons[14]));
                    btn_pressed = true;
                    break;
                case joystickaxis.btn16:
                    working = 1000 * +(1000 * Convert.ToInt16(CamJoyButtons[15]));
                    btn_pressed = true;
                    break;
                case joystickaxis.btn17:
                    working = 1000 * +(1000 * Convert.ToInt16(CamJoyButtons[16]));
                    btn_pressed = true;
                    break;
                case joystickaxis.btn18:
                    working = 1000 * +(1000 * Convert.ToInt16(CamJoyButtons[17]));
                    btn_pressed = true;
                    break;
                case joystickaxis.btn19:
                    working = 1000 * +(1000 * Convert.ToInt16(CamJoyButtons[18]));
                    btn_pressed = true;
                    break;
                case joystickaxis.btn20:
                    working = 1000 * +(1000 * Convert.ToInt16(CamJoyButtons[19]));
                    btn_pressed = true;
                    break;                
            }

            if (btn_pressed == true)
            {
                if (rev)
                {
                    if (working == 1000)
                        working = 2000;
                    else
                        working = 1000;
                }
                return (ushort)working;
            }

            // between 0 and 65535 - convert to int -500 to 500
            working = (int)(working / 65.535) - 500;
            if (rev)
                working *= -1;

            // save for later
            int raw = working;

            working = (int)Expo(working, expo, min, max, trim);

            if (working > 507 && working < 517)
                working = 512;

            //add limits to movement
            working = Math.Max(min, working);
            working = Math.Min(max, working);
            return (ushort)working;
        }

        public static double Expo(double input, double expo, double min, double max, double mid)
        {
            // input range -500 to 500

            double expomult = expo / 100.0;

            if (input >= 0)
            {
                // linear scale
                double linearpwm = map(input, 0, 500, mid, max);
                double expomid = (max - mid) / 2;
                double factor = 0;

                // over half way though input
                if (input > 250)
                    factor = 250 - (input - 250);
                else
                    factor = input;

                return linearpwm - (factor * expomult);
            }
            else
            {
                double linearpwm = map(input, -500, 0, min, mid);

                double expomid = (mid - min) / 2;

                double factor = 0;

                // over half way though input
                if (input < -250)
                    factor = -250 - (input + 250);
                else
                    factor = input;

                return linearpwm - (factor * expomult);
            }
        }

        static double map(double x, double in_min, double in_max, double out_min, double out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

        double Constrain(double value, double min, double max)
        {
            if (value > max)
                return max;
            if (value < min)
                return min;
            return value;
        }

        public void SendGimbalCommand(ushort roll, ushort pitch, ushort zoom_in, ushort zoom_out)
        {
            float _roll;
            float _pitch;
            float zoom;
            MAVLink.mavlink_command_long_t cmd_long = new MAVLink.mavlink_command_long_t();
            _roll = (float)roll - 0x200;
            if (roll != 0x200)
                _roll = (float)(_roll / (float)0x200);

            _pitch = (float)pitch - 0x200;
            if (pitch != 0x200)
                _pitch = (float)(_pitch / (float)0x200);

            if (!MainV2.comPort.BaseStream.IsOpen)
                return;
            if (zoom_in == zoom_out)
                zoom = (float)NvMavExtCmds.SetGimbalArgs.ZoomStop;
            else if (zoom_in == 2000)
                zoom = (float)NvMavExtCmds.SetGimbalArgs.ZoomIn;
            else
                zoom = (float)NvMavExtCmds.SetGimbalArgs.ZoomOut;            
            
            MainV2.comPort.doCommand(MainV2.comPort.MAV.sysid, MainV2.comPort.MAV.compid, MAVLink.MAV_CMD.DO_DIGICAM_CONTROL, (float)NvMavExtCmds.Cmd.SetGimbal, _roll, _pitch, zoom, GndCrsAlt, 0, 0, false);
        }

        public void TrackOnPosition(int posX, int posY)
        {
            if (!MainV2.comPort.BaseStream.IsOpen)
                return;
            MainV2.comPort.doCommand(MainV2.comPort.MAV.sysid, MainV2.comPort.MAV.compid,MAVLink.MAV_CMD.DO_DIGICAM_CONTROL,
                                    (float)NvMavExtCmds.Cmd.SetSystemMode,
                                    (float)NvMavExtCmds.SetSystemModeArgs.Tracking,
                                    posX, posY, 0, 0, 0, false);
        }
        
        public void SetCameraMode(NvMavExtCmds.SetSystemModeArgs camMode)
        {
            if (!MainV2.comPort.BaseStream.IsOpen)
                return;
            MainV2.comPort.doCommand(MainV2.comPort.MAV.sysid, MainV2.comPort.MAV.compid,MAVLink.MAV_CMD.DO_DIGICAM_CONTROL,
                        (float)NvMavExtCmds.Cmd.SetSystemMode,
                        (float)camMode,
                        0, 0, 0, 0, 0, false);
        }

        public void SetCameraSharpness(NvMavExtCmds.SetSensorArgs sensor, NvMavExtCmds.SetSharpnessArgs sharpnessLevel)
        {
            if (!MainV2.comPort.BaseStream.IsOpen)
                return;
            MainV2.comPort.doCommand(MainV2.comPort.MAV.sysid, MainV2.comPort.MAV.compid,MAVLink.MAV_CMD.DO_DIGICAM_CONTROL,
                        (float)NvMavExtCmds.Cmd.SetSharpness,
                        (float)sensor,
                        (float)sharpnessLevel, 0, 0, 0, 0, false);
        }

        public void ClearRetractBlock()
        {
            if (!MainV2.comPort.BaseStream.IsOpen)
                return;
            MainV2.comPort.doCommand(MainV2.comPort.MAV.sysid, MainV2.comPort.MAV.compid,MAVLink.MAV_CMD.DO_DIGICAM_CONTROL,
                        (float)NvMavExtCmds.Cmd.ClearRetractLock,
                        0, 0, 0, 0, 0, 0, false);
        }

        public void SetCameraMode(NvMavExtCmds.SetSystemModeArgs camMode, float pitch, float roll)
        {
            if (!MainV2.comPort.BaseStream.IsOpen)
                return;
            MainV2.comPort.doCommand(MainV2.comPort.MAV.sysid, MainV2.comPort.MAV.compid,MAVLink.MAV_CMD.DO_DIGICAM_CONTROL,
                        (float)NvMavExtCmds.Cmd.SetSystemMode,
                        (float)camMode,
                        pitch, roll, 0, 0, 0, false);
        }

        public void DoBtnCommand(NvMavExtCmds.BtnCommand cmd)
        {
            btn_func_array[(int)cmd].CamJoystickBtntStateMachine(2000);
        }
    }
}