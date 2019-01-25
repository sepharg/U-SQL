using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace USQL.LocalRunner
{
    public class USQLAnalyticsInitializer : IUSQLAnalyticsInitializer
    {
        private readonly IUSQLAnalyticsJobRunner _usqlAnalyticsJobRunner;

        public void InitializeUSQLDatabase(IEnumerable<IInitializationScriptBundle> initializationScriptBundles)
        {
            CreateDataRootDependencies(initializationScriptBundles);
        }

        public USQLAnalyticsInitializer(IUSQLAnalyticsJobRunner usqlAnalyticsJobRunner)
        {
            _usqlAnalyticsJobRunner = usqlAnalyticsJobRunner;
            CreateDirectoriesIfNotExist();
            CleanupOldUsqlFolderIfExist();
        }

        /// <summary>
        /// Sometimes U-SQL is a bit problematic if we try to work on existing files, so to be on the safe side we cleanup anything leftover in the directories.
        /// </summary>
        private void CleanupOldUsqlFolderIfExist()
        {
            try
            {
                var usqlWorkDirDirectories = Directory.GetDirectories(_usqlAnalyticsJobRunner.USQLAnalyticsConfiguration.USQLWorkDir);
                if (usqlWorkDirDirectories.Length > 0)
                {
                    var usqlSpecialDir = usqlWorkDirDirectories[0];

                    foreach (var rootFile in Directory.GetFiles(usqlSpecialDir))
                    {
                        AllowDeletionOfSpecialFile(rootFile);
                    }

                    // sometimes script execution fails because some cache is left here. can't delete the whole thing because of access problems, trying to delete as much as possible though
                    foreach (var directory in Directory.GetDirectories(usqlSpecialDir))
                    {
                        Directory.Delete(directory, true);
                    }

                    Directory.Delete(usqlSpecialDir, true);
                }

                // Delete U-SQL database
                var usqlDatabasePath = _usqlAnalyticsJobRunner.USQLAnalyticsConfiguration.USQLDataRoot + "\\localrunmetadata";
                AllowDeletionOfSpecialFile(usqlDatabasePath);
                if (File.Exists(usqlDatabasePath))
                {
                    File.Delete(usqlDatabasePath);
                }
            }
            catch (DirectoryNotFoundException e)
            {
                File.AppendAllText(_usqlAnalyticsJobRunner.USQLAnalyticsConfiguration.InitializationLogFilePath, "ERROR DirectoryNotFoundException" + e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                File.AppendAllText(_usqlAnalyticsJobRunner.USQLAnalyticsConfiguration.InitializationLogFilePath, "ERROR UnauthorizedAccessException" + e.Message);
            }
        }

        private void CreateDirectoriesIfNotExist()
        {
            if (!Directory.Exists(_usqlAnalyticsJobRunner.USQLAnalyticsConfiguration.USQLWorkDir))
            {
                Directory.CreateDirectory(_usqlAnalyticsJobRunner.USQLAnalyticsConfiguration.USQLWorkDir);
            }

            if (!Directory.Exists(_usqlAnalyticsJobRunner.USQLAnalyticsConfiguration.USQLDataRoot))
            {
                Directory.CreateDirectory(_usqlAnalyticsJobRunner.USQLAnalyticsConfiguration.USQLDataRoot);
            }
        }

        private void AllowDeletionOfSpecialFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
            }
        }

        protected void DeleteIfExists(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        private string CombineScriptsAsOne(params string[] scriptsPaths)
        {
            var mergedFilePath = Path.Combine(_usqlAnalyticsJobRunner.USQLAnalyticsConfiguration.USQLWorkDir, "mergedScript.usql");

            var stringBuilder = new StringBuilder();
            foreach (var scriptPath in scriptsPaths)
            {
                stringBuilder.Append(File.ReadAllText(scriptPath));
            }
            File.WriteAllText(mergedFilePath, stringBuilder.ToString());

            return mergedFilePath;
        }

        public void CreateDataRootDependencies(IEnumerable<IInitializationScriptBundle> initializationScriptsBundles)
        {
            // This needs to run at least once before running the scripts, because otherwise the USQL database won't be found
            DeleteIfExists(_usqlAnalyticsJobRunner.USQLAnalyticsConfiguration.InitializationLogFilePath);

            if (initializationScriptsBundles == null || !initializationScriptsBundles.Any())
            {
                throw new ArgumentException("InitializationScriptsBundles cannot be null or empty, at least one database has to be initialized");
            }

            foreach (var initializationScriptBundle in initializationScriptsBundles)
            {
                _usqlAnalyticsJobRunner.RunAnalyticsJob(CombineScriptsAsOne(initializationScriptBundle.ScriptsToRun), _usqlAnalyticsJobRunner.USQLAnalyticsConfiguration.InitializationLogFilePath);
            }

            DeleteIfExists(_usqlAnalyticsJobRunner.USQLAnalyticsConfiguration.RunLogFilePath);
        }
    }
}