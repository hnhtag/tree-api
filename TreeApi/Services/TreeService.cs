using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TreeApi.Data;
using TreeApi.DTOs;
using TreeApi.Models;

namespace TreeApi.Services
{
    public class TreeService(AppDbContext context, IMapper mapper, IMemoryCache cache)
    {
        private readonly AppDbContext _context = context;
        private readonly IMapper _mapper = mapper;
        private readonly IMemoryCache _cache = cache;

        const string cacheKey = "FlatNodeList";

        public async Task<List<NodeDto>> GetRootTreeAsync()
        {
            var roots = await _context.TreeNodes.AsQueryable().AsNoTracking()
                .Where(n => n.ParentId == null)
                .Include(n => n.Children)
                .ThenInclude(c => c.Children)
                .ToListAsync();

            return [.. roots.Select(r => NodeDto.FromEntity(r, _context))];
        }

        public async Task<NodeDto?> GetSubTreeAsync(Guid parentId)
        {
            var parent = await _context.TreeNodes.AsQueryable().AsNoTracking()
                .Where(n => n.Id == parentId)
                .Include(n => n.Children)
                .ThenInclude(c => c.Children)
                .FirstOrDefaultAsync();

            return parent == null ? null : NodeDto.FromEntity(parent, _context);
        }

        public async Task<List<NodeDto>> GetFullTreeAsync()
        {
            var allNodes = await GetCachedNodesAsync() ?? throw new Exception("Err getting nodes");
            // group by ParentId
            var nodeLookup = allNodes.ToLookup(n => n.ParentId);

            List<NodeDto> BuildTree(Guid? parentId)
            {
                return [.. nodeLookup[parentId]
                    .Select(n => new NodeDto
                    {
                        Id = n.Id,
                        Name = n.Name,
                        HasChildren = nodeLookup[n.Id].Any(),
                        Children = BuildTree(n.Id)
                    })];
            }

            return BuildTree(null);
        }

        public async Task<Node?> GetByIdAsync(Guid id)
        {
            return await _context.TreeNodes.AsQueryable().AsNoTracking().Where(n => n.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Node> CreateAsync(NodeCreateDto dto)
        {
            var node = _mapper.Map<Node>(dto);

            _context.TreeNodes.Add(node);
            await _context.SaveChangesAsync();

            _cache.Remove(cacheKey);

            return node;
        }

        public async Task<bool> UpdateAsync(NodeUpdateDto dto)
        {
            var existing = await _context.TreeNodes.FindAsync(dto.Id);
            if (existing == null) return false;

            existing.Name = dto.Name;
            existing.ParentId = dto.ParentId;

            _context.TreeNodes.Update(existing);
            await _context.SaveChangesAsync();

            _cache.Remove(cacheKey);

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var node = await _context.TreeNodes
                .Include(n => n.Children)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (node == null) return false;

            DeleteNodeAndChildren(node);

            await _context.SaveChangesAsync();

            _cache.Remove(cacheKey);

            return true;
        }

        private void DeleteNodeAndChildren(Node node)
        {
            foreach (var child in node.Children.ToList())
            {
                DeleteNodeAndChildren(child);
            }
            _context.TreeNodes.Remove(node);
        }

        private async Task<List<Node>?> GetCachedNodesAsync()
        {
            if (!_cache.TryGetValue(cacheKey, out List<Node>? cachedNodes))
            {
                cachedNodes = await _context.TreeNodes.AsNoTracking().ToListAsync();

                _cache.Set(cacheKey, cachedNodes, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                });
            }

            return cachedNodes;
        }
    }
}
