using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Common
{
    public abstract class BaseModel
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? CreatedByApplicationUserId { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedByApplicationUserId { get; set; }

        public DateTime? DeletedAt { get; set; }

        public int? DeletedByApplicationUserId { get; set; }
    }
}
