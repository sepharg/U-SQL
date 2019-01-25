using System;
using System.Diagnostics;
using System.IO;

namespace USQL.LocalRunner
{
    /// <summary>
    /// Base class that provides functionality to run U-SQL Analytics jobs locally.
    /// </summary>
    public class USQLAnalyticsJobRunner : IUSQLAnalyticsJobRunner
    {
        protected readonly IUSQLAnalyticsConfiguration _usqlAnalyticsConfiguration;
        private readonly object lockObject = new object();

        public IUSQLAnalyticsConfiguration USQLAnalyticsConfiguration => _usqlAnalyticsConfiguration;

        public USQLAnalyticsJobRunner(IUSQLAnalyticsConfiguration _usqlAnalyticsConfiguration)
        {
            this._usqlAnalyticsConfiguration = _usqlAnalyticsConfiguration;
        }

        /// <summary>
        /// Runs a U-SQL job for the given script, using the provided configuration.
        /// </summary>
        /// <param name="scriptPath"></param>
        public void RunAnalyticsJob(string scriptPath)
        {
            RunAnalyticsJob(scriptPath, null);
        }

        public void RunAnalyticsJob(string scriptPath, string logFilePath)
        {
            if (!File.Exists(scriptPath))
                throw new FileNotFoundException($"Couldn't find {scriptPath}");
            if (string.IsNullOrEmpty(logFilePath))
                logFilePath = _usqlAnalyticsConfiguration.RunLogFilePath;

            var processStartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(_usqlAnalyticsConfiguration.ADLToolsFolderPath, "LocalRunHelper.exe"),
                Arguments = $"run -Script \"{scriptPath}\" -DataRoot \"{_usqlAnalyticsConfiguration.USQLDataRoot}\" -Verbose -WorkDir \"{_usqlAnalyticsConfiguration.USQLWorkDir}\"",
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            string stdOut;
            using (var exeProcess = new Process())
            {
                exeProcess.StartInfo = processStartInfo;
                exeProcess.EnableRaisingEvents = true;
                try
                {
                    exeProcess.Start();
                }
                catch (Exception e)
                {
                    throw new Exception($"Couldn´t start Analytics Process {processStartInfo.FileName} with the following arguments {processStartInfo.Arguments}", e);
                }

                stdOut = $"RUN SCRIPT {scriptPath} LOG START" + Environment.NewLine + exeProcess.StandardOutput.ReadToEnd() + Environment.NewLine + $"RUN SCRIPT {scriptPath} LOG END" + Environment.NewLine;

                exeProcess.WaitForExit();
            }

            lock (lockObject)
            {
                File.AppendAllText(logFilePath, stdOut);
            }
        }
    }
}
