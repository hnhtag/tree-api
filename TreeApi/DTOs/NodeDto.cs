using TreeApi.Data;
using TreeApi.Models;

namespace TreeApi.DTOs
{
    public class NodeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool HasChildren { get; set; }
        public List<NodeDto> Children { get; set; } = [];

        public static NodeDto FromEntity(Node node, AppDbContext context)
        {
            return new NodeDto
            {
                Id = node.Id,
                Name = node.Name,
                HasChildren = node.Children.Count != 0,
                Children = [.. node.Children.Select(child => new NodeDto
                {
                    Id = child.Id,
                    Name = child.Name,
                    HasChildren = child.Children.Count != 0,
                    Children = [.. child.Children.Select(grandchild => new NodeDto
                    {
                        Id = grandchild.Id,
                        Name = grandchild.Name,
                        HasChildren = context.TreeNodes.Any(c => c.ParentId == grandchild.Id),
                        Children = []
                    })]
                })]
            };
        }
    }
}
