using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitEdit.Models
{
    public class GitSignature
    {
        public GitSignature(Signature signature)
        {
            this.Name = signature.Name;
            this.Email = signature.Email;
            this.When = signature.When;
        }

        public string Name { get; } = string.Empty;
        public string Email { get; } = string.Empty;
        public DateTimeOffset When { get; }
    }
}
