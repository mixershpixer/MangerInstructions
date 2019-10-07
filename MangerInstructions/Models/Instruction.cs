using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MangerInstructions.Models
{
    public class Instruction
    {
        //private List<StepInstruction> steps = new List<StepInstruction>();
        //private List<Comment> comments = new List<Comment>();

        public String Id { get; set; }
        public virtual User Author { get; set; }
        public String Name { get; set; }
        public String ShortDescription { get; set; }
        public virtual String CategoryIndex { get; set; }
        public DateTime DateTime { get; set; }
        public virtual List<Tag> Tags { get; set; } = new List<Tag>();
        public virtual List<Vote> Votes { get; set; } = new List<Vote>();
        public virtual List<Comment> Comments { get; set; } = new List<Comment>();
        public virtual List<StepInstruction> Steps { get; set; } = new List<StepInstruction>();

        public String PersonalPageId { get; set; }
        public virtual PersonalPage PersonalPage { get; set; }

        public void AddStep(StepInstruction step)
        {
            if (Steps.Count > 0)
                step.Indexer = Steps[Steps.Count - 1].Indexer + 1;
            else
                step.Indexer = 0;
            Steps.Add(step);
        }

        public void AddComment(Comment comment)
        {
            Comments.Add(comment);
        }

        public void RemoveComment(Comment comment)
        {
            Comments.Remove(comment);
        }

        public void RemoveLastStep()
        {
            if (Steps.Count > 0)
                Steps.RemoveAt(Steps.Count - 1);
        }

        public void RemoveVoteOfUser(String userId)
        {
            Votes.Remove(Votes.FirstOrDefault(v => v.User.Id == userId));
        }

        public double GetRating()
        {
            if (Votes.Count > 0)
                return Votes.Select(r => r.Rating).Average();
            else
                return 0;
        }
    }
}

