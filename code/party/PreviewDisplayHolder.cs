using System;
using System.Drawing;
using System.Windows.Forms;

namespace party
{
    class PreviewDisplayHolder : Control
    {
        public PreviewDisplayHolder()
        {
            BackColor = Color.Black;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            if (!DesignMode)
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
            }
        }
    }
}
