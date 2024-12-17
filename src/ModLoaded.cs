using StardewModdingAPI;

namespace BetterWateringCanAndHoe;

public class ModLoaded {
    public bool PrismaticToolsContinuedLoaded { get; set; }

    public ModLoaded(IModHelper helper) {
        PrismaticToolsContinuedLoaded=helper.ModRegistry.IsLoaded("iargue.PrismaticToolsContinued");
    }
}