using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoIdemController : ControllerBase
{
    private readonly AppDbContext _context;
    public TodoIdemController(AppDbContext context)
    {
        _context = context;
    }

    // ─────────────────────────────────────────
    // GET /api/todoitems
    // Returns all todo items
    // ─────────────────────────────────────────
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItemDto>>> GetAll()
    {
        var items = await _context.TodoItems
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => ToDto(t))
            .ToListAsync();

        return Ok(items);
    }


    // ─────────────────────────────────────────
    // GET /api/todoitems/{id}
    // Returns a single todo item or 404
    // ─────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItemDto>> GetById(int id)
    {
        var item = await _context.TodoItems.FindAsync(id);

        if (item is null)
            return NotFound(new { message = $"Todo item with id {id} was not found." });

        return Ok(ToDto(item));
    }

    // ─────────────────────────────────────────
    // POST /api/todoitems
    // Creates a new todo item
    // ─────────────────────────────────────────
    [HttpPost]
    public async Task<ActionResult<TodoItemDto>> Create(CreateTodoItemDto dto)
    {
        var item = new TodoItem
        {
            Title = dto.Title,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Add(item);
        await _context.SaveChangesAsync();

        // 201 Created — includes a Location header pointing to the new resource
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, ToDto(item));
    }

    // ─────────────────────────────────────────
    // PUT /api/todoitems/{id}
    // Fully updates an existing todo item
    // ─────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<ActionResult<TodoItemDto>> Update(int id, UpdateTodoItemDto dto)
    {
        var item = await _context.TodoItems.FindAsync(id);

        if (item is null)
            return NotFound(new { message = $"Todo item with id {id} was not found." });

        // Update only the fields the client is allowed to change
        item.Title = dto.Title;
        item.Description = dto.Description;
        item.IsCompleted = dto.IsCompleted;

        // Set Completed timestamp when marking as complete
        if (dto.IsCompleted && item.CompletedAt is null)
            item.CompletedAt = DateTime.UtcNow;
        else if (!dto.IsCompleted)
            item.CompletedAt = null;

        await _context.SaveChangesAsync();

        return Ok(ToDto(item));
    }

    // ─────────────────────────────────────────
    // PATCH /api/todoitems/{id}/toggle
    // Toggles the completed state
    // ─────────────────────────────────────────
    [HttpPatch("{id}/toggle")]
    public async Task<ActionResult<TodoItemDto>> Toggle(int id)
    {
        var item = await _context.TodoItems.FindAsync(id);

        if (item is null)
            return NotFound(new { message = $"Todo item with id {id} was not found." });

        item.IsCompleted = !item.IsCompleted;
        item.CompletedAt = item.IsCompleted ? DateTime.UtcNow : null;

        await _context.SaveChangesAsync();

        return Ok(ToDto(item));
    }

    // ─────────────────────────────────────────
    // DELETE /api/todoitems/{id}
    // Deletes a todo item
    // ─────────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.TodoItems.FindAsync(id);

        if (item is null)
            return NotFound(new { message = $"Todo item with id {id} was not found." });

        _context.TodoItems.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent(); // 204 — success, nothing to return
    }

    private static TodoItemDto ToDto(TodoItem item) => new()
    {
        Id = item.Id,
        Title = item.Title,
        Description = item.Description,
        IsCompleted = item.IsCompleted,
        CompletedAt = item.CompletedAt
    };
}