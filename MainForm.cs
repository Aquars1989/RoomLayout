using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
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

        private float _MainScale;
        private float MainScale
        {
            get { return _MainScale; }
            set
            {
                value = Math.Max(value, 0.05F);
                if (_MainScale == value) return;
                _MainScale = value;
                foreach (LayoutObject layoutObject in _Objects)
                {
                    layoutObject.Scale = value;
                }
            }
        }

        private SizeSet _MainSize;
        [Description("版面尺寸"), DisplayName("版面尺寸")]
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
                 MainScale = Math.Min((picMain.Width - 60) / (float)_MainSize.Width, (picMain.Height - 60) / (float)_MainSize.Height);
                 picMain.Invalidate();
             };
            LoadData();
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            LayoutObject newObj = new LayoutObject("新物件", _MainSize.Width / 2, _MainSize.Height / 2, 50, 50)
            {
                OffsetX = _DrawPaddingX,
                OffsetY = _DrawPaddingY,
                ParentWidth = _MainSize.Width,
                ParentHeight = _MainSize.Height,
                Scale = MainScale
            };
            _Objects.Add(newObj);
            lvList.Items.Add(new ListViewItem(string.Format("{0}：{1}", newObj.ID.ToString().PadLeft(3, '0'), newObj.Name)) { Tag = newObj.ID });
            picMain.Invalidate();
            SaveData();
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
            SaveData();
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
                    _SelectedObjects.Clear();
                    pgPropertys.SelectedObject = null;
                    break;
                case 1:
                    _SelectedObjects.Clear();
                    LayoutObject layoutObject = _Objects.Find((x) => { return x.ID == (int)lvList.SelectedItems[0].Tag; });
                    _SelectedObjects.Add(layoutObject);
                    pgPropertysRefresh();
                    break;
                default:
                    _SelectedObjects.Clear();
                    HashSet<int> searchID = new HashSet<int>();
                    for (int i = 0; i < lvList.SelectedItems.Count; i++)
                    {
                        searchID.Add((int)lvList.SelectedItems[i].Tag);
                    }
                    _SelectedObjects = _Objects.FindAll((x) => { return searchID.Contains(x.ID); });
                    pgPropertysRefresh();
                    break;
            }
            picMain.Invalidate();
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            lvList.Height = Height - lvList.Top - 40;
        }

        private Pen _PenSelected = new Pen(Color.IndianRed, 2);
        private void picMain_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            e.Graphics.DrawString(string.Format("{0:P0}", MainScale), Font, Brushes.Red, _DrawPaddingX, 5);
            e.Graphics.DrawRectangle(Pens.ForestGreen, _DrawPaddingX, _DrawPaddingY, MainSize.Width * MainScale, MainSize.Height * MainScale);

            foreach (LayoutObject layoutObject in _Objects)
            {
                layoutObject.DrawSelf(e.Graphics);
            }

            foreach (LayoutObject layoutObject in _SelectedObjects)
            {
                _PenSelected.DashStyle = hardSet ? DashStyle.Dash : DashStyle.Solid;
                e.Graphics.DrawPolygon(_PenSelected, layoutObject.DrawPoints);
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
            SaveData();
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
                            if (_Objects[i].InRectangle(e.Location))
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
                        int moveX = (int)((e.X - _DragBaseX) / _MainScale);
                        int moveY = (int)((e.Y - _DragBaseY) / _MainScale);
                        if (moveX == 0 && moveY == 0) return;

                        List<LayoutObject> changed = new List<LayoutObject>();
                        foreach (LayoutObject layoutObject in _SelectedObjects)
                        {
                            int oldX = layoutObject.X;
                            int oldY = layoutObject.Y;

                            layoutObject.X += moveX;
                            layoutObject.Y += moveY;

                            if (!hardSet)
                            {
                                changed.Add(layoutObject);
                                bool resume = false;
                                foreach (LayoutObject chkObject in _Objects)
                                {
                                    if (!_SelectedObjects.Contains(chkObject) && chkObject.IsIntersect(layoutObject))
                                    {
                                        double oldDistance = Math.Pow(chkObject.X - oldX, 2) + Math.Pow(chkObject.Y - oldY, 2);
                                        double newDistance = Math.Pow(chkObject.X - layoutObject.X, 2) + Math.Pow(chkObject.Y - layoutObject.Y, 2);
                                        if (oldDistance > newDistance)
                                        {
                                            resume = true;
                                            break;
                                        }
                                    }
                                }

                                if (resume)
                                {
                                    foreach (LayoutObject resumeObject in changed)
                                    {
                                        resumeObject.X -= moveX;
                                        resumeObject.Y -= moveY;
                                    }
                                    break;
                                }
                            }
                        }
                        _DragChanged = true;
                        _DragBaseX = e.X;
                        _DragBaseY = e.Y;
                        pgPropertys.Refresh();
                        picMain.Refresh();
                    }
                    break;
                case 2:
                    {
                        int moveX = e.X - _DragBaseX;
                        if (moveX == 0) return;

                        List<LayoutObject> changed = new List<LayoutObject>();
                        foreach (LayoutObject layoutObject in _SelectedObjects)
                        {
                            layoutObject.Angle += moveX;

                            if (!hardSet)
                            {
                                changed.Add(layoutObject);
                                bool resume = false;
                                foreach (LayoutObject chkObject in _Objects)
                                {
                                    if (!_SelectedObjects.Contains(chkObject) && chkObject.IsIntersect(layoutObject))
                                    {
                                        resume = true;
                                        break;
                                    }
                                }

                                if (resume)
                                {
                                    foreach (LayoutObject resumeObject in changed)
                                    {
                                        layoutObject.Angle -= moveX;
                                    }
                                    break;
                                }
                            }
                        }
                        _DragChanged = true;
                        _DragBaseX = e.X;
                        pgPropertys.Refresh();
                        picMain.Refresh();
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
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    if (_HoverObject != null)
                    {
                        _DragMode = 1;
                        _DragBaseX = e.X;
                        _DragBaseY = e.Y;
                        picMain.Cursor = _NullCursor;
                        if (_SelectedObjects.Contains(_HoverObject)) return;
                    }

                    _SelectedObjects.Clear();
                    if (_HoverObject != null)
                    {
                        foreach (ListViewItem item in lvList.Items)
                        {
                            item.Selected = (int)item.Tag == _HoverObject.ID;
                        }
                        _SelectedObjects.Add(_HoverObject);
                        _DragChanged = false;
                        _DragObject = _HoverObject;
                    }
                    else
                    {
                        foreach (ListViewItem item in lvList.Items)
                        {
                            item.Selected = false;
                        }
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
                PointF center = _DragObject.GetCenter();
                Cursor.Position = PointToScreen(new Point((int)center.X, (int)center.Y));
            }
            picMain_MouseMove(sender, e);
            SaveData();
        }

        private void picMain_SizeChanged(object sender, EventArgs e)
        {
            MainScale = Math.Min((picMain.Width - _DrawPaddingX * 2) / (float)_MainSize.Width, (picMain.Height - _DrawPaddingY * 2) / (float)_MainSize.Height);
            picMain.Refresh();
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
        private void SaveData()
        {
            List<string> write = new List<string>();
            write.Add(string.Format("{0}x{1}", _MainSize.Width, _MainSize.Height));
            foreach (LayoutObject layoutObject in _Objects)
            {
                write.Add(string.Format("{0},{1},{2},{3},{4},{5}", layoutObject.Name, layoutObject.X, layoutObject.Y, layoutObject.Width, layoutObject.Height, layoutObject.Angle));
            }
            File.WriteAllLines(_FilePath, write);
        }

        private void LoadData()
        {
            if (File.Exists(_FilePath))
            {
                string[] lines = File.ReadAllLines(_FilePath);
                if (lines.Length == 0) return;

                string[] size = lines[0].Split('x');
                int width, height;
                if (size.Length >= 2 && int.TryParse(size[0], out width) && int.TryParse(size[1], out height))
                {
                    MainSize.SetSize(width, height);
                }

                _Objects.Clear();
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] values = lines[i].Split(',');
                    if (values.Length != 6) continue;

                    int x, y, objWidth, objHeight, angle;
                    if (int.TryParse(values[1], out x) && int.TryParse(values[2], out y) && int.TryParse(values[3], out objWidth) &&
                       int.TryParse(values[4], out objHeight) && int.TryParse(values[5], out angle))
                    {
                        _Objects.Add(new LayoutObject(values[0], x, y, objWidth, objHeight)
                        {
                            Angle = angle,
                            OffsetX = _DrawPaddingX,
                            OffsetY = _DrawPaddingY,
                            Scale = MainScale,
                            ParentWidth = MainSize.Width,
                            ParentHeight = MainSize.Height
                        });
                    }
                }
                RefreshListView();
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
