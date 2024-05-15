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
    /// <summary>Timer for SelectTemporary setting.</summary>
    private int timerCounter=0;
    /// <summary>Timer starer value in OnUpdateTicked 3600 means ~60 seconds.</summary>
    private int timerStart=3600;
    
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

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("configMenu.enabled.name"),
                tooltip: () => this.Helper.Translation.Get("configMenu.enabled.tooltip"),
                getValue: () => this.Config.ModEnabled,
                setValue: value => this.Config.ModEnabled = value
            );

            configMenu.AddKeybind(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("configMenu.selectionOpenKey.name"),
                tooltip: () => this.Helper.Translation.Get("configMenu.selectionOpenKey.tooltip"),
                getValue: () => this.Config.SelectionOpenKey,
                setValue: value => this.Config.SelectionOpenKey = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("configMenu.alwaysHighestOption.name"),
                tooltip: () => this.Helper.Translation.Get("configMenu.alwaysHighestOption.tooltip"),
                getValue: () => this.Config.AlwaysHighestOption,
                setValue: value => this.Config.AlwaysHighestOption = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("configMenu.selectTemporary.name"),
                tooltip: () => this.Helper.Translation.Get("configMenu.selectTemporary.tooltip"),
                getValue: () => this.Config.SelectTemporary,
                setValue: value => this.Config.SelectTemporary = value
            );
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e){
            if (!this.Config.ModEnabled)
                return;

            if (!Context.IsWorldReady)
                return;
            
            if (Game1.player.CurrentTool is not WateringCan)
                return;

            if(this.Data.SelectedOption>GetMaximumSelectableOptionValue() || this.Data.SelectedOption<0){
                this.Data.SelectedOption=0;
                this.dataChanged=true;
            }

            if(this.Config.AlwaysHighestOption){
                int highestOption=this.GetMaximumSelectableOptionValue();
                if(this.Config.SelectTemporary && timerCounter!=0){
                    timerCounter--;
                }else if(this.Data.SelectedOption!=highestOption){
                    this.Data.SelectedOption=highestOption;
                    this.dataChanged=true;
                }
            }

            if(dataChanged){
                ModDataWrite();
            }

            Game1.player.toolPower.Value=this.Data.SelectedOption;
        }

        /// <summary>Raised after the player pressed/released any buttons on the keyboard, mouse, or controller.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e){
            if (!this.Config.ModEnabled)
                return;
                
            if (!Context.IsWorldReady)
                return;

            if (Game1.player.IsBusyDoingSomething())
                return;

            if (Game1.player.CurrentTool is not WateringCan)
                return;

            if(this.Config.AlwaysHighestOption && !this.Config.SelectTemporary){
                return;
            }

            SButtonState state = this.Helper.Input.GetState(this.Config.SelectionOpenKey);
            if (state==SButtonState.Released){
                if(this.Config.AlwaysHighestOption && this.Config.SelectTemporary){
                    this.timerCounter=this.timerStart;
                }
                SelectionOpen();
            }
        }

        /// <summary>Display a dialogue window to the player. Content depending on the level of the watering can.</summary>
        private void SelectionOpen(){
            List<Response> choices = new List<Response>();
            string selectionText=this.Helper.Translation.Get("dialogbox.currentOption");
            for(int i=0;i<=GetMaximumSelectableOptionValue();i++){
                string responseKey=$"{i}";
                string responseText=this.Helper.Translation.Get($"dialogbox.option{i}");
                choices.Add(new Response(responseKey,responseText+(this.Data.SelectedOption==i?$" --- {selectionText} ---":"")));
            }
            Game1.currentLocation.createQuestionDialogue(this.Helper.Translation.Get("dialogbox.question"), choices.ToArray(), new GameLocation.afterQuestionBehavior(DialogueSet));
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

        /// <summary>Save the selected option after dialogbox closed.</summary>
        /// <param name="who">Actual farmer.</param>
        /// <param name="selectedOption">Selected option.</param>
        private void DialogueSet(Farmer who, string selectedOption){
            this.Data.SelectedOption=int.Parse(selectedOption);
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