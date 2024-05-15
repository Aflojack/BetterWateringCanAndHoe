using StardewModdingAPI;

class ModConfig{
   public bool ModEnabled { get; set; } = true;
   public SButton SelectionOpenKey { get; set; } = SButton.R;
   public bool AlwaysHighestOption { get; set; } = false;
   public bool SelectTemporary { get; set; } = false;
}