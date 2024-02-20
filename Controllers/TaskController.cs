using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Dtos;
using TaskManager.Models;

namespace TaskManager.Controller {
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController: ControllerBase {
        private readonly AppDbContext _appDbContext;
        public TaskController(AppDbContext appDbContext) {
            this._appDbContext = appDbContext;
        }

        [HttpPost("createTask")]
        [Authorize]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createTaskDto) {
            try {
                if(!ModelState.IsValid) return BadRequest("Invalid request data");

                var user = await _appDbContext.Users.FirstOrDefaultAsync((u)=> u.id == createTaskDto.CreatedByUserId);
                if(user==null) {
                    return BadRequest("Invalid user");
                }
                
                Console.WriteLine("ready for db update");
                var task = new Tasks(
                    createTaskDto.Title,
                    createTaskDto.Description,
                    createTaskDto.Status,
                    createTaskDto.Priority,
                    DateTime.Parse(createTaskDto.DueDate),
                    createTaskDto.CreatedByUserId,
                    createTaskDto.AssignedToUserId
                );

                user.CreatedTasks.Add(task);
                task.CreatedByUser = user;

                if(task.AssignedToUserID.HasValue) {
                    var assignee = await _appDbContext.Users.FirstOrDefaultAsync((u)=> u.id==task.AssignedToUserID);
                    if(assignee != null) {
                        task.AssignedToUser = assignee;
                        assignee.AssignedTasks.Add(task);
                    }
                }
                await _appDbContext.SaveChangesAsync();

                return Ok("Task Created");
            }
            catch(Exception err) {
                Console.WriteLine("error occured: "+err.Message);
                return StatusCode(500,"INTERNAL SERVER ERROR");
            }
        }

        [HttpGet("getAll")]
        [Authorize]
        public async Task<IActionResult> getAllTasks() {
            try {
                var tasks = await _appDbContext.Tasks
                    .Include(t => t.CreatedByUser)
                    .Include(t => t.AssignedToUser)
                    .ToListAsync();

                var jsonTasks = tasks.Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.Priority,
                    t.DueDate,
                    CreatedByUser = new
                    {
                        Id = t.CreatedByUser.id,
                        t.CreatedByUser.Name,
                        t.CreatedByUser.Email
                    },
                    AssignedToUser = t.AssignedToUser != null ? new
                    {
                        Id = t.AssignedToUser.id,
                        t.AssignedToUser.Name,
                        t.AssignedToUser.Email
                    } : null
                });

                return Ok(jsonTasks);
            }
            catch(Exception err) {
                Console.WriteLine("error occured: "+err.Message);
                return StatusCode(500,"INTERNAL SERVER ERROR");
            }
        }

        [Authorize]
        [HttpPut("update/{taskId}")]
        public async Task<IActionResult> UpdateTask(int taskId,[FromBody] UpdateTaskDto updateTaskDto) {
            try {
                var task = await _appDbContext.Tasks.FindAsync(taskId);

                if(task==null) {
                    return BadRequest("task not found");
                }

                task.Title = updateTaskDto.Title ?? task.Title;
                task.Description = updateTaskDto.Description ?? task.Description;
                task.Status = updateTaskDto.Status ?? task.Status;
                task.Priority = updateTaskDto.Priority ?? task.Priority;
                task.DueDate = !string.IsNullOrEmpty(updateTaskDto.DueDate) ?DateTime.Parse(updateTaskDto.DueDate) : task.DueDate;

                if (updateTaskDto.AssignedToUserId.HasValue)
                {
                    var assignedToUser = await _appDbContext.Users.FindAsync(updateTaskDto.AssignedToUserId.Value);
                    if (assignedToUser != null)
                    {
                        task.AssignedToUser = assignedToUser;
                        assignedToUser.AssignedTasks.Add(task);
                    }
                }

                _appDbContext.Tasks.Update(task);
                await _appDbContext.SaveChangesAsync();

                return Ok("updated tasks successfully");
            }
            catch(Exception err) {
                Console.WriteLine("error occured: "+err.Message);
                return StatusCode(500,"INTERNAL SERVER ERROR");
            }
        }

        [Authorize]
        [HttpDelete("delete/{taskId}")]
        public async Task<IActionResult> DeleteTask(int taskId) {
            try {
                var task = await _appDbContext.Tasks.FindAsync(taskId);

                if(task==null) {
                    return BadRequest("task not found");
                }

                _appDbContext.Tasks.Remove(task);
                await _appDbContext.SaveChangesAsync();
                return Ok("task deleted successfully");
            }
            catch(Exception err) {
                Console.WriteLine("error occured: "+err.Message);
                return StatusCode(500,"INTERNAL SERVER ERROR");
            }
        }

        [Authorize]
        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetTask(int taskId) {
            try {
                var task = await _appDbContext.Tasks
                    .Include(t => t.CreatedByUser)
                    .Include(t => t.AssignedToUser)
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if(task==null) {
                    return BadRequest("task not found");
                }
                
                var jsonTask = new {
                    task.Id,
                    task.Title,
                    task.Description,
                    task.Status,
                    task.Priority,
                    task.DueDate,
                    CreatedByUser = task.CreatedByUser != null ? new
                    {
                        Id = task.CreatedByUser.id,
                        task.CreatedByUser.Name,
                        task.CreatedByUser.Email
                    } : null,
                    AssignedToUser = task.AssignedToUser != null ? new
                    {
                        Id = task.AssignedToUser.id,
                        task.AssignedToUser.Name,
                        task.AssignedToUser.Email
                    } : null
                };

                return Ok(jsonTask);

            }
            catch(Exception err) {
                Console.WriteLine("error occured: "+err.Message);
                return StatusCode(500,"INTERNAL SERVER ERROR");
            }
        }
    }
}