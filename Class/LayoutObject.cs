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
        /// <summary>
        /// 最新序號
        /// </summary>
        public static int IDMax = 0;

        /// <summary>
        /// 繪製文字設定
        /// </summary>
        private static StringFormat _DrawStringFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        /// <summary>
        /// 是否重新取得繪製點
        /// </summary>
        private bool _RebuildPoints = true;

        private LayoutPoints _Points = new LayoutPoints();
        /// <summary>
        /// 繪製點
        /// </summary>
        [Browsable(false)]
        public LayoutPoints Points
        {
            get
            {
                if (_RebuildPoints)
                {
                    double rotateH = Angle * Math.PI / 180F;
                    double rotateV = (Angle + 90) * Math.PI / 180F;

                    float widFixX = (float)(Width / 2F * Math.Cos(rotateH)); //寬度修正
                    float widFixY = (float)(Width / 2F * Math.Sin(rotateH)); //寬度修正
                    float heiFixX = (float)(Height / 2F * Math.Cos(rotateV)); //高度修正
                    float heiFixY = (float)(Height / 2F * Math.Sin(rotateV)); //高度修正

                    float[] x = { X - widFixX + heiFixX,
                                  X + widFixX + heiFixX,
                                  X + widFixX - heiFixX,
                                  X - widFixX - heiFixX};
                    float[] y = { Y - widFixY + heiFixY,
                                  Y + widFixY + heiFixY,
                                  Y + widFixY - heiFixY,
                                  Y - widFixY - heiFixY};

                    float minX = x.Min(),
                          minY = y.Min(),
                          maxX = x.Max(),
                          maxY = y.Max();
                    float fixX = 0, fixY = 0;

                    if (minX < 0)
                    {
                        fixX = -(int)(minX * 2) / 2F;
                    }
                    else if (maxX > ParentWidth)
                    {
                        fixX = (int)((ParentWidth - maxX) * 2) / 2F;
                    }

                    if (minY < 0)
                    {
                        fixY = -(int)(minY * 2) / 2F;
                    }
                    else if (maxY > ParentHeight)
                    {
                        fixY = (int)((ParentHeight - maxY) * 2) / 2F;
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        PointF origin = new PointF(x[i] + fixX, y[i] + fixY);
                        _Points.Origin[i] = origin;
                        _Points.ForDraw[i] = new PointF(ParentLeft + origin.X * Scale, ParentTop + origin.Y * Scale);
                    }
                    _X += fixX;
                    _Y += fixY;
                    _RebuildPoints = false;
                }
                return _Points;
            }
        }

        /// <summary>
        /// 物件辨識碼
        /// </summary>
        //[Description("ID"), DisplayName("索引"), Category("基本")]
        [Browsable(false)]
        public int ID { get; private set; }

        /// <summary>
        /// 物件名稱
        /// </summary>
        [Description("名稱"), DisplayName("名稱"), Category("基本")]
        public string Name { get; set; }

        private float _X;
        /// <summary>
        /// X座標
        /// </summary>
        [Description("物件中心X座標"), DisplayName("X座標"), Category("位置")]
        public float X
        {
            get { return _X; }
            set
            {
                value = (int)(value * 2) / 2F;
                if (_X == value) return;
                _X = value;
                _RebuildPoints = true;
            }
        }

        private float _Y;
        /// <summary>
        /// Y座標
        /// </summary>
        [Description("物件中心Y座標"), DisplayName("Y座標"), Category("位置")]
        public float Y
        {
            get { return _Y; }
            set
            {
                value = (int)(value * 2) / 2F;
                if (_Y == value) return;
                _Y = value;
                _RebuildPoints = true;
            }
        }

        private float _Width;
        /// <summary>
        /// 寬度
        /// </summary>
        [Description("寬度"), DisplayName("寬度"), Category("配置")]
        public float Width
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

        private float _Height;
        /// <summary>
        /// 高度
        /// </summary>
        [Description("高度"), DisplayName("高度"), Category("配置")]
        public float Height
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
        /// <summary>
        /// 角度
        /// </summary>
        [Description("角度(+-180)"), DisplayName("角度"), Category("配置")]
        public int Angle
        {
            get { return _Angle; }
            set
            {
                if (value > 180)
                {
                    value = (value % 180) - 180;
                }
                else if (value < -180)
                {
                    value = (value % 180) + 180;
                }

                if (_Angle == value) return;
                _Angle = value;
                _RebuildPoints = true;
            }
        }

        private int _ParentLeft;
        /// <summary>
        /// 容器左上角X座標
        /// </summary>
        [Browsable(false)]
        public int ParentLeft
        {
            get { return _ParentLeft; }
            set
            {
                _ParentLeft = value;
                _RebuildPoints = true;
            }
        }

        /// <summary>
        /// 容器左上角Y座標
        /// </summary>
        private int _ParentTop;
        [Browsable(false)]
        public int ParentTop
        {
            get { return _ParentTop; }
            set
            {
                _ParentTop = value;
                _RebuildPoints = true;
            }
        }

        private int _ParentWidth;
        /// <summary>
        /// 容器寬度(未縮放)
        /// </summary>
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
        /// <summary>
        /// 容器高度(未縮放)
        /// </summary>
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
        /// <summary>
        /// 縮放值
        /// </summary>
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

        public LayoutObject(int id, string name, float x, float y, float width, float height)
        {
            ID = id;
            Name = name;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            IDMax = Math.Max(IDMax, ID);
        }

        public LayoutObject(string name, float x, float y, float width, float height)
        {
            IDMax++;
            ID = IDMax;
            Name = name;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// 繪製本身
        /// </summary>
        /// <param name="g">Graphics物件</param>
        /// <param name="backColor">背景色</param>
        /// <param name="borderColoe">框線顏色</param>
        /// <param name="textColor">文字顏色</param>
        public void DrawSelf(Graphics g, Color backColor, Color borderColoe, Color textColor)
        {
            using (Pen penBorder = new Pen(borderColoe))
            using (SolidBrush brushBack = new SolidBrush(backColor))
            using (SolidBrush brushText = new SolidBrush(textColor))
            {
                PointF[] pots = Points.ForDraw;
                g.FillPolygon(brushBack, pots);
                g.DrawPolygon(penBorder, pots);

                bool sizeSwap = Height > Width;
                float width = (sizeSwap ? Height : Width) * Scale;  //長邊
                float height = (sizeSwap ? Width : Height) * Scale; //短邊

                float fontSize = Math.Min(width / (Name.Length + 1), height) * 0.7F;
                bool outside = false; //字是否放在框外
                if (fontSize < 10)
                {
                    fontSize = 10;
                    outside = true;
                }

                Font font = new Font("微軟正黑體", fontSize);
                SizeF charSize = g.MeasureString("國", font);
                double charDistance = outside ? fontSize * 1.5F : (width * 0.75F / Name.Length);

                PointF drawCenter = GetDrawCenter();
                int angle = sizeSwap ? Angle + 90 : Angle;
                if (angle > 180)
                {
                    angle = (angle % 180) - 180;
                }

                double rotate2 = (angle + 90) * Math.PI / 180F;
                if (angle > 145) angle += 180;
                else if ((angle < -45)) angle += 180;

                double rotate = angle * Math.PI / 180F;
                float charFixX = (float)(charDistance * Math.Cos(rotate)); //每個字元偏移X
                float charFixY = (float)(charDistance * Math.Sin(rotate)); //每個字元偏移Y
                float fixX = 0, fixY = 0;
                if (outside)
                {
                    fixX = (float)((height * 0.7F + 15) * Math.Cos(rotate2));
                    fixY = (float)((height * 0.7F + 15) * Math.Sin(rotate2));
                    float fixX2 = (float)((height * 0.7F + 5) * Math.Cos(rotate2));
                    float fixY2 = (float)((height * 0.7F + 5) * Math.Sin(rotate2));
                    g.DrawLine(penBorder, drawCenter, new PointF(drawCenter.X + fixX2, drawCenter.Y + fixY2));
                }

                float drawX = drawCenter.X - (charFixX * (Name.Length - 1) / 2) + fixX;
                float drawY = drawCenter.Y - (charFixY * (Name.Length - 1) / 2) + fixY;
                for (int i = 0; i < Name.Length; i++)
                {
                    g.DrawString(Name[i].ToString(), font, brushText, new PointF(drawX, drawY), _DrawStringFormat);
                    drawX += charFixX;
                    drawY += charFixY;
                }
            }
        }

        /// <summary>
        /// 取得點是否在繪製圖形內
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool IsInsideDraw(PointF point)
        {
            return Function.IsPointInside(Points.ForDraw, point);
        }

        /// <summary>
        /// 取得點是否在原始圖形內
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool IsInsideOrigin(PointF point)
        {
            return Function.IsPointInside(Points.Origin, point);
        }

        /// <summary>
        /// 取得是否與圖形相交
        /// </summary>
        /// <param name="layoutObject">圖形</param>
        /// <returns>是否相交</returns>
        public bool IsIntersectOrigin(LayoutObject layoutObject)
        {
            return Function.IsIntersect(Points.Origin, layoutObject.Points.Origin);
        }

        /// <summary>
        /// 移動指定數值,直到超出邊界
        /// </summary>
        /// <param name="moveX">X軸移動值</param>
        /// <param name="moveY">Y軸移動值</param>
        /// <returns>是否移動成功</returns>
        public bool Move(float moveX, float moveY)
        {
            PointF[] pots = Points.Origin;

            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            foreach (PointF pot in pots)
            {
                minX = Math.Min(minX, pot.X);
                minY = Math.Min(minY, pot.Y);
                maxX = Math.Max(maxX, pot.X);
                maxY = Math.Max(maxY, pot.Y);
            }

            if ((moveX < 0 && minX + moveX < 0) ||
                (moveX > 0 && maxX + moveX > ParentWidth) ||
                (moveY < 0 && minY + moveY < 0) ||
                (moveY > 0 && maxY + moveY > ParentHeight))
            {
                return false;
            }
            else
            {
                X += moveX;
                Y += moveY;
                return true;
            }
        }

        /// <summary>
        /// 取得中心點
        /// </summary>
        /// <returns></returns>
        public PointF GetDrawCenter()
        {
            PointF[] pots = Points.ForDraw;
            return new PointF((pots[0].X + pots[2].X) / 2, (pots[0].Y + pots[2].Y) / 2);
        }

        /// <summary>
        /// 記錄配置物件座標
        /// </summary>
        public class LayoutPoints
        {
            /// <summary>
            /// 原始座標組
            /// </summary>
            public PointF[] Origin { get; private set; }

            /// <summary>
            /// 縮放後座標組
            /// </summary>
            public PointF[] ForDraw { get; private set; }

            public LayoutPoints()
            {
                Origin = new PointF[4];
                ForDraw = new PointF[4];
            }
        }
    }
}
