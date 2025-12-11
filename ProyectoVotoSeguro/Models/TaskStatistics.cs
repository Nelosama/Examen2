namespace ProyectoVotoSeguro.Models
{
    public class TaskStatistics
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int HighPriorityTasks { get; set; }
        public int MediumPriorityTasks { get; set; }
        public int LowPriorityTasks { get; set; }
        public int OverdueTasks { get; set; }
        public double CompletionRate { get; set; }
        public List<TasksByUser> TasksByUsers { get; set; } = new List<TasksByUser>();
        public List<TasksByPriority> TasksByPriorities { get; set; } = new List<TasksByPriority>();
        public List<TasksByStatus> TasksByStatuses { get; set; } = new List<TasksByStatus>();
    }

    public class TasksByUser
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
    }

    public class TasksByPriority
    {
        public string Priority { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class TasksByStatus
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}