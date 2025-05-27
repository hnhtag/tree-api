namespace TreeApi.Models
{
    public class Node
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";

        public Guid? ParentId { get; set; }
        public Node? Parent { get; set; }
        public ICollection<Node> Children { get; set; } = [];
    }

}
