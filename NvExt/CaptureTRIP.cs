using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;
using NextVisionVideoControlUILessLibrary;

namespace MissionPlanner.Utilities
{
    public delegate void TRIPCamImage(Image camimage);

    public class CaptureTRIP
    {
        public event TRIPCamImage trip_cam_image;
        private VideoControl vid_ctl;

        public CaptureTRIP()
        {
            /* create the video control object */
            vid_ctl = new VideoControl();
        }

        public int Start(string ip, int port, string vid_fwd_ip, int vid_fwd_port)
        {
            /* create the video control object */
            if (Settings.Instance["enable_ajc"] != null)
            {
                if (Settings.Instance["enable_ajc"] == "true")
                    return vid_ctl.VideoControlStartStream(ip, port, raw_frame_callback, true, vid_fwd_ip, vid_fwd_port);
                else
                    return vid_ctl.VideoControlStartStream(ip, port, raw_frame_callback, false, vid_fwd_ip, vid_fwd_port);
            }
            else
                return vid_ctl.VideoControlStartStream(ip, port, raw_frame_callback, false, vid_fwd_ip, vid_fwd_port);
        }

        public bool StartRec()
        {
            /* check that the stream is locked */
            if (vid_ctl.VideoControlGetStreamStatus() == stream_status.StreamDetectionOk)
            {
                if (vid_ctl.VideoControlGetRecordStatus() == recording_status.RecordingIdle)
                {
                    string rec_dir = Settings.GetUserDataDirectory();
                    rec_dir += "\\Recordings";                   
                    
                    /* create the directory if its not there */
                    System.IO.Directory.CreateDirectory(rec_dir);

                    /* generate the recording file name */
                    var time = DateTime.Now;
                    string rec_file = rec_dir + "\\" + time.ToString("dd-MM-yyyy__HH-mm-ss") + ".ts";

                    /* start the recording */
                    vid_ctl.VideoControlStartRec(rec_file);
                    return true;
                }
            }
            return false;
        }

        public bool StopRec()
        {
            if (vid_ctl.VideoControlGetRecordStatus() == recording_status.RecordingEnabled)
            {
                vid_ctl.VideoControlStopRec();
                return true;
            }
            return false;
        }

        private void raw_frame_callback(byte[] frame_buf, stream_status status, int width, int height)
        {        
            /* If the stream status is Ok put the image on hud1 bgimage */
            if (status == stream_status.StreamDetectionOk)
            {
                /* create a bitmap */
                Bitmap bitmap = new Bitmap(width, height, 3 * width, System.Drawing.Imaging.PixelFormat.Format24bppRgb, Marshal.UnsafeAddrOfPinnedArrayElement(frame_buf, 0));
                trip_cam_image(bitmap);             
            }
        }
        public bool GetKlvTag(int tag_num, out byte[] value )
        {
            klv_tag tag;
            value = null;

            if (vid_ctl.VideoControlGetKlvTag(tag_num, out tag) == true)
            {
                if (tag.valid)
                {
                    value = (byte[])tag.data.Clone();
                    return true;
                }               
            }
            return false;
        }
    }
}
