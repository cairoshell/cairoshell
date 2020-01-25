using DarkUI.Config;
using DarkUI.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace DarkUI.Docking
{
    public class DarkDockPanel : UserControl
    {
        #region Event Region

        public event EventHandler<DockContentEventArgs> ActiveContentChanged;
        public event EventHandler<DockContentEventArgs> ContentAdded;
        public event EventHandler<DockContentEventArgs> ContentRemoved;

        #endregion

        #region Field Region

        private List<DarkDockContent> _contents;
        private Dictionary<DarkDockArea, DarkDockRegion> _regions;

        private DarkDockContent _activeContent;
        private bool _switchingContent = false;

        #endregion

        #region Property Region

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DarkDockContent ActiveContent
        {
            get { return _activeContent; }
            set
            {
                // Don't let content visibility changes re-trigger event
                if (_switchingContent)
                    return;

                _switchingContent = true;

                _activeContent = value;

                ActiveGroup = _activeContent.DockGroup;
                ActiveRegion = ActiveGroup.DockRegion;

                foreach (var region in _regions.Values)
                    region.Redraw();

                if (ActiveContentChanged != null)
                    ActiveContentChanged(this, new DockContentEventArgs(_activeContent));

                _switchingContent = false;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DarkDockRegion ActiveRegion { get; internal set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DarkDockGroup ActiveGroup { get; internal set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DarkDockContent ActiveDocument
        {
            get
            {
                return _regions[DarkDockArea.Document].ActiveDocument;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DockContentDragFilter DockContentDragFilter { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DockResizeFilter DockResizeFilter { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<DarkDockSplitter> Splitters { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MouseButtons MouseButtonState
        {
            get
            {
                var buttonState = MouseButtons;
                return buttonState;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Dictionary<DarkDockArea, DarkDockRegion> Regions
        {
            get
            {
                return _regions;
            }
        }

        #endregion

        #region Constructor Region

        public DarkDockPanel()
        {
            Splitters = new List<DarkDockSplitter>();
            DockContentDragFilter = new DockContentDragFilter(this);
            DockResizeFilter = new DockResizeFilter(this);

            _regions = new Dictionary<DarkDockArea, DarkDockRegion>();
            _contents = new List<DarkDockContent>();

            BackColor = Colors.GreyBackground;

            CreateRegions();
        }

        #endregion

        #region Method Region

        public void AddContent(DarkDockContent dockContent)
        {
            AddContent(dockContent, null);
        }

        public void AddContent(DarkDockContent dockContent, DarkDockGroup dockGroup)
        {
            if (_contents.Contains(dockContent))
                RemoveContent(dockContent);

            dockContent.DockPanel = this;
            _contents.Add(dockContent);

            if (dockGroup != null)
                dockContent.DockArea = dockGroup.DockArea;

            if (dockContent.DockArea == DarkDockArea.None)
                dockContent.DockArea = dockContent.DefaultDockArea;

            var region = _regions[dockContent.DockArea];
            region.AddContent(dockContent, dockGroup);

            if (ContentAdded != null)
                ContentAdded(this, new DockContentEventArgs(dockContent));

            dockContent.Select();
        }

        public void InsertContent(DarkDockContent dockContent, DarkDockGroup dockGroup, DockInsertType insertType)
        {
            if (_contents.Contains(dockContent))
                RemoveContent(dockContent);

            dockContent.DockPanel = this;
            _contents.Add(dockContent);

            dockContent.DockArea = dockGroup.DockArea;

            var region = _regions[dockGroup.DockArea];
            region.InsertContent(dockContent, dockGroup, insertType);

            if (ContentAdded != null)
                ContentAdded(this, new DockContentEventArgs(dockContent));

            dockContent.Select();
        }

        public void RemoveContent(DarkDockContent dockContent)
        {
            if (!_contents.Contains(dockContent))
                return;

            dockContent.DockPanel = null;
            _contents.Remove(dockContent);

            var region = _regions[dockContent.DockArea];
            region.RemoveContent(dockContent);

            if (ContentRemoved != null)
                ContentRemoved(this, new DockContentEventArgs(dockContent));
        }

        public bool ContainsContent(DarkDockContent dockContent)
        {
            return _contents.Contains(dockContent);
        }

        public List<DarkDockContent> GetDocuments()
        {
            return _regions[DarkDockArea.Document].GetContents();
        }

        private void CreateRegions()
        {
            var documentRegion = new DarkDockRegion(this, DarkDockArea.Document);
            _regions.Add(DarkDockArea.Document, documentRegion);

            var leftRegion = new DarkDockRegion(this, DarkDockArea.Left);
            _regions.Add(DarkDockArea.Left, leftRegion);

            var rightRegion = new DarkDockRegion(this, DarkDockArea.Right);
            _regions.Add(DarkDockArea.Right, rightRegion);

            var bottomRegion = new DarkDockRegion(this, DarkDockArea.Bottom);
            _regions.Add(DarkDockArea.Bottom, bottomRegion);

            // Add the regions in this order to force the bottom region to be positioned
            // between the left and right regions properly.
            Controls.Add(documentRegion);
            Controls.Add(bottomRegion);
            Controls.Add(leftRegion);
            Controls.Add(rightRegion);

            // Create tab index for intuitive tabbing order
            documentRegion.TabIndex = 0;
            rightRegion.TabIndex = 1;
            bottomRegion.TabIndex = 2;
            leftRegion.TabIndex = 3;
        }

        public void DragContent(DarkDockContent content)
        {
            DockContentDragFilter.StartDrag(content);
        }

        #endregion

        #region Serialization Region

        public DockPanelState GetDockPanelState()
        {
            var state = new DockPanelState();

            state.Regions.Add(new DockRegionState(DarkDockArea.Document));
            state.Regions.Add(new DockRegionState(DarkDockArea.Left, _regions[DarkDockArea.Left].Size));
            state.Regions.Add(new DockRegionState(DarkDockArea.Right, _regions[DarkDockArea.Right].Size));
            state.Regions.Add(new DockRegionState(DarkDockArea.Bottom, _regions[DarkDockArea.Bottom].Size));

            var _groupStates = new Dictionary<DarkDockGroup, DockGroupState>();

            var orderedContent = _contents.OrderBy(c => c.Order);
            foreach (var content in orderedContent)
            {
                foreach (var region in state.Regions)
                {
                    if (region.Area == content.DockArea)
                    {
                        DockGroupState groupState;

                        if (_groupStates.ContainsKey(content.DockGroup))
                        {
                            groupState = _groupStates[content.DockGroup];
                        }
                        else
                        {
                            groupState = new DockGroupState();
                            region.Groups.Add(groupState);
                            _groupStates.Add(content.DockGroup, groupState);
                        }

                        groupState.Contents.Add(content.SerializationKey);
                    }
                }
            }

            return state;
        }

        public void RestoreDockPanelState(DockPanelState state, Func<string, DarkDockContent> getContentBySerializationKey)
        {
            foreach (var region in state.Regions)
            {
                switch (region.Area)
                {
                    case DarkDockArea.Left:
                        _regions[DarkDockArea.Left].Size = region.Size;
                        break;
                    case DarkDockArea.Right:
                        _regions[DarkDockArea.Right].Size = region.Size;
                        break;
                    case DarkDockArea.Bottom:
                        _regions[DarkDockArea.Bottom].Size = region.Size;
                        break;
                }

                foreach (var group in region.Groups)
                {
                    DarkDockContent previousContent = null;

                    foreach (var contentKey in group.Contents)
                    {
                        var content = getContentBySerializationKey(contentKey);

                        if (content == null)
                            continue;

                        if (previousContent == null)
                            AddContent(content);
                        else
                            AddContent(content, previousContent.DockGroup);

                        previousContent = content;
                    }
                }
            }
        }

        #endregion
    }
}
    