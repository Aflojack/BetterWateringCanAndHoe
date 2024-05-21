using StardewModdingAPI;
using StardewValley;

namespace BetterWateringCanAndHoe;

public sealed class GardenToolManager {
    /*********
     ** Properties
     *********/
    private readonly bool _enable;
    private readonly bool _alwaysHighest;
    private readonly bool _selectTemporary;
    private readonly int _timerStartValue;
    private readonly GardenTool _gardenTool;
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
            Game1.currentLocation.createQuestionDialogue(helper.Translation.Get(_gardenTool.TranslationKey), choices.ToArray(), new GameLocation.afterQuestionBehavior(_gardenTool.DialogueSet));
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

    public bool DataChange(){
        return _gardenTool.DataChanged;
    }

    public void DataChangeReset(){
        _gardenTool.DataChanged = false;
    }

    public int SelectedOption(){
        return _gardenTool.SelectedOption;
    }

    /**********
     ** Private methods
     *********/
    private void TimerReset(){
        _timer = _timerStartValue;
    }

    private void TimerTick(){
        if (_timer != 0)
            _timer--;
    }

    private bool TimerEnded(){
        return _timer == 0;
    }
}