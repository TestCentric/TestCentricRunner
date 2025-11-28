// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Linq;
using TestCentric.Gui.Controls;
using TestCentric.Gui.Model;
using TestCentric.Gui.Presenters;

namespace TestCentric.Gui
{
    /// <summary>
    /// The VisualState class holds the latest visual state for a project.
    /// </summary>
    //[Serializable]
    public class VisualState : IXmlSerializable
    {
        // Default constructor is required for serialization
        public VisualState() : this("NUNIT_TREE") { }

        public VisualState(string strategyID)
        {
            DisplayStrategy = strategyID;
        }

        public VisualState(string strategyID, string groupID, bool showNamespace)
        {
            DisplayStrategy = strategyID;
            GroupBy = groupID;
            ShowNamespace = showNamespace;
        }

        public VisualState(string strategyID, string groupID)
        {
            if (strategyID == "NUNIT_TREE")
                throw new ArgumentException("When strategy is NUNIT_TREE, groupID may not be specified", nameof(groupID));

            DisplayStrategy = strategyID;
            GroupBy = groupID;
        }

        #region Fields

        //[XmlAttribute]
        public string DisplayStrategy;

        //[XmlAttribute]
        public string GroupBy;

        //[XmlIgnore]
        public bool GroupBySpecified => GroupBy != null;

        //[XmlAttribute, DefaultValue(false)]
        public bool ShowCheckBoxes;

        public bool ShowNamespace;

        // TODO: Categories not yet supported
        //public List<string> SelectedCategories;
        //public bool ExcludeCategories;

        //[XmlArrayItem("Node")]
        public List<VisualTreeNode> Nodes = new List<VisualTreeNode>();

        //private Dictionary<string, VisualTreeNode> _nodeIndex = new Dictionary<string, VisualTreeNode>();

        #endregion

        #region Public Methods

        public static string GetVisualStateFileName(string testFileName)
        {
            return string.IsNullOrEmpty(testFileName)
                ? "VisualState.xml"
                : testFileName + ".VisualState.xml";
        }

        #region Load VisualState from a Tree

        public VisualState LoadFrom(TreeView treeView)
        {
            ShowCheckBoxes = treeView.CheckBoxes;

            treeView.InvokeIfRequired(() =>
            {
                foreach (TreeNode treeNode in treeView.Nodes)
                    ProcessTreeNode(treeNode, this.Nodes);
            });

            return this;

            void ProcessTreeNode(TreeNode treeNode, List<VisualTreeNode> visualNodes)
            {
                bool isSelectedNode = treeNode == treeNode.TreeView.SelectedNode;
                bool isTopNode = treeNode == treeNode.TreeView.TopNode;

                var visualNode = new VisualTreeNode()
                {
                    Name = GetName(treeNode),
                    Expanded = treeNode.IsExpanded,
                    Checked = treeNode.Checked,
                    Selected = isSelectedNode,
                    IsTopNode = isTopNode
                };

                if (treeNode.IsExpanded || treeNode.Checked || isSelectedNode || isTopNode)
                    RecordVisualNode(visualNode, visualNodes);

                foreach (TreeNode childNode in treeNode.Nodes)
                    ProcessTreeNode(childNode, visualNode.Nodes);
            }
        }

        private void RecordVisualNode(VisualTreeNode visualNode, List<VisualTreeNode> visualNodes)
        {
            visualNodes.Add(visualNode);
        }

        #endregion

        #region Apply VisualState to a Tree

        public void ApplyTo(TreeView treeView)
        {
            // TODO: Use of these members is a HACK! Should remove.
            _selectedTreeNode = null;
            _topTreeNode = null;

            treeView.CheckBoxes = ShowCheckBoxes;

            ApplyVisualNodesToTreeNodes(this.Nodes, treeView.Nodes);

            if (_selectedTreeNode != null)
                treeView.SelectedNode = _selectedTreeNode;

            if (_topTreeNode != null)
                treeView.TopNode = _topTreeNode;
        }

        private void ApplyVisualNodesToTreeNodes(List<VisualTreeNode> visualNodes, TreeNodeCollection treeNodes)
        {
            // Find matching names
            foreach (var visualNode in visualNodes)
            {
                foreach (TreeNode treeNode in treeNodes)
                {
                    string name = GetName(treeNode);
                    if (name == visualNode.Name || treeNode.Text == "Not Run")
                    {
                        ApplyVisualNodeToTreeNode(visualNode, treeNode);
                        break;
                    }
                }
            }
        }

