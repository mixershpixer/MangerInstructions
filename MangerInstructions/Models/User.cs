using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MangerInstructions.Models
{
    public enum Role
    {
        Admin,
        User
    }

    public class User
    {
        public String Id { get; set; }
        public String Name { get; set; }
        public String Email { get; set; }
        public String Password { get; set; }
        public bool IsBlock { get; set; } = false;
        public virtual PersonalPage PersonalPage { get; set; }
        public Role Role { get; set; } = Role.User;

        public virtual List<Vote> Votes { get; set; }
        public virtual List<Like> Likes { get; set; }
        public virtual List<Comment> Comments { get; set; }
    }
}
