using GitEdit.Models;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
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

        #region Edit Properties
        private string activeCommitMessage = "";
        public string ActiveCommitMessage
        {
            get { return activeCommitMessage; }
            set
            {
                activeCommitMessage = value;
                OnNotifyPropertyChanged();
            }
        }
        private DateTime activeCommitAuthorDate;
        public DateTime ActiveCommitAuthorDate
        {
            get { return activeCommitAuthorDate; }
            set
            {
                activeCommitAuthorDate = value;
                OnNotifyPropertyChanged();
            }
        }
        private DateTime activeCommitCommitterDate;
        public DateTime ActiveCommitCommitterDate
        {
            get { return activeCommitCommitterDate; }
            set
            {
                activeCommitCommitterDate = value;
                OnNotifyPropertyChanged();
            }
        }
        #endregion

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

        public ObservableCollection<GitCommit> Commits
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
        private ObservableCollection<GitCommit> commits = new ObservableCollection<GitCommit>();

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
                            Commits.Add(new GitCommit(commit));
                        }
                    }
                }
            }
        }

        private GitCommit? activeCommit;
        public GitCommit? ActiveCommit
        {
            get { return activeCommit; }
            set
            {
                activeCommit = value;
                OnNotifyPropertyChanged();
                RaisePropertyChangedEvent(nameof(ActiveCommitSelected));
                ChangeCommit();
            }
        }

        public bool ActiveCommitSelected
        {
            get { return ActiveCommit != null; }
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

        public ICommand SaveCommitCommand
        {
            get { return new DelegateCommand(SaveCommit); }
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

        private void ChangeCommit()
        {
            if (ActiveCommit == null) return;

            ActiveCommitAuthorDate = ActiveCommit.Author.When.DateTime;
            ActiveCommitCommitterDate = ActiveCommit.Committer.When.DateTime;
            ActiveCommitMessage = ActiveCommit.Message;
        }

        private void SaveCommit()
        {
            using (var repo = gitRepo)
            {
                if (repo == null)
                    throw new ArgumentNullException("Git repo could not be opened");

                var branch = repo.Branches.First(x => x.FriendlyName == ActiveBranch);
                var commit = branch.Commits.First(x => x.Id.Sha == ActiveCommit!.Id);

                RemoveOriginalReferences(repo, branch);

                var randomSecondAdd = new Random().Next(0, 59);

                if (ActiveCommitAuthorDate.Second == 0)
                {
                    ActiveCommitAuthorDate = ActiveCommitAuthorDate.AddSeconds(randomSecondAdd);
                }
                if (ActiveCommitCommitterDate.Second == 0)
                {
                    ActiveCommitCommitterDate = ActiveCommitCommitterDate.AddSeconds(randomSecondAdd);
                }

                repo.Refs.RewriteHistory(new RewriteHistoryOptions
                {
                    OnError = OnError,
                    OnSucceeding = OnSucceeding,
                    CommitHeaderRewriter = (c) =>
                    {
                        return CommitRewriteInfo.From(
                            c,
                            new Signature(
                                new Identity(c.Author.Name, c.Author.Email),
                                ActiveCommitAuthorDate),
                            new Signature(
                                new Identity(c.Committer.Name, c.Committer.Email),
                                ActiveCommitCommitterDate),
                           c.Message
                                );
                    }
                }, commit);

                ActiveBranch = ActiveBranch;
            }
        }

        private static void RemoveOriginalReferences(Repository? repo, Branch branch)
        {
            string? refName = null;

            foreach (var reference in repo.Refs)
            {
                if (reference.CanonicalName.Contains("original")
                    && reference.CanonicalName.Contains(branch.FriendlyName))
                {
                    refName = reference.CanonicalName;
                }
            }

            if (refName != null)
                repo.Refs.Remove(refName);
        }

        private void OnSucceeding()
        {
        }

        private void OnError(Exception obj)
        {
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
                ActiveCommit = null;

                foreach (var branch in repo.Branches)
                {
                    if (!branch.IsRemote)
                    {
                        Branches.Add(branch.FriendlyName);
                    }
                }
            }
        }

        private bool DirectoryPathIsValid()
        {
            return Directory.Exists(gitPath);
        }
    }
}
