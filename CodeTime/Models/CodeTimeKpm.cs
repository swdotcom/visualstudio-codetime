using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CodeTime
{
    public class CodeTimeKpm
    {
        public string type { get; set; }
        // sublime = 1, vs code = 2, eclipse = 3, intellij = 4, visualstudio = 6, atom = 7
        public int pluginId { get; set; }
        public string version { get; set; }
        public string os { get; set; }

        // a unique list of file infos (each info represents a file and its metadata)
        public List<CodeTimeKpmMetrics> source;
        public long keystrokes { get; set; }

        // start and end are in seconds
        public long start { get; set; }
        public long local_start { get; set; }
        public long end { get; set; }
        public long local_end { get; set; }
        public string timezone { get; set; }
        public double offset { get; set; }
        public long elapsed_seconds { get; set; }
        public string project_null_error { get; set; }
        public string workspace_name { get; set; }
        public string hostname { get; set; }
        public CodeTimeProject project { get; set; }

        public CodeTimeKpm(string projectName, string projectDirectory)
        {
            this.type = "Events";
            this.pluginId = EnvUtil.getPluginId();
            NowTime nowTime = TimeUtil.GetNowTime();
            start = nowTime.now;
            local_start = nowTime.local_now;
            offset = nowTime.offset_minutes;
            version = EnvUtil.GetVersion();
            os = EnvUtil.GetOs();
            source = new List<CodeTimeKpmMetrics>();
            project = GetPluginProjectUsingDir(projectDirectory);
            elapsed_seconds = 0;
            project_null_error = "";
            workspace_name = "";
            hostname = "";
        }

        public static async Task<CodeTimeProject> GetPluginProject()
        {
            string projectDir = await PackageManager.GetSolutionDirectory();
            return GetPluginProjectUsingDir(projectDir);
        }

        public static CodeTimeProject GetPluginProjectUsingDir(string projectDir)
        {
            string name = "Unnamed";
            CodeTimeProject project;
            if (projectDir != null && !projectDir.Equals(""))
            {
                FileInfo fi = new FileInfo(projectDir);
                name = fi.Name;
                project = new CodeTimeProject(name, projectDir);
            }
            else
            {
                project = new CodeTimeProject(name, "Untitled");
            }

            return project;
        }

        public async Task<string> CompletePayloadAndReturnJsonString()
        {
            RepoResourceInfo resourceInfo = null;
            // make sure we have a valid project and identifier if possible
            if (this.project == null || this.project.directory == null || this.project.directory.Equals("Untitled"))
            {
                // try to get a valid project
                string projectDir = await PackageManager.GetSolutionDirectory();
                if (projectDir != null && !projectDir.Equals(""))
                {
                    FileInfo fi = new FileInfo(projectDir);
                    project = new CodeTimeProject(fi.Name, projectDir);
                    resourceInfo = GitUtilManager.GetResourceInfo(projectDir, false);
                }
            }
            else
            {
                resourceInfo = GitUtilManager.GetResourceInfo(this.project.directory, false);
            }

            if (resourceInfo != null && resourceInfo.identifier != null && !resourceInfo.identifier.Equals(""))
            {
                project.identifier = resourceInfo.identifier;
            }

            NowTime nowTime = TimeUtil.GetNowTime();
            this.end = nowTime.now;
            this.local_end = nowTime.local_now;

            // make sure all of the end times are set
            foreach (CodeTimeKpmMetrics pdFileInfo in this.source)
            {
                pdFileInfo.EndFileInfoTime(nowTime);
            }

            double offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes;
            this.offset = Math.Abs((int)offset);
            if (TimeZone.CurrentTimeZone.DaylightName != null
                && TimeZone.CurrentTimeZone.DaylightName != TimeZone.CurrentTimeZone.StandardName)
            {
                this.timezone = TimeZone.CurrentTimeZone.DaylightName;
            }
            else
            {
                this.timezone = TimeZone.CurrentTimeZone.StandardName;
            }

            // create the json payload
            JObject jsonObj = new JObject();
            jsonObj.Add("start", this.start);
            jsonObj.Add("local_start", this.local_start);
            jsonObj.Add("pluginId", this.pluginId);
            jsonObj.Add("type", this.type);
            jsonObj.Add("keystrokes", this.keystrokes);
            jsonObj.Add("project", this.project.GetAsJson());
            jsonObj.Add("timezone", this.timezone);
            jsonObj.Add("offset", this.offset);
            jsonObj.Add("version", this.version);
            jsonObj.Add("os", this.os);
            jsonObj.Add("end", this.end);
            jsonObj.Add("local_end", this.local_end);
            jsonObj.Add("elapsed_seconds", this.elapsed_seconds);
            jsonObj.Add("workspace_name", this.workspace_name);
            jsonObj.Add("hostname", this.hostname);
            jsonObj.Add("project_null_error", this.project_null_error);

            // get the source as json
            jsonObj.Add("source", BuildSourceJson());

            return jsonObj.ToString();
        }

        private JObject BuildSourceJson()
        {
            JObject sourceData = new JObject();
            foreach (CodeTimeKpmMetrics fileInfo in source)
            {
                sourceData.Add(fileInfo.file, fileInfo.GetPluginDataFileInfoAsJson());
            }
            return sourceData;
        }

        public CodeTimeKpmMetrics GetFileInfo(string file)
        {
            for (int i = 0; i < source.Count; i++)
            {
                CodeTimeKpmMetrics fileInfo = source[i];
                if (fileInfo.file.Equals(file))
                {
                    return fileInfo;
                }
            }
            return null;
        }

        public void InitFileInfoIfNotExists(string file)
        {
            if (GetFileInfo(file) == null)
            {
                CodeTimeKpmMetrics fileInfo = new CodeTimeKpmMetrics(file);
                fileInfo.lines = DocEventManager.CountLinesLINQ(file);
                fileInfo.length = new FileInfo(file).Length;
                source.Add(fileInfo);
            }
        }

        public List<FileInfoSummary> GetSourceFileInfoList()
        {
            List<FileInfoSummary> fileInfoList = new List<FileInfoSummary>();

            foreach (CodeTimeKpmMetrics pdFileInfo in source)
            {
                // go through the properties of this and check if any have data
                // close, open, paste, delete, keys
                FileInfoSummary fileInfo = new FileInfoSummary();
                fileInfo.close = pdFileInfo.close;
                fileInfo.open = pdFileInfo.open;
                fileInfo.paste = pdFileInfo.paste;
                fileInfo.linesAdded = pdFileInfo.linesAdded;
                fileInfo.linesRemoved = pdFileInfo.linesRemoved;
                fileInfo.delete = pdFileInfo.delete;
                fileInfo.add = pdFileInfo.add;
                fileInfo.keystrokes = pdFileInfo.keystrokes;
                fileInfo.syntax = pdFileInfo.syntax;
                fileInfo.local_start = pdFileInfo.local_start;
                fileInfo.local_end = pdFileInfo.local_end;
                fileInfo.start = pdFileInfo.start;
                fileInfo.end = pdFileInfo.end;

                // wrapper for a file path
                FileInfo fi = new FileInfo(pdFileInfo.file);
                fileInfo.name = fi.Name;
                fileInfo.fsPath = fi.FullName;
                fileInfo.duration_seconds = fileInfo.end - fileInfo.start;

                fileInfoList.Add(fileInfo);
            }

            return fileInfoList;
        }
    }
}
