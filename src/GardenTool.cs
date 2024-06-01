using StardewModdingAPI;
using StardewValley;

namespace BetterWateringCanAndHoe;

public sealed class GardenTool{
    /*********
     ** Fields
     *********/
    /// <summary>
    /// Actual garden tool if it has reaching enchantment.
    /// </summary>
    private bool _hasReachingEnchantment;

    /// <summary>
    /// Current selected option.
    /// </summary>
    private int _selectedOption;
    
    /*********
     ** Properties
     *********/
    public int SelectedOption{
        get{ return _selectedOption; }
        set{
            if (value <= GetMaximumSelectableOptionValue() && value >= 0){
                if (value == _selectedOption){
                    return;
                }
                _selectedOption = value;
                DataChanged = true;
                return;
            }
            _selectedOption = 0;
            DataChanged = true;
        }
    }

    /// <summary>
    /// Actual garden tool upgrade level.
    /// </summary>
    public int UpgradeLevel{ get; private set; }

    /// <summary>
    /// Variable if selected mode is actually changed.
    /// </summary>
    public bool DataChanged{ get; set; }

    /// <summary>
    /// Stored TranslationKey for get text through i18n.
    /// </summary>
    public string TranslationKey{ get; }

    /**********
     ** Public methods
     *********/
    public GardenTool(string translationKey, int selectedOption){
        TranslationKey = translationKey;
        _selectedOption = selectedOption;
    }
    
    /// <summary>
    /// When it called it will refresh information of the garden tool.
    /// </summary>
    public void Refresh(){
        UpgradeLevel = GetUpgradeLevel();
        _hasReachingEnchantment = HasReachingEnchantment();
        SelectedOption = _selectedOption;
    }
    
    /// <summary>Determine which is the maximum selectable option value with the current upgradeLevel and enchangement.</summary>
    /// <returns>Maximum selectable option.</returns>
    public int GetMaximumSelectableOptionValue(){
        switch(UpgradeLevel){
            case 0:
            case 1:
            case 2:
            case 3: return UpgradeLevel;
            case 4: 
                return _hasReachingEnchantment ? 5 : 4;
            default: return 0;
        }
    }
    
    /// <summary>Save the selected option for tool after dialog closed.</summary>
    /// <param name="who">Actual farmer.</param>
    /// <param name="selectedOption">Selected option.</param>
    public void DialogueSet(Farmer who, string selectedOption){
        SelectedOption=int.Parse(selectedOption);
    }

    /**********
     ** Private methods
     *********/
    /// <summary>
    /// Queries the current upgradeLevel from the game.
    /// </summary>
    /// <returns>Current tool upgradeLevel.</returns>
    private static int GetUpgradeLevel(){
        return Game1.player.CurrentTool.UpgradeLevel;
    }

    /// <summary>
    /// Queries the current enchantment if it is Reaching from the game.
    /// </summary>
    /// <returns>If the tool has reaching enchantment.</returns>
    private static bool HasReachingEnchantment(){
        return string.Equals(Game1.player.CurrentTool.enchantments.ToString(), $"StardewValley.Enchantments.ReachingToolEnchantment");
    }
}