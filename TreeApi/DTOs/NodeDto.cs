namespace TreeApi.DTOs
{
    public class NodeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool HasChildren { get; set; }
        public List<NodeDto> Children { get; set; } = [];
    }
}
