using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MangerInstructions.Models
{
    public class Comment
    {
        public String Id { get; set; }
        public virtual User User { get; set; }
        public virtual DateTime Time { get; set; }
        public String Text { get; set; }
        public virtual List<Like> Likes { get; set; } = new List<Like>();

        public String InstructionId { get; set; }
        public virtual Instruction Instruction { get; set; }
    }
}

