using System;
using System.IO;
using Xunit;

namespace USQL.LocalRunner.Tests
{
    [Collection("USQL Collection")]
    public class USQLTests
    {
        private readonly USQLFixture _fixture;

        public USQLTests(USQLFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void InitializeAndRunScript()
        {
            //The following test copies the input file to the data root, then executes a U-SQL script that copies the contents (except the header) to another file.
            var usqlTestConfiguration = new USQLAnalyticsConfiguration();
            File.Copy("InputFile.tsv", Path.Combine(usqlTestConfiguration.USQLDataRoot, "InputFile.tsv"), true);
            var usqlAnalyticsJobRunner = new USQLAnalyticsJobRunner(usqlTestConfiguration);

            usqlAnalyticsJobRunner.RunAnalyticsJob(Path.GetFullPath("runScript.usql"));

            Assert.True(File.Exists(Path.Combine(usqlTestConfiguration.USQLDataRoot, "OutputFile.tsv")));
        }
    }

    public class USQLFixture
    {
        public USQLFixture()
        {
            var usqlTestConfiguration = new USQLAnalyticsConfiguration();
            var usqlAnalyticsInitializer = new USQLAnalyticsInitializer(new USQLAnalyticsJobRunner(usqlTestConfiguration));
            usqlAnalyticsInitializer.CreateDataRootDependencies(new []{new InitializationBundle() });
        }
    }

    public class USQLAnalyticsConfiguration : IUSQLAnalyticsConfiguration
    {
        public string USQLWorkDir => Path.Combine(Environment.CurrentDirectory, "USQLWorkDir");
        public string InitializationLogFilePath => Path.Combine(USQLWorkDir, "usqltestlog-init.txt");
        public string RunLogFilePath => Path.Combine(USQLWorkDir, "usqltestlog-out.txt");
        public string ADLToolsFolderPath => Path.GetFullPath("..\\..\\..\\packages\\Microsoft.Azure.DataLake.USQL.SDK.1.4.190114\\build\\runtime");
        public string USQLDataRoot => Path.Combine(Environment.CurrentDirectory, "USQLDataRoot");
    }

    [CollectionDefinition("USQL Collection")]
    public class USQLCollection : ICollectionFixture<USQLFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    public class InitializationBundle : IInitializationScriptBundle
    {
        public string[] ScriptsToRun => new[] {Path.Combine(Environment.CurrentDirectory, "initscript.usql")};
    }
}
