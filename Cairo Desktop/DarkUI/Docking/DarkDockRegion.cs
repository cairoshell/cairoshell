using DarkUI.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DarkUI.Docking
{
    [ToolboxItem(false)]
    public class DarkDockRegion : Panel
    {
        #region Field Region

        private List<DarkDockGroup> _groups;

        private Form _parentForm;
        private DarkDockSplitter _splitter;

        #endregion

        #region Property Region

        public DarkDockPanel DockPanel { get; private set; }

        public DarkDockArea DockArea { get; private set; }

        public DarkDockContent ActiveDocument
        {
            get
            {
                if (DockArea != DarkDockArea.Document || _groups.Count == 0)
                    return null;

                return _groups[0].VisibleContent;
            }
        }

        public List<DarkDockGroup> Groups
        {
            get
            {
                return _groups.ToList();
            }
        }

        #endregion

        #region Constructor Region

        public DarkDockRegion(DarkDockPanel dockPanel, DarkDockArea dockArea)
        {
            _groups = new List<DarkDockGroup>();

            DockPanel = dockPanel;
            DockArea = dockArea;

            BuildProperties();
        }

        #endregion

        #region Method Region

        internal void AddContent(DarkDockContent dockContent)
        {
            AddContent(dockContent, null);
        }

        internal void AddContent(DarkDockContent dockContent, DarkDockGroup dockGroup)
        {
            // If no existing group is specified then create a new one
            if (dockGroup == null)
            {
                // If this is the document region, then default to first group if it exists
                if (DockArea == DarkDockArea.Document && _groups.Count > 0)
                    dockGroup = _groups[0];
                else
                    dockGroup = CreateGroup();
            }

            dockContent.DockRegion = this;
            dockGroup.AddContent(dockContent);

            if (!Visible)
            {
                Visible = true;
                CreateSplitter();
            }

            PositionGroups();
        }

        internal void InsertContent(DarkDockContent dockContent, DarkDockGroup dockGroup, DockInsertType insertType)
        {
            var order = dockGroup.Order;

            if (insertType == DockInsertType.After)
                order++;

            var newGroup = InsertGroup(order);

            dockContent.DockRegion = this;
            newGroup.AddContent(dockContent);

            if (!Visible)
            {
                Visible = true;
                CreateSplitter();
            }

            PositionGroups();
        }

        internal void RemoveContent(DarkDockContent dockContent)
        {
            dockContent.DockRegion = null;

            var group = dockContent.DockGroup;
            group.RemoveContent(dockContent);

            dockContent.DockArea = DarkDockArea.None;

            // If that was the final content in the group then remove the group
            if (group.ContentCount == 0)
                RemoveGroup(group);

            // If we just removed the final group, and this isn't the document region, then hide
            if (_groups.Count == 0 && DockArea != DarkDockArea.Document)
            {
                Visible = false;
                RemoveSplitter();
            }

            PositionGroups();
        }

        public List<DarkDockContent> GetContents()
        {
            var result = new List<DarkDockContent>();
            
            foreach (var group in _groups)
                result.AddRange(group.GetContents());

            return result;
        }

        private DarkDockGroup CreateGroup()
        {
            var order = 0;

            if (_groups.Count >= 1)
            {
                order = -1;
                foreach (var group in _groups)
                {
                    if (group.Order >= order)
                        order = group.Order + 1;
                }
            }

            var newGroup = new DarkDockGroup(DockPanel, this, order);
            _groups.Add(newGroup);
            Controls.Add(newGroup);

            return newGroup;
        }

        private DarkDockGroup InsertGroup(int order)
        {
            foreach (var group in _groups)
            {
                if (group.Order >= order)
                    group.Order++;
            }

            var newGroup = new DarkDockGroup(DockPanel, this, order);
            _groups.Add(newGroup);
            Controls.Add(newGroup);

            return newGroup;
        }

        private void RemoveGroup(DarkDockGroup group)
        {
            var lastOrder = group.Order;

            _groups.Remove(group);
            Controls.Remove(group);

            foreach (var otherGroup in _groups)
            {
                if (otherGroup.Order > lastOrder)
                    otherGroup.Order--;
            }
        }

        private void PositionGroups()
        {
            DockStyle dockStyle;

            switch (DockArea)
            {
                default:
                case DarkDockArea.Document:
                    dockStyle = DockStyle.Fill;
                    break;
                case DarkDockArea.Left:
                case DarkDockArea.Right:
                    dockStyle = DockStyle.Top;
                    break;
                case DarkDockArea.Bottom:
                    dockStyle = DockStyle.Left;
                    break;
            }

            if (_groups.Count == 1)
            {
                _groups[0].Dock = DockStyle.Fill;
                return;
            }

            if (_groups.Count > 1)
            {
                var lastGroup = _groups.OrderByDescending(g => g.Order).First();

                foreach (var group in _groups.OrderByDescending(g => g.Order))
                {
                    group.SendToBack();

                    if (group.Order == lastGroup.Order)
                        group.Dock = DockStyle.Fill;
                    else
                        group.Dock = dockStyle;
                }

                SizeGroups();
            }
        }

        private void SizeGroups()
        {
            if (_groups.Count <= 1)
                return;

            var size = new Size(0, 0);

            switch (DockArea)
            {
                default:
                case DarkDockArea.Document:
                    return;
                case DarkDockArea.Left:
                case DarkDockArea.Right:
                    size = new Size(ClientRectangle.Width, ClientRectangle.Height / _groups.Count);
                    break;
                case DarkDockArea.Bottom:
                    size = new Size(ClientRectangle.Width / _groups.Count, ClientRectangle.Height);
                    break;
            }

            foreach (var group in _groups)
                group.Size = size;
        }

        private void BuildProperties()
        {
            MinimumSize = new Size(50, 50);

            switch (DockArea)
            {
                default:
                case DarkDockArea.Document:
                    Dock = DockStyle.Fill;
                    Padding = new Padding(0, 1, 0, 0);
                    break;
                case DarkDockArea.Left:
                    Dock = DockStyle.Left;
                    Padding = new Padding(0, 0, 1, 0);
                    Visible = false;
                    break;
                case DarkDockArea.Right:
                    Dock = DockStyle.Right;
                    Padding = new Padding(1, 0, 0, 0);
                    Visible = false;
                    break;
                case DarkDockArea.Bottom:
                    Dock = DockStyle.Bottom;
                    Padding = new Padding(0, 0, 0, 0);
                    Visible = false;
                    break;
            }
        }

        private void CreateSplitter()
        {
            if (_splitter != null && DockPanel.Splitters.Contains(_splitter))
                DockPanel.Splitters.Remove(_splitter);

            switch (DockArea)
            {
                case DarkDockArea.Left:
                    _splitter = new DarkDockSplitter(DockPanel, this, DarkSplitterType.Right);
                    break;
                case DarkDockArea.Right:
                    _splitter = new DarkDockSplitter(DockPanel, this, DarkSplitterType.Left);
                    break;
                case DarkDockArea.Bottom:
                    _splitter = new DarkDockSplitter(DockPanel, this, DarkSplitterType.Top);
                    break;
                default:
                    return;
            }

            DockPanel.Splitters.Add(_splitter);
        }

        private void RemoveSplitter()
        {
            if (DockPanel.Splitters.Contains(_splitter))
                DockPanel.Splitters.Remove(_splitter);
        }

        #endregion

        #region Event Handler Region

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            _parentForm = FindForm();
            _parentForm.ResizeEnd += ParentForm_ResizeEnd;
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);

            SizeGroups();
        }

        private void ParentForm_ResizeEnd(object sender, EventArgs e)
        {
            if (_splitter != null)
                _splitter.UpdateBounds();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);

            if (_splitter != null)
                _splitter.UpdateBounds();
        }

        #endregion

        #region Paint Region

        public void Redraw()
        {
            Invalidate();

            foreach (var group in _groups)
                group.Redraw();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            if (!Visible)
                return;

            // Fill body
            using (var b = new SolidBrush(Colors.GreyBackground))
            {
                g.FillRectangle(b, ClientRectangle);
            }

            // Draw border
            using (var p = new Pen(Colors.DarkBorder))
            {
                // Top border
                if (DockArea == DarkDockArea.Document)
                    g.DrawLine(p, ClientRectangle.Left, 0, ClientRectangle.Right, 0);

                // Left border
                if (DockArea == DarkDockArea.Right)
                    g.DrawLine(p, ClientRectangle.Left, 0, ClientRectangle.Left, ClientRectangle.Height);

                // Right border
                if (DockArea == DarkDockArea.Left)
                    g.DrawLine(p, ClientRectangle.Right - 1, 0, ClientRectangle.Right - 1, ClientRectangle.Height);
            }
        }

        #endregion
    }
}
