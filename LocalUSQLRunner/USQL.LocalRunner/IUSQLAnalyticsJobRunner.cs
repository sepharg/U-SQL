namespace USQL.LocalRunner
{
    public interface IUSQLAnalyticsJobRunner
    {
        /// <summary>
        /// Runs a U-SQL job for the given script, using the provided configuration.
        /// </summary>
        /// <param name="scriptPath">Full path to the usql script to run.</param>
        void RunAnalyticsJob(string scriptPath);

        /// <summary>
        /// Runs a U-SQL job for the given script, using the provided configuration.
        /// </summary>
        /// <param name="scriptPath">Full path to the usql script to run.</param>
        /// <param name="logFilePath">Full path to where to store logging information of the run.</param>
        void RunAnalyticsJob(string scriptPath, string logFilePath);

        /// <summary>
        /// Configuration for the runner.
        /// </summary>
        IUSQLAnalyticsConfiguration USQLAnalyticsConfiguration { get; }
    }
}