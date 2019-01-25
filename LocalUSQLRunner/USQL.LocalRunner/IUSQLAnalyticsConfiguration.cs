namespace USQL.LocalRunner
{
    public interface IUSQLAnalyticsConfiguration
    {
        /// <summary>
        /// Path to the U-SQL SDK -- Update this if the nuget package is updated as the version will change!
        /// </summary>
        string ADLToolsFolderPath { get; }

        /// <summary>
        /// Path where U-SQL stores local metadata, including databases, tables, table-valued functions.
        /// Also used as input and output paths that are defined as relative paths in U-SQL.
        /// </summary>
        string USQLDataRoot { get; }

        /// <summary>
        /// Working directory for USQL (where the scripts are compiled).
        /// In addition to the compilation outputs, the needed runtime files for local execution will be shadow copied to this working directory.
        /// </summary>
        string USQLWorkDir { get; }

        /// <summary>
        /// Path to the initialization log file.
        /// </summary>
        string InitializationLogFilePath { get; }

        /// <summary>
        /// Path to the standard output log file.
        /// </summary>
        string RunLogFilePath { get; }
    }
}