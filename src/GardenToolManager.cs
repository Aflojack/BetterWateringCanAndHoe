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
    private readonly bool _enable;
    /// <summary>
    /// Variable if mod need to automatically select highest option.
    /// </summary>
    private readonly bool _alwaysHighest;
    /// <summary>
    /// Variable if mod need to allow temporary selection.
    /// </summary>
    private readonly bool _selectTemporary;
    /// <summary>
    /// Reset value for timer.
    /// </summary>
    private readonly int _timerStartValue;
    /// <summary>
    /// Actual garden tool.
    /// </summary>
    private readonly GardenTool _gardenTool;
    /// <summary>
    /// Variable for timer.
    /// </summary>
    private int _timer;

    public GardenToolManager(bool enable, bool alwaysHighest, bool selectTemporary, GardenTool gardenTool, int timerStartValue){
        _enable = enable;
        _alwaysHighest = alwaysHighest;
        _selectTemporary = selectTemporary;
        _gardenTool = gardenTool;
        _timerStartValue = timerStartValue;
    }

    /**********
     ** Public methods
     *********/
    /// <summary>
    /// Getter and setter for DataChange property.
    /// </summary>
    public bool DataChange{
        get => _gardenTool.DataChanged;
        set => _gardenTool.DataChanged = value;
    }

    /// <summary>
    /// Getter for SelectedOption property.
    /// </summary>
    public int SelectedOption{
        get => _gardenTool.SelectedOption;
    }
    /// <summary>Button change action.</summary>
    public void ButtonAction(SButton state, IModHelper helper){
        if (!_enable)
            return;
        
        _gardenTool.Refresh();
        
        if(_alwaysHighest && !_selectTemporary)
            return;
        
        if(helper.Input.GetState(state)==SButtonState.Released){
            if(_alwaysHighest && _selectTemporary){
                TimerReset();
            }
            List<Response> choices = new List<Response>();
            string selectionText=helper.Translation.Get("dialogbox.currentOption");
            for(int i=0;i<=_gardenTool.GetMaximumSelectableOptionValue();i++){
                string responseKey=$"{i}";
                string responseText=helper.Translation.Get($"dialogbox.option{i}");
                choices.Add(new Response(responseKey,responseText+(_gardenTool.SelectedOption==i?$" --- {selectionText} ---":"")));
            }
            Game1.currentLocation.createQuestionDialogue(helper.Translation.Get(_gardenTool.TranslationKey), choices.ToArray(), _gardenTool.DialogueSet);
        }
    }
    
    /// <summary>Tick method for watering can mod.</summary>
    public void Tick(){
        _gardenTool.Refresh();
        
        if(_alwaysHighest){
            if(_selectTemporary && !TimerEnded()){
                TimerTick();
            }else{
                _gardenTool.SelectedOption = _gardenTool.GetMaximumSelectableOptionValue();
            }
        }
        Game1.player.toolPower.Value=_gardenTool.SelectedOption;
    }

    /**********
     ** Private methods
     *********/
    /// <summary>
    /// Reset the timer with _timerStartValue.
    /// </summary>
    private void TimerReset(){
        _timer = _timerStartValue;
    }

    /// <summary>
    /// If the timer is not zero then it will continue countdown.
    /// </summary>
    private void TimerTick(){
        if (_timer != 0)
            _timer--;
    }

    /// <summary>
    /// Check if the timer is ended.
    /// </summary>
    /// <returns>If the timer ended (_timer==0)</returns>
    private bool TimerEnded(){
        return _timer == 0;
    }
}