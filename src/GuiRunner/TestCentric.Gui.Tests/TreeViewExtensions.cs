// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Windows.Forms;

namespace TestCentric.Gui
{
    public static class TreeViewExtensions
    {
        #region Tree Extensions tailored for use in our tests

        public static TreeNode Search(this TreeView treeView, string text)
        {
            return Search(treeView.Nodes, text);
        }

        private static TreeNode Search(TreeNodeCollection nodes, string text)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Text == text)
                    return node;

                TreeNode child = Search(node.Nodes, text);
                if (child != null)
                    return child;
            }

            return null;
        }

        public static void Remove(this TreeView treeView, params string[] items)
        {
            foreach (string item in items)
                treeView.Search(item)?.Remove();
        }

        public static void Expand(this TreeView treeView, params string[] items)
        {
            foreach (string item in items)
                treeView.Search(item)?.Expand();
        }

        public static void Check(this TreeView treeView, params string[] items)
        {
            foreach (string item in items)
            {
                TreeNode node = treeView.Search(item);
                if (node != null)
                    node.Checked = true;
            }
        }

        // TreeNode Extension
        public static void AddSibling(this TreeNode treeNode, TreeNode sibling)
        {
            var parent = treeNode.Parent;
            if (parent != null)
                parent.Nodes.Add(sibling);
            else
                treeNode.TreeView.Nodes.Add(sibling);
        }

        #endregion
    }
}
