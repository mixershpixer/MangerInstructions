using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MangerInstructions.ViewModel
{
    public class StepInstructionViewModel
    {
        [Required(ErrorMessage = "EnterNameOfStep")]
        public String Name { get; set; }
        public List<string> ImageLinks { get; set; } = new List<string>();
        public IFormFileCollection FormImages { get; set; }
        [Required(ErrorMessage = "TextRequired")]
        public String Text { get; set; }
    }
}

