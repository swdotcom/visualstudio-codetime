using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CodeTime
{
    internal class ExecUtil
    {
        public static string GetFirstCommandResult(string cmd, string dir)
        {
            List<string> result = RunCommand(cmd, dir);
            string firstResult = result != null && result.Count > 0 ? result[0] : null;
            return firstResult;
        }

        public static List<string> RunCommand(string cmd, string dir)
        {
            List<string> result = CacheManager.GetCmdResultCachedValue(dir, cmd);
            if (result != null)
            {
                return result;
            }
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c " + cmd;
                if (dir != null)
                {
                    process.StartInfo.WorkingDirectory = dir;
                }
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();

                //* Read the output (or the error)
                List<string> output = new List<string>();

                while (process.StandardOutput.Peek() > -1)
                {
                    output.Add(process.StandardOutput.ReadLine().TrimEnd());
                }

                while (process.StandardError.Peek() > -1)
                {
                    output.Add(process.StandardError.ReadLine().TrimEnd());
                }
                process.WaitForExit();

                // all of the callers are expecting a 1 line response. return the 1st line
                if (output.Count > 0)
                {
                    CacheManager.UpdateCmdResult(dir, cmd, output);
                    return output;
                }
            }
            catch (Exception e)
            {
                LogManager.Error("Code Time: Unable to execute command, error: " + e.Message);
            }
            return null;
        }

        public static List<string> GetCommandResultList(string cmd, string dir)
        {
            List<string> resultList = RunCommand(cmd, dir);
            return resultList;
        }
    }
}
