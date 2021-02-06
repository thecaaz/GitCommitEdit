using LibGit2Sharp;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
                OnNotifyPropertyChanged();

                if (GitPathIsValid())
                {
                    ChangeGitDirectory();
                }
                else
                {
                    Branches.Clear();
                }
            }
        }

        public ObservableCollection<string> Branches
        {
            get
            {
                return branches;
            }
            set
            {
                branches = value;
                OnNotifyPropertyChanged();
            }
        }
        private ObservableCollection<string> branches = new ObservableCollection<string>();

        public ObservableCollection<string> Commits
        {
            get
            {
                return commits;
            }
            set
            {
                commits = value;
                OnNotifyPropertyChanged();
            }
        }
        private ObservableCollection<string> commits = new ObservableCollection<string>();

        private string? activeBranch;
        public string? ActiveBranch
        {
            get { return activeBranch; }
            set
            {
                activeBranch = value;
                OnNotifyPropertyChanged();

                using (gitRepo)
                {
                    if (gitRepo != null)
                    {
                        var branch = gitRepo.Branches.First(x => x.FriendlyName == activeBranch);

                        Commits.Clear();

                        foreach (var commit in branch.Commits)
                        {
                            Commits.Add(commit.Message);
                        }
                    }
                }
            }
        }

        private string? activeCommit;
        public string? ActiveCommit
        {
            get { return activeCommit; }
            set
            {
                activeCommit = value;
                OnNotifyPropertyChanged();
            }
        }

        private Repository? gitRepo
        {
            get
            {
                if (GitPathIsValid())
                {
                    return new Repository(GitPath);
                }
                return null;
            }
        }

        public ICommand SelectDirectoryCommand
        {
            get { return new DelegateCommand(SelectDirectory); }
        }


        public MainViewModel()
        {
            GitPath = @"D:\Users\oarth\Documents\GitHub\GitEdit";
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
            using (var repo = new Repository(GitPath))
            {
                Branches.Clear();
                Commits.Clear();

                foreach (var branch in repo.Branches)
                {
                    Branches.Add(branch.FriendlyName);
                }

                RaisePropertyChangedEvent(nameof(Branches));
            }
        }

        private bool DirectoryPathIsValid()
        {
            return Directory.Exists(gitPath);
        }
    }
}
