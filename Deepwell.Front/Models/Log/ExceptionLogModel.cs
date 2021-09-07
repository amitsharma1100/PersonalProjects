using System;
using System.ComponentModel.DataAnnotations;

namespace Deepwell.Front.Models.Log
{
    public class ExceptionLogModel
    {
        [Display(Name = "Severity")]
        public string Level { get; set; }

        public string Message { get; set; }

        [Display(Name = "Date")]
        public DateTime ExceptionDate { get; set; }

        public string Logger { get; set; }
    }
}