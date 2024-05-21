using StardewModdingAPI;
using StardewValley;

namespace BetterWateringCanAndHoe;

public sealed class GardenTool{
    /*********
     ** Properties
     *********/
    private readonly string _translationKey;
    private int _upgradeLevel;
    private bool _hasReachingEnchantment;
    private bool _dataChanged;
    private int _selectedOption;

    public GardenTool(string translationKey, int selectedOption){
        _translationKey = translationKey;
        SelectedOption = selectedOption;
    }
    
    /**********
     ** Public methods
     *********/
    public int SelectedOption{
        get{ return _selectedOption; }
        set{
            if (value <= GetMaximumSelectableOptionValue() && value >= 0){
                if (value == _selectedOption){
                    return;
                }
                _selectedOption = value;
                _dataChanged = true;
                return;
            }
            _selectedOption = 0;
        }
    }

    public bool DataChanged{
        get => _dataChanged;
        set => _dataChanged = value;
    }

    public string TranslationKey{
        get => _translationKey;
    }

    public void Refresh(){
        _upgradeLevel = GetUpgradeLevel();
        _hasReachingEnchantment = HasReachingEnchantment();
        SelectedOption = _selectedOption;
    }
    
    /// <summary>Determine which is the maximum selectable option value with the current Watering Can.</summary>
    public int GetMaximumSelectableOptionValue(){
        switch(_upgradeLevel){
            case 0:
            case 1:
            case 2:
            case 3: return _upgradeLevel;
            case 4: 
                return _hasReachingEnchantment ? 5 : 4;
            default: return 0;
        }
    }
    
    /// <summary>Save the selected option for watering can after dialogbox closed.</summary>
    /// <param name="who">Actual farmer.</param>
    /// <param name="selectedOption">Selected option.</param>
    public void DialogueSet(Farmer who, string selectedOption){
        SelectedOption=int.Parse(selectedOption);
    }

    /**********
     ** Private methods
     *********/
    private static int GetUpgradeLevel(){
        return Game1.player.CurrentTool.UpgradeLevel;
    }

    private static bool HasReachingEnchantment(){
        return string.Equals(Game1.player.CurrentTool.enchantments.ToString(), $"StardewValley.Enchantments.ReachingToolEnchantment");
    }
}