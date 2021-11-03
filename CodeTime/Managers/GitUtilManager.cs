using System;
using System.IO;

namespace CodeTime
{
    public sealed class GitUtilManager
    {

        public static RepoResourceInfo GetResourceInfo(string projectDir)
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
