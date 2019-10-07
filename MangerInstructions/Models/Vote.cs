using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MangerInstructions.Models
{
    public class Vote
    {
        public String Id { get; set; }
        public virtual User User { get; set; }
        public int Rating { get; set; }

        public String InstructionId { get; set; }
        public virtual Instruction Instruction { get; set; }
    }
}