// WpfLogin/Models/OrganizationNode.cs
using System.Collections.ObjectModel;

namespace WpfLogin.Models
{
    public class OrganizationNode
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public ObservableCollection<OrganizationNode> Children { get; set; }

        public OrganizationNode()
        {
            Children = new ObservableCollection<OrganizationNode>();
        }
    }
}