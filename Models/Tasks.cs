using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Models {
    public class Tasks {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id{get;set;}

        public string? Title{get;set;}
        public string? Description{get;set;}
        public string? Status{get;set;}
        public string? Priority{get;set;}
        public DateTime DueDate{get;set;}
        public int CreatedByUserID {get;set;}
        public int? AssignedToUserID{get;set;}
        public User? CreatedByUser {get;set;}
        public User? AssignedToUser {get;set;}

        public Tasks(string title, string description, string status, string priority, DateTime dueDate, int createdByUserId, int? assignedToUserId)
        {
            Title = title;
            Description = description;
            Status = status;
            Priority = priority;
            DueDate = dueDate;
            CreatedByUserID = createdByUserId;
            AssignedToUserID = assignedToUserId;
        }

        public Tasks() {}
    }
}