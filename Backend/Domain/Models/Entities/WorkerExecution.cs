using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Entities
{
    public class WorkerExecution
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string WorkerName { get; set; } = string.Empty;

        [Required]
        public DateTime ExecutionDate { get; set; }

        [Required]
        public DateTime LastExecutedAt { get; set; }

        public int DocumentsProcessed { get; set; }

        public int ErrorsCount { get; set; }

        public bool IsSuccess { get; set; }

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }
    }
}
