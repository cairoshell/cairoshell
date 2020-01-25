using DarkUI.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DarkUI.Controls
{
    public class DarkListView : DarkScrollView
    {
        #region Event Region

        public event EventHandler SelectedIndicesChanged;

        #endregion

        #region Field Region

        private int _itemHeight = 20;
        private bool _multiSelect;

        private ObservableCollection<DarkListItem> _items;
        private List<int> _selectedIndices;
        private int _anchoredItemStart = -1;
        private int _anchoredItemEnd = -1;

        #endregion

        #region Property Region

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ObservableCollection<DarkListItem> Items
        {
            get { return _items; }
            set
            {
                if (_items != null)
                    _items.CollectionChanged -= Items_CollectionChanged;

                _items = value;

                _items.CollectionChanged += Items_CollectionChanged;

                UpdateListBox();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<int> SelectedIndices
        {
            get { return _selectedIndices; }
        }

        [Category("Appearance")]
        [Description("Determines the height of the individual list view items.")]
        [DefaultValue(20)]
        public int ItemHeight
        {
            get { return _itemHeight; }
            set
            {
                _itemHeight = value;
                UpdateListBox();
            }
        }

        [Category("Behaviour")]
        [Description("Determines whether multiple list view items can be selected at once.")]
        [DefaultValue(false)]
        public bool MultiSelect
        {
            get { return _multiSelect; }
            set { _multiSelect = value; }
        }

        #endregion

        #region Constructor Region

        public DarkListView()
        {
            Items = new ObservableCollection<DarkListItem>();
            _selectedIndices = new List<int>();
        }

        #endregion

        #region Event Handler Region

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                using (var g = CreateGraphics())
                {
                    // Set the area size of all new items
                    foreach (DarkListItem item in e.NewItems)
                    {
                        item.TextChanged += Item_TextChanged;
                        UpdateItemSize(item, g);
                    }
                }

                // Find the starting index of the new item list and update anything past that
                if (e.NewStartingIndex < (Items.Count - 1))
                {
                    for (var i = e.NewStartingIndex; i <= Items.Count - 1; i++)
                    {
                        UpdateItemPosition(Items[i], i);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (DarkListItem item in e.OldItems)
                    item.TextChanged -= Item_TextChanged;

                // Find the starting index of the old item list and update anything past that
                if (e.OldStartingIndex < (Items.Count - 1))
                {
                    for (var i = e.OldStartingIndex; i <= Items.Count - 1; i++)
                    {
                        UpdateItemPosition(Items[i], i);
                    }
                }
            }

            if (Items.Count == 0)
            {
                if (_selectedIndices.Count > 0)
                {
                    _selectedIndices.Clear();

                    if (SelectedIndicesChanged != null)
                        SelectedIndicesChanged(this, null);
                }
            }

            UpdateContentSize();
        }

        private void Item_TextChanged(object sender, EventArgs e)
        {
            var item = (DarkListItem)sender;

            UpdateItemSize(item);
            UpdateContentSize(item);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (Items.Count == 0)
                return;

            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right)
                return;

            var pos = OffsetMousePosition;

            var range = ItemIndexesInView().ToList();

            var top = range.Min();
            var bottom = range.Max();
            var width = Math.Max(ContentSize.Width, Viewport.Width);

            for (var i = top; i <= bottom; i++)
            {
                var rect = new Rectangle(0, i * ItemHeight, width, ItemHeight);

                if (rect.Contains(pos))
                {
                    if (MultiSelect && ModifierKeys == Keys.Shift)
                        SelectAnchoredRange(i);
                    else if (MultiSelect && ModifierKeys == Keys.Control)
                        ToggleItem(i);
                    else
                        SelectItem(i);
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (Items.Count == 0)
                return;

            if (e.KeyCode != Keys.Down && e.KeyCode != Keys.Up)
                return;

            if (MultiSelect && ModifierKeys == Keys.Shift)
            {
                if (e.KeyCode == Keys.Up)
                {
                    if (_anchoredItemEnd - 1 >= 0)
                    {
                        SelectAnchoredRange(_anchoredItemEnd - 1);
                        EnsureVisible();
                    }
                }
                else if (e.KeyCode == Keys.Down)
                {
                    if (_anchoredItemEnd + 1 <= Items.Count - 1)
                    {
                        SelectAnchoredRange(_anchoredItemEnd + 1);
                    }
                }
            }
            else
            {
                if (e.KeyCode == Keys.Up)
                {
                    if (_anchoredItemEnd - 1 >= 0)
                        SelectItem(_anchoredItemEnd - 1);
                }
                else if (e.KeyCode == Keys.Down)
                {
                    if (_anchoredItemEnd + 1 <= Items.Count - 1)
                        SelectItem(_anchoredItemEnd + 1);
                }
            }

            EnsureVisible();
        }

        #endregion

        #region Method Region

        public int GetItemIndex(DarkListItem item)
        {
            return Items.IndexOf(item);
        }

        public void SelectItem(int index)
        {
            if (index < 0 || index > Items.Count - 1)
                throw new IndexOutOfRangeException($"Value '{index}' is outside of valid range.");

            _selectedIndices.Clear();
            _selectedIndices.Add(index);

            if (SelectedIndicesChanged != null)
                SelectedIndicesChanged(this, null);

            _anchoredItemStart = index;
            _anchoredItemEnd = index;

            Invalidate();
        }

        public void SelectItems(IEnumerable<int> indexes)
        {
            _selectedIndices.Clear();

            var list = indexes.ToList();

            foreach (var index in list)
            {
                if (index < 0 || index > Items.Count - 1)
                    throw new IndexOutOfRangeException($"Value '{index}' is outside of valid range.");

                _selectedIndices.Add(index);
            }

            if (SelectedIndicesChanged != null)
                SelectedIndicesChanged(this, null);

            _anchoredItemStart = list[list.Count - 1];
            _anchoredItemEnd = list[list.Count - 1];

            Invalidate();
        }

        public void ToggleItem(int index)
        {
            if (_selectedIndices.Contains(index))
            {
                _selectedIndices.Remove(index);

                // If we just removed both the anchor start AND end then reset them
                if (_anchoredItemStart == index && _anchoredItemEnd == index)
                {
                    if (_selectedIndices.Count > 0)
                    {
                        _anchoredItemStart = _selectedIndices[0];
                        _anchoredItemEnd = _selectedIndices[0];
                    }
                    else
                    {
                        _anchoredItemStart = -1;
                        _anchoredItemEnd = -1;
                    }
                }

                // If we just removed the anchor start then update it accordingly
                if (_anchoredItemStart == index)
                {
                    if (_anchoredItemEnd < index)
                        _anchoredItemStart = index - 1;
                    else if (_anchoredItemEnd > index)
                        _anchoredItemStart = index + 1;
                    else
                        _anchoredItemStart = _anchoredItemEnd;
                }

                // If we just removed the anchor end then update it accordingly
                if (_anchoredItemEnd == index)
                {
                    if (_anchoredItemStart < index)
                        _anchoredItemEnd = index - 1;
                    else if (_anchoredItemStart > index)
                        _anchoredItemEnd = index + 1;
                    else
                        _anchoredItemEnd = _anchoredItemStart;
                }
            }
            else
            {
                _selectedIndices.Add(index);
                _anchoredItemStart = index;
                _anchoredItemEnd = index;
            }

            if (SelectedIndicesChanged != null)
                SelectedIndicesChanged(this, null);

            Invalidate();
        }

        public void SelectItems(int startRange, int endRange)
        {
            _selectedIndices.Clear();

            if (startRange == endRange)
                _selectedIndices.Add(startRange);

            if (startRange < endRange)
            {
                for (var i = startRange; i <= endRange; i++)
                    _selectedIndices.Add(i);
            }
            else if (startRange > endRange)
            {
                for (var i = startRange; i >= endRange; i--)
                    _selectedIndices.Add(i);
            }

            if (SelectedIndicesChanged != null)
                SelectedIndicesChanged(this, null);

            Invalidate();
        }

        private void SelectAnchoredRange(int index)
        {
            _anchoredItemEnd = index;
            SelectItems(_anchoredItemStart, index);
        }

        private void UpdateListBox()
        {
            using (var g = CreateGraphics())
            {
                for (var i = 0; i <= Items.Count - 1; i++)
                {
                    var item = Items[i];
                    UpdateItemSize(item, g);
                    UpdateItemPosition(item, i);
                }
            }

            UpdateContentSize();
        }

        private void UpdateItemSize(DarkListItem item)
        {
            using (var g = CreateGraphics())
            {
                UpdateItemSize(item, g);
            }
        }

        private void UpdateItemSize(DarkListItem item, Graphics g)
        {
            var size = g.MeasureString(item.Text, Font);
            size.Width++;

            item.Area = new Rectangle(item.Area.Left, item.Area.Top, (int)size.Width, item.Area.Height);
        }

        private void UpdateItemPosition(DarkListItem item, int index)
        {
            item.Area = new Rectangle(2, (index * ItemHeight), item.Area.Width, ItemHeight);
        }

        private void UpdateContentSize()
        {
            var highestWidth = 0;

            foreach (var item in Items)
            {
                if (item.Area.Right + 1 > highestWidth)
                    highestWidth = item.Area.Right + 1;
            }

            var width = highestWidth;
            var height = Items.Count * ItemHeight;

            if (ContentSize.Width != width || ContentSize.Height != height)
            {
                ContentSize = new Size(width, height);
                Invalidate();
            }
        }

        private void UpdateContentSize(DarkListItem item)
        {
            var itemWidth = item.Area.Right + 1;

            if (itemWidth == ContentSize.Width)
            {
                UpdateContentSize();
                return;
            }

            if (itemWidth > ContentSize.Width)
            {
                ContentSize = new Size(itemWidth, ContentSize.Height);
                Invalidate();
            }
        }

        public void EnsureVisible()
        {
            if (SelectedIndices.Count == 0)
                return;

            var itemTop = -1;

            if (!MultiSelect)
                itemTop = SelectedIndices[0] * ItemHeight;
            else
                itemTop = _anchoredItemEnd * ItemHeight;

            var itemBottom = itemTop + ItemHeight;

            if (itemTop < Viewport.Top)
                VScrollTo(itemTop);

            if (itemBottom > Viewport.Bottom)
                VScrollTo((itemBottom - Viewport.Height));
        }

        private IEnumerable<int> ItemIndexesInView()
        {
            var top = (Viewport.Top / ItemHeight) - 1;

            if (top < 0)
                top = 0;

            var bottom = ((Viewport.Top + Viewport.Height) / ItemHeight) + 1;

            if (bottom > Items.Count)
                bottom = Items.Count;

            var result = Enumerable.Range(top, bottom - top);
            return result;
        }

        private IEnumerable<DarkListItem> ItemsInView()
        {
            var indexes = ItemIndexesInView();
            var result = indexes.Select(index => Items[index]).ToList();
            return result;
        }

        #endregion

        #region Paint Region

        protected override void PaintContent(Graphics g)
        {
            var range = ItemIndexesInView().ToList();

            if (range.Count == 0)
                return;

            var top = range.Min();
            var bottom = range.Max();

            for (var i = top; i <= bottom; i++)
            {
                var width = Math.Max(ContentSize.Width, Viewport.Width);
                var rect = new Rectangle(0, i * ItemHeight, width, ItemHeight);

                // Background
                var odd = i % 2 != 0;
                var bgColor = !odd ? Colors.HeaderBackground : Colors.GreyBackground;

                if (SelectedIndices.Count > 0 && SelectedIndices.Contains(i))
                    bgColor = Focused ? Colors.BlueSelection : Colors.GreySelection;

                using (var b = new SolidBrush(bgColor))
                {
                    g.FillRectangle(b, rect);
                }

                // DEBUG: Border
                /*using (var p = new Pen(Colors.DarkBorder))
                {
                    g.DrawLine(p, new Point(rect.Left, rect.Bottom - 1), new Point(rect.Right, rect.Bottom - 1));
                }*/

                // Text
                using (var b = new SolidBrush(Items[i].TextColor))
                {
                    var stringFormat = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center
                    };

                    var modFont = new Font(Font, Items[i].FontStyle);

                    var modRect = new Rectangle(rect.Left + 2, rect.Top, rect.Width, rect.Height);
                    g.DrawString(Items[i].Text, modFont, b, modRect, stringFormat);
                }
            }
        }

        #endregion
    }
}
