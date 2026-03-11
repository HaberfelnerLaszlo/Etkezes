using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Etkezes_Models
{
    public class SyncData
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime SyncDate { get; set; }  = DateTime.UtcNow;
        [Required]
        public SyncType Type { get; set; } = SyncType.Up;
        public bool IsSuccess { get; set; } = false;
        [Required]
        public string Table { get; set; } = "User";
         public string? Description { get; set; }
    }
    public enum SyncType
    {
        Up,
        Down
    }
}
