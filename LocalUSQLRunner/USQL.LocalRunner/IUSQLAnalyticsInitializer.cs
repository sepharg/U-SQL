using System.Collections.Generic;

namespace USQL.LocalRunner
{
    public interface IUSQLAnalyticsInitializer
    {
        void InitializeUSQLDatabase(IEnumerable<IInitializationScriptBundle> initializationScriptBundles);
    }
}