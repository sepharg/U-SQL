namespace USQL.LocalRunner
{
    /// <summary>
    /// Defines a set of scripts to run which initialize the U-SQL database.
    /// </summary>
    public interface IInitializationScriptBundle
    {
        /// <summary>
        /// Array of full paths that point to usql scripts to initialize a database.
        /// </summary>
        string[] ScriptsToRun { get; }
    }
}