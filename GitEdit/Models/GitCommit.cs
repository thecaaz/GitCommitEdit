using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitEdit.Models
{
    public class GitCommit
    {
        public GitCommit(Commit commit)
        {
            this.Message = commit.Message;
            this.MessageShort = commit.MessageShort;
            this.Encoding = commit.Encoding;
            this.Author = new GitSignature(commit.Author);
            this.Committer = new GitSignature(commit.Committer);
            this.Id = commit.Id.Sha;
        }

        public string Message { get; } = string.Empty;
        public string MessageShort { get; } = string.Empty;
        public string Encoding { get; } = string.Empty;
        public virtual GitSignature Author { get; }
        public virtual GitSignature Committer { get; }
        public virtual string Id { get; }
    }
}
