using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MangerInstructions.Models
{
    public class PersonalPage
    {
        public String Id { get; set; }
        public virtual List<Instruction> Instructions { get; set; } = new List<Instruction>();

        public String UserId { get; set; }
        public virtual User User { get; set; }

        public void AddInstruction(Instruction instruction)
        {
            Instructions.Add(instruction);
        }

        public void RemoveInstruction(Instruction instruction)
        {
            Instructions.Remove(instruction);
        }
    }
}