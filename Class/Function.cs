using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RoomLayout
{
    class Function
    {
        public static bool IsLineCross(PointF lineA1, PointF lineA2, PointF lineB1, PointF lineB2)
        {
            float p1 = lineA2.Y - lineA1.Y;
            float p2 = lineA1.X - lineA2.X;
            float p3 = lineA2.X * lineA1.Y - lineA1.X * lineA2.Y;

            float q1 = lineB2.Y - lineB1.Y;
            float q2 = lineB1.X - lineB2.X;
            float q3 = lineB2.X * lineB1.Y - lineB1.X * lineB2.Y;

            float sign1 = (p1 * lineB1.X + p2 * lineB1.Y + p3) * (p1 * lineB2.X + p2 * lineB2.Y + p3);
            float sign2 = (q1 * lineA1.X + q2 * lineA1.Y + q3) * (q1 * lineA2.X + q2 * lineA2.Y + q3);
            return (sign1 < 0 && sign2 < 0);
        }
    }
}
