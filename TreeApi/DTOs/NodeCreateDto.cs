namespace TreeApi.DTOs
{
    public class NodeCreateDto
    {
        public string Name { get; set; } = string.Empty;

        public Guid? ParentId { get; set; }
    }
}
