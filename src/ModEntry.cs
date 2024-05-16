using GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;

namespace BetterWateringCan{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod{
        
    /*********
    ** Properties
    *********/
    /// <summary>The mod configuration from the player.</summary>
    private ModConfig Config;
    /// <summary>The mod data from the player.</summary>
    private ModData Data;
    /// <summary>Bool value for data change detection.</summary>
    private bool dataChanged;
    /// <summary>Timer for Watering Can SelectTemporary setting.</summary>
    private int wateringCanTimerCounter=0;
    /// <summary>Timer for Hoe SelectTemporary setting.</summary>
    private int hoeTimerCounter=0;
    /// <summary>Timer starer value in OnUpdateTicked 3600 means ~60 seconds.</summary>
    private readonly int timerStart=3600;
    
        /**********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper){
            this.Config = this.Helper.ReadConfig<ModConfig>();
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }

        /**********
        ** Private methods
        *********/
        /// <summary>Raised when the day started.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object? sender, DayStartedEventArgs e){
            if(this.Data is null)
                ModDataLoad();
        }
        
        /// <summary>Raised when the game launched. Function to build config with GenericModConfigMenu.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e){
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.Config)
            );

            configMenu.AddKeybind(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("configMenu.selectionOpenKey.name"),
                tooltip: () => this.Helper.Translation.Get("configMenu.selectionOpenKey.tooltip"),
                getValue: () => this.Config.SelectionOpenKey,
                setValue: value => this.Config.SelectionOpenKey = value
            );

            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => this.Helper.Translation.Get("configMenu.wateringCan.title")
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("configMenu.enabled.name"),
                tooltip: () => this.Helper.Translation.Get("configMenu.enabled.tooltip"),
                getValue: () => this.Config.BetterWateringCanModEnabled,
                setValue: value => this.Config.BetterWateringCanModEnabled = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("configMenu.alwaysHighestOption.name"),
                tooltip: () => this.Helper.Translation.Get("configMenu.alwaysHighestOption.tooltip"),
                getValue: () => this.Config.WateringCanAlwaysHighestOption,
                setValue: value => this.Config.WateringCanAlwaysHighestOption = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("configMenu.selectTemporary.name"),
                tooltip: () => this.Helper.Translation.Get("configMenu.selectTemporary.tooltip"),
                getValue: () => this.Config.WateringCanSelectTemporary,
                setValue: value => this.Config.WateringCanSelectTemporary = value
            );

            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => this.Helper.Translation.Get("configMenu.hoe.title")
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("configMenu.enabled.name"),
                tooltip: () => this.Helper.Translation.Get("configMenu.enabled.tooltip"),
                getValue: () => this.Config.BetterHoeModEnabled,
                setValue: value => this.Config.BetterHoeModEnabled = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("configMenu.alwaysHighestOption.name"),
                tooltip: () => this.Helper.Translation.Get("configMenu.alwaysHighestOption.tooltip"),
                getValue: () => this.Config.HoeAlwaysHighestOption,
                setValue: value => this.Config.HoeAlwaysHighestOption = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("configMenu.selectTemporary.name"),
                tooltip: () => this.Helper.Translation.Get("configMenu.selectTemporary.tooltip"),
                getValue: () => this.Config.HoeSelectTemporary,
                setValue: value => this.Config.HoeSelectTemporary = value
            );
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e){
            if (!Context.IsWorldReady)
                return;

            if(this.Config.BetterWateringCanModEnabled && Game1.player.CurrentTool is WateringCan){
                WateringCanModTick();
                return;
            }

            if(this.Config.BetterHoeModEnabled && Game1.player.CurrentTool is Hoe){
                HoeModTick();
                return;
            }
        }

        /// <summary>Tick method for watering can mod.</summary>
        private void WateringCanModTick(){
            if(this.Data.WateringCanSelectedOption>GetMaximumSelectableOptionValue() || this.Data.WateringCanSelectedOption<0){
                this.Data.WateringCanSelectedOption=0;
                this.dataChanged=true;
            }

            if(this.Config.WateringCanAlwaysHighestOption){
                int highestOption=this.GetMaximumSelectableOptionValue();
                if(this.Config.WateringCanSelectTemporary && wateringCanTimerCounter!=0){
                    wateringCanTimerCounter--;
                }else if(this.Data.WateringCanSelectedOption!=highestOption){
                    this.Data.WateringCanSelectedOption=highestOption;
                    this.dataChanged=true;
                }
            }

            if(dataChanged){
                ModDataWrite();
            }

            Game1.player.toolPower.Value=this.Data.WateringCanSelectedOption;
        }

        /// <summary>Tick method for hoe mod.</summary>
        private void HoeModTick(){
            if(this.Data.HoeSelectedOption>GetMaximumSelectableOptionValue() || this.Data.HoeSelectedOption<0){
                this.Data.HoeSelectedOption=0;
                this.dataChanged=true;
            }

            if(this.Config.HoeAlwaysHighestOption){
                int highestOption=this.GetMaximumSelectableOptionValue();
                if(this.Config.HoeSelectTemporary && hoeTimerCounter!=0){
                    hoeTimerCounter--;
                }else if(this.Data.HoeSelectedOption!=highestOption){
                    this.Data.HoeSelectedOption=highestOption;
                    this.dataChanged=true;
                }
            }

            if(dataChanged){
                ModDataWrite();
            }

            Game1.player.toolPower.Value=this.Data.HoeSelectedOption;
        }

        /// <summary>Raised after the player pressed/released any buttons on the keyboard, mouse, or controller.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e){
            if (!Context.IsWorldReady)
                return;

            if (Game1.player.IsBusyDoingSomething())
                return;

            if (this.Config.BetterWateringCanModEnabled && Game1.player.CurrentTool is WateringCan){
                WateringCanButtonAction();
                return;
            }

            if (this.Config.BetterHoeModEnabled && Game1.player.CurrentTool is Hoe){
                HoeButtonAction();
                return;
            }
        }

        /// <summary>Button change action for watering can mod.</summary>
        private void WateringCanButtonAction(){
            if(this.Config.WateringCanAlwaysHighestOption && !this.Config.WateringCanSelectTemporary){
                return;
            }

            SButtonState state = this.Helper.Input.GetState(this.Config.SelectionOpenKey);
            if(state==SButtonState.Released){
                if(this.Config.WateringCanAlwaysHighestOption && this.Config.WateringCanSelectTemporary){
                    this.wateringCanTimerCounter=this.timerStart;
                }
                List<Response> choices = new List<Response>();
                string selectionText=this.Helper.Translation.Get("dialogbox.currentOption");
                for(int i=0;i<=GetMaximumSelectableOptionValue();i++){
                    string responseKey=$"{i}";
                    string responseText=this.Helper.Translation.Get($"dialogbox.option{i}");
                    choices.Add(new Response(responseKey,responseText+(this.Data.WateringCanSelectedOption==i?$" --- {selectionText} ---":"")));
                }
                Game1.currentLocation.createQuestionDialogue(this.Helper.Translation.Get("dialogbox.wateringCanQuestion"), choices.ToArray(), new GameLocation.afterQuestionBehavior(DialogueSetWateringCan));
            }
        }

        /// <summary>Button change action for hoe mod.</summary>
        private void HoeButtonAction(){
            if(this.Config.HoeAlwaysHighestOption && !this.Config.HoeSelectTemporary){
                return;
            }

            SButtonState state = this.Helper.Input.GetState(this.Config.SelectionOpenKey);
            if(state==SButtonState.Released){
                if(this.Config.HoeAlwaysHighestOption && this.Config.HoeSelectTemporary){
                    this.hoeTimerCounter=this.timerStart;
                }
                List<Response> choices = new List<Response>();
                string selectionText=this.Helper.Translation.Get("dialogbox.currentOption");
                for(int i=0;i<=GetMaximumSelectableOptionValue();i++){
                    string responseKey=$"{i}";
                    string responseText=this.Helper.Translation.Get($"dialogbox.option{i}");
                    choices.Add(new Response(responseKey,responseText+(this.Data.HoeSelectedOption==i?$" --- {selectionText} ---":"")));
                }
                Game1.currentLocation.createQuestionDialogue(this.Helper.Translation.Get("dialogbox.hoeQuestion"), choices.ToArray(), new GameLocation.afterQuestionBehavior(DialogueSetHoe));
            }
        }

        /// <summary>Determine which is the maximum seletable option value with the current Watering Can.</summary>
        private int GetMaximumSelectableOptionValue(){
            int upgradeLevel=Game1.player.CurrentTool.UpgradeLevel;
            bool isHaveReachingEnchantment=String.Equals(Game1.player.CurrentTool.enchantments.ToString(),$"StardewValley.Enchantments.ReachingToolEnchantment");
            switch(upgradeLevel){
                case 0:
                case 1:
                case 2:
                case 3: return upgradeLevel;
                case 4: 
                        if(isHaveReachingEnchantment){
                            return 5;
                        }
                        return 4;
                default: return 0;
            }
        }

        /// <summary>Save the selected option for watering can after dialogbox closed.</summary>
        /// <param name="who">Actual farmer.</param>
        /// <param name="selectedOption">Selected option.</param>
        private void DialogueSetWateringCan(Farmer who, string selectedOption){
            this.Data.WateringCanSelectedOption=int.Parse(selectedOption);
            this.dataChanged=true;
        }

        /// <summary>Save the selected option for hoe after dialogbox closed.</summary>
        /// <param name="who">Actual farmer.</param>
        /// <param name="selectedOption">Selected option.</param>
        private void DialogueSetHoe(Farmer who, string selectedOption){
            this.Data.HoeSelectedOption=int.Parse(selectedOption);
            this.dataChanged=true;
        }

        /// <summary>Load currect player mod data json from data folder.</summary>
        private void ModDataLoad(){
            this.Data = this.Helper.Data.ReadJsonFile<ModData>($"data/{StardewModdingAPI.Constants.SaveFolderName}.json") ?? new ModData();
            this.dataChanged=false;
        }

        /// <summary>Write currect player mod data json to data folder.</summary>
        private void ModDataWrite(){
            this.Helper.Data.WriteJsonFile($"data/{StardewModdingAPI.Constants.SaveFolderName}.json", this.Data);
            this.dataChanged=false;
        }
    }
}