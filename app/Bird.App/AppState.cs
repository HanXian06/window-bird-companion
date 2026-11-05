using System.Text.Json;
using System.Windows.Threading;

namespace Bird.App;
public sealed class AppState
{
    public BirdCatalog Catalog {get;}
    public ProgressEngine Engine {get;}
    public SaveData Data=>Engine.Data;
    public BirdInfo Current=>Catalog.ById[Data.CurrentId];
    public ProgressStore Store {get;}
    public event Action? Changed;
    public event Action? ClockChanged;
    public bool Developer {get;private set;}
    public PetWindow? Pet {get;set;}
    private GuideWindow? guide;
    private SettingsWindow? settings;
    private FavoritesWindow? favorites;
    private readonly DispatcherTimer timer;
    private double lastTime=Native.ActiveSeconds, saveTime;
    private int clicks;
    private DateTime lastClick;
    public bool StorageFailed {get;private set;}
    private readonly Action<string> reportStorageError;
    public AppState(string resources,string savePath,Action<string>? reportStorageError=null)
    {
        this.reportStorageError=reportStorageError??(message=>MessageBox.Show(message,"无法保存",MessageBoxButton.OK,MessageBoxImage.Warning));
        Catalog=BirdCatalog.Load(resources); Store=new ProgressStore(savePath); Engine=new ProgressEngine(Catalog,Store.Load());
        Theme.Apply(Data.Dark);
        timer=new DispatcherTimer{Interval=TimeSpan.FromSeconds(1)}; timer.Tick+=Tick;
    }
    public void Start() { lastTime=Native.ActiveSeconds; timer.Start(); }
    public void Stop() { timer.Stop(); Save(); }
    public void Notify()=>Changed?.Invoke();
    public bool Commit(Action action)
    {
        var old=JsonSerializer.Serialize(Data);
        try { action(); Store.Save(Data); StorageFailed=false; Changed?.Invoke(); return true; }
        catch(Exception e) when(e is IOException or UnauthorizedAccessException) { Engine.Restore(JsonSerializer.Deserialize<SaveData>(old)!); StorageFailed=true; reportStorageError("进度保存失败，本次操作未生效。请检查文件权限或磁盘空间。\n"+e.Message); Changed?.Invoke(); return false; }
    }
    public bool Save()
    {
        try { Store.Save(Data); StorageFailed=false; return true; }
        catch(Exception e) when(e is IOException or UnauthorizedAccessException) { if(!StorageFailed) reportStorageError("进度尚未保存，请检查磁盘与权限后重试。\n"+e.Message); StorageFailed=true; return false; }
    }
    private void Tick(object? sender,EventArgs e)
    {
        var now=Native.ActiveSeconds; var elapsed=now-lastTime; lastTime=now;
        if(StorageFailed) { ClockChanged?.Invoke(); return; }
        var previous=Data.Chests; Engine.Advance(elapsed); saveTime+=elapsed;
        if(previous!=Data.Chests || saveTime>=30) { Save(); saveTime=0; if(previous!=Data.Chests) Notify(); }
        if(Data.AutoOpen && Data.Chests>0 && !Engine.Complete && !StorageFailed) OpenChest(false);
        if(!StorageFailed) { if(Data.RotationMinutes>0 && Data.RotationSeconds+elapsed>=Data.RotationMinutes*60) Commit(()=>Engine.AdvanceRotation(elapsed));else Engine.AdvanceRotation(elapsed); }
        ClockChanged?.Invoke();
    }
    public string ClockText=>StorageFailed?"保存失败，计时已暂停":Engine.Complete?"当前可收集鸟种已全部解锁":"下个宝箱  "+FormatCountdown(Engine.ChestIntervalSeconds-Data.Seconds);
    private static string FormatCountdown(double seconds){var total=(long)Math.Ceiling(Math.Max(0,seconds));return $"{total/60:00}:{total%60:00}";}
    public void Select(string id)=>Commit(()=>Engine.Select(id));
    public void Move(int direction)=>Commit(()=>Engine.Move(direction));
    public void RandomBird()=>Commit(Engine.RandomBird);
    public void ToggleFavorite(string id)=>Commit(()=>Engine.ToggleFavorite(id));
    public void OpenFavorites(){if(favorites==null){favorites=new FavoritesWindow(this);favorites.Closed+=(_,_)=>favorites=null;}favorites.Show();if(favorites.WindowState==WindowState.Minimized)favorites.WindowState=WindowState.Normal;favorites.Activate();}
    public void OpenGuide() { if(guide==null) { guide=new GuideWindow(this); guide.Closed+=(_,_)=>guide=null; } guide.ShowCurrent(); guide.Show(); if(guide.WindowState==WindowState.Minimized) guide.WindowState=WindowState.Normal; guide.Activate(); }
    public void OpenSettings(bool countClick=false)
    {
        if(countClick) { var now=DateTime.UtcNow; if(clicks==0 || (now-lastClick).TotalSeconds>3) {clicks=1;lastClick=now;} else clicks++; if(clicks>=5) { Developer=true; clicks=0; } }
        else clicks=0;
        if(settings==null) { settings=new SettingsWindow(this); settings.Closed+=(_,_)=>settings=null; }
        settings.RefreshDeveloper(); settings.Show(); settings.Activate();
    }
    public void ResetClickSequence()=>clicks=0;
    public void OpenChest(bool showResult=true)
    {
        if(Data.Chests<=0 || Engine.Complete) { if(showResult) MessageBox.Show(Engine.Complete?"当前可收集鸟种已全部解锁。":ClockText,"宝箱"); return; }
        List<BirdInfo> rewards=[];
        if(!Commit(()=>rewards=Engine.OpenChest()) || rewards.Count==0) return;
        if(showResult) ShowRewards(rewards);
    }
    private void ShowRewards(List<BirdInfo> rewards)
    {
        var panel=new StackPanel{Margin=new Thickness(26)};
        panel.Children.Add(Ui.Text("有新朋友来做客",26));
        panel.Children.Add(Ui.Text($"本次解锁 {rewards.Count} 种{rewards[0].RarityName}小鸟",14,"Muted"));
        var dialog=Ui.Dialog("宝箱里的新朋友",Ui.Scroll(panel),540,Math.Min(780,220+rewards.Count*115));
        foreach(var b in rewards) {
            var row=new DockPanel{Margin=new Thickness(0,12,0,5)};
            var image=new Image{Source=Images.Load(b),Width=100,Height=86,Margin=new Thickness(0,0,20,0)}; DockPanel.SetDock(image,Dock.Left); row.Children.Add(image);
            var text=new StackPanel(); text.Children.Add(Ui.Text(b.Name,18)); text.Children.Add(Ui.Latin(b.Scientific)); text.Children.Add(Ui.Button("设为桌宠",()=>Select(b.Id))); row.Children.Add(text); panel.Children.Add(row);
        }
        panel.Children.Add(Ui.Button("收好啦",()=>dialog.Close())); dialog.Show();
    }
    public void ShowHistory()
    {
        var p=new StackPanel{Margin=new Thickness(25)}; p.Children.Add(Ui.Text("最近的相遇",25));
        if(Data.History.Count==0) p.Children.Add(Ui.Text("还没有开过宝箱。让小鸟陪伴半小时，就能获得一个宝箱。",14,"Muted"));
        foreach(var entry in Data.History.AsEnumerable().Reverse().Take(50)) { p.Children.Add(Ui.Text(entry.At.LocalDateTime.ToString("MM-dd HH:mm"),12,"Muted")); p.Children.Add(Ui.Text(string.Join("、",entry.Ids.Select(id=>Catalog.ById.TryGetValue(id,out var b)?b.Name:id)))); }
        Ui.Dialog("解锁记录",Ui.Scroll(p)).Show();
    }
}
