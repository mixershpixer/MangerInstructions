using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MangerInstructions.Models
{
    public class StepInstruction
    {
        public String Id { get; set; }

        public long Indexer { get; set; }

        public String Name { get; set; }

        public String Text { get; set; }

        public virtual List<String> ImageLinks { get; set; } = new List<String>();

        public String InstructionId { get; set; }
        public virtual Instruction Instruction { get; set; }
    }
}
