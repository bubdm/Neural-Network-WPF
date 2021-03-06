using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Qualia.Controls
{
    public partial class PresenterControl : UserControl
    {
        public Graphics G;

        Bitmap DrawArea;
        bool IsRenderNeeded = true;

        public PresenterControl()
        {
            InitializeComponent();

            SizeChanged += DrawBox_SizeChanged;
            BackColor = Color.White;

            Disposed += OnDisposed;
        }

        private void OnDisposed(object sender, EventArgs e)
        {
            if (DrawArea != null)
            {
                DrawArea.Dispose();
                DrawArea = null;
            }

            if (G != null)
            {
                G.Dispose();
                G = null;
            }
        }

        private void DrawBox_SizeChanged(object sender, EventArgs e)
        {
            IsRenderNeeded = true;
        }

        public void StartRender()
        {
            if (IsRenderNeeded && Width > 0 && Height > 0)
            {
                IsRenderNeeded = false;

                if (G != null)
                {
                    DrawArea.Dispose();
                    G.Dispose();
                }
                DrawArea = new Bitmap(Width, Height);
                CtlBox.Image = DrawArea;
                G = Graphics.FromImage(DrawArea);
                //G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            }
        }

        public void Clear()
        {
            G.Clear(BackColor);
        }
    }
}
