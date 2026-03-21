using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TodoItemsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TodoItemsController(AppDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new InvalidOperationException("User ID claim missing"));

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItemDto>>> GetAll()
    {
        var userId = GetCurrentUserId();

        var items = await _context.TodoItems
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => ToDto(t))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItemDto>> GetById(int id)
    {
        var userId = GetCurrentUserId();
        var item = await _context.TodoItems
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (item is null) return NotFound();
        return Ok(ToDto(item));
    }

    [HttpPost]
    public async Task<ActionResult<TodoItemDto>> Create(CreateTodoItemDto dto)
    {
        var userId = GetCurrentUserId();

        var item = new TodoItem
        {
            Title = dto.Title,
            Description = dto.Description,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.TodoItems.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, ToDto(item));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TodoItemDto>> Update(int id, UpdateTodoItemDto dto)
    {
        var userId = GetCurrentUserId();
        var item = await _context.TodoItems
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (item is null) return NotFound();

        item.Title = dto.Title;
        item.Description = dto.Description;
        item.IsCompleted = dto.IsCompleted;

        if (dto.IsCompleted && item.CompletedAt is null)
            item.CompletedAt = DateTime.UtcNow;
        else if (!dto.IsCompleted)
            item.CompletedAt = null;

        await _context.SaveChangesAsync();
        return Ok(ToDto(item));
    }

    [HttpPatch("{id}/toggle")]
    public async Task<ActionResult<TodoItemDto>> Toggle(int id)
    {
        var userId = GetCurrentUserId();
        var item = await _context.TodoItems
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (item is null) return NotFound();

        item.IsCompleted = !item.IsCompleted;
        item.CompletedAt = item.IsCompleted ? DateTime.UtcNow : null;

        await _context.SaveChangesAsync();
        return Ok(ToDto(item));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var item = await _context.TodoItems
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (item is null) return NotFound();

        _context.TodoItems.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static TodoItemDto ToDto(TodoItem item) => new()
    {
        Id = item.Id,
        Title = item.Title,
        Description = item.Description,
        IsCompleted = item.IsCompleted,
        CreatedAt = item.CreatedAt,
        CompletedAt = item.CompletedAt
    };
}