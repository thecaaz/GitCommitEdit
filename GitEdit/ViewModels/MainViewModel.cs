using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitEdit.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private string output;
        public string Output
        {
            get { return output; }
            set
            {
                output = value;
                RaisePropertyChangedEvent(nameof(Output));
            }
        }

        private Branch activeBranch;
        public Branch ActiveBranch
        {
            get { return activeBranch; }
            set
            {
                activeBranch = value;
                RaisePropertyChangedEvent(nameof(ActiveBranch));
            }
        }

        public MainViewModel()
        {
            var repoPath = @"D:\Users\oarth\Documents\GitHub\GitEdit";

            using (var repo = new Repository(repoPath))
            {
                AppendLine("Branches");
                AppendLine("-----------------");
                var branches = repo.Branches;
                foreach (var b in branches)
                {
                    ActiveBranch = b;
                    AppendLine(b.FriendlyName);

                    foreach (var commit in b.Commits)
                    {
                        AppendLine(commit.ToString());
                    }
                }
                AppendLine("-----------------");
            }
        }

        private void AppendLine(string content)
        {
            Output += content + Environment.NewLine;
        }
    }
}
