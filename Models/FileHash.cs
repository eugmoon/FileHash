using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FileHash.Models {

    public class FileMeta {
        [Key]
        public int FileId { get; set; }
        [Display(Name = "Name")]
        [StringLength(255, MinimumLength = 1)]
        [Required]
        public string Filename { get; set; }
         [StringLength(255)]
        public string Hash { get; set; }
        [Display(Name = "Type of hash")]
        [StringLength(127)]
        [Required]
        public string HashType { get; set; }
        [StringLength(127)]
        [Required]
        public string Size { get; set; }
        [Display(Name = "Last modified (UTC)")]
        [DataType(DataType.DateTime)]
        public DateTime Last { get; set; }
    }

    public class HashViewModel {
        public int FileId { get; set; }
        public string Filename { get; set; }
        public string Hash { get; set; }
        [Display(Name = "Type of hash")]
        public string HashType { get; set; }
        public SelectList HashTypes { get; set; }
        public string Size { get; set; }
        [Display(Name = "File to upload")]
        [Required(ErrorMessage = "Please select a file.")]
        [DataType(DataType.Upload)]
        public IFormFile LocalFile { get; set; }
        public long Last { get; set; }
    }

    public class HashTypeViewModel {
        public List<FileMeta> HashList { get; set; }
        public SelectList HashTypes { get; set; }
        public string HashType { get; set; }
        public string searchString { get; set; }
    }
}