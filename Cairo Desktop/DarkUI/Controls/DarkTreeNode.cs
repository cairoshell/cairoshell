using DarkUI.Collections;
using System;
using System.Drawing;

namespace DarkUI.Controls
{
    public class DarkTreeNode
    {
        #region Event Region

        public event EventHandler<ObservableListModified<DarkTreeNode>> ItemsAdded;
        public event EventHandler<ObservableListModified<DarkTreeNode>> ItemsRemoved;

        public event EventHandler TextChanged;
        public event EventHandler NodeExpanded;
        public event EventHandler NodeCollapsed;

        #endregion

        #region Field Region

        private string _text;
        private bool _isRoot;
        private DarkTreeView _parentTree;
        private DarkTreeNode _parentNode;

        private ObservableList<DarkTreeNode> _nodes;

        private bool _expanded;

        #endregion

        #region Property Region

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value)
                    return;

                _text = value;

                OnTextChanged();
            }
        }

        public Rectangle ExpandArea { get; set; }

        public Rectangle IconArea { get; set; }

        public Rectangle TextArea { get; set; }

        public Rectangle FullArea { get; set; }

        public bool ExpandAreaHot { get; set; }

        public Bitmap Icon { get; set; }

        public Bitmap ExpandedIcon { get; set; }

        public bool Expanded
        {
            get { return _expanded; }
            set
            {
                if (_expanded == value)
                    return;

                if (value == true && Nodes.Count == 0)
                    return;

                _expanded = value;

                if (_expanded)
                {
                    if (NodeExpanded != null)
                        NodeExpanded(this, null);
                }
                else
                {
                    if (NodeCollapsed != null)
                        NodeCollapsed(this, null);
                }
            }
        }

        public ObservableList<DarkTreeNode> Nodes
        {
            get { return _nodes; }
            set
            {
                if (_nodes != null)
                {
                    _nodes.ItemsAdded -= Nodes_ItemsAdded;
                    _nodes.ItemsRemoved -= Nodes_ItemsRemoved;
                }

                _nodes = value;

                _nodes.ItemsAdded += Nodes_ItemsAdded;
                _nodes.ItemsRemoved += Nodes_ItemsRemoved;
            }
        }

        public bool IsRoot
        {
            get { return _isRoot; }
            set { _isRoot = value; }
        }

        public DarkTreeView ParentTree
        {
            get { return _parentTree; }
            set
            {
                if (_parentTree == value)
                    return;

                _parentTree = value;

                foreach (var node in Nodes)
                    node.ParentTree = _parentTree;
            }
        }

        public DarkTreeNode ParentNode
        {
            get { return _parentNode; }
            set { _parentNode = value; }
        }

        public bool Odd { get; set; }

        public object NodeType { get; set; }

        public object Tag { get; set; }

        public string FullPath
        {
            get
            {
                var parent = ParentNode;
                var path = Text;

                while (parent != null)
                {
                    path = string.Format("{0}{1}{2}", parent.Text, "\\", path);
                    parent = parent.ParentNode;
                }

                return path;
            }
        }

        public DarkTreeNode PrevVisibleNode { get; set; }

        public DarkTreeNode NextVisibleNode { get; set; }

        public int VisibleIndex { get; set; }

        public bool IsNodeAncestor(DarkTreeNode node)
        {
            var parent = ParentNode;
            while (parent != null)
            {
                if (parent == node)
                    return true;

                parent = parent.ParentNode;
            }

            return false;
        }

        #endregion

        #region Constructor Region

        public DarkTreeNode()
        {
            Nodes = new ObservableList<DarkTreeNode>();
        }

        public DarkTreeNode(string text)
            : this()
        {
            Text = text;
        }

        #endregion

        #region Method Region

        public void Remove()
        {
            if (ParentNode != null)
                ParentNode.Nodes.Remove(this);
            else
                ParentTree.Nodes.Remove(this);
        }

        public void EnsureVisible()
        {
            var parent = ParentNode;

            while (parent != null)
            {
                parent.Expanded = true;
                parent = parent.ParentNode;
            }
        }

        #endregion

        #region Event Handler Region

        private void OnTextChanged()
        {
            if (ParentTree != null && ParentTree.TreeViewNodeSorter != null)
            {
                if (ParentNode != null)
                    ParentNode.Nodes.Sort(ParentTree.TreeViewNodeSorter);
                else
                    ParentTree.Nodes.Sort(ParentTree.TreeViewNodeSorter);
            }

            if (TextChanged != null)
                TextChanged(this, null);
        }

        private void Nodes_ItemsAdded(object sender, ObservableListModified<DarkTreeNode> e)
        {
            foreach (var node in e.Items)
            {
                node.ParentNode = this;
                node.ParentTree = ParentTree;
            }

            if (ParentTree != null && ParentTree.TreeViewNodeSorter != null)
                Nodes.Sort(ParentTree.TreeViewNodeSorter);

            if (ItemsAdded != null)
                ItemsAdded(this, e);
        }

        private void Nodes_ItemsRemoved(object sender, ObservableListModified<DarkTreeNode> e)
        {
            if (Nodes.Count == 0)
                Expanded = false;

            if (ItemsRemoved != null)
                ItemsRemoved(this, e);
        }

        #endregion
    }
}
