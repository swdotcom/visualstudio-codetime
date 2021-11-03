using System;
using System.Collections.Generic;
using System.IO;

namespace CodeTime
{
    public sealed class GitUtilManager
    {

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
