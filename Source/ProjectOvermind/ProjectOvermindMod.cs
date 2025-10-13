using System;
using System.Reflection;
using Verse;

namespace ProjectOvermind
{
    [StaticConstructorOnStartup]
    public static class ProjectOvermindMod
    {
        static ProjectOvermindMod()
        {
            try
            {
                Log.Message("[Project Overmind] Initializing Mindshift Abilities mod...");
                Log.Message($"[Project Overmind] Version: {Assembly.GetExecutingAssembly().GetName().Version}");
                Log.Message("[Project Overmind] Translocate Target psycast loaded successfully.");
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] Error during initialization: {ex}");
            }
        }
    }
}
