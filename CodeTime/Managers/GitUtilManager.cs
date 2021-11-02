﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CodeTime
{
    public sealed class GitUtilManager
    {

        public static CommitChangeStats GetUncommitedChanges(string projectDir)
        {
            if (!IsGitProject(projectDir))
            {
                return new CommitChangeStats();
            }
            string cmd = "git diff --stat";

            return GetChangeStats(cmd, projectDir); ;
        }

        public static CommitChangeStats GetTodaysCommits(string projectDir, string email)
        {
            if (!IsGitProject(projectDir))
            {
                return new CommitChangeStats();
            }
            return GetCommitsForRange("today", projectDir, email);
        }

        public static CommitChangeStats GetYesterdayCommits(string projectDir, string email)
        {
            if (!IsGitProject(projectDir))
            {
                return new CommitChangeStats();
            }
            return GetCommitsForRange("yesterday", projectDir, email);
        }

        public static CommitChangeStats GetThisWeeksCommits(string projectDir, string email)
        {
            if (!IsGitProject(projectDir))
            {
                return new CommitChangeStats();
            }
            return GetCommitsForRange("thisWeek", projectDir, email);
        }

        public static CommitChangeStats GetCommitsForRange(string rangeType, string projectDir, string email)
        {
            if (!IsGitProject(projectDir))
            {
                return new CommitChangeStats();
            }
            NowTime nowTime = TimeUtil.GetNowTime();
            string sinceTime = nowTime.start_of_today.ToString("yyyy-MM-ddTHH:mm:sszzz");
            string untilTime = null;
            if (rangeType == "yesterday")
            {
                sinceTime = nowTime.start_of_yesterday_dt.ToString("yyyy-MM-ddTHH:mm:sszzz");
                untilTime = nowTime.start_of_today.ToString("yyyy-MM-ddTHH:mm:sszzz");
            }
            else if (rangeType == "thisWeek")
            {
                sinceTime = nowTime.start_of_week_dt.ToString("yyyy-MM-ddTHH:mm:sszzz");
            }

            string cmd = "git log --stat --pretty=\"COMMIT:% H,% ct,% cI,% s\" --since=\"" + sinceTime + "\"";
            if (untilTime != null)
            {
                cmd += " --until=\"" + untilTime + "\"";
            }
            if (email != null && !email.Equals(""))
            {
                cmd += " --author=" + email;
            }
            return GetChangeStats(cmd, projectDir);
        }

        private static CommitChangeStats GetChangeStats(string cmd, string projectDir)
        {
            if (!IsGitProject(projectDir))
            {
                return new CommitChangeStats();
            }
            CommitChangeStats stats = new CommitChangeStats();

            /**
	         * example:
             * -mbp-2:swdc-vscode xavierluiz$ git diff --stat
                lib/KpmProviderManager.ts | 22 ++++++++++++++++++++--
                1 file changed, 20 insertions(+), 2 deletions(-)

                for multiple files it will look like this...
                7 files changed, 137 insertions(+), 55 deletions(-)
             */
            List<string> results = ExecUtil.GetCommandResultList(cmd, projectDir);

            if (results == null || results.Count == 0)
            {
                // something went wrong, but don't try to parse a null or undefined str
                return stats;
            }

            // just look for the line with "insertions" and "deletions"
            return AccumulateChangeStats(results);
        }

        public static CommitChangeStats AccumulateChangeStats(List<string> results)
        {
            CommitChangeStats stats = new CommitChangeStats();

            if (results != null)
            {
                foreach (string line in results)
                {
                    string lineData = line.Trim();
                    lineData = Regex.Replace(lineData, @"\s+", " ");
                    // look for lines with insertion and deletion
                    if (lineData.IndexOf("changed") != -1 &&
                        (lineData.IndexOf("insertion") != -1 || lineData.IndexOf("deletion") != -1))
                    {
                        string[] parts = lineData.Split(' ');
                        // the 1st element is the number of files changed
                        int fileCount = int.Parse(parts[0]);
                        stats.fileCount += fileCount;
                        stats.commitCount += 1;
                        for (int x = 0; x < parts.Count(); x++)
                        {
                            string part = parts[x];
                            if (part.IndexOf("insertion") != -1)
                            {
                                int insertions = int.Parse(parts[x - 1]);
                                stats.insertions += insertions;
                            }
                            else if (part.IndexOf("deletion") != -1)
                            {
                                int deletions = int.Parse(parts[x - 1]);
                                stats.deletions += deletions;
                            }
                        }
                    }
                }
            }

            return stats;
        }

        public static RepoResourceInfo GetResourceInfo(string projectDir, bool includeMembers)
        {
            if (!IsGitProject(projectDir))
            {
                return new RepoResourceInfo();
            }
            RepoResourceInfo info = new RepoResourceInfo();
            try
            {
                string identifier = ExecUtil.GetFirstCommandResult("git config remote.origin.url", projectDir);
                if (identifier != null && !identifier.Equals(""))
                {
                    info.identifier = identifier;

                    // only get these since identifier is available
                    string email = ExecUtil.GetFirstCommandResult("git config user.email", projectDir);
                    if (email != null && !email.Equals(""))
                    {
                        info.email = email;

                    }
                    string branch = ExecUtil.GetFirstCommandResult("git symbolic-ref --short HEAD", projectDir);
                    if (branch != null && !branch.Equals(""))
                    {
                        info.branch = branch;
                    }
                    string tag = ExecUtil.GetFirstCommandResult("git describe --all", projectDir);

                    if (tag != null && !tag.Equals(""))
                    {
                        info.tag = tag;
                    }

                    if (includeMembers)
                    {
                        List<RepoMember> repoMembers = new List<RepoMember>();
                        string gitLogData = ExecUtil.GetFirstCommandResult("git log --pretty=%an,%ae | sort", projectDir);

                        IDictionary<string, string> memberMap = new Dictionary<string, string>();

                        if (gitLogData != null && !gitLogData.Equals(""))
                        {
                            string[] lines = gitLogData.Split(
                                new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                            if (lines != null && lines.Length > 0)
                            {
                                for (int i = 0; i < lines.Length; i++)
                                {
                                    string line = lines[i];
                                    string[] memberInfos = line.Split(',');
                                    if (memberInfos != null && memberInfos.Length > 1)
                                    {
                                        string name = memberInfos[0].Trim();
                                        string memberEmail = memberInfos[1].Trim();
                                        if (!memberMap.ContainsKey(email))
                                        {
                                            memberMap.Add(email, name);
                                            repoMembers.Add(new RepoMember(name, email));
                                        }
                                    }
                                }
                            }
                        }
                        info.members = repoMembers;
                    }
                }

            }
            catch (Exception ex)
            {
                LogManager.Error("GetResourceInfo , error :" + ex.Message, ex);

            }
            return info;
        }

        public static string GetUsersEmail(string projectDir)
        {
            if (!IsGitProject(projectDir))
            {
                return "";
            }
            return ExecUtil.GetFirstCommandResult("git config user.email", projectDir);
        }

        public static string GetRepoUrlLink(string projectDir)
        {
            if (!IsGitProject(projectDir))
            {
                return "";
            }
            string repoUrl = ExecUtil.GetFirstCommandResult("git config --get remote.origin.url", projectDir);
            if (repoUrl != null)
            {
                repoUrl = repoUrl.Substring(0, repoUrl.LastIndexOf(".git"));
            }
            return repoUrl;
        }

        public static CommitInfo GetLastCommitInfo(string projectDir, string email)
        {
            if (!IsGitProject(projectDir))
            {
                return new CommitInfo();
            }
            CommitInfo commitInfo = new CommitInfo();

            string authorArg = (email != null) ? " --author=" + email + " " : " ";
            string cmd = "git log --pretty=%H,%s" + authorArg + "--max-count=1";

            List<string> results = ExecUtil.GetCommandResultList(cmd, projectDir);

            if (results != null && results.Count > 0)
            {
                string[] parts = results[0].Split(',');
                if (parts != null && parts.Length == 2)
                {
                    commitInfo.commitId = parts[0];
                    commitInfo.comment = parts[1];
                    commitInfo.email = email;
                }
            }

            return commitInfo;
        }

        public static bool IsGitProject(string projDir)
        {
            if (projDir == null || projDir.Equals(""))
            {
                return false;
            }
            string gitDir = projDir + "\\.git";
            bool hasGitDir = Directory.Exists(gitDir);
            return hasGitDir;
        }
    }
}
