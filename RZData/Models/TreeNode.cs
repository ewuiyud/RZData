using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.Models
{
    public class TreeNode
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public TreeNode Parent { get; set; }
        public List<TreeNode> Children { get; set; }

        public TreeNode(string key, string value)
        {
            Key = key;
            Value = value;
            Children = new List<TreeNode>();
        }

        public void AddChild(TreeNode child)
        {
            child.Parent = this;
            Children.Add(child);
        }
        public void SetParent(TreeNode parent)
        {
            parent.AddChild(this);
        }
        public void RemoveParent()
        {
            Parent.Children.Remove(this);
            Parent = null;
        }

        public void RemoveChild(TreeNode child)
        {
            child.Parent = null;
            Children.Remove(child);
        }

        internal List<TreeNode> GetAllChirlds()
        {
            List<TreeNode> chirds = new List<TreeNode>();
            foreach (var item in Children)
            {
                chirds.Add(item);
                chirds.AddRange(item.GetAllChirlds());
            }
            return chirds;
        }

        public bool IsLeaf => Children.Count == 0;
        public bool IsRoot => Parent == null;
    }
}
