using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RoomLayout
{
    public partial class MainForm : Form
    {
        #region===== 存取相關 =====
        /// <summary>
        /// 還原步驟索引
        /// </summary>
        private int _BackIndex = -1;

        /// <summary>
        /// 還原步驟列表
        /// </summary>
        private List<List<string>> _BackData = new List<List<string>>();
        #endregion

        #region ===== 繪製相關 =====
        /// <summary>
        /// 字型-路徑
        /// </summary>
        private static Font _FontPath = new Font("Consola", 10);

        /// <summary>
        /// 畫筆-外框
        /// </summary>
        private static Pen _PenBorder = new Pen(Color.RoyalBlue);

        /// <summary>
        /// 畫筆-格線(100)
        /// </summary>
        private static Pen _PenGrid = new Pen(Color.FromArgb(150, 0, 200, 100));

        /// <summary>
        /// 畫筆-格線(50)
        /// </summary>
        private static Pen _PenGrid2 = new Pen(Color.FromArgb(60, 0, 200, 100));

        /// <summary>
        /// 筆刷-格線數值
        /// </summary>
        private static SolidBrush _BrushGridText = new SolidBrush(Color.FromArgb(170, 0, 200, 100));

        /// <summary>
        /// 畫筆-選取框線
        /// </summary>
        private static Pen _PenSelected = new Pen(Color.Red);

        /// <summary>
        /// 字型-格線數值
        /// </summary>
        private static Font _FontGridText = new Font("微軟正黑體", 9);

        /// <summary>
        /// 配置-格線數值
        /// </summary>
        private static StringFormat _FormatGridText = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        /// <summary>
        /// 筆刷-提示訊息
        /// </summary>
        private static SolidBrush _BrushTip = new SolidBrush(Color.Teal);

        /// <summary>
        /// 筆刷-提示訊息背景
        /// </summary>
        private static SolidBrush _BrushTipBack = new SolidBrush(Color.FromArgb(220, 255, 255, 200));

        /// <summary>
        /// 字型-提示訊息
        /// </summary>
        private static Font _FontTip = new Font("微軟正黑體", 10);

        /// <summary>
        /// 配置-提示訊息
        /// </summary>
        private static StringFormat _FormatTip = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
        #endregion

        #region ===== 拖曳相關 =====
        /// <summary>
        /// 拖曳主物件
        /// </summary>
        private LayoutObject _DragObject;

        /// <summary>
        /// 拖曳模式
        /// </summary>
        private DragMode _DragMode = DragMode.None;

        /// <summary>
        /// 預備拖曳模式
        /// </summary>
        private DragMode _DragModeReady = DragMode.None;

        /// <summary>
        /// 拖曳模式起始點X軸座標
        /// </summary>
        private int _DragBaseX = 0;

        /// <summary>
        /// 拖曳模式起始點Y軸座標
        /// </summary>
        private int _DragBaseY = 0;

        /// <summary>
        /// 拖曳模式目標點X軸座標
        /// </summary>
        private int _DragToX = 0;

        /// <summary>
        /// 拖曳模式目標點Y軸座標
        /// </summary>
        private int _DragToY = 0;

        /// <summary>
        /// 拖曳模式是否造成變動
        /// </summary>
        private bool _DragChanged = false;

        /// <summary>
        /// 強制設定
        /// </summary>
        private bool _DragHardSet = false;
        #endregion

        #region ===== 配置物件 =====
        /// <summary>
        /// 物件集合
        /// </summary>
        private List<LayoutObject> _Objects = new List<LayoutObject>();

        /// <summary>
        /// 是否正在選取物件(避免重覆選取)
        /// </summary>
        private bool _ObjectSelecting = false;

        /// <summary>
        /// 選取的物件集合
        /// </summary>
        private List<LayoutObject> _SelectedObjects = new List<LayoutObject>();

        /// <summary>
        /// 指標指到的物件
        /// </summary>
        private LayoutObject _HoverObject = null;
        #endregion

        #region ===== 畫面配置 =====
        /// <summary>
        /// 縮放文字規則
        /// </summary>
        private static Regex _RegexNumber = new Regex("[0-9,]+");

        /// <summary>
        /// 自動縮放尺寸列表
        /// </summary>
        private static float[] _AutoScaleRatios = { 0.1F, 0.25F, 0.5F, 1, 2, 4, 8 };

        /// <summary>
        /// 隱藏的滑鼠圖示
        /// </summary>
        private static Cursor _NullCursor = new Cursor(new Bitmap(1, 1).GetHicon());

        /// <summary>
        /// 左邊界距離
        /// </summary>
        private int _DrawPaddingLeft = 40;

        /// <summary>
        /// 上邊界距離
        /// </summary>
        private int _DrawPaddingTop = 60;

        /// <summary>
        /// 縮放是否設定中,避免重覆設定
        /// </summary>
        private bool _ScaleSetting = false;

        private float _MainScale = 1;
        /// <summary>
        /// 縮放比例
        /// </summary>
        private float MainScale
        {
            get { return _MainScale; }
            set
            {
                if (_ScaleSetting) return;
                _ScaleSetting = true;

                if (value < 0.1F) value = 0.1F;
                else if (value > 10) value = 10;

                cbScale.Text = string.Format("{0:P0}", value);
                cbScale.Select(0, 0);
                if (_MainScale != value)
                {
                    _MainScale = value;
                    foreach (LayoutObject layoutObject in _Objects)
                    {
                        layoutObject.Scale = value;
                    }
                    picMain.Refresh();
                }
                _ScaleSetting = false;
            }
        }

        private SizeSet _MainSize;
        [Description("版面尺寸"), DisplayName("版面尺寸(未縮放)")]
        public SizeSet MainSize
        {
            get { return _MainSize; }
        }
        #endregion

        #region ===== 鍵盤相關 =====
        /// <summary>
        /// 鍵盤事件是否造成位置變動
        /// </summary>
        private bool _KeyDownMove = false;
        #endregion

        public MainForm()
        {
            InitializeComponent();
            _MainSize = new SizeSet(500, 500);
            _MainSize.SizeChanged += (x, e) =>
             {
                 foreach (LayoutObject layoutObject in _Objects)
                 {
                     layoutObject.ParentWidth = MainSize.Width;
                     layoutObject.ParentHeight = MainSize.Height;
                 }
                 picMain_SizeChanged(picMain, null);
             };
            LoadData();
        }

        #region ===== 視窗動作 =====
        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            lvList.Height = Height - lvList.Top - 40;
        }
        #endregion

        #region ===== 縮放控制組動作 =====
        private void cbScale_Validated(object sender, EventArgs e)
        {
            string value = _RegexNumber.Match(cbScale.Text).ToString().Replace(",", "");
            if (string.IsNullOrWhiteSpace(value))
            {
                MainScale = MainScale;
            }
            else
            {
                float oldMainScale = MainScale;
                MainScale = int.Parse(value) / 100F;
                if (oldMainScale != MainScale)
                {
                    ckAutoScale.Checked = false;
                }
            }
        }

        private void cbScale_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    cbScale_Validated(cbScale, null);
                    cbScale.SelectAll();
                    break;
            }
        }

        private void cbScale_SelectedValueChanged(object sender, EventArgs e)
        {
            cbScale_Validated(cbScale, null);
            cbScale.SelectAll();
        }

        private void ckAutoScale_CheckedChanged(object sender, EventArgs e)
        {
            picMain_SizeChanged(picMain, null);
            tbCatch.Focus();
        }
        #endregion

        #region ===== 控制按鈕動作 =====
        private void btnNew_Click(object sender, EventArgs e)
        {
            LayoutObject newObj = new LayoutObject("新物件", _MainSize.Width / 2, _MainSize.Height / 2, 50, 50)
            {
                ParentLeft = _DrawPaddingLeft,
                ParentTop = _DrawPaddingTop,
                ParentWidth = _MainSize.Width,
                ParentHeight = _MainSize.Height,
                Scale = MainScale
            };
            _Objects.Add(newObj);
            lvList.Items.Add(new ListViewItem(string.Format("{0}：{1}", newObj.ID.ToString().PadLeft(3, '0'), newObj.Name)) { Tag = newObj.ID });
            picMain.Invalidate();
            SaveData(true);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            ListViewItem[] selectObject = new ListViewItem[lvList.SelectedItems.Count];
            lvList.SelectedItems.CopyTo(selectObject, 0);

            HashSet<int> removeID = new HashSet<int>();
            foreach (ListViewItem item in selectObject)
            {
                int id = (int)item.Tag;
                lvList.Items.Remove(item);
                removeID.Add(id);
            }
            _Objects.RemoveAll((x) => { return removeID.Contains(x.ID); });
            picMain.Invalidate();
            SaveData(true);
        }
        #endregion

        #region ===== 物件清單動作 =====
        /// <summary>
        /// ListView子項繪製(選取樣式常駐)
        /// </summary>
        private void lvList_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Item.Selected)
            {
                e.Graphics.FillRectangle(Brushes.LightSkyBlue, e.Bounds);
            }
            e.DrawText();
        }

        private void lvList_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (lvList.SelectedItems.Count)
            {
                case 0:
                    ClearSelectObjects();
                    break;
                case 1:
                    SetSelectObjects((int)lvList.SelectedItems[0].Tag);
                    break;
                default:
                    HashSet<int> searchID = new HashSet<int>();
                    for (int i = 0; i < lvList.SelectedItems.Count; i++)
                    {
                        searchID.Add((int)lvList.SelectedItems[i].Tag);
                    }
                    SetSelectObjects(searchID);
                    break;
            }
            picMain.Invalidate();
        }
        #endregion

        #region ===== 屬性視窗動作 =====
        private void pgPropertys_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            picMain.Invalidate();
            RefreshListView();
            SaveData(true);
        }
        #endregion

        #region ===== 鍵盤控制動作 =====
        private void tbCatch_KeyDown(object sender, KeyEventArgs e)
        {
            bool move = false;
            float moveX = 0, moveY = 0;
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    btnDelete.PerformClick();
                    break;
                case Keys.Z:
                    if (e.Control)
                    {
                        BackStep();
                    }
                    break;
                case Keys.Y:
                    if (e.Control)
                    {
                        NextStep();
                    }
                    break;
                case Keys.Up:
                    move = true;
                    moveY = e.Control ? -1 : -5;
                    break;
                case Keys.Down:
                    move = true;
                    moveY = e.Control ? 1 : 5;
                    break;
                case Keys.Left:
                    move = true;
                    moveX = e.Control ? -1 : -5;
                    break;
                case Keys.Right:
                    move = true;
                    moveX = e.Control ? 1 : 5;
                    break;
                case Keys.Space:
                    if (_DragMode == DragMode.Move || _DragMode == DragMode.Rotate)
                    {
                        _DragHardSet = true;
                        picMain.Invalidate();
                    }
                    break;
            }

            if (move)
            {
                do
                {
                    if (MoveSelectedObject(moveX, moveY))
                    {
                        pgPropertys.Refresh();
                        picMain.Refresh();
                        _KeyDownMove = true;
                        break;
                    }
                    else
                    {
                        moveX = (int)moveX / 2F;
                        moveY = (int)moveY / 2F;
                    }
                }
                while (moveX != 0 || moveY != 0);
            }
        }

        private void tbCatch_KeyUp(object sender, KeyEventArgs e)
        {
            if (_KeyDownMove)
            {
                SaveData(true);
                _KeyDownMove = false;
            }

            switch (e.KeyCode)
            {
                case Keys.Space:
                    if (_DragMode == DragMode.Move || _DragMode == DragMode.Rotate)
                    {
                        _DragHardSet = false;
                        picMain.Invalidate();
                    }
                    break;
            }
        }
        #endregion

        #region ===== 繪圖區動作 =====
        private void picMain_Paint(object sender, PaintEventArgs e)
        {
            //繪製格線
            int gridWidth = (int)(5 * ((int)(10 / MainScale)) / 10 * 10);
            int darkLineWidth = int.Parse("1" + new string('0', gridWidth.ToString().Length));
            RectangleF mainRect = new RectangleF(_DrawPaddingLeft, _DrawPaddingTop, MainSize.Width * MainScale, MainSize.Height * MainScale);
            for (int i = gridWidth; i < MainSize.Width; i += gridWidth)
            {
                bool dark = i % darkLineWidth == 0;
                float left = i * MainScale;
                e.Graphics.DrawLine(dark ? _PenGrid : _PenGrid2, mainRect.Left + left, mainRect.Top, mainRect.Left + left, mainRect.Top + mainRect.Height);
                e.Graphics.DrawString(i.ToString(), _FontGridText, _BrushGridText, mainRect.Left + left, mainRect.Top - 10, _FormatGridText);
            }

            for (int j = gridWidth; j < MainSize.Height; j += gridWidth)
            {
                bool dark = j % darkLineWidth == 0;
                float top = j * MainScale;
                e.Graphics.DrawLine(dark ? _PenGrid : _PenGrid2, mainRect.Left, mainRect.Top + top, mainRect.Left + mainRect.Width, mainRect.Top + top);
                e.Graphics.DrawString(j.ToString(), _FontGridText, _BrushGridText, mainRect.Left - 20, mainRect.Top + top, _FormatGridText);
            }

            //繪製外框及檔名文字
            e.Graphics.DrawString(string.Format("檔案：{0}", Global.FilePath), _FontPath, Brushes.Tomato, _DrawPaddingLeft, 5);
            e.Graphics.DrawRectangle(_PenBorder, _DrawPaddingLeft, _DrawPaddingTop, MainSize.Width * MainScale, MainSize.Height * MainScale);

            //繪製物件
            foreach (LayoutObject layoutObject in _Objects)
            {
                e.Graphics.SmoothingMode = layoutObject.Angle % 90 == 0 ? SmoothingMode.None : SmoothingMode.HighQuality;

                if (_SelectedObjects.Contains(layoutObject))
                {
                    bool drawEmpty = (_DragMode == DragMode.Move || _DragMode == DragMode.Rotate) && _DragHardSet;
                    layoutObject.DrawSelf(e.Graphics, drawEmpty ? Color.Empty : Color.FromArgb(150, 255, 220, 220), Color.Red, Color.Red);
                    switch (_DragMode)
                    {
                        case DragMode.Move:
                            {
                                PointF center = layoutObject.GetDrawCenter();
                                int drawSize = (int)(Math.Min(layoutObject.Width, layoutObject.Height) * 0.4F * MainScale);
                                if (drawSize > 6)
                                {
                                    drawSize = Math.Min(drawSize, 20);
                                    e.Graphics.DrawImage(Properties.Resources.Icon_Move, center.X - drawSize, center.Y - drawSize, drawSize * 2, drawSize * 2);
                                }
                            }
                            break;
                        case DragMode.Rotate:
                            {
                                PointF center = layoutObject.GetDrawCenter();
                                int drawSize = (int)(Math.Min(layoutObject.Width, layoutObject.Height) * 0.4F * MainScale);
                                if (drawSize > 6)
                                {
                                    drawSize = Math.Min(drawSize, 20);
                                    e.Graphics.DrawImage(Properties.Resources.Icon_Rotate, center.X - drawSize, center.Y - drawSize, drawSize * 2, drawSize * 2);
                                }
                            }
                            break;
                    }
                }
                else
                {
                    layoutObject.DrawSelf(e.Graphics, Color.FromArgb(150, 200, 220, 255), Color.RoyalBlue, Color.RoyalBlue);
                }
            }

            //繪製選取外框
            foreach (LayoutObject layoutObject in _SelectedObjects)
            {
                e.Graphics.SmoothingMode = layoutObject.Angle % 90 == 0 ? SmoothingMode.None : SmoothingMode.HighQuality;
                e.Graphics.DrawLines(_PenSelected, layoutObject.Points.ForDraw);
            }

            if (_DragMode == DragMode.Select)
            {
                Point[] pots = { new Point(_DragBaseX,_DragBaseY),
                                 new Point(_DragToX,_DragBaseY),
                                 new Point(_DragToX,_DragToY),
                                 new Point(_DragBaseX,_DragToY)};
                e.Graphics.DrawPolygon(_PenSelected, pots);
            }

            if (_HoverObject != null && _DragMode == DragMode.None)
            {
                Point screenBase = picMain.PointToScreen(new Point(0, 0));
                string tip;
                if (_SelectedObjects.Contains(_HoverObject))
                {
                    tip = "左鍵：移動選取物件\n" +
                          "右鍵：旋轉選取物件\n" +
                          "Ctrl+左鍵：取消選取";
                }
                else
                {
                    tip = "左鍵：選取\n" +
                          "Shift+左鍵：加入選取\n" +
                          "Ctrl+左鍵：加入選取";
                }
                SizeF tipSize = e.Graphics.MeasureString(tip, _FontTip);
                RectangleF tipRect = new RectangleF(Cursor.Position.X - screenBase.X + 15, Cursor.Position.Y - screenBase.Y, tipSize.Width, tipSize.Height + 5);
                e.Graphics.FillRectangle(_BrushTipBack, tipRect);
                e.Graphics.DrawString(tip, _FontTip, _BrushTip, tipRect, _FormatTip);
            }
        }

        private void picMain_MouseMove(object sender, MouseEventArgs e)
        {
            switch (_DragMode)
            {
                case DragMode.None:
                    {
                        switch (_DragModeReady)
                        {
                            case DragMode.Move:
                                if (e.X == _DragBaseX && e.Y == _DragBaseY) break;
                                _DragModeReady = DragMode.None;
                                _DragMode = DragMode.Move;
                                _DragBaseX = e.X;
                                _DragBaseY = e.Y;
                                _DragChanged = false;
                                _DragObject = _HoverObject;
                                _DragHardSet = false;
                                picMain.Cursor = _NullCursor;
                                break;
                            default:
                                for (int i = _Objects.Count - 1; i >= 0; i--)
                                {
                                    if (_Objects[i].IsInsideDraw(e.Location))
                                    {
                                        _HoverObject = _Objects[i];
                                        picMain.Cursor = Cursors.Hand;
                                        picMain.Invalidate();
                                        return;
                                    }
                                }
                                _HoverObject = null;
                                picMain.Cursor = Cursors.Default;
                                picMain.Invalidate();
                                break;
                        }
                    }
                    break;
                case DragMode.Move:
                    {
                        float moveX = e.X - _DragBaseX;
                        float moveY = e.Y - _DragBaseY;
                        if (MainScale > 1)
                        {
                            moveX = (int)(moveX / MainScale);
                            moveY = (int)(moveY / MainScale);
                        }

                        if (moveX == 0 && moveY == 0) return;

                        do
                        {
                            if (MoveSelectedObject(moveX, moveY))
                            {
                                _DragChanged = true;
                                pgPropertys.Refresh();
                                picMain.Refresh();
                                break;
                            }
                            else
                            {
                                moveX = (int)moveX / 2F;
                                moveY = (int)moveY / 2F;
                            }
                        }
                        while (moveX != 0 || moveY != 0);

                        Cursor.Position = new Point(Cursor.Position.X - (e.X - _DragBaseX), Cursor.Position.Y - (e.Y - _DragBaseY));
                        //_DragBaseX = e.X;
                        //_DragBaseY = e.Y;
                    }
                    break;
                case DragMode.Rotate:
                    {
                        int moveX = e.X - _DragBaseX;
                        if (moveX == 0) return;

                        bool resume = false;
                        List<PointF> oldXY = new List<PointF>();
                        List<int> oldAngles = new List<int>();
                        foreach (LayoutObject layoutObject in _SelectedObjects)
                        {
                            float oldX = layoutObject.X;
                            float oldY = layoutObject.Y;
                            int oldAngle = layoutObject.Angle;

                            layoutObject.Angle += moveX;

                            oldXY.Add(new PointF(oldX, oldY));
                            oldAngles.Add(oldAngle);
                            if (!_DragHardSet)
                            {
                                foreach (LayoutObject chkObject in _Objects)
                                {
                                    if (!_SelectedObjects.Contains(chkObject) && chkObject.IsIntersectOrigin(layoutObject))
                                    {
                                        resume = true;
                                        break;
                                    }
                                }

                                if (resume)
                                {
                                    break;
                                }
                            }
                        }

                        Cursor.Position = new Point(Cursor.Position.X - (e.X - _DragBaseX), Cursor.Position.Y - (e.Y - _DragBaseY));
                        //_DragBaseX = e.X;
                        if (resume)
                        {
                            for (int i = 0; i < oldXY.Count; i++)
                            {
                                _SelectedObjects[i].X = oldXY[i].X;
                                _SelectedObjects[i].Y = oldXY[i].Y;
                                _SelectedObjects[i].Angle = oldAngles[i];
                            }
                        }
                        else
                        {
                            _DragChanged = true;
                            pgPropertys.Refresh();
                            picMain.Refresh();
                        }
                    }
                    break;
                case DragMode.Select:
                    _DragToX = e.X;
                    _DragToY = e.Y;
                    picMain.Invalidate();
                    break;
            }
        }

        private void picMain_MouseDown(object sender, MouseEventArgs e)
        {
            tbCatch.Focus();
            _DragModeReady = DragMode.None;
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    if (_HoverObject != null)
                    {
                        if (!_SelectedObjects.Contains(_HoverObject))
                        {
                            if ((ModifierKeys & Keys.Shift) == Keys.Shift ||
                                (ModifierKeys & Keys.Control) == Keys.Control)
                            {
                                AddSelectObjects(_HoverObject.ID);
                            }
                            else
                            {
                                SetSelectObjects(_HoverObject.ID);
                                _DragModeReady = DragMode.Move;
                                _DragBaseX = e.X;
                                _DragBaseY = e.Y;
                            }
                        }
                        else
                        {
                            if ((ModifierKeys & Keys.Control) == Keys.Control)
                            {
                                RemoveSelectObjects(_HoverObject.ID);
                            }
                            else
                            {
                                _DragMode = DragMode.Move;
                                _DragBaseX = e.X;
                                _DragBaseY = e.Y;
                                _DragChanged = false;
                                _DragObject = _HoverObject;
                                _DragHardSet = false;
                                picMain.Cursor = _NullCursor;
                            }
                        }
                    }
                    else
                    {
                        _DragMode = DragMode.Select;
                        _DragBaseX = _DragToX = e.X;
                        _DragBaseY = _DragToY = e.Y;
                        _DragChanged = false;
                        _DragObject = null;
                        //ClearSelectObjects();
                    }
                    BindingPropertys();
                    picMain.Invalidate();
                    break;
                case System.Windows.Forms.MouseButtons.Right:
                    if (_HoverObject != null)
                    {
                        if (_SelectedObjects.Contains(_HoverObject))
                        {
                            _DragMode = DragMode.Rotate;
                            _DragBaseX = e.X;
                            _DragBaseY = e.Y;
                            picMain.Cursor = _NullCursor;
                            _DragChanged = false;
                            picMain.Invalidate();
                            _DragHardSet = false;
                        }
                    }
                    break;
            }
        }

        private void picMain_MouseUp(object sender, MouseEventArgs e)
        {
            switch (_DragMode)
            {
                case DragMode.Move:
                case DragMode.Rotate:
                    if (_DragObject != null && _DragChanged)
                    {
                        PointF center = _DragObject.GetDrawCenter();
                        Cursor.Position = PointToScreen(new Point((int)center.X, (int)center.Y));
                        SaveData(true);
                    }
                    break;
                case DragMode.Select:
                    PointF[] pots = { new Point(_DragBaseX,_DragBaseY),
                                      new Point(_DragToX,_DragBaseY),
                                      new Point(_DragToX,_DragToY),
                                      new Point(_DragBaseX,_DragToY)};

                    List<int> selectID = new List<int>();
                    foreach (LayoutObject lo in _Objects)
                    {
                        if (Function.IsIntersect(lo.Points.ForDraw, pots))
                        {
                            selectID.Add(lo.ID);
                        }
                    }

                    SetSelectObjects(selectID);
                    break;
            }

            _DragMode = DragMode.None;
            _DragModeReady = DragMode.None;
            picMain_MouseMove(sender, e);
            picMain.Invalidate();
        }

        private void picMain_SizeChanged(object sender, EventArgs e)
        {
            if (ckAutoScale.Checked)
            {
                float scale = Math.Min((picMain.Width - _DrawPaddingLeft * 2) / (float)_MainSize.Width, (picMain.Height - _DrawPaddingTop * 2) / (float)_MainSize.Height);
                for (int i = _AutoScaleRatios.Length - 1; i >= 0; i--)
                {
                    if (scale > _AutoScaleRatios[i])
                    {
                        MainScale = _AutoScaleRatios[i];
                        return;
                    }
                }
                MainScale = _AutoScaleRatios[0];
            }
        }
        #endregion

        #region ===== 存取相關方法 =====
        /// <summary>
        /// 還原到上一步驟
        /// </summary>
        private void BackStep()
        {
            if (_BackData.Count < 2 || _BackIndex == 0) return;

            if (_BackIndex < 0)
            {
                _BackIndex = _BackData.Count - 2;
            }
            else
            {
                _BackIndex--;
            }

            LoadData(_BackData[_BackIndex]);
        }

        /// <summary>
        /// 前進到下一步驟
        /// </summary>
        private void NextStep()
        {
            if (_BackData.Count == 0 || _BackIndex < 0 || _BackIndex == _BackData.Count - 1) return;
            _BackIndex++;
            LoadData(_BackData[_BackIndex]);
        }

        /// <summary>
        /// 儲存資料到檔案
        /// </summary>
        /// <param name="backUp">是否將資料加入還原步驟</param>
        private void SaveData(bool backUp)
        {
            List<string> write = new List<string>();
            write.Add(string.Format("{0}x{1}", _MainSize.Width, _MainSize.Height));
            foreach (LayoutObject layoutObject in _Objects)
            {
                write.Add(string.Format("{0},{1},{2},{3},{4},{5}", layoutObject.Name, layoutObject.X, layoutObject.Y, layoutObject.Width, layoutObject.Height, layoutObject.Angle));
            }
            File.WriteAllLines(Global.FilePath, write);
            if (backUp)
            {
                if (_BackIndex >= 0)
                {
                    _BackData.RemoveRange(_BackIndex + 1, _BackData.Count - _BackIndex - 1);
                }

                _BackData.Add(write);
                _BackIndex = -1;
                if (_BackData.Count > 20)
                {
                    _BackData.RemoveRange(0, _BackData.Count - 20);
                }
            }
        }

        /// <summary>
        /// 從檔案讀取資料
        /// </summary>
        private void LoadData()
        {
            if (File.Exists(Global.FilePath))
            {
                string[] lines = File.ReadAllLines(Global.FilePath);
                if (lines.Length == 0) return;

                LoadData(lines);

                _BackData.Clear();
                _BackData.Add(new List<string>(lines));
                _BackIndex = -1;
            }
        }

        /// <summary>
        /// 從字串列表讀取資料
        /// </summary>
        /// <param name="lines">資料字串</param>
        private void LoadData(IList<string> lines)
        {
            string[] size = lines[0].Split('x');
            int width, height;
            if (size.Length >= 2 && int.TryParse(size[0], out width) && int.TryParse(size[1], out height))
            {
                MainSize.SetSize(width, height);
            }

            LayoutObject.IDMax = 0;
            _Objects.Clear();
            for (int i = 1; i < lines.Count; i++)
            {
                string[] values = lines[i].Split(',');
                if (values.Length != 6) continue;

                float x, y, objWidth, objHeight;
                int angle;
                if (float.TryParse(values[1], out x) && float.TryParse(values[2], out y) &&
                    float.TryParse(values[3], out objWidth) && float.TryParse(values[4], out objHeight) &&
                    int.TryParse(values[5], out angle))
                {
                    _Objects.Add(new LayoutObject(values[0], x, y, objWidth, objHeight)
                    {
                        Angle = angle,
                        ParentLeft = _DrawPaddingLeft,
                        ParentTop = _DrawPaddingTop,
                        Scale = MainScale,
                        ParentWidth = MainSize.Width,
                        ParentHeight = MainSize.Height
                    });
                }
            }

            ClearSelectObjects();
            _HoverObject = null;
            _DragObject = null;
            _DragMode = DragMode.None;
            RefreshListView();
            picMain.Refresh();
        }
        #endregion

        #region ===== 選取相關方法 =====
        /// <summary>
        /// 清除選取物件
        /// </summary>
        public void ClearSelectObjects()
        {
            if (_ObjectSelecting) return;

            _ObjectSelecting = true;
            _SelectedObjects.Clear();
            foreach (ListViewItem item in lvList.Items)
            {
                item.Selected = false;
            }
            _ObjectSelecting = false;
            BindingPropertys();
        }

        /// <summary>
        /// 設定選取指定ID的物件
        /// </summary>
        /// <param name="id">物件ID</param>
        public void SetSelectObjects(int id)
        {
            if (_ObjectSelecting) return;

            _SelectedObjects.Clear();
            _ObjectSelecting = true;
            foreach (LayoutObject obj in _Objects)
            {
                if (obj.ID == id)
                {
                    _SelectedObjects.Add(obj);
                }
            }

            foreach (ListViewItem item in lvList.Items)
            {
                item.Selected = (int)item.Tag == id;

            }
            _ObjectSelecting = false;
            BindingPropertys();
        }

        /// <summary>
        /// 增加選取指定ID的物件
        /// </summary>
        /// <param name="id">物件ID</param>
        public void AddSelectObjects(int id)
        {
            if (_ObjectSelecting) return;

            _ObjectSelecting = true;
            foreach (LayoutObject obj in _Objects)
            {
                if (obj.ID == id)
                {
                    _SelectedObjects.Add(obj);
                }
            }

            foreach (ListViewItem item in lvList.Items)
            {
                if ((int)item.Tag == id)
                {
                    item.Selected = true;
                }
            }
            _ObjectSelecting = false;
            BindingPropertys();
        }

        /// <summary>
        /// 移除選取指定ID的物件
        /// </summary>
        /// <param name="id">物件ID</param>
        public void RemoveSelectObjects(int id)
        {
            if (_ObjectSelecting) return;

            _ObjectSelecting = true;
            _SelectedObjects.RemoveAll((x) => { return x.ID == id; });
            foreach (ListViewItem item in lvList.Items)
            {
                if ((int)item.Tag == id)
                {
                    item.Selected = false;
                }
            }
            _ObjectSelecting = false;
            BindingPropertys();
        }

        /// <summary>
        /// 選取在ID列表中的物件
        /// </summary>
        /// <param name="idList">ID列表</param>
        public void SetSelectObjects(ICollection<int> idList)
        {
            if (_ObjectSelecting) return;

            _SelectedObjects.Clear();
            _ObjectSelecting = true;
            foreach (LayoutObject obj in _Objects)
            {
                if (idList.Contains(obj.ID))
                {
                    _SelectedObjects.Add(obj);
                }
            }

            foreach (ListViewItem item in lvList.Items)
            {
                item.Selected = idList.Contains((int)item.Tag);

            }
            _ObjectSelecting = false;
            BindingPropertys();
        }

        /// <summary>
        /// 增加選取在ID列表中的物件
        /// </summary>
        /// <param name="isList">ID列表</param>
        public void AddSelectObjects(ICollection<int> isList)
        {
            if (_ObjectSelecting) return;

            _ObjectSelecting = true;
            foreach (LayoutObject obj in _Objects)
            {
                if (isList.Contains(obj.ID))
                {
                    _SelectedObjects.Add(obj);
                }
            }

            foreach (ListViewItem item in lvList.Items)
            {
                if (isList.Contains((int)item.Tag))
                {
                    item.Selected = true;
                }
            }
            _ObjectSelecting = false;
            BindingPropertys();
        }

        /// <summary>
        /// 移除選取在ID列表中的物件
        /// </summary>
        /// <param name="idList">ID列表</param>
        public void RemoveSelectObjects(ICollection<int> idList)
        {
            if (_ObjectSelecting) return;

            _ObjectSelecting = true;
            _SelectedObjects.RemoveAll((x) => { return idList.Contains(x.ID); });
            foreach (ListViewItem item in lvList.Items)
            {
                if (idList.Contains((int)item.Tag))
                {
                    item.Selected = false;
                }
            }
            _ObjectSelecting = false;
            BindingPropertys();
        }

        /// <summary>
        /// 移動選取的物件
        /// </summary>
        /// <param name="moveX">X座標移動值</param>
        /// <param name="moveY">Y座標移動值</param>
        /// <returns>是否移動成功</returns>
        private bool MoveSelectedObject(float moveX, float moveY)
        {
            bool resume = false;
            List<PointF> oldXY = new List<PointF>();
            foreach (LayoutObject layoutObject in _SelectedObjects)
            {
                float oldX = layoutObject.X;
                float oldY = layoutObject.Y;

                if (!layoutObject.Move(moveX, moveY))
                {
                    resume = true;
                    break;
                }

                oldXY.Add(new PointF(oldX, oldY));
                if (!_DragHardSet)
                {
                    foreach (LayoutObject chkObject in _Objects)
                    {
                        if (!_SelectedObjects.Contains(chkObject) && layoutObject.IsIntersectOrigin(chkObject))
                        {
                            resume = true;
                            break;
                        }
                    }

                    if (resume)
                    {
                        break;
                    }
                }
            }


            if (resume)
            {
                for (int i = 0; i < oldXY.Count; i++)
                {
                    _SelectedObjects[i].X = oldXY[i].X;
                    _SelectedObjects[i].Y = oldXY[i].Y;
                }
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region ===== 其他方法 =====
        /// <summary>
        /// 重設ListView內容
        /// </summary>
        private void RefreshListView()
        {
            lvList.Items.Clear();
            foreach (LayoutObject layoutObject in _Objects)
            {
                lvList.Items.Add(new ListViewItem(string.Format("{0}", layoutObject.Name)) { Tag = layoutObject.ID, Selected = _SelectedObjects.Contains(layoutObject) });
            }
        }

        /// <summary>
        /// 重新設置屬性視窗關聯物件
        /// </summary>
        private void BindingPropertys()
        {
            if (_SelectedObjects.Count > 0)
            {
                pgPropertys.SelectedObjects = _SelectedObjects.ToArray();
            }
            else
            {
                pgPropertys.SelectedObject = MainSize;
            }

        }
        #endregion

        /// <summary>
        /// 拖曳模式列舉
        /// </summary>
        public enum DragMode
        {
            /// <summary>
            /// 無
            /// </summary>
            None,

            /// <summary>
            /// 框選
            /// </summary>
            Select,

            /// <summary>
            /// 移動
            /// </summary>
            Move,

            /// <summary>
            /// 旋轉
            /// </summary>
            Rotate
        }

        /// <summary>
        /// 封裝Size物件(供屬性配置視窗用)
        /// </summary>
        public class SizeSet
        {
            public event EventHandler SizeChanged;

            public void OnSizeChanged()
            {
                if (SizeChanged != null)
                {
                    SizeChanged(this, new EventArgs());
                }
            }


            private int _Width;
            [Description("寬度"), DisplayName("寬度"), Category("區域大小")]
            public int Width
            {
                get { return _Width; }
                set
                {
                    if (_Width == value) return;
                    _Width = value;
                    OnSizeChanged();
                }
            }

            private int _Height;
            [Description("高度"), DisplayName("高度"), Category("區域大小")]
            public int Height
            {
                get { return _Height; }
                set
                {
                    if (_Height == value) return;
                    _Height = value;
                    OnSizeChanged();
                }
            }

            public SizeSet(int width, int height)
            {
                _Width = width;
                _Height = height;
                OnSizeChanged();
            }

            public void SetSize(int width, int height)
            {
                _Width = width;
                _Height = height;
                OnSizeChanged();
            }
        }
    }
}
