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
        private static Cursor _NullCursor = new Cursor(new Bitmap(1, 1).GetHicon());

        private int _DrawPaddingX = 30;
        private int _DrawPaddingY = 30;

        private List<LayoutObject> _Objects = new List<LayoutObject>();
        private List<LayoutObject> _SelectedObjects = new List<LayoutObject>();
        private LayoutObject _HoverObject = null;

        private bool _ScaleSetting = false;
        private float _MainScale;
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

        private void btnNew_Click(object sender, EventArgs e)
        {
            LayoutObject newObj = new LayoutObject("新物件", _MainSize.Width / 2, _MainSize.Height / 2, 50, 50)
            {
                ParentLeft = _DrawPaddingX,
                ParentTop = _DrawPaddingY,
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

        private void RefreshListView()
        {
            lvList.Items.Clear();
            foreach (LayoutObject layoutObject in _Objects)
            {
                lvList.Items.Add(new ListViewItem(string.Format("{0}：{1}", layoutObject.ID.ToString().PadLeft(3, '0'), layoutObject.Name)) { Tag = layoutObject.ID });
            }
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

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            lvList.Height = Height - lvList.Top - 40;
        }

        private Pen _PenGrid = new Pen(Color.FromArgb(60, 255, 0, 0));
        private Pen _PenGrid2 = new Pen(Color.FromArgb(30, 255, 100, 100));
        private Pen _PenSelected = new Pen(Color.IndianRed, 2);
        private void picMain_Paint(object sender, PaintEventArgs e)
        {
            RectangleF mainRect = new RectangleF(_DrawPaddingX, _DrawPaddingY, MainSize.Width * MainScale, MainSize.Height * MainScale);
            bool half = false;
            for (int i = 0; i < MainSize.Width; i += 50)
            {
                half = !half;
                float left = i * MainScale;
                e.Graphics.DrawLine(half ? _PenGrid : _PenGrid2, mainRect.Left + left, mainRect.Top, mainRect.Left + left, mainRect.Top + mainRect.Height);
            }

            half = false;
            for (int j = 0; j < MainSize.Height; j += 50)
            {
                half = !half;
                float top = j * MainScale;
                e.Graphics.DrawLine(half ? _PenGrid : _PenGrid2, mainRect.Left, mainRect.Top + top, mainRect.Left + mainRect.Width, mainRect.Top + top);
            }

            e.Graphics.DrawString(string.Format("{0:P0}", MainScale), Font, Brushes.Red, _DrawPaddingX, 5);
            e.Graphics.DrawRectangle(Pens.ForestGreen, _DrawPaddingX, _DrawPaddingY, MainSize.Width * MainScale, MainSize.Height * MainScale);

            foreach (LayoutObject layoutObject in _Objects)
            {
                e.Graphics.SmoothingMode = layoutObject.Angle % 90 == 0 ? SmoothingMode.None : SmoothingMode.HighQuality;
                layoutObject.DrawSelf(e.Graphics);
            }

            foreach (LayoutObject layoutObject in _SelectedObjects)
            {
                _PenSelected.DashStyle = hardSet ? DashStyle.Dash : DashStyle.Solid;
                e.Graphics.SmoothingMode = layoutObject.Angle % 90 == 0 ? SmoothingMode.None : SmoothingMode.HighQuality;
                e.Graphics.DrawPolygon(_PenSelected, layoutObject.Points.ForDraw);
            }
        }

        private void pgPropertysRefresh()
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

        private void pgPropertys_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            picMain.Invalidate();
            RefreshListView();
            SaveData(true);
        }

        private void lvList_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Item.Selected)
            {
                e.Graphics.FillRectangle(Brushes.LightSkyBlue, e.Bounds);
            }
            e.DrawText();
        }

        private void picMain_MouseMove(object sender, MouseEventArgs e)
        {
            switch (_DragMode)
            {
                case 0:
                    {
                        for (int i = _Objects.Count - 1; i >= 0; i--)
                        {
                            if (_Objects[i].IsInsideDraw(e.Location))
                            {
                                _HoverObject = _Objects[i];
                                picMain.Cursor = Cursors.Hand;
                                return;
                            }
                        }
                        _HoverObject = null;
                        picMain.Cursor = Cursors.Default;
                    }
                    break;
                case 1:
                    {
                        float moveX = e.X - _DragBaseX;
                        float moveY = e.Y - _DragBaseY;
                        if (MainScale > 1)
                        {
                            moveX = (int)(moveX / MainScale);
                            moveY = (int)(moveY / MainScale);
                        }

                        if (moveX == 0 && moveY == 0) return;

                        bool move = false;
                        do
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
                                if (!hardSet)
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
                                moveX = (int)moveX / 2F;
                                moveY = (int)moveY / 2F;
                            }
                            else
                            {
                                move = true;
                            }
                        }
                        while (!move && (moveX != 0 || moveY != 0));

                        _DragBaseX = e.X;
                        _DragBaseY = e.Y;
                        if (move)
                        {
                            _DragChanged = true;
                            pgPropertys.Refresh();
                            picMain.Refresh();
                        }
                    }
                    break;
                case 2:
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
                            if (!hardSet)
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
                            _DragBaseX = e.X;
                            pgPropertys.Refresh();
                            picMain.Refresh();
                        }
                    }
                    break;
            }
        }

        private LayoutObject _DragObject;
        private bool _DragChanged = false;
        private int _DragMode = 0;
        private int _DragBaseX = 0;
        private int _DragBaseY = 0;
        private void picMain_MouseDown(object sender, MouseEventArgs e)
        {
            tbCatch.Focus();
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    if (_HoverObject != null)
                    {
                        _DragMode = 1;
                        _DragBaseX = e.X;
                        _DragBaseY = e.Y;
                        picMain.Cursor = _NullCursor;
                        _DragChanged = false;
                        if (_SelectedObjects.Contains(_HoverObject)) return;
                    }

                    _SelectedObjects.Clear();
                    if (_HoverObject != null)
                    {
                        SetSelectObjects(_HoverObject.ID);
                        _DragObject = _HoverObject;
                    }
                    else
                    {
                        ClearSelectObjects();
                        _DragObject = null;
                    }
                    pgPropertysRefresh();
                    picMain.Invalidate();
                    break;
                case System.Windows.Forms.MouseButtons.Right:
                    _DragMode = 2;
                    _DragBaseX = e.X;
                    _DragBaseY = e.Y;
                    picMain.Cursor = _NullCursor;
                    _DragChanged = false;
                    break;
            }
        }

        private void picMain_MouseUp(object sender, MouseEventArgs e)
        {
            _DragMode = 0;
            if (_DragObject != null && _DragChanged)
            {
                PointF center = _DragObject.GetDrawCenter();
                Cursor.Position = PointToScreen(new Point((int)center.X, (int)center.Y));
                SaveData(true);
            }
            picMain_MouseMove(sender, e);
        }

        private float[] _AutoScaleRatios = { 0.1F, 0.25F, 0.5F, 1, 2, 4, 8 };
        private void picMain_SizeChanged(object sender, EventArgs e)
        {
            if (ckAutoScale.Checked)
            {
                float scale = Math.Min((picMain.Width - _DrawPaddingX * 2) / (float)_MainSize.Width, (picMain.Height - _DrawPaddingY * 2) / (float)_MainSize.Height);
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

        private void ckAutoScale_CheckedChanged(object sender, EventArgs e)
        {
            picMain_SizeChanged(picMain, null);
            tbCatch.Focus();
        }

        private Regex _RegexNumber = new Regex("[0-9,]+");
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

        bool hardSet = false;
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.ShiftKey:
                    hardSet = true;
                    picMain.Invalidate();
                    break;
            }
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.ShiftKey:
                    hardSet = false;
                    picMain.Invalidate();
                    break;
            }
        }

        private static string _FilePath = "config.txt";

        private int _BackIndex = -1;
        private List<List<string>> _BackData = new List<List<string>>();

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

        private void NextStep()
        {
            if (_BackData.Count == 0 || _BackIndex < 0 || _BackIndex == _BackData.Count - 1) return;
            _BackIndex++;
            LoadData(_BackData[_BackIndex]);
        }

        private void SaveData(bool backUp)
        {
            List<string> write = new List<string>();
            write.Add(string.Format("{0}x{1}", _MainSize.Width, _MainSize.Height));
            foreach (LayoutObject layoutObject in _Objects)
            {
                write.Add(string.Format("{0},{1},{2},{3},{4},{5}", layoutObject.Name, layoutObject.X, layoutObject.Y, layoutObject.Width, layoutObject.Height, layoutObject.Angle));
            }
            File.WriteAllLines(_FilePath, write);
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

        private void LoadData()
        {
            if (File.Exists(_FilePath))
            {
                string[] lines = File.ReadAllLines(_FilePath);
                if (lines.Length == 0) return;

                LoadData(lines);

                _BackData.Clear();
                _BackData.Add(new List<string>(lines));
                _BackIndex = -1;
            }
        }

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
                        ParentLeft = _DrawPaddingX,
                        ParentTop = _DrawPaddingY,
                        Scale = MainScale,
                        ParentWidth = MainSize.Width,
                        ParentHeight = MainSize.Height
                    });
                }
            }

            ClearSelectObjects();
            _HoverObject = null;
            _DragObject = null;
            _DragMode = 0;
            RefreshListView();
            picMain.Refresh();
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

        private bool _ObjectSelecting = false;
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
            pgPropertysRefresh();
        }

        public void SetSelectObjects(int index)
        {
            if (_ObjectSelecting) return;

            _SelectedObjects.Clear();
            _ObjectSelecting = true;
            foreach (LayoutObject obj in _Objects)
            {
                if (obj.ID == index)
                {
                    _SelectedObjects.Add(obj);
                }
            }

            foreach (ListViewItem item in lvList.Items)
            {
                item.Selected = (int)item.Tag == index;

            }
            _ObjectSelecting = false;
            pgPropertysRefresh();
        }

        public void AddSelectObjects(int index)
        {
            if (_ObjectSelecting) return;

            _ObjectSelecting = true;
            foreach (LayoutObject obj in _Objects)
            {
                if (obj.ID == index)
                {
                    _SelectedObjects.Add(obj);
                }
            }

            foreach (ListViewItem item in lvList.Items)
            {
                if ((int)item.Tag == index)
                {
                    item.Selected = true;
                }
            }
            _ObjectSelecting = false;
            pgPropertysRefresh();
        }

        public void RemoveSelectObjects(int index)
        {
            if (_ObjectSelecting) return;

            _ObjectSelecting = true;
            _SelectedObjects.RemoveAll((x) => { return x.ID == index; });
            foreach (ListViewItem item in lvList.Items)
            {
                if ((int)item.Tag == index)
                {
                    item.Selected = false;
                }
            }
            _ObjectSelecting = false;
            pgPropertysRefresh();
        }

        public void SetSelectObjects(ICollection<int> index)
        {
            if (_ObjectSelecting) return;

            _SelectedObjects.Clear();
            _ObjectSelecting = true;
            foreach (LayoutObject obj in _Objects)
            {
                if (index.Contains(obj.ID))
                {
                    _SelectedObjects.Add(obj);
                }
            }

            foreach (ListViewItem item in lvList.Items)
            {
                item.Selected = index.Contains((int)item.Tag);

            }
            _ObjectSelecting = false;
            pgPropertysRefresh();
        }

        public void AddSelectObjects(ICollection<int> index)
        {
            if (_ObjectSelecting) return;

            _ObjectSelecting = true;
            foreach (LayoutObject obj in _Objects)
            {
                if (index.Contains(obj.ID))
                {
                    _SelectedObjects.Add(obj);
                }
            }

            foreach (ListViewItem item in lvList.Items)
            {
                if (index.Contains((int)item.Tag))
                {
                    item.Selected = true;
                }
            }
            _ObjectSelecting = false;
            pgPropertysRefresh();
        }

        public void RemoveSelectObjects(ICollection<int> index)
        {
            if (_ObjectSelecting) return;

            _ObjectSelecting = true;
            _SelectedObjects.RemoveAll((x) => { return index.Contains(x.ID); });
            foreach (ListViewItem item in lvList.Items)
            {
                if (index.Contains((int)item.Tag))
                {
                    item.Selected = false;
                }
            }
            _ObjectSelecting = false;
            pgPropertysRefresh();
        }

        private void tbCatch_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    btnDelete.PerformClick();
                    break;
                case Keys.Z:
                    BackStep();
                    break;
                case Keys.Y:
                    NextStep();
                    break;
            }
        }
    }

    /// <summary>
    /// 封裝Size物件
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
        [Description("寬度"), DisplayName("寬度")]
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
        [Description("高度"), DisplayName("高度")]
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
