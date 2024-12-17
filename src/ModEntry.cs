using GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;

namespace BetterWateringCanAndHoe {
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod {
        /*********
         ** Fields
         *********/
        /// <summary>The mod configuration from the player.</summary>
        private ModConfig Config;

        /// <summary>The mod data from the player.</summary>
        private ModData Data;

        /// <summary>Manager class for Better Hoe mod.</summary>
        private ModCore ModBetterHoe;

        /// <summary>Manager class for Better Watering Can mod.</summary>
        private ModCore ModBetterWateringCan;

        /**********
         ** Public methods
         *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper) {
            Config = Helper.ReadConfig<ModConfig>();
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.OneSecondUpdateTicked += OnSecondUpdateTicked;
            helper.Events.Input.ButtonReleased += OnButtonReleased;
        }

        /**********
         ** Private methods
         *********/
        /// <summary>Raised when the save file loaded.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e) {
            try {
                ModDataLoad();
                ModLoaded modLoaded = new ModLoaded(Helper);
                ModBetterHoe = new ModCore(
                    Config.BetterHoeModEnabled,
                    Config.HoeAlwaysHighestOption,
                    Config.HoeSelectTemporary,
                    Config.HoeTimerStart,
                    "dialogbox.hoeQuestion",
                    Data.HoeSelectedOption,
                    modLoaded
                );
                ModBetterWateringCan = new ModCore(
                    Config.BetterWateringCanModEnabled,
                    Config.WateringCanAlwaysHighestOption,
                    Config.WateringCanSelectTemporary,
                    Config.WateringCanTimerStart,
                    "dialogbox.wateringCanQuestion",
                    Data.WateringCanSelectedOption,
                    modLoaded
                );
            } catch (Exception exception) {
                Monitor.Log($"Caught exception while save loaded. Exception: {exception.Message}", LogLevel.Error);
            }
        }

        /// <summary>Raised when the game launched. Function to build config with GenericModConfigMenu.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e) {
            try {
                var configMenu =
                    Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
                if (configMenu is null)
                    return;

                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );

                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("configMenu.selectionOpenKey.name"),
                    tooltip: () => Helper.Translation.Get("configMenu.selectionOpenKey.tooltip"),
                    getValue: () => Config.SelectionOpenKey,
                    setValue: value => Config.SelectionOpenKey = value
                );

                configMenu.AddSectionTitle(
                    mod: ModManifest,
                    text: () => Helper.Translation.Get("configMenu.wateringCan.title")
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("configMenu.enabled.name"),
                    tooltip: () => Helper.Translation.Get("configMenu.enabled.tooltip"),
                    getValue: () => Config.BetterWateringCanModEnabled,
                    setValue: value => Config.BetterWateringCanModEnabled = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("configMenu.alwaysHighestOption.name"),
                    tooltip: () => Helper.Translation.Get("configMenu.alwaysHighestOption.tooltip"),
                    getValue: () => Config.WateringCanAlwaysHighestOption,
                    setValue: value => Config.WateringCanAlwaysHighestOption = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("configMenu.selectTemporary.name"),
                    tooltip: () => Helper.Translation.Get("configMenu.selectTemporary.tooltip"),
                    getValue: () => Config.WateringCanSelectTemporary,
                    setValue: value => Config.WateringCanSelectTemporary = value
                );

                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("configMenu.selectTemporaryTimer.name"),
                    tooltip: () => Helper.Translation.Get("configMenu.selectTemporaryTimer.tooltip"),
                    getValue: () => Config.WateringCanTimerStart / 60,
                    setValue: value => Config.WateringCanTimerStart = value * 60,
                    min: 10,
                    max: 90,
                    interval: 1
                );

                configMenu.AddSectionTitle(
                    mod: ModManifest,
                    text: () => Helper.Translation.Get("configMenu.hoe.title")
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("configMenu.enabled.name"),
                    tooltip: () => Helper.Translation.Get("configMenu.enabled.tooltip"),
                    getValue: () => Config.BetterHoeModEnabled,
                    setValue: value => Config.BetterHoeModEnabled = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("configMenu.alwaysHighestOption.name"),
                    tooltip: () => Helper.Translation.Get("configMenu.alwaysHighestOption.tooltip"),
                    getValue: () => Config.HoeAlwaysHighestOption,
                    setValue: value => Config.HoeAlwaysHighestOption = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("configMenu.selectTemporary.name"),
                    tooltip: () => Helper.Translation.Get("configMenu.selectTemporary.tooltip"),
                    getValue: () => Config.HoeSelectTemporary,
                    setValue: value => Config.HoeSelectTemporary = value
                );

                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("configMenu.selectTemporaryTimer.name"),
                    tooltip: () => Helper.Translation.Get("configMenu.selectTemporaryTimer.tooltip"),
                    getValue: () => Config.HoeTimerStart / 60,
                    setValue: value => Config.HoeTimerStart = value * 60,
                    min: 10,
                    max: 90,
                    interval: 1
                );
            }
            catch (Exception exception) {
                Monitor.Log($"Caught exception while GenericModConfigMenu was built. Exception: {exception.Message}", LogLevel.Error);
            }
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e) {
            try {
                if (!Context.IsWorldReady)
                    return;

                ModBetterWateringCan.TimerTick();
                ModBetterHoe.TimerTick();

                switch (Game1.player.CurrentTool) {
                    case WateringCan:
                        ModBetterWateringCan.Tick();
                        break;
                    case Hoe:
                        ModBetterHoe.Tick();
                        break;
                }

                ModDataWrite();
            }
            catch (Exception exception) {
                Monitor.Log($"Caught exception while game ticked. Mod disabled. Exception: {exception.Message}", LogLevel.Error);
                ModBetterHoe.Enabled = false;
                ModBetterWateringCan.Enabled = false;
            }
        }

        /// <summary>Raised after the game state is updated (per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSecondUpdateTicked(object? sender, OneSecondUpdateTickedEventArgs e) {
            try {
                if (!Context.IsWorldReady || Helper.Input.GetState(SButton.MouseLeft) == SButtonState.Released ||
                    Helper.Input.GetState(SButton.MouseLeft) == SButtonState.None)
                    return;

                switch (Game1.player.CurrentTool) {
                    case WateringCan:
                        ModBetterWateringCan.ButtonActionSingleLeftClick();
                        break;
                    case Hoe:
                        ModBetterHoe.ButtonActionSingleLeftClick();
                        break;
                }
            }
            catch (Exception exception) {
                Monitor.Log($"Caught exception while game ticked. Mod disabled. Exception: {exception.Message}", LogLevel.Error);
                ModBetterHoe.Enabled = false;
                ModBetterWateringCan.Enabled = false;
            }
        }

        /// <summary>Raised after the player released any button on the keyboard, mouse, or controller.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonReleased(object? sender, ButtonReleasedEventArgs e) {
            try {
                if (!Context.IsWorldReady || Game1.player.IsBusyDoingSomething())
                    return;

                if (e.Button == Config.SelectionOpenKey) {
                    switch (Game1.player.CurrentTool) {
                        case WateringCan:
                            ModBetterWateringCan.ButtonActionOpenSelection(Helper);
                            break;
                        case Hoe:
                            ModBetterHoe.ButtonActionOpenSelection(Helper);
                            break;
                    }
                }

                ModDataWrite();
            }
            catch (Exception exception) {
                Monitor.Log($"Caught exception while button action was performed. Exception: {exception.Message}", LogLevel.Error);
            }
        }

        /// <summary>Load current player mod data json from data folder.</summary>
        private void ModDataLoad() {
            Data = Helper.Data.ReadJsonFile<ModData>($"data/{Constants.SaveFolderName}.json") ?? new ModData();
        }

        /// <summary>Write current player mod data json to data folder if that necessary.</summary>
        private void ModDataWrite() {
            if (!ModBetterWateringCan.DataChanged && !ModBetterHoe.DataChanged)
                return;

            Data.WateringCanSelectedOption = ModBetterWateringCan.SelectedOption;
            Data.HoeSelectedOption = ModBetterHoe.SelectedOption;
            Helper.Data.WriteJsonFile($"data/{Constants.SaveFolderName}.json", Data);
            ModBetterWateringCan.DataChanged = false;
            ModBetterHoe.DataChanged = false;
        }
    }
}