using System;
using System.ComponentModel.DataAnnotations;

namespace crossblog.Domain
{
    public class Article : BaseEntity
    {
        [Required]
        [StringLength(120)]
        public string Title { get; set; }

        [Required]
        [StringLength(32000)]
        public string Content { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public bool Published { get; set; }
    }
}