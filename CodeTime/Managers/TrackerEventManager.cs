﻿using System;
using System.Threading.Tasks;

namespace CodeTime
{
    class TrackerEventManager
    {
        private static TrackerManager tracker = null;

        public static void init()
        {
            if (tracker != null)
            {
                return;
            }

            try
            {
                tracker = new TrackerManager("CodeTime", "visualstudio-codetime");
                tracker.initializeTracker().ContinueWith(result =>
                {
                    _ = TrackEditorActionEvent("editor", "activate");
                });
            }
            catch (Exception e)
            {
                LogManager.Warning("Error initializing tracker: " + e.ToString());
            }
        }

        public static async Task TrackCodeTimeEventAsync(CodeTimeKpm pluginData)
        {
            if (pluginData == null)
            {
                return;
            }

            if (tracker == null || !tracker.initialized)
            {
                return;
            }
            
            var authEntity = GetAuthEntity();
            if (authEntity == null)
            {
                return;
            }

            try
            {
                PluginEntity pluginEntity = GetPluginEntity();

                RepoEntity repoEntity = null;
                foreach (CodeTimeKpmMetrics fileInfo in pluginData.source)
                {
                    CodetimeEvent codetimeEvent = new CodetimeEvent();
                    codetimeEvent.keystrokes = fileInfo.keystrokes;
                    codetimeEvent.lines_added = fileInfo.linesAdded;
                    codetimeEvent.lines_deleted = fileInfo.linesRemoved;
                    codetimeEvent.end_time = TimeUtil.ToRfc3339String(fileInfo.end);
                    codetimeEvent.start_time = TimeUtil.ToRfc3339String(fileInfo.start);
                    codetimeEvent.characters_added = fileInfo.characters_added;
                    codetimeEvent.characters_deleted = fileInfo.characters_deleted;
                    codetimeEvent.single_adds = fileInfo.single_adds;
                    codetimeEvent.multi_adds = fileInfo.multi_adds;
                    codetimeEvent.single_deletes = fileInfo.single_deletes;
                    codetimeEvent.multi_deletes = fileInfo.multi_deletes;
                    codetimeEvent.auto_indents = fileInfo.auto_indents;
                    codetimeEvent.replacements = fileInfo.replacements;
                    codetimeEvent.is_net_change = fileInfo.is_net_change;

                    // entities
                    codetimeEvent.pluginEntity = pluginEntity;
                    codetimeEvent.authEntity = authEntity;

                    codetimeEvent.fileEntity = await GetFileEntity(fileInfo.file);
                    codetimeEvent.projectEntity = await GetProjectEntity(fileInfo.file);


                    if (repoEntity == null)
                    {
                        repoEntity = GetRepoEntity(codetimeEvent.projectEntity.project_directory);
                    }

                    codetimeEvent.repoEntity = repoEntity;

                    tracker.TrackCodetimeEvent(codetimeEvent);
                }
            }
            catch (Exception e)
            {
                LogManager.Error("Unable to process code time information", e);
            }
        }

        public static async Task TrackEditorActionEvent(string entity, string type)
        {
            TrackEditorFileActionEvent(entity, type, null);
        }

        public static async Task TrackEditorFileActionEvent(string entity, string type, string fileName)
        {
            if (tracker == null || !tracker.initialized)
            {
                return;
            }
            
            var authEntity = GetAuthEntity();
            if (authEntity == null)
            {
                return;
            }

            EditorActionEvent editorActionEvent = new EditorActionEvent();
            editorActionEvent.entity = entity;
            editorActionEvent.type = type;

            // entities
            editorActionEvent.pluginEntity = GetPluginEntity();
            editorActionEvent.authEntity = authEntity;

            editorActionEvent.fileEntity = await GetFileEntity(fileName);
            editorActionEvent.projectEntity = await GetProjectEntity(fileName);

            editorActionEvent.repoEntity = GetRepoEntity(editorActionEvent.projectEntity.project_directory);

            tracker.TrackEditorActionEvent(editorActionEvent);
        }

        public static async Task TrackUIInteractionEvent(UIInteractionType interaction_type, UIElementEntity uIElementEntity)
        {
            if (tracker == null || !tracker.initialized)
            {
                return;
            }
            var authEntity = GetAuthEntity();
            if (authEntity == null)
            {
                return;
            }

            UIInteractionEvent uIInteractionEvent = new UIInteractionEvent();
            uIInteractionEvent.interaction_type = interaction_type;
            uIInteractionEvent.uiElementEntity = uIElementEntity;

            // entities
            uIInteractionEvent.pluginEntity = GetPluginEntity();
            uIInteractionEvent.authEntity = authEntity;

            tracker.TrackUIInteractionEvent(uIInteractionEvent);
        }

        private static AuthEntity GetAuthEntity()
        {
            string jwt = FileManager.getItemAsString("jwt");
            jwt = !string.IsNullOrEmpty(jwt) && jwt.StartsWith("JWT ") ? jwt.Substring("JWT ".Length) : jwt;
            if (String.IsNullOrEmpty(jwt))
            {
                return null;
            } else
            {
                return new AuthEntity  { jwt = jwt };
            }
        }

        private static PluginEntity GetPluginEntity()
        {
            PluginEntity pluginEntity = new PluginEntity();
            pluginEntity.plugin_version = EnvUtil.GetVersion();
            pluginEntity.plugin_id = EnvUtil.getPluginId();
            pluginEntity.plugin_name = EnvUtil.getPluginName();
            return pluginEntity;
        }

        public static async Task<ProjectEntity> GetProjectEntity(string fileName)
        {
            FileDetails fd = await FileInfoManager.GetFileDatails(fileName);
            ProjectEntity projectEntity = new ProjectEntity();
            projectEntity.project_directory = fd.project_directory;
            projectEntity.project_name = fd.project_name;
            return projectEntity;
        }

        public static RepoEntity GetRepoEntity(string projectDir)
        {
            RepoResourceInfo info = GitUtilManager.GetResourceInfo(projectDir);
            RepoEntity repoEntity = new RepoEntity();
            repoEntity.git_branch = info.branch;
            repoEntity.git_tag = info.tag;
            repoEntity.owner_id = info.ownerId;
            repoEntity.repo_identifier = info.identifier;
            repoEntity.repo_name = info.repoName;
            return repoEntity;
        }

        public static async Task<FileEntity> GetFileEntity(string fileName)
        {
            FileDetails fd = await FileInfoManager.GetFileDatails(fileName);
            FileEntity fileEntity = new FileEntity();
            // standardize the project file name
            string projectFileName = fd.project_file_name.Replace(@"\", @"/");
            fileEntity.file_name = projectFileName;
            fileEntity.file_path = fd.full_file_name;
            fileEntity.character_count = fd.character_count;
            fileEntity.line_count = fd.line_count;
            fileEntity.syntax = fd.syntax;

            return fileEntity;
        }
    }
}
