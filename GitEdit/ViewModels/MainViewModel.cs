using LibGit2Sharp;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Input;

namespace GitEdit.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private string gitPath = "";
        public string GitPath
        {
            get { return gitPath; }
            set
            {
                gitPath = value;
                RaisePropertyChangedEvent(nameof(GitPath));
                if (GitPathIsValid())
                {
                    ChangeGitDirectory();
                }
                else
                {
                    Output = "Directory Path is not valid a valid git repository";
                }
            }
        }

        private string output = "";
        public string Output
        {
            get { return output; }
            set
            {
                output = value;
                RaisePropertyChangedEvent(nameof(Output));
            }
        }

        private Branch? activeBranch;
        public Branch? ActiveBranch
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
            GitPath = @"D:\Users\oarth\Documents\GitHub\GitEdit";
        }

        public ICommand SelectDirectoryCommand
        {
            get { return new DelegateCommand(SelectDirectory); }
        }

        private void SelectDirectory()
        {
            var a = new VistaFolderBrowserDialog();

            if (DirectoryPathIsValid())
            {
                a.SelectedPath = GitPath;
            }

            if (a.ShowDialog() == true)
            {
                GitPath = a.SelectedPath;
            }
        }

        private bool GitPathIsValid()
        {
            if (DirectoryPathIsValid())
            {
                try
                {
                    using (_ = new Repository(GitPath)) { }
                }
                catch (RepositoryNotFoundException)
                {
                    return false;
                }
            }
            return true;
        }

        private void ChangeGitDirectory()
        {
            Output = "";

            using (var repo = new Repository(GitPath))
            {
                AppendLine("Branches");
                AppendLine("-----------------");
                var branches = repo.Branches;
                foreach (var b in branches)
                {
                    ActiveBranch = b;
                    AppendLine(b.FriendlyName);
                }
                AppendLine("-----------------");
            }
        }

        private bool DirectoryPathIsValid()
        {
            return Directory.Exists(gitPath);
        }

        private void AppendLine(string content)
        {
            Output += content + Environment.NewLine;
        }
    }
}
