using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace party
{
    class InterpolatorEditorControl : Control
    {
        public event EventHandler EntrySelectionChanged;

        InterpolatorGroup group;
        public InterpolatorGroup Group
        {
            get { return group; }
            set
            {
                group = value;
                selected = null;
                selected_entry = null;
                UpdateScale();
                Invalidate();
            }
        }

        Interpolator selected;
        public Interpolator Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                selected_entry = null;
                UpdateScale();
                Invalidate();
            }
        }

        enum EntryEditMode
        {
            None,
            Min,
            Max,
            MinMax,
            Keyframe
        };

        InterpolatorEntry selected_entry;
        EntryEditMode entry_editor;
        public InterpolatorEntry SelectedEntry
        {
            get { return selected_entry; }
            set
            {
                selected_entry = value;
                Invalidate();
            }
        }

        public InterpolatorEditorControl()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        }

        Point[] x_axis_lines = new Point[2];
        Point[] y_axis_lines = new Point[2];
        const int axis_margin = 25;
        const int bottom_margin = 25;
        Rectangle graph_rect;
        float scale_x = 1.0f;
        float scale_y = 1.0f;
        float min_y = 0.0f;
        bool manual_scale = false;

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            graph_rect = new Rectangle(axis_margin, axis_margin, Size.Width - axis_margin * 2, Size.Height - axis_margin - bottom_margin);
            UpdateScale();
        }

        void UpdateScale(bool force = false)
        {
            float x_max = float.NegativeInfinity;
            float y_min = float.PositiveInfinity;
            float y_max = float.NegativeInfinity;
            if (selected != null)
            {
                foreach (InterpolatorEntry entry in selected.Entries)
                {
                    if (entry.Keyframe > x_max) x_max = entry.Keyframe;

                    float min = Math.Min(entry.Value.Min, entry.Value.Max);
                    float max = Math.Max(entry.Value.Min, entry.Value.Max);
                    if (min < y_min) y_min = min;
                    if (max > y_max) y_max = max;
                }
            }
            else if (group != null)
            {
                foreach (Interpolator interpolator in group.Interpolators)
                {
                    foreach (InterpolatorEntry entry in interpolator.Entries)
                    {
                        if (entry.Keyframe > x_max) x_max = entry.Keyframe;

                        float min = Math.Min(entry.Value.Min, entry.Value.Max);
                        float max = Math.Max(entry.Value.Min, entry.Value.Max);
                        if (min < y_min) y_min = min;
                        if (max > y_max) y_max = max;
                    }
                }
            }

            if (y_max < 1) y_max = 1;
            if (y_min > 0) y_min = 0;
            if (x_max < 1) x_max = 1;


            float new_scale_x = (float)graph_rect.Width / x_max;
            float new_scale_y = (float)graph_rect.Height / (y_max - y_min);

            if (!manual_scale || (manual_scale && force))
            {
                scale_x = new_scale_x;
                scale_y = new_scale_y;
            }
            min_y = y_min;

            UpdateLines();
        }

        void UpdateLines()
        {
            int zero_line = graph_rect.Bottom - (int)Math.Floor(-min_y * scale_y);
            x_axis_lines[0] = new Point(graph_rect.Left, zero_line);
            x_axis_lines[1] = new Point(Size.Width, zero_line);
            y_axis_lines[0] = new Point(graph_rect.Left, 0);
            y_axis_lines[1] = new Point(graph_rect.Left, Size.Height - bottom_margin);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.FillRectangle(Brushes.Black, e.ClipRectangle);
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(40, Color.White)), graph_rect.Left, graph_rect.Bottom, ClientRectangle.Right - graph_rect.Left, bottom_margin);

            Pen axis_pen = new Pen(Color.DarkGray, 1);
            axis_pen.DashStyle = DashStyle.Dot;

            e.Graphics.DrawLines(axis_pen, x_axis_lines);
            e.Graphics.DrawLines(axis_pen, y_axis_lines);

            Font label_font = new Font(FontFamily.GenericSansSerif, 7);
            Brush label_brush = new SolidBrush(Color.DarkGray);

            StringFormat label_format = new StringFormat(StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip | StringFormatFlags.NoWrap);
            label_format.LineAlignment = StringAlignment.Center;
            label_format.Alignment = StringAlignment.Far;
            label_format.Trimming = StringTrimming.None;

            Rectangle label_rect = new Rectangle(0, x_axis_lines[0].Y - 5, graph_rect.Left - 3, 10);
            e.Graphics.DrawString("0", label_font, label_brush, label_rect, label_format);

            label_rect.Y = graph_rect.Top - 5;
            float max_value = (float)Math.Round(min_y + (graph_rect.Height / scale_y), 1);
            e.Graphics.DrawString(max_value.ToString(), label_font, label_brush, label_rect, label_format);

            if (Math.Abs(min_y * scale_y) > 15)
            {
                label_rect.Y = graph_rect.Bottom - 5;
                float min_value = (float)Math.Round(min_y, 1);
                e.Graphics.DrawString(min_value.ToString(), label_font, label_brush, label_rect, label_format);
            }

            if (selected != null)
            {
                DrawInterpolator(e.Graphics, selected);
            }
            else if (group != null)
            {
                foreach (Interpolator interpolator in group.Interpolators)
                {
                    DrawInterpolator(e.Graphics, interpolator);
                }
            }
        }

        Point GetMinPoint(InterpolatorEntry entry)
        {
            int x = graph_rect.X + (int)Math.Floor(entry.Keyframe * scale_x);
            int y = graph_rect.Bottom - (int)Math.Floor((entry.Value.Min - min_y) * scale_y);
            return new Point(x, y);
        }

        Point GetMaxPoint(InterpolatorEntry entry)
        {
            int x = graph_rect.X + (int)Math.Floor(entry.Keyframe * scale_x);
            int y = graph_rect.Bottom - (int)Math.Floor((entry.Value.Max - min_y) * scale_y);
            return new Point(x, y);
        }

        void DrawInterpolator(Graphics g, Interpolator interpolator)
        {
            Color color = GetColorForParameter(interpolator.Parameter);
            Brush fill_brush = new SolidBrush(Color.FromArgb(100, color));
            Brush selected_brush = new SolidBrush(color);
            Pen outline_pen = new Pen(Color.FromArgb(128, color), 1);
            Pen normal_keyframe_pen = new Pen(Color.FromArgb(60, color), 1);
            normal_keyframe_pen.DashStyle = DashStyle.Dash;
            Pen selected_keyframe_pen = new Pen(Color.FromArgb(60, color), 1);
            selected_keyframe_pen.DashStyle = DashStyle.Dash;

            SmoothingMode default_smoothing = g.SmoothingMode;

            List<InterpolatorEntry> entries = interpolator.Entries;
            List<Point> polygon = new List<Point>((interpolator.Entries.Count + 2) * 2);

            Point implied_point = GetMaxPoint(entries[0]);
            implied_point.X = graph_rect.Left;
            polygon.Add(implied_point);

            foreach (InterpolatorEntry entry in entries)
            {
                Point point = GetMaxPoint(entry);
                polygon.Add(point);
            }

            implied_point = GetMaxPoint(entries[entries.Count - 1]);
            implied_point.X = ClientRectangle.Right;
            polygon.Add(implied_point);

            implied_point = GetMinPoint(entries[entries.Count - 1]);
            implied_point.X = ClientRectangle.Right;
            polygon.Add(implied_point);

            foreach (InterpolatorEntry entry in entries.Reverse<InterpolatorEntry>())
            {
                Point point = GetMinPoint(entry);
                polygon.Add(point);
            }

            implied_point = GetMinPoint(entries[0]);
            implied_point.X = graph_rect.Left;
            polygon.Add(implied_point);

            g.SmoothingMode = SmoothingMode.HighQuality;
            Point[] polygon_array = polygon.ToArray();
            g.FillPolygon(fill_brush, polygon_array);
            g.DrawPolygon(outline_pen, polygon_array);
            g.SmoothingMode = default_smoothing;

            if (selected != null)
            {
                foreach (InterpolatorEntry entry in entries)
                {
                    Point max_point = GetMaxPoint(entry);

                    Pen keyframe_pen;
                    Brush max_brush;
                    Brush min_brush;
                    if (entry == selected_entry)
                    {
                        keyframe_pen = selected_keyframe_pen;
                        if (entry_editor == EntryEditMode.Max || !entry.Random) max_brush = selected_brush;
                        else max_brush = fill_brush;

                        if (entry_editor == EntryEditMode.Min) min_brush = selected_brush;
                        else min_brush = fill_brush;
                    }
                    else
                    {
                        keyframe_pen = normal_keyframe_pen;
                        max_brush = min_brush = fill_brush;
                    }

                    g.DrawLine(keyframe_pen, max_point.X, ClientRectangle.Top, max_point.X, ClientRectangle.Bottom);

                    if (entry.Random)
                    {
                        DrawTriangleUp(g, max_brush, max_point);
                        Point min_point = GetMinPoint(entry);
                        DrawTriangleDown(g, min_brush, min_point);
                    }
                    else
                    {
                        g.FillRectangle(max_brush, max_point.X - 3, max_point.Y - 3, 6, 6);
                    }
                }
            }

        }

        void DrawTriangleUp(Graphics g, Brush b, Point p)
        {
            Point[] points = new Point[3];
            points[0] = p;
            points[1] = new Point(p.X - 3, p.Y - 6);
            points[2] = new Point(p.X + 3, p.Y - 6);
            g.FillPolygon(b, points);
        }

        void DrawTriangleDown(Graphics g, Brush b, Point p)
        {
            Point[] points = new Point[3];
            points[0] = p;
            points[1] = new Point(p.X - 3, p.Y + 6);
            points[2] = new Point(p.X + 3, p.Y + 6);
            g.FillPolygon(b, points);
        }


        Point last_mouse_position;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();

            if (selected == null || e.Button != MouseButtons.Left) return;

            InterpolatorEntry new_selected_entry = null;
            foreach (InterpolatorEntry entry in selected.Entries)
            {
                Point max_point = GetMaxPoint(entry);
                if (Math.Abs(max_point.X - e.Location.X) <= 3)
                {
                    new_selected_entry = entry;
                    entry_editor = EntryEditMode.Keyframe;

                    int delta_y = max_point.Y - e.Location.Y;
                    if (entry.Random)
                    {
                        if (delta_y >= 0 && delta_y <= 6)
                        {
                            entry_editor = EntryEditMode.Max;
                        }

                        Point min_point = GetMinPoint(entry);
                        delta_y = e.Location.Y - min_point.Y;
                        if (delta_y >= 0 && delta_y <= 6)
                        {
                            entry_editor = EntryEditMode.Min;
                        }
                    }
                    else
                    {
                        if (Math.Abs(delta_y) <= 3)
                        {
                            entry_editor = EntryEditMode.MinMax;
                        }
                    }

                    Capture = true;
                    PartyAPI.BeginUpdate();
                }
            }

            if (new_selected_entry == null && graph_rect.Bottom < e.Location.Y && graph_rect.Left < e.Location.X)
            {
                float keyframe_time = (e.Location.X - graph_rect.Left) / scale_x;
                if (keyframe_time < 0) keyframe_time = 0;

                InterpolatorEntry previous_entry = selected.Entries.LastOrDefault(other_entry => other_entry.Keyframe < keyframe_time);
                InterpolatorEntry next_entry = selected.Entries.FirstOrDefault(other_entry => other_entry.Keyframe > keyframe_time);

                InterpolatorEntry entry = new InterpolatorEntry();
                entry.Keyframe = keyframe_time;
                if (previous_entry == null)
                {
                    entry.Random = next_entry.Random;
                    entry.Value = next_entry.Value;
                }
                else if (next_entry == null)
                {
                    entry.Random = previous_entry.Random;
                    entry.Value = previous_entry.Value;
                }
                else
                {
                    entry.Random = previous_entry.Random || next_entry.Random;
                    float alpha = (keyframe_time - previous_entry.Keyframe) / (next_entry.Keyframe - previous_entry.Keyframe);
                    MinMaxField value = new MinMaxField();
                    value.Min = (float)Math.Round(previous_entry.Value.Min + (next_entry.Value.Min - previous_entry.Value.Min) * alpha, 3);
                    value.Max = (float)Math.Round(previous_entry.Value.Max + (next_entry.Value.Max - previous_entry.Value.Max) * alpha, 3);
                    entry.Value = value;
                }
                selected.Entries.Add(entry);
                selected.Entries.Sort((a, b) => a.Keyframe.CompareTo(b.Keyframe));

                new_selected_entry = entry;
            }

            if (new_selected_entry != selected_entry)
            {
                selected_entry = new_selected_entry;
                if (EntrySelectionChanged != null) EntrySelectionChanged(this, EventArgs.Empty);
            }

            last_mouse_position = e.Location;

            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (Capture && e.Button == MouseButtons.Left && selected_entry != null)
            {
                Point delta_mouse = last_mouse_position;
                delta_mouse.X -= e.Location.X;
                delta_mouse.Y -= e.Location.Y;

                bool value_changed = false;
                float scaled_delta;
                switch (entry_editor)
                {
                    case EntryEditMode.None: return;
                    case EntryEditMode.Min:
                    case EntryEditMode.Max:
                        if (delta_mouse.Y == 0) return;

                        scaled_delta = delta_mouse.Y / scale_y;
                        var value = selected_entry.Value;
                        if (entry_editor == EntryEditMode.Max)
                        {
                            value.Max = (float)Math.Round(value.Max + scaled_delta, 3);
                        }
                        else
                        {
                            value.Min = (float)Math.Round(value.Min + scaled_delta, 3);
                        }

                        if (value.Normalize()) entry_editor = entry_editor == EntryEditMode.Max ? EntryEditMode.Min : EntryEditMode.Max; // flip

                        value_changed = selected_entry.Value != value;
                        selected_entry.Value = value;

                        break;
                    case EntryEditMode.MinMax:
                        if (delta_mouse.Y == 0) return;

                        scaled_delta = delta_mouse.Y / scale_y;
                        float new_value = (float)Math.Round(selected_entry.Value.Max + scaled_delta, 3);

                        value_changed = selected_entry.Value != new_value;
                        selected_entry.Value = new MinMaxField(new_value);
                        break;

                    case EntryEditMode.Keyframe:
                        if (delta_mouse.X == 0) return;

                        scaled_delta = delta_mouse.X / scale_x;
                        float new_keyframe = (float)Math.Round(selected_entry.Keyframe - scaled_delta, 3);
                        new_keyframe = Math.Max(0, new_keyframe);

                        value_changed = selected_entry.Keyframe != new_keyframe;
                        selected_entry.Keyframe = new_keyframe;

                        selected.Entries.Sort((a, b) => a.Keyframe.CompareTo(b.Keyframe));
                        break;

                }

                last_mouse_position = e.Location;
                if (value_changed)
                {
                    group.OnChanged(selected);
                    Invalidate();
                }

                if (EntrySelectionChanged != null) EntrySelectionChanged(this, EventArgs.Empty);

            }
            else
            {
                Cursor new_cursor = Cursors.Default;
                if (selected != null)
                {
                    bool over_keyframe = false;
                    foreach (InterpolatorEntry entry in selected.Entries)
                    {
                        Point max_point = GetMaxPoint(entry);
                        if (Math.Abs(max_point.X - e.Location.X) <= 3)
                        {
                            over_keyframe = true;
                            new_cursor = Cursors.VSplit;

                            int delta_y = max_point.Y - e.Location.Y;
                            if (entry.Random)
                            {
                                if (delta_y >= 0 && delta_y <= 10)
                                {
                                    new_cursor = Cursors.HSplit;
                                }

                                Point min_point = GetMinPoint(entry);
                                delta_y = e.Location.Y - min_point.Y;
                                if (delta_y >= 0 && delta_y <= 10)
                                {
                                    new_cursor = Cursors.HSplit;
                                }
                            }
                            else if (Math.Abs(delta_y) <= 3)
                            {
                                new_cursor = Cursors.HSplit;
                            }
                            break;
                        }
                    }

                    if (!over_keyframe && graph_rect.Bottom < e.Location.Y && graph_rect.Left < e.Location.X)
                    {
                        new_cursor = Cursors.UpArrow;
                    }
                }

                if (Cursor != new_cursor) Cursor = new_cursor;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            PartyAPI.EndUpdate();

            UpdateScale();
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (e.Delta == 0) return;

            manual_scale = true;
            float scale_adjust = (e.Delta / 120) * 0.1f;
            if (ModifierKeys == Keys.Shift)
            {
                scale_x = scale_x + scale_x * scale_adjust;
            }
            else
            {
                scale_y = scale_y + scale_y * scale_adjust;
            }

            UpdateLines();
            Invalidate();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.KeyCode == Keys.Delete) DeleteKeyframe();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (e.KeyChar == (char)Keys.Back) DeleteKeyframe();
        }

        void DeleteKeyframe()
        {
            if (selected_entry == null) return;

            selected.Entries.Remove(selected_entry);
            selected.Entries.Sort((a, b) => a.Keyframe.CompareTo(b.Keyframe));
            if (selected.Entries.Count == 0) selected.Reset();

            PartyAPI.BeginUpdate();
            group.OnChanged(selected);
            PartyAPI.EndUpdate();
            if (EntrySelectionChanged != null) EntrySelectionChanged(this, EventArgs.Empty);
            Invalidate();
        }


        public void PropertyChanged()
        {
            UpdateScale();
            Invalidate();
            if (selected != null)
            {
                PartyAPI.BeginUpdate();
                group.OnChanged(selected);
                PartyAPI.EndUpdate();
            }
        }

        static Color GetColorForParameter(ModelParameterType parameter)
        {
            switch (parameter)
            {
                case ModelParameterType.Red: return Color.Red;
                case ModelParameterType.Green: return Color.Lime;
                case ModelParameterType.Blue: return Color.DodgerBlue;
                case ModelParameterType.Alpha: return Color.White;
                case ModelParameterType.Size: return Color.LightSlateGray;
                case ModelParameterType.Mass: return Color.DarkOrange;
                case ModelParameterType.Angle: return Color.Yellow;
                case ModelParameterType.TextureIndex: return Color.DarkSlateBlue;
                case ModelParameterType.RotationSpeed: return Color.MediumSeaGreen;
                case ModelParameterType.SizeMultiplier: return Color.Cyan;
                default: return Color.Transparent;
            }
        }
    }
}
