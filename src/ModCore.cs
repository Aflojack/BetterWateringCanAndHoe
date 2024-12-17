using StardewModdingAPI;
using StardewValley;
using StardewValley.Enchantments;

namespace BetterWateringCanAndHoe;

public sealed class ModCore {
    /*********
     ** Fields
     *********/
    
    /// <summary>
    /// Current selected option.
    /// </summary>
    private int _selectedOption;
    
    /*********
     ** Properties
     *********/
    
    /// <summary>
    /// Variable if mod is enabled or disabled.
    /// </summary>
    public bool Enabled { get; set; }
    
    public int SelectedOption {
        get => _selectedOption;
        private set {
            if (value <= GetMaximumSelectableOptionValue() && value >= 0) {
                if (value == _selectedOption) {
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
    /// Variable if selected mode is actually changed.
    /// </summary>
    public bool DataChanged { get; set; }

    /// <summary>
    /// Variable if mod need to automatically select highest option.
    /// </summary>
    private bool AlwaysHighest { get; }

    /// <summary>
    /// Variable if mod need to allow temporary selection.
    /// </summary>
    private bool SelectTemporary { get; }

    /// <summary>
    /// Reset value for timer.
    /// </summary>
    private int TimerStartValue { get; }

    /// <summary>
    /// Variable for timer.
    /// </summary>
    private int Timer { get; set; }

    /// <summary>
    /// Stored TranslationKey for get text through i18n.
    /// </summary>
    private string TranslationKey { get; }

    /**********
     ** Public methods
     *********/
    public ModCore(bool enabled, bool alwaysHighest, bool selectTemporary, int timerStartValue, string translationKey, int selectedOption) {
        Enabled = enabled;
        AlwaysHighest = alwaysHighest;
        SelectTemporary = selectTemporary;
        TimerStartValue = timerStartValue;
        TranslationKey = translationKey;
        _selectedOption = selectedOption;
    }

    /// <summary>
    /// Button change action.
    /// </summary>
    public void ButtonActionOpenSelection(IModHelper helper) {
        if (!Enabled || AlwaysHighest && !SelectTemporary)
            return;

        if (GetMaximumSelectableOptionValue() == -1) {
            throw new Exception(
                $"Unsupported tool! Mod not compatible with {Game1.player.CurrentTool.ItemId} upgrade level: {GetUpgradeLevel()}");
        }

        if (AlwaysHighest && SelectTemporary)
            TimerReset();

        Refresh();
        ShowSelectionMenu(helper);
    }
    
    /// <summary>
    /// Prevent charging effect if selected option is tile (1) or current tool isn't upgraded yet.
    /// </summary>
    public void ButtonActionSingleLeftClick() {
        if (!Enabled || (Game1.player.IsBusyDoingSomething() && !Game1.player.UsingTool) ||
            (GetUpgradeLevel() != 0 && SelectedOption != 0))
            return;

        Game1.player.EndUsingTool();
    }

    /// <summary>
    /// Tick method for garden tool.
    /// </summary>
    public void Tick() {
        if (!Enabled) {
            return;
        }

        Refresh();

        if (GetMaximumSelectableOptionValue() == -1) {
            throw new Exception(
                $"Unsupported tool! Mod not compatible with {Game1.player.CurrentTool.ItemId} upgrade level: {GetUpgradeLevel()}");
        }

        if (AlwaysHighest) {
            if (!SelectTemporary || (SelectTemporary && TimerEnded())) {
                SelectedOption = GetMaximumSelectableOptionValue();
            }
        }

        Game1.player.toolHold.Value = 600;
        Game1.player.toolPower.Value = SelectedOption;
    }

    /// <summary>
    /// If the timer is not zero then it will continue countdown.
    /// </summary>
    public void TimerTick() {
        if (!TimerEnded())
            Timer--;
    }

    /**********
     ** Private methods
     *********/
    
    /// <summary>
    /// Reset the timer with _timerStartValue.
    /// </summary>
    private void TimerReset() {
        Timer = TimerStartValue;
    }

    /// <summary>
    /// Check if the timer is ended.
    /// </summary>
    /// <returns>If the timer ended (_timer==0)</returns>
    private bool TimerEnded() {
        return Timer == 0;
    }
    
    /// <summary>
    /// Build dialog choices depends on current tool upgrade level and enchantment then show selection menu to the player.
    /// </summary>
    /// <param name="helper"></param> SMAPI helper
    private void ShowSelectionMenu(IModHelper helper) {
        List<Response> choices = new List<Response>();
        string currentText = helper.Translation.Get("dialogbox.currentOption");
        for (int i = 0; i <= GetMaximumSelectableOptionValue(); i++) {
            string responseKey = $"{i}";
            string responseText = helper.Translation.Get($"dialogbox.option{i}");
            choices.Add(new Response(responseKey, responseText + (SelectedOption == i ? $" --- {currentText} ---" : "")));
        }
        Game1.currentLocation.createQuestionDialogue(helper.Translation.Get(TranslationKey), choices.ToArray(), DialogueSet);
    }
    
    /// <summary>
    /// When it called it will refresh information of the garden tool.
    /// </summary>
    private void Refresh() {
        SelectedOption = _selectedOption;
    }

    /// <summary>
    /// Determine which is the maximum selectable option value with the current upgradeLevel and enchantment.
    /// </summary>
    /// <returns>Maximum selectable option.</returns>
    private int GetMaximumSelectableOptionValue() {
        switch (GetUpgradeLevel()) {
            case 0:
            case 1:
            case 2:
            case 3: return GetUpgradeLevel();
            case 4:
                return HasReachingEnchantment() ? 5 : 4;
            default: return -1;
        }
    }

    /// <summary>Save the selected option for tool after dialog closed.</summary>
    /// <param name="who">Actual farmer.</param>
    /// <param name="selectedOption">Selected option.</param>
    private void DialogueSet(Farmer who, string selectedOption) {
        SelectedOption = int.Parse(selectedOption);
    }

    /// <summary>
    /// Return the current upgradeLevel from the game.
    /// </summary>
    /// <returns>Current tool upgradeLevel.</returns>
    private int GetUpgradeLevel() {
        return Game1.player.CurrentTool.UpgradeLevel;
    }

    /// <summary>
    /// Return the current enchantment if it is Reaching from the game.
    /// </summary>
    /// <returns>If the tool has reaching enchantment.</returns>
    private bool HasReachingEnchantment() {
        return Game1.player.CurrentTool.hasEnchantmentOfType<ReachingToolEnchantment>();
    }
}