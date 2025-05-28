using Microsoft.AspNetCore.Mvc;
using TreeApi.DTOs;
using TreeApi.Services;

namespace TreeApi.Controllers
{
    [ApiController]
    [Route("api/tree")]
    public class TreeController(TreeService service) : ControllerBase
    {
        private readonly TreeService _service = service;

        [HttpGet]
        public async Task<IActionResult> GetTree(Guid? parentId, bool? eager)
        {
            if (eager == true)
            {
                var fullTree = await _service.GetFullTreeAsync();
                if (fullTree == null) return NotFound();
                return Ok(fullTree);
            }

            if (parentId == null)
            {
                var tree = await _service.GetRootTreeAsync();
                return Ok(tree);
            }
            else
            {
                var subtree = await _service.GetSubTreeAsync(parentId.Value);
                if (subtree == null) return NotFound();
                return Ok(subtree);
            }
        }

        [HttpPost("node")]
        public async Task<IActionResult> Create([FromBody] NodeCreateDto node)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _service.CreateAsync(node);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpGet("node/{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var node = await _service.GetByIdAsync(id);
            if (node == null) return NotFound();
            return Ok(node);
        }

        [HttpPut("node/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] NodeUpdateDto node)
        {
            if (id != node.Id) return BadRequest("Id mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _service.UpdateAsync(node);
            if (!updated) return NotFound();

            return NoContent();
        }

        [HttpDelete("node/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
