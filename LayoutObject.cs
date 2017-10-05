using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace RoomLayout
{
    public class LayoutObject
    {
        private static int _IDMax = 0;

        private bool _RebuildPoints = true;

        private PointF[] _DrawPoints = new PointF[4];
        [Browsable(false)]
        public PointF[] DrawPoints
        {
            get
            {
                if (_RebuildPoints)
                {
                    double rotateH = Angle * Math.PI / 180F;
                    double rotateV = (Angle + 90) * Math.PI / 180F;

                    float x = X * Scale;
                    float y = Y * Scale;
                    float x1 = (float)(Width / 2 * Math.Cos(rotateH)) * Scale; //寬度修正
                    float y1 = (float)(Width / 2 * Math.Sin(rotateH)) * Scale; //寬度修正
                    float x2 = (float)(Height / 2 * Math.Cos(rotateV)) * Scale; //高度修正
                    float y2 = (float)(Height / 2 * Math.Sin(rotateV)) * Scale; //高度修正
                    _DrawPoints[0] = new PointF(OffsetX + x - x1 + x2, OffsetY + y - y1 + y2);
                    _DrawPoints[1] = new PointF(OffsetX + x + x1 + x2, OffsetY + y + y1 + y2);
                    _DrawPoints[2] = new PointF(OffsetX + x + x1 - x2, OffsetY + y + y1 - y2);
                    _DrawPoints[3] = new PointF(OffsetX + x - x1 - x2, OffsetY + y - y1 - y2);

                    int fixX = 0, fixY = 0;
                    float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
                    float limitX = OffsetX + (ParentWidth * Scale);
                    float limitY = OffsetY + (ParentHeight * Scale);
                    foreach (PointF pots in _DrawPoints)
                    {
                        minX = Math.Min(minX, pots.X);
                        minY = Math.Min(minY, pots.Y);
                        maxX = Math.Max(maxX, pots.X);
                        maxY = Math.Max(maxY, pots.Y);
                    }

                    if (minX < OffsetX)
                    {
                        fixX = (int)(OffsetX - minX);
                    }
                    else if (maxX > limitX)
                    {
                        fixX = -(int)(maxX - limitX);
                    }

                    if (minY < OffsetY)
                    {
                        fixY = (int)(OffsetY - minY);
                    }
                    else if (maxY > limitY)
                    {
                        fixY = -(int)(maxY - limitY);
                    }

                    if (fixX != 0 || fixY != 0)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            _DrawPoints[i] = new PointF(_DrawPoints[i].X + fixX, _DrawPoints[i].Y + fixY);
                        }
                        _X += fixX;
                        _Y += fixY;
                    }
                    _RebuildPoints = false;
                }
                return _DrawPoints;
            }
        }


        [Description("ID"), DisplayName("0.索引")]
        public int ID { get; private set; }

        [Description("名稱"), DisplayName("1.名稱")]
        public string Name { get; set; }

        private int _X;
        [Description("X座標"), DisplayName("2.X座標")]
        public int X
        {
            get { return _X; }
            set
            {
                if (_X == value) return;
                _X = value;
                _RebuildPoints = true;
            }
        }

        private int _Y;
        [Description("Y座標"), DisplayName("3.Y座標")]
        public int Y
        {
            get { return _Y; }
            set
            {
                if (_Y == value) return;
                _Y = value;
                _RebuildPoints = true;
            }
        }

        private int _Width;
        [Description("寬度"), DisplayName("4.寬度")]
        public int Width
        {
            get { return _Width; }
            set
            {
                value = Math.Max(value, 5);
                if (_Width == value) return;
                _Width = value;
                _RebuildPoints = true;
            }
        }

        private int _Height;
        [Description("高度"), DisplayName("5.高度")]
        public int Height
        {
            get { return _Height; }
            set
            {
                value = Math.Max(value, 5);
                if (_Height == value) return;
                _Height = value;
                _RebuildPoints = true;
            }
        }

        private int _Angle;
        [Description("角度"), DisplayName("6.角度")]
        public int Angle
        {
            get { return _Angle; }
            set
            {
                value %= 180;
                if (_Angle == value) return;
                _Angle = value;
                _RebuildPoints = true;
            }
        }

        private int _OffsetX;
        [Browsable(false)]
        public int OffsetX
        {
            get { return _OffsetX; }
            set
            {
                _OffsetX = value;
                _RebuildPoints = true;
            }
        }

        private int _OffsetY;
        [Browsable(false)]
        public int OffsetY
        {
            get { return _OffsetY; }
            set
            {
                _OffsetY = value;
                _RebuildPoints = true;
            }
        }

        private int _ParentWidth;
        [Browsable(false)]
        public int ParentWidth
        {
            get { return _ParentWidth; }
            set
            {
                _ParentWidth = value;
                _RebuildPoints = true;
            }
        }

        private int _ParentHeight;
        [Browsable(false)]
        public int ParentHeight
        {
            get { return _ParentHeight; }
            set
            {
                _ParentHeight = value;
                _RebuildPoints = true;
            }
        }

        private float _Scale;
        [Browsable(false)]
        public float Scale
        {
            get { return _Scale; }
            set
            {
                _Scale = value;
                _RebuildPoints = true;
            }
        }

        public LayoutObject(int id, string name, int x, int y, int width, int height)
        {
            ID = id;
            Name = name;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            _IDMax = Math.Max(_IDMax, ID);
        }

        public LayoutObject(string name, int x, int y, int width, int height)
        {
            _IDMax++;
            ID = _IDMax;
            Name = name;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        private SolidBrush _BrushBack = new SolidBrush(Color.FromArgb(150, 200, 220, 255));
        private StringFormat _DrawStringFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        public void DrawSelf(Graphics g)
        {
            PointF[] pots = DrawPoints;
            g.FillPolygon(_BrushBack, pots);
            g.DrawPolygon(Pens.RoyalBlue, pots);

            Font font = new Font("微軟正黑體", Math.Min(Width, Height) / (Name.Length + 1) * Scale);
            SizeF size = g.MeasureString("國", font);
            double distance = Math.Max(size.Width, size.Height) * 0.75F;
            PointF center = GetCenter();

            int angle = Angle;
            if (Height > Width) angle += 90;
            if (angle > 145) angle += 180;
            else if (angle < 0 && angle > -145) angle += 180;

            double rotate = angle * Math.PI / 180F;

            float fixX = (float)(distance * Math.Cos(rotate)) * Scale;
            float fixY = (float)(distance * Math.Sin(rotate)) * Scale;
            float drawX = center.X - fixX * (Name.Length - 1) / 2;
            float drawY = center.Y - fixY * (Name.Length - 1) / 2;
            for (int i = 0; i < Name.Length; i++)
            {
                g.DrawString(Name[i].ToString(), font, Brushes.RoyalBlue, new PointF(drawX, drawY), _DrawStringFormat);
                drawX += fixX;
                drawY += fixY;
            }





        }

        public bool InRectangle(PointF point)
        {
            PointF[] pots = DrawPoints;
            return Multiply(point, pots[0], pots[1]) * Multiply(point, pots[3], pots[2]) <= 0 &&
                   Multiply(point, pots[3], pots[0]) * Multiply(point, pots[2], pots[1]) <= 0;
        }

        public bool IsIntersect(LayoutObject layoutObject)
        {
            PointF[] pots1 = DrawPoints;
            PointF[] pots2 = layoutObject.DrawPoints;
            for (int i = 0; i < 4; i++)
            {
                if (layoutObject.InRectangle(pots1[i]))
                {
                    return true;
                }

                if (InRectangle(pots2[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public static double Multiply(PointF p1, PointF p2, PointF p0)
        {
            return ((p1.X - p0.X) * (p2.Y - p0.Y) - (p2.X - p0.X) * (p1.Y - p0.Y));
        }

        public PointF GetCenter()
        {
            PointF[] pots = DrawPoints;
            return new PointF((pots[0].X + pots[2].X) / 2, (pots[0].Y + pots[2].Y) / 2);
        }
    }
}
