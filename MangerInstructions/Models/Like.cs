using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MangerInstructions.Models
{
    public class Like
    {
        public String Id { get; set; }

        public virtual User User { get; set; }

        public String CommentId { get; set; }

        public virtual Comment Comment { get; set; }
    }
}