        private TreeNode _selectedTreeNode;
        private TreeNode _topTreeNode;

        private void ApplyVisualNodeToTreeNode(VisualTreeNode visualNode, TreeNode treeNode)
        {
            if (treeNode.IsExpanded != visualNode.Expanded)
                treeNode.Toggle();

            treeNode.Checked = visualNode.Checked;

            if (visualNode.IsTopNode)
                _topTreeNode = treeNode;

            if (visualNode.Selected)
                _selectedTreeNode = treeNode;

            ApplyVisualNodesToTreeNodes(visualNode.Nodes, treeNode.Nodes);
        }

        #endregion

        #region Load VisualState from a File

        public static VisualState LoadFrom(string fileName)
        {
            using (StreamReader reader = new StreamReader(fileName))
            {
                return LoadFrom(reader);
            }
        }

        public static VisualState LoadFrom(TextReader reader)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(VisualState));
            return (VisualState)serializer.Deserialize(reader);
        }

        #endregion

        #region Save to a File

        public void Save(string fileName)
        {
            string path = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                return;

            try
            {
                using StreamWriter writer = new StreamWriter(fileName);
                Save(writer);
            }
            catch (Exception e)
            {
                // Catch all kind of exceptions while writing file (Security, IO-Exception, ...)
                Logger log = InternalTrace.GetLogger(nameof(VisualState));
                log.Error($"Failed to write VisualState file: {fileName}; {e.Message}");
            }
        }

        public void Save(TextWriter writer)
        {
            XmlSerializer serializer = new XmlSerializer(GetType());

            try
            {
                serializer.Serialize(writer, this);
            }
            catch(InvalidOperationException ex)
            {
                throw new Exception("Unable to serialize VisualState. This may be due to duplicate node names in the tree.", ex);
            }
        }

        #endregion

        #region Dump VisualState to Console 

        public void Dump()
        {
            var writer = new StringWriter();
            Save(writer);

            Console.WriteLine(writer.GetStringBuilder().ToString());
        }

        #endregion

        #region IXmlSerializable Implementation

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            // We start out positioned on the root element which
            // has already been read for us, so process it.

            // Get DisplayStrategy, which will not be present if the VisualState
            // was saved using version 1 of the GUI. In that case, we rely on
            // the default constructor having set the strategy to "NUNIT_TREE".
            var strategy = reader.GetAttribute("DisplayStrategy");
            if (strategy != null) DisplayStrategy = strategy;
            GroupBy = reader.GetAttribute("GroupBy");
            // GroupBy is null for NUnitTree strategy, otherwise required
            if (GroupBy == null && strategy != "NUNIT_TREE") GroupBy = "ASSEMBLY";
            ShowCheckBoxes = reader.GetAttribute("ShowCheckBoxes") == "True";
            ShowNamespace = reader.GetAttribute("ShowNamespace") != "False";

            while (reader.Read())
            {
                switch(reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "Nodes":
                                GetNodeList(Nodes);
                                break;
                            case "Node":
                                Console.WriteLine($"Unprocessed {reader.NodeType} {reader.Name}");
                                break;
                            default:
                                Console.WriteLine($"Unexpected Element {reader.Name}");
                                break;
                        }
                        break;

                    case XmlNodeType.EndElement:
                        switch (reader.Name)
                        {
                            case "Nodes":
                                Console.WriteLine($"Unprocessed {reader.NodeType} {reader.Name}");
                                break;
                            case "Node":
                                Console.WriteLine($"Unprocessed {reader.NodeType} {reader.Name}");
                                break;
                            case "VisualState":
                                break; // Nothing to do
                            default:
                                Console.WriteLine($"Unexpected EndElement {reader.Name}");
                                break;
                        }
                        break;

                    default:
                        Console.WriteLine($"Unexpected NodeType {reader.NodeType}");
                        break;
                }
            }

            //GetNodeList(Nodes);

            //Console.WriteLine($"Finished reading VisualState at depth {reader.Depth}");
            //reader.ReadEndElement();

            void GetNodeList(List<VisualTreeNode> nodeList)
            {
                VisualTreeNode lastNodeRead = null;

                // We  are called when Nodes element has been read,
                // so we look for individual nodes until we see the
                // corresponding EndElement.
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            switch (reader.Name)
                            {
                                case "Nodes":
                                    if (lastNodeRead != null)
                                        GetNodeList(lastNodeRead.Nodes);
                                    break;
                                case "Node":
                                    lastNodeRead = new VisualTreeNode();

                                    lastNodeRead.Name = reader.GetAttribute("Name");
                                    lastNodeRead.Expanded = reader.GetAttribute("Expanded") == "True";
                                    lastNodeRead.Checked = reader.GetAttribute("Checked") == "True";
                                    lastNodeRead.Selected = reader.GetAttribute("Selected") == "True";
                                    lastNodeRead.IsTopNode = reader.GetAttribute("IsTopNode") == "True";

                                    nodeList.Add(lastNodeRead);

                                    break;
                            }
                            break;
                        case XmlNodeType.EndElement:
                            switch (reader.Name)
                            {
                                case "Nodes":
                                    return;
                                case "Node":
                                    break;
                            }
                            break;
                    }
                }
            }

            //VisualTreeNode GetVisualTreeNode()
            //{
            //    var node = new VisualTreeNode();

            //    node.Name = reader.GetAttribute("Name");
            //    node.Expanded = reader.GetAttribute("Expanded") == "True";
            //    node.Checked = reader.GetAttribute("Checked") == "True";
            //    node.Selected = reader.GetAttribute("Selected") == "True";
            //    node.IsTopNode = reader.GetAttribute("IsTopNode") == "True";

            //    return node;
            //}
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("DisplayStrategy", DisplayStrategy);
            if (GroupBy != null)
                writer.WriteAttributeString("GroupBy", GroupBy);
            if (ShowCheckBoxes)
                writer.WriteAttributeString("ShowCheckBoxes", "True");
            if (!ShowNamespace)
                writer.WriteAttributeString("ShowNamespace", "False");

            WriteVisualTreeNodes(Nodes);

            void WriteVisualTreeNodes(List<VisualTreeNode> nodes)
            {
                writer.WriteStartElement("Nodes");

                foreach (VisualTreeNode node in nodes)
                    WriteVisualTreeNode(node);

                writer.WriteEndElement();
            }

            void WriteVisualTreeNode(VisualTreeNode node)
            {
                writer.WriteStartElement("Node");

                writer.WriteAttributeString("Name", node.Name);
                if (node.Expanded)
                    writer.WriteAttributeString("Expanded", "True");
                if (node.Checked)
                    writer.WriteAttributeString("Checked", "True");
                if (node.Selected)
                    writer.WriteAttributeString("Selected", "True");
                if (node.IsTopNode)
                    writer.WriteAttributeString("IsTopNode", "True");

                if (node.Nodes.Count > 0)
                    WriteVisualTreeNodes(node.Nodes);

                writer.WriteEndElement();
            }
        }

        #endregion

        private static string GetName(TreeNode treeNode)
        {
            if (treeNode.Tag is TestNode testNode)
                return testNode.Name;
            else if (treeNode.Tag is TestGroup testGroup)
                return testGroup.Name;

            return treeNode.Text;
        }

        #endregion
    }

    //[Serializable]
    public class VisualTreeNode
    {
        //[XmlAttribute]
        public string Name;

        //[XmlAttribute, DefaultValue(false)]
        public bool Expanded;

        //[XmlAttribute, DefaultValue(false)]
        public bool Checked;

        //[XmlAttribute, DefaultValue(false)]
        public bool Selected;

        //[XmlAttribute, DefaultValue(false)]
        public bool IsTopNode;

        //[XmlArrayItem("Node")]
        public List<VisualTreeNode> Nodes = new List<VisualTreeNode>();

        //[XmlIgnore]
        public bool NodesSpecified => Nodes.Count > 0;

        // Provided for use in test output
        public override string ToString()
        {
            return $"Name={Name},Expanded={Expanded},Checked={Checked},Selected={Selected},IsTopNode={IsTopNode}";
        }

        public override bool Equals(object obj)
        {
            var other = obj as VisualTreeNode;

            return
                other != null &&
                Name == other.Name &&
                Expanded == other.Expanded &&
                Checked == other.Checked &&
                Selected == other.Selected &&
                IsTopNode == other.IsTopNode;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
