using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionPlanner.NvExt
{
    public static class NvMavExtCmds
    {
        public static string NvVersion = " NvExt 1.0.4";

        public enum Cmd
        {
            SetSystemMode,
            TakeSnapShot,
            SetRecordState,
            SetSensor,
            SetFOV,
            SetSharpness,
            SetGimbal,
            DoBIT,
            SetIrPolarity,
            SetSingleYawMode,
            SetFollowMode,
            DoNUC,
            SetReportInterval,
            ClearRetractLock,
            SetSystemTime,
            SetIrColor,
            SetJoystickMode,
            SetGndCrsAltitude,
            SetRollDerotation,
            SetLaser,
            RebootSystem,
            ReconfigureVideoStreamBitrate,
            SetTrackingIcon,
            SetZoom,
            FreezeVideo,
            SetColibExtMode,
            PilotView,
            RvtPostion,
            SnapShotInterval,
            UpadteRemoteIP,
            UpdateVideoIP,
            ConfigurationCommand,
            ClearSDCard,
            SetRateMultiplier,
            SetIRGainLevel,
            SetVideoStreamTransmissionState,
            SetDayWhiteBalance,
            SetLaserMode,
            SetHoldCoordinateMode,
            NA,
            CameraTest,
            SetAIDetection,
            ResumeAIScan,
            SetFlyAbove,
            SetCameraStabilization =50,
            UpdateStreamBitrate,
            DoIperfTest,
            SetGeoAvg,
            DetectionControl,
            ARMarkerControl,
            GeoMapControl,
            StreamControl,
            VMDControl,
            MultipleGCSControl,
        }

        public enum SetSystemModeArgs
        {
            Stow,
            Pilot,
            Hold,
            Observation,
            LocalPosition,
            GlobalPosition,
            GRR,
            Tracking,
            EPR,
            Nadir,
            NadirScan,
            TwoDScan,
            PTC,
            UnstabilizedPosition,
        }

        public enum DisArgs
        {
            Disable,
            Enable,
            Toggle
        }

        public enum SetSensorArgs
        {
            DaySensor,
            IrSensor,
            ToggleSensor
        }

        public enum SetSharpnessArgs
        {
            NoSharpnessBoost,
            LowSharpnessBoost,
            HighSharpnessBoost
        }

        public enum SetGimbalArgs
        {
            ZoomStop,
            ZoomIn,
            ZoomOut,
            ZoomNoChange
        }

        public enum SetPolarityArgs
        {
            WhiteHot,
            BlackHot,
            TogglePolarity,
        }

        public enum ReportType
        {
            System,
            Los,
            GndCrs,
            RVT,
            RVTLocation,
            Snapshot,
            SDCard,
            Video,
            LosRate,
            ObjectDetection,
            IMU,
            Fire,
            Tracking,
            LPR,
            ARMarker,
            Parameter,
            CarCount,
            OGLR
        }
        public enum BtnCommand
        {
            DoNUC = 0,
            SetIrPolarity = 1,
            SetSensor = 2,
            SetRecordState = 3,
            TakeSnapShot = 4,
            SetSingleYawMode = 7,
            DoBIT = 8,
            Retract = 9,
            SetFollowMode = 10,
        }
        public enum TrackingMode
        {
            Idle = 0,
            Enabled = 1,
            Track = 2,
            Retrack = 3,
            TrackOnPos1 = 4,
            TrackOnPos2 = 5,
        }

        public enum SystemMode
        {
            Stow,
            Pilot,
            Retract,
            RetractLock,
            Observation,
            GRR,
            Hold,
            PTC,
            LocalPosition,
            GlobalPosition,
            Tracking,
            EPR,
            BIT,
        }

        public enum POICommand
        {
            AddTarget = 1,
            DelTarget = 2,
            DelAllTargets = 3,
        }


        public enum SetThermalGainLevelArgs
        {
            LevelDecrement = 0,
            LevelIncrement,
            GainDecrement,
            GainIncrement,
            GainLevelReset,
        }

        public enum DetectionControlArgs
        {
            DetectorEnableDisable = 0,
            DetectorSelect,
            DetectorThreshold,
            FireThreshold,
        }

        public enum GeoMapControlArgs
        {
            RefineLocation = 0,
            SetReferencePoint
        }

        public enum StreamControlArgs
        {
            SetStreamMode = 0,
            SetPIPMode,
            SetSBSMode
        }

        public enum SetStreamModeArgs
        {
            Disabled = 0,
            Day,
            IR,
            Fusion,
            PIP,
            SideBySide
        }

        public enum SetPIPModeArgs
        {
            VisableLarge = 0,
            IRLarge
        }

        public enum SetSBSModeArgs
        {
            VisableLeft = 0,
            IRLeft
        }

        public enum VMDControlArgs
        {
            VMDEnable = 0,
            VMDSetColor,
            VMDSendReports,
        }

        public enum MultipleGCSControlArgs
        {
            MultipleGCSEnableSecondaryControl = 0,
            MultipleGCSEnableSecondaryReports,
        }
    }
}
