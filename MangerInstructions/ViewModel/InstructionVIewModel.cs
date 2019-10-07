using MangerInstructions.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MangerInstructions.ViewModel
{
    public class InstructionViewModel
    {
        public String Id { get; set; }
        public User Author { get; set; }
        [Required(ErrorMessage = "EnterNameOfInstruction")]
        public string Name { get; set; }
        [Required(ErrorMessage = "SelectCategory")]
        public string Category { get; set; }
        [MaxLength(300)]
        [Required(ErrorMessage = "EnterDescription")]
        public string ShortDescription { get; set; }
        public List<StepInstructionViewModel> Steps { get; set; } = new List<StepInstructionViewModel>();
        public string Tags { get; set; }

        public InstructionViewModel() { }
        public InstructionViewModel(Instruction instruction)
        {
            Author = instruction.Author;
            Name = instruction.Name;
            Category = instruction.CategoryIndex;
            ShortDescription = instruction.ShortDescription;
            foreach (var step in instruction.Steps.OrderBy(s => s.Indexer))
                Steps.Add(new StepInstructionViewModel { Name = step.Name, Text = step.Text, ImageLinks = step.ImageLinks });
            Tags = String.Join(',', instruction.Tags.Select(t => t.TagName));
        }
    }
}