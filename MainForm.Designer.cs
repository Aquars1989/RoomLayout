namespace RoomLayout
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.picMain = new System.Windows.Forms.PictureBox();
            this.pgPropertys = new System.Windows.Forms.PropertyGrid();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ckAutoScale = new System.Windows.Forms.CheckBox();
            this.cbScale = new System.Windows.Forms.ComboBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnNew = new System.Windows.Forms.Button();
            this.lvList = new System.Windows.Forms.ListView();
            this.tbCatch = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.picMain)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // picMain
            // 
            this.picMain.BackColor = System.Drawing.Color.White;
            this.picMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picMain.Location = new System.Drawing.Point(0, 0);
            this.picMain.Name = "picMain";
            this.picMain.Size = new System.Drawing.Size(658, 603);
            this.picMain.TabIndex = 0;
            this.picMain.TabStop = false;
            this.picMain.SizeChanged += new System.EventHandler(this.picMain_SizeChanged);
            this.picMain.Paint += new System.Windows.Forms.PaintEventHandler(this.picMain_Paint);
            this.picMain.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picMain_MouseDown);
            this.picMain.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picMain_MouseMove);
            this.picMain.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picMain_MouseUp);
            // 
            // pgPropertys
            // 
            this.pgPropertys.Dock = System.Windows.Forms.DockStyle.Right;
            this.pgPropertys.Font = new System.Drawing.Font("微軟正黑體", 12F);
            this.pgPropertys.Location = new System.Drawing.Point(858, 0);
            this.pgPropertys.Name = "pgPropertys";
            this.pgPropertys.Size = new System.Drawing.Size(180, 603);
            this.pgPropertys.TabIndex = 1;
            this.pgPropertys.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.pgPropertys_PropertyValueChanged);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.panel1.Controls.Add(this.ckAutoScale);
            this.panel1.Controls.Add(this.cbScale);
            this.panel1.Controls.Add(this.btnDelete);
            this.panel1.Controls.Add(this.btnNew);
            this.panel1.Controls.Add(this.lvList);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(658, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 603);
            this.panel1.TabIndex = 3;
            // 
            // ckAutoScale
            // 
            this.ckAutoScale.AutoSize = true;
            this.ckAutoScale.Checked = true;
            this.ckAutoScale.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckAutoScale.Font = new System.Drawing.Font("微軟正黑體", 12F);
            this.ckAutoScale.Location = new System.Drawing.Point(125, 14);
            this.ckAutoScale.Name = "ckAutoScale";
            this.ckAutoScale.Size = new System.Drawing.Size(60, 24);
            this.ckAutoScale.TabIndex = 6;
            this.ckAutoScale.Text = "自動";
            this.ckAutoScale.UseVisualStyleBackColor = true;
            this.ckAutoScale.CheckedChanged += new System.EventHandler(this.ckAutoScale_CheckedChanged);
            // 
            // cbScale
            // 
            this.cbScale.BackColor = System.Drawing.Color.White;
            this.cbScale.Font = new System.Drawing.Font("微軟正黑體", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cbScale.FormattingEnabled = true;
            this.cbScale.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.cbScale.ItemHeight = 19;
            this.cbScale.Items.AddRange(new object[] {
            "25%",
            "50%",
            "100%",
            "200%",
            "400%"});
            this.cbScale.Location = new System.Drawing.Point(7, 10);
            this.cbScale.Name = "cbScale";
            this.cbScale.Size = new System.Drawing.Size(110, 27);
            this.cbScale.TabIndex = 5;
            this.cbScale.SelectedValueChanged += new System.EventHandler(this.cbScale_SelectedValueChanged);
            this.cbScale.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cbScale_KeyDown);
            this.cbScale.Validated += new System.EventHandler(this.cbScale_Validated);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.btnDelete.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnDelete.Location = new System.Drawing.Point(100, 45);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(95, 30);
            this.btnDelete.TabIndex = 4;
            this.btnDelete.Text = "刪除";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnNew
            // 
            this.btnNew.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.btnNew.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnNew.Location = new System.Drawing.Point(5, 45);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(95, 30);
            this.btnNew.TabIndex = 3;
            this.btnNew.Text = "新增";
            this.btnNew.UseVisualStyleBackColor = false;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // lvList
            // 
            this.lvList.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.lvList.Font = new System.Drawing.Font("微軟正黑體", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lvList.HideSelection = false;
            this.lvList.Location = new System.Drawing.Point(5, 80);
            this.lvList.Name = "lvList";
            this.lvList.OwnerDraw = true;
            this.lvList.Size = new System.Drawing.Size(190, 515);
            this.lvList.TabIndex = 2;
            this.lvList.TileSize = new System.Drawing.Size(170, 30);
            this.lvList.UseCompatibleStateImageBehavior = false;
            this.lvList.View = System.Windows.Forms.View.Tile;
            this.lvList.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.lvList_DrawItem);
            this.lvList.SelectedIndexChanged += new System.EventHandler(this.lvList_SelectedIndexChanged);
            // 
            // tbCatch
            // 
            this.tbCatch.Location = new System.Drawing.Point(0, 0);
            this.tbCatch.Name = "tbCatch";
            this.tbCatch.Size = new System.Drawing.Size(0, 22);
            this.tbCatch.TabIndex = 4;
            this.tbCatch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbCatch_KeyDown);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1038, 603);
            this.Controls.Add(this.tbCatch);
            this.Controls.Add(this.picMain);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pgPropertys);
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.SizeChanged += new System.EventHandler(this.MainForm_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.picMain)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picMain;
        private System.Windows.Forms.PropertyGrid pgPropertys;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.ListView lvList;
        private System.Windows.Forms.ComboBox cbScale;
        private System.Windows.Forms.CheckBox ckAutoScale;
        private System.Windows.Forms.TextBox tbCatch;
    }
}