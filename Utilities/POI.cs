using GMap.NET.WindowsForms;
using MissionPlanner.Controls;
using MissionPlanner.Maps;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MissionPlanner.Utilities
{
    public class POI
    {
        /// <summary>
        /// Store points of interest
        /// </summary>
        static ObservableCollection<PointLatLngAlt> POIs = new ObservableCollection<PointLatLngAlt>();

        private static EventHandler _POIModified;

        public static event EventHandler POIModified
        {
            add
            {
                _POIModified += value;
                try
                {
                    //if (File.Exists(filename))
                    //    LoadFile(filename);
                }
                catch
                {
                }
            }
            remove { _POIModified -= value; }
        }

        private static string filename = Settings.GetUserDataDirectory() + "poi.txt";
        private static bool loading;
        /* NextVision Variables */
        private static bool disconnecting;

        static POI()
        {
            POIs.CollectionChanged += POIs_CollectionChanged;
        }

        private static void POIs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (loading || disconnecting)
                    return;
                SaveFile(filename);
            }
            catch { }
        }

        public static void POIAdd(PointLatLngAlt Point, string tag)
        {
            // local copy
            PointLatLngAlt pnt = Point;

            pnt.Tag = tag + "\n" + pnt.ToString();

            POI.POIs.Add(pnt);

            if (_POIModified != null && !loading)
                _POIModified(null, null);
        }

        public static void POIAdd(PointLatLngAlt Point)
        {
            if (Point == null)
                return;

            PointLatLngAlt pnt = Point;

            string output = "";

            if (DialogResult.OK != InputBox.Show("POI", "Enter ID", ref output))
                return;

            POIAdd(Point, output);
        }

        public static void POIDelete(GMapMarkerPOI Point)
        {
            if (Point == null)
                return;

            for (int a = 0; a < POI.POIs.Count; a++)
            {
                if (POI.POIs[a].Point() == Point.Position)
                {
                    POI.POIs.RemoveAt(a);
                    if (_POIModified != null)
                        _POIModified(null, null);
                    return;
                }
            }
        }

        public static void POIEdit(GMapMarkerPOI Point)
        {
            if (Point == null)
                return;

            string output = "";

            if (DialogResult.OK != InputBox.Show("POI", "Enter ID", ref output))
                return;

            for (int a = 0; a < POI.POIs.Count; a++)
            {
                if (POI.POIs[a].Point() == Point.Position)
                {
                    POI.POIs[a].Tag = output + "\n" + Point.Position.ToString();
                    if (_POIModified != null)
                        _POIModified(null, null);
                    return;
                }
            }
        }

        public static void POIMove(GMapMarkerPOI Point)
        {
            for (int a = 0; a < POI.POIs.Count; a++)
            {
                if (POIs[a].Tag == Point.ToolTipText)
                {
                    POIs[a].Lat = Point.Position.Lat;
                    POIs[a].Lng = Point.Position.Lng;
                    POIs[a].Tag = POIs[a].Tag.Substring(0, POIs[a].Tag.IndexOf('\n')) + "\n" + Point.Position.ToString();
                    break;
                }
            }

            if (_POIModified != null)
                _POIModified(null, null);
        }

        public static void POISave()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Poi File|*.txt";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    SaveFile(sfd.FileName);
                }
            }
        }

        private static void SaveFile(string fileName)
        {
            using (Stream file = File.Open(fileName, FileMode.Create))
            {
                foreach (var item in POI.POIs)
                {
                    string line = item.Lat.ToString(CultureInfo.InvariantCulture) + "\t" +
                                  item.Lng.ToString(CultureInfo.InvariantCulture) + "\t" + item.Tag.Substring(0, item.Tag.IndexOf('\n')) + "\r\n";
                    byte[] buffer = ASCIIEncoding.ASCII.GetBytes(line);
                    file.Write(buffer, 0, buffer.Length);
                }
            }
        }


        /****************************************************************************************************************************
         *                                                    NextVision
        *                                                      POILoad()
        * Description : Loads all the POIs from the POI file, and returns the POIs collection    
        *
        ****************************************************************************************************************************/
        public static ObservableCollection<PointLatLngAlt> POILoad()
        {
            using (OpenFileDialog sfd = new OpenFileDialog())
            {
                sfd.Filter = "Poi File|*.txt";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    LoadFile(sfd.FileName);
                }
            }
            return POIs;
        }

        /****************************************************************************************************************************
        *                                                    NextVision
        *                                                      LoadFile()
        * Description : Loads all the POIs from the POI file   
        *
        ****************************************************************************************************************************/
        private static void LoadFile(string fileName)
        {
            loading = true;
            int poi_tag_id = 0;
            using (Stream file = File.Open(fileName, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    while (!sr.EndOfStream)
                    {
                        string[] items = sr.ReadLine().Split('\t');

                        if (items.Count() < 3)
                            continue;

                        //POIAdd(new PointLatLngAlt(double.Parse(items[0], CultureInfo.InvariantCulture), double.Parse(items[1], CultureInfo.InvariantCulture)), items[2]);
                        PointLatLngAlt pnt = new PointLatLngAlt(double.Parse(items[0], CultureInfo.InvariantCulture), double.Parse(items[1], CultureInfo.InvariantCulture));
                        pnt.TagId = poi_tag_id++;
                        POIAdd(pnt, items[2]);
                    }
                }
            }
            loading = false;
            // redraw now
            if (_POIModified != null)
                _POIModified(null, null);
        }

        public static void UpdateOverlay(GMap.NET.WindowsForms.GMapOverlay poioverlay)
        {
            if (poioverlay == null)
                return;

            poioverlay.Clear();

            foreach (var pnt in POIs)
            {
                poioverlay.Markers.Add(new GMapMarkerPOI(pnt)
                {
                    ToolTipMode = MarkerTooltipMode.OnMouseOver,
                    ToolTipText = pnt.Tag
                });
            }
        }

        /****************************************************************************************************************************
         *                                            
         *                                              NextVision functions 
         * 
        ****************************************************************************************************************************/
        /****************************************************************************************************************************
        *                                                      POI_Add()
        * Description : Adding a Point to POIs     
        *
        ****************************************************************************************************************************/
        public static PointLatLngAlt POI_Add(PointLatLngAlt Point)
        {
            string output = "";

            if (Point == null)
            {
                CustomMessageBox.Show("Invalid POI");
                return null;
            }

            if (POIs.Count >= 16)
            {
                CustomMessageBox.Show("POI Limit Reached");
                return null;
            }

            if (DialogResult.OK != InputBox.Show("POI", "Enter Name", ref output))
                return null;

            PointLatLngAlt pnt = Point;

            bool[] poi_id_use_table = new bool[16];
            for (int a = 0; a < POI.POIs.Count; a++)
                poi_id_use_table[POI.POIs[a].TagId] = true;

            for (int a = 0; a < 16; a++)
            {
                if (poi_id_use_table[a] == false)
                {
                    Point.TagId = a;
                    break;
                }
            }

            POIAdd(Point, output);

            return Point;
        }

        /****************************************************************************************************************************
        *                                                      POI_Delete()
        * Description : Deletes a Point from POIs     
        *
        ****************************************************************************************************************************/
        public static PointLatLngAlt POI_Delete(GMapMarkerPOI Point)
        {
            if (Point == null)
                return null;

            for (int a = 0; a < POI.POIs.Count; a++)
            {
                if (POI.POIs[a].Point() == Point.Position)
                {
                    PointLatLngAlt pnt = POI.POIs[a];
                    POI.POIs.RemoveAt(a);
                    if (_POIModified != null)
                        _POIModified(null, null);
                    return pnt;
                }
            }
            return null;
        }

        /****************************************************************************************************************************
        *                                                      POIDeletAll()
        * Description : Deletes all POIs from the POIs collection    
        *
        ****************************************************************************************************************************/
        public static void POIDeletAll()
        {
            disconnecting = true;
            POI.POIs.Clear();
            if (_POIModified != null)
                _POIModified(null, null);
            disconnecting = false;
        }

        /****************************************************************************************************************************
        *                                                      POILoadDefaultPath()
        * Description : Loads the POIs from default Path    
        *
        ****************************************************************************************************************************/
        public static ObservableCollection<PointLatLngAlt> POILoadDefaultPath()
        {
            LoadFile(filename);
            return POIs;
        }

        /****************************************************************************************************************************
        *                                                      POI_Move()
        * Description : Moving the POI to the position of Point(GMapMarkerPOI) Point    
        *
        ****************************************************************************************************************************/
        public static PointLatLngAlt POI_Move(GMapMarkerPOI Point)
        {
            int a;
            for (a = 0; a < POI.POIs.Count; a++)
            {
                if (POIs[a].Tag == Point.ToolTipText)
                {
                    POIs[a].Lat = Point.Position.Lat;
                    POIs[a].Lng = Point.Position.Lng;
                    POIs[a].Tag = POIs[a].Tag.Substring(0, POIs[a].Tag.IndexOf('\n')) + "\n" + Point.Position.ToString();
                    break;
                }
            }

            if (_POIModified != null)
                _POIModified(null, null);

            /* save the poi file after movement */
            SaveFile(filename);

            return POIs[a];
        }
    }
}