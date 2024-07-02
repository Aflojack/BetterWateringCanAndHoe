using StardewModdingAPI;
using StardewValley;

namespace BetterWateringCanAndHoe;

public sealed class GardenToolManager {
    /*********
     ** Properties
     *********/
    /// <summary>
    /// Variable if mod is enabled or disabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Variable if mod need to automatically select highest option.
    /// </summary>
    public bool AlwaysHighest { get; }

    /// <summary>
    /// Variable if mod need to allow temporary selection.
    /// </summary>
    public bool SelectTemporary { get; }

    /// <summary>
    /// Reset value for timer.
    /// </summary>
    public int TimerStartValue { get; }

    /// <summary>
    /// Actual garden tool.
    /// </summary>
    public GardenTool GardenTool { get; }

    /// <summary>
    /// Variable for timer.
    /// </summary>
    public int Timer { get; private set; }

    public bool DataChange {
        get => GardenTool.DataChanged;
        set => GardenTool.DataChanged = value;
    }

    public int SelectedOption {
        get => GardenTool.SelectedOption;
    }

    /**********
     ** Public methods
     *********/
    public GardenToolManager(bool enabled, bool alwaysHighest, bool selectTemporary, GardenTool gardenTool,
        int timerStartValue) {
        Enabled = enabled;
        AlwaysHighest = alwaysHighest;
        SelectTemporary = selectTemporary;
        GardenTool = gardenTool;
        TimerStartValue = timerStartValue;
    }

    /// <summary>
    /// Button change action.
    /// </summary>
    public void ButtonActionOpenSelection(IModHelper helper) {
        if (!Enabled || AlwaysHighest && !SelectTemporary)
            return;

        if (GardenTool.GetMaximumSelectableOptionValue() == -1) {
            throw new Exception(
                $"Unsupported tool! Mod not compatible with {Game1.player.CurrentTool.ItemId} upgrade level: {GardenTool.GetUpgradeLevel()}");
        }

        if (AlwaysHighest && SelectTemporary)
            TimerReset();

        GardenTool.Refresh();
        ShowSelectionMenu(helper);
    }
    
    /// <summary>
    /// Prevent charging effect if selected option is tile (1) or current tool isn't upgraded yet.
    /// </summary>
    public void ButtonActionSingleLeftClick() {
        if (!Enabled || (Game1.player.IsBusyDoingSomething() && !Game1.player.UsingTool) ||
            (GardenTool.GetUpgradeLevel() != 0 && GardenTool.SelectedOption != 0))
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

        GardenTool.Refresh();

        if (GardenTool.GetMaximumSelectableOptionValue() == -1) {
            throw new Exception(
                $"Unsupported tool! Mod not compatible with {Game1.player.CurrentTool.ItemId} upgrade level: {GardenTool.GetUpgradeLevel()}");
        }

        if (AlwaysHighest) {
            if (!SelectTemporary || (SelectTemporary && TimerEnded())) {
                GardenTool.SelectedOption = GardenTool.GetMaximumSelectableOptionValue();
            }
        }

        Game1.player.toolHold.Value = 600;
        Game1.player.toolPower.Value = GardenTool.SelectedOption;
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
        for (int i = 0; i <= GardenTool.GetMaximumSelectableOptionValue(); i++) {
            string responseKey = $"{i}";
            string responseText = helper.Translation.Get($"dialogbox.option{i}");
            choices.Add(new Response(responseKey, responseText + (GardenTool.SelectedOption == i ? $" --- {currentText} ---" : "")));
        }
        Game1.currentLocation.createQuestionDialogue(helper.Translation.Get(GardenTool.TranslationKey), choices.ToArray(), GardenTool.DialogueSet);
    }
}