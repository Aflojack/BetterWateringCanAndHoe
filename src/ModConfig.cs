using StardewModdingAPI;

class ModConfig{
   public bool BetterWateringCanModEnabled { get; set; } = true;
   public bool BetterHoeModEnabled { get; set; } = true;
   public SButton SelectionOpenKey { get; set; } = SButton.R;
   public bool WateringCanAlwaysHighestOption { get; set; } = false;
   public bool HoeAlwaysHighestOption { get; set; } = false;
   public bool WateringCanSelectTemporary { get; set; } = false;
   public bool HoeSelectTemporary { get; set; } = false;
   public int WateringCanTimerStart { get; set; } = 3600;
   public int HoeTimerStart { get; set; } = 3600;
}