namespace Bird.App;
public sealed class ChestSpeedSettings : StackPanel
{
    private readonly AppState state;
    private readonly TextBlock description=Ui.Text("",13,"Muted");
    private readonly StackPanel editor=new(){Name="ChestSpeedEditor"};
    private readonly TextBox minutes=new(){Name="ChestMinutesInput"};
    private readonly TextBlock feedback=Ui.Text("",12,"Muted");
    public ChestSpeedSettings(AppState state)
    {
        this.state=state;Children.Add(description);Children.Add(editor);editor.Children.Add(Ui.Text("自定义宝箱间隔（分钟）",13,"Muted"));editor.Children.Add(minutes);
        editor.Children.Add(Ui.Button("应用宝箱间隔",()=>{if(!double.TryParse(minutes.Text,out var value)||!double.IsFinite(value)||value<0.1||value>10080){feedback.Text="请填写 0.1–10080 之间的分钟数。";return;}if(state.Commit(()=>state.Engine.SetChestMinutes(value)))feedback.Text="已保存，当前宝箱完成比例保留。";else{minutes.Text=state.Data.ChestMinutes.ToString();feedback.Text="未保存，已恢复原间隔。";}}));editor.Children.Add(feedback);Refresh();
    }
    public void Refresh(){editor.Visibility=state.Engine.CanCustomizeChestInterval?Visibility.Visible:Visibility.Collapsed;if(!minutes.IsKeyboardFocusWithin)minutes.Text=state.Data.ChestMinutes.ToString();description.Text=$"当前已解锁 {state.Engine.UnlockedCount} 种，每 {state.Engine.ChestIntervalSeconds/60:0.##} 分钟获得一个宝箱。\n少于 50 种享 10 分钟加速；超过 50 种可自定义（默认 30 分钟）。睡眠和退出不计时。";}
}
