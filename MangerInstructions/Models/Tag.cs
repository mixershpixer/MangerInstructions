using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MangerInstructions.Models
{
    public class Tag
    {
        public String Id { get; set; }
        public String TagName { get; set; }

        public String InstructionId { get; set; }
        public virtual Instruction Instruction { get; set; }
    }
}
