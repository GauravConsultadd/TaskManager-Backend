using Microsoft.VisualBasic;

namespace TaskManager.Dtos {
    public class CreateTaskDto {
        public string Title {get;set;}
        public string Description{get;set;}
        public string Status{get;set;}
        public string Priority{get;set;}
        public string DueDate{get;set;}
        public int CreatedByUserId{get;set;}
        public int? AssignedToUserId { get; set; }
    }

    public class UpdateTaskDto {
        public string? Title {get;set;}
        public string? Description{get;set;}
        public string? Status{get;set;}
        public string? Priority{get;set;}
        public string? DueDate{get;set;}
        public int? AssignedToUserId { get; set; }
    }
}