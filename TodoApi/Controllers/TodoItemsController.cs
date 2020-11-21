using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Auth.AuthenticationConfig;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _context;
        private readonly IConfiguration _config;
        private readonly Authentication _authentication;
        private readonly ILogger<TodoItemsController> _logger;

        public TodoItemsController(TodoContext context, IWebHostEnvironment env,
            Authentication authentication,
            ILogger<TodoItemsController> logger)
        {
            _context = context;
            _config = Utils.Environment.GetConfiguration(env);
            _logger = logger;
            _authentication = authentication;
        }


        [Route("healthz")]
        [HttpGet]
        public string Healthz()
        {
            return "TodoItems active";
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
        {
            var userID = Request.HttpContext.Items["x-userID"]?.ToString();
            if (userID == null) { return Unauthorized(); }
            return await _context.TodoItems
                .Select(x => ItemToDo(x))
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(long id)
        {
            var userID = Request.HttpContext.Items["x-userID"]?.ToString();
            if (userID == null) { return Unauthorized(); }
            var todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return ItemToDo(todoItem);
            //return null;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodoItem(long id, TodoItemDTO todoItemDTO)
        {
            var userID = Request.HttpContext.Items["x-userID"]?.ToString();
            if (userID == null) { return Unauthorized(); }

            if (id != todoItemDTO.Id)
            {
                return BadRequest();
            }

            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            todoItem.Name = todoItemDTO.Name;
            todoItem.IsComplete = todoItemDTO.IsComplete;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!TodoItemExists(id))
            {
                return NotFound();
            }

            return Ok("Update success!!!");
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] UserDTO userLogin)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                if(userLogin.UserName != "bvminh" || userLogin.Password != "password") return BadRequest("Login fail");

                var token = _authentication.GenerateJwtToken(userLogin.UserName, userLogin.Password);
                if (token == null) return Unauthorized();
                return Ok(token);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
                return BadRequest($"{ex}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TodoItemDTO>> CreateTodoItem(TodoItemDTO todoItemDTO)
        {
            var userID = Request.HttpContext.Items["x-userID"]?.ToString();
            if (userID == null) { return Unauthorized(); }

            var todoItem = new TodoItem
            {
                IsComplete = todoItemDTO.IsComplete,
                Name = todoItemDTO.Name,
                Secret = "123"
            };

            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetTodoItem),
                new { id = todoItem.Id },
                ItemToDo(todoItem));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(long id)
        {
            var userID = Request.HttpContext.Items["x-userID"]?.ToString();
            if (userID == null) { return Unauthorized(); }

            var todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();

            return Ok("Delete success!!!");
        }

        private bool TodoItemExists(long id) =>
             _context.TodoItems.Any(e => e.Id == id);

        private static TodoItem ItemToDo(TodoItem todoItem) =>
            new TodoItem
            {
                Id = todoItem.Id,
                Name = todoItem.Name,
                IsComplete = todoItem.IsComplete,
                Secret = todoItem.Secret
            };
    }
}

