using System;
using System.ComponentModel.DataAnnotations;

namespace crossblog.Model
{
    public class ArticleModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(120)]
        public string Title { get; set; }

        [Required]
        [MaxLength(32000)]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        public bool Published { get; set; }
    }
}