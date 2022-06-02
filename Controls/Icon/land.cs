using System.Drawing;

namespace MissionPlanner.Controls.Icon
{
    public class land : Icon
    {
        internal override void doPaint(Graphics g)
        {
            var mid = Width / 2;

            var points = new Point[]
            {
                
                new Point( mid - 7, mid + 9),
                new Point( mid + 7, mid + 9)
            };

            g.DrawLines(LinePen, points);

            points = new Point[]
            {
                new Point( mid, mid -11),
                 new Point( mid, mid + 7),
                new Point( mid - 7, mid + 0),
                new Point( mid + 7, mid + 0),
                new Point( mid, mid + 7),
            };

            g.DrawLines(LinePen, points);
        }
    }
}
