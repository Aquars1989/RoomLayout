using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RoomLayout
{
    class Function
    {
        /// <summary>
        /// 取得線段是否相交
        /// </summary>
        /// <param name="lineA1">線段A端點1</param>
        /// <param name="lineA2">線段A端點2</param>
        /// <param name="lineB1">線段B端點1</param>
        /// <param name="lineB2">線段B端點2</param>
        /// <returns>線段是否相交</returns>
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

        /// <summary>
        /// 取得點是否在圖形內
        /// </summary>
        /// <param name="points">圖形座標組</param>
        /// <param name="checkPoint">檢查點</param>
        /// <returns>是否在圖形內</returns>
        public static bool IsPointInside(PointF[] points, PointF checkPoint)
        {
            return Multiply(checkPoint, points[0], points[1]) * Multiply(checkPoint, points[3], points[2]) < 0 &&
                   Multiply(checkPoint, points[3], points[0]) * Multiply(checkPoint, points[2], points[1]) < 0;
        }

        private static double Multiply(PointF p1, PointF p2, PointF p0)
        {
            return ((p1.X - p0.X) * (p2.Y - p0.Y) - (p2.X - p0.X) * (p1.Y - p0.Y));
        }

        /// <summary>
        /// 取得兩圖形是否相交
        /// </summary>
        /// <param name="points1">圖形座標組1</param>
        /// <param name="points2">圖形座標組2</param>
        /// <returns>圖形是否相交</returns>
        public static bool IsIntersect(PointF[] points1, PointF[] points2)
        {
            for (int i = 0; i < 4; i++)
            {
                int i2 = i < 3 ? i + 1 : 0;
                for (int j = 0; j < 4; j++)
                {
                    int j2 = j < 3 ? j + 1 : 0;
                    if (Function.IsLineCross(points1[i], points1[i2], points2[j], points2[j2]))
                    {
                        return true;
                    }
                }

                if (Function.IsLineCross(points1[i], points1[i2], points2[0], points2[2]) ||
                    Function.IsLineCross(points1[i], points1[i2], points2[1], points2[3]))
                {
                    return true;
                }
            }

            if (IsPointInside(points2, points1[0]) || IsPointInside(points1, points2[0]))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 取德旋轉後角度
        /// </summary>
        /// <param name="baseAngle">原始角度</param>
        /// <param name="rotate">調整角度</param>
        /// <returns>旋轉後角度</returns>
        public static int GetRotateAngle(int baseAngle, int rotate)
        {
            int result = baseAngle + rotate;
            if (result > 180)
            {
                result = (result % 180) - 180;
            }
            else if (result <= -180)
            {
                result = (result % 180) + 180;
            }
            return result;
        }
    }
}
