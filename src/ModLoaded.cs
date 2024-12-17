using StardewModdingAPI;

namespace BetterWateringCanAndHoe;

/// <summary>
/// Track if interested mod is loaded for compatibility reasons
/// </summary>
public class ModLoaded {
    
    /// <summary>
    /// Bool value for Prismatic Tools Continued Mod
    /// </summary>
    public bool PrismaticToolsContinuedLoaded { get; set; }

    public ModLoaded(IModHelper helper) {
        PrismaticToolsContinuedLoaded=helper.ModRegistry.IsLoaded("iargue.PrismaticToolsContinued");
    }
}