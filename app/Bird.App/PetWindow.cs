using System.Windows.Input;
using System.Windows.Threading;

namespace Bird.App;
public sealed class PetWindow : Window
{
    private readonly AppState state;
    private readonly Grid body=new();
    private readonly AlphaImage bird=new(){Stretch=Stretch.Uniform,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center};
    private readonly Image previousBird=new(){Stretch=Stretch.Uniform,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center,IsHitTestVisible=false,Visibility=Visibility.Collapsed};
    private readonly PetTransitions transitions;
    private readonly Border card;
    private readonly StackPanel cardText=new();
    private readonly Border toolbar;
    private readonly Border switchName;
    private readonly StackPanel switchText=new();
    private readonly TextBlock clock=Ui.Text("",11,"Muted");
    private readonly Button chest;
    private readonly DispatcherTimer hideTimer=new(){Interval=TimeSpan.FromMilliseconds(800)};
    private readonly DispatcherTimer positionTimer=new(){Interval=TimeSpan.FromMilliseconds(500)};
    private readonly DispatcherTimer fullscreenTimer=new(){Interval=TimeSpan.FromMilliseconds(500)};
    private readonly FullscreenVisibility visibilityPolicy=new();
    private Point start;
    private bool pressed, brief;
    private string lastBird="";
    public PetWindow(AppState state)
    {
        SetResourceReference(StyleProperty,typeof(Window));
        ShowActivated=false;
        this.state=state; Title="窗边小鸟"; WindowStyle=WindowStyle.None; AllowsTransparency=true; Background=Brushes.Transparent; ResizeMode=ResizeMode.NoResize; ShowInTaskbar=false;
        var root=new Grid(); root.RowDefinitions.Add(new RowDefinition()); root.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});
        Content=root; root.Children.Add(body); body.Children.Add(bird);body.Children.Add(previousBird);
        card=Ui.Panel(cardText,20); card.MaxWidth=340; card.Margin=new Thickness(12,0,12,0); card.VerticalAlignment=VerticalAlignment.Center; body.Children.Add(card);
        switchName=Ui.Panel(switchText,9);switchName.HorizontalAlignment=HorizontalAlignment.Center;switchName.VerticalAlignment=VerticalAlignment.Top;switchName.MaxWidth=310;switchName.IsHitTestVisible=false;switchName.Visibility=Visibility.Collapsed;body.Children.Add(switchName);
        transitions=new PetTransitions(bird,previousBird,switchName);
        var tools=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Center};
        void Tool(string label,Action action,string tip) { var button=Ui.Button(label,()=>{state.ResetClickSequence(); action();},tip); button.Padding=new Thickness(10,8,10,8); button.Margin=new Thickness(1,0,1,0); tools.Children.Add(button); }
        Tool("图鉴",state.OpenGuide,"当前小鸟的详细资料"); Tool("‹",()=>state.Move(-1),"上一只已解锁小鸟"); Tool("›",()=>state.Move(1),"下一只已解锁小鸟"); Tool("随机",state.RandomBird,"随机选择已解锁小鸟");
        var settings=Ui.Button("设置",()=>state.OpenSettings(true),"连续点击 5 次进入开发者模式"); settings.Padding=new Thickness(10,8,10,8); settings.Margin=new Thickness(1,0,1,0); tools.Children.Add(settings);
        chest=Ui.Button("宝箱",()=>{state.ResetClickSequence(); state.OpenChest();}); chest.Padding=new Thickness(10,8,10,8); chest.Margin=new Thickness(1,0,1,0); tools.Children.Add(chest);
        var tray=new StackPanel(); tray.Children.Add(tools); clock.HorizontalAlignment=HorizontalAlignment.Center; clock.Margin=new Thickness(0,7,0,0); tray.Children.Add(clock);
        toolbar=Ui.Panel(tray,8); toolbar.HorizontalAlignment=HorizontalAlignment.Center; toolbar.Margin=new Thickness(0,3,0,6); toolbar.Visibility=Visibility.Hidden; Grid.SetRow(toolbar,1); root.Children.Add(toolbar);
        hideTimer.Tick+=(_,_)=>{hideTimer.Stop();if(!IsMouseOver)toolbar.Visibility=Visibility.Hidden;};
        MouseEnter+=(_,_)=>{hideTimer.Stop();toolbar.Visibility=Visibility.Visible;}; MouseLeave+=(_,_)=>hideTimer.Start();
        body.MouseLeftButtonDown+=(_,e)=>{ if(FindButton(e.OriginalSource as DependencyObject)!=null) return; start=e.GetPosition(this); pressed=true; body.CaptureMouse(); e.Handled=true; };
        body.MouseMove+=(_,e)=>{if(!pressed || e.LeftButton!=MouseButtonState.Pressed)return; var point=e.GetPosition(this);if(Math.Abs(point.X-start.X)<SystemParameters.MinimumHorizontalDragDistance && Math.Abs(point.Y-start.Y)<SystemParameters.MinimumVerticalDragDistance)return; pressed=false;body.ReleaseMouseCapture();DragMove();Native.Clamp(this);PersistPosition();};
        body.MouseLeftButtonUp+=(_,e)=>{if(!pressed)return;pressed=false;body.ReleaseMouseCapture();brief=!brief;RefreshView();e.Handled=true;};
        body.LostMouseCapture+=(_,_)=>pressed=false;
        PreviewMouseWheel+=(_,e)=>{if((Keyboard.Modifiers&ModifierKeys.Control)==0)return;SetSize(state.Data.Size+(e.Delta>0?20:-20));e.Handled=true;};
        KeyDown+=(_,e)=>{if(e.Key==Key.Left)state.Move(-1);if(e.Key==Key.Right)state.Move(1);if(e.Key==Key.Escape){brief=false;RefreshView();}};
        positionTimer.Tick+=(_,_)=>{positionTimer.Stop();PersistPosition();};
        LocationChanged+=(_,_)=>{if(!IsLoaded)return;positionTimer.Stop();positionTimer.Start();};
        state.Changed+=RefreshView; state.ClockChanged+=RefreshClock;
        Loaded+=(_,_)=>{if(state.Data.Left.HasValue && state.Data.Top.HasValue){Left=state.Data.Left.Value;Top=state.Data.Top.Value;}else{Left=SystemParameters.WorkArea.Right-Width-45;Top=SystemParameters.WorkArea.Bottom-Height-30;} Native.Clamp(this);fullscreenTimer.Start();SyncVisibility();};
        fullscreenTimer.Tick+=(_,_)=>SyncVisibility();
        Microsoft.Win32.SystemEvents.DisplaySettingsChanged+=DisplayChanged;
        DpiChanged+=(_,_)=>Dispatcher.BeginInvoke(()=>Native.Clamp(this));
        Closed+=(_,_)=>{Microsoft.Win32.SystemEvents.DisplaySettingsChanged-=DisplayChanged;state.Changed-=RefreshView;state.ClockChanged-=RefreshClock;positionTimer.Stop();hideTimer.Stop();fullscreenTimer.Stop();transitions.Cancel();};
        Closing+=(_,e)=>{if(!Application.Current.Dispatcher.HasShutdownStarted){e.Cancel=true;visibilityPolicy.SetManualHidden(true);Hide();}};
        RefreshView();
    }
    private static Button? FindButton(DependencyObject? d) { while(d!=null){if(d is Button b)return b;d=VisualTreeHelper.GetParent(d);}return null; }
    private void DisplayChanged(object? sender,EventArgs e)=>Dispatcher.BeginInvoke(()=>Native.Clamp(this));
    public void SetSize(double size)=>state.Commit(()=>state.Data.Size=Math.Clamp(size,80,Math.Min(600,Math.Max(80,SystemParameters.WorkArea.Height-160))));
    public void RestorePosition(){Left=SystemParameters.WorkArea.Right-Width-45;Top=SystemParameters.WorkArea.Bottom-Height-30;Native.Clamp(this);PersistPosition();ShowByUser();}
    public void ShowByUser(){visibilityPolicy.SetManualHidden(false);SyncVisibility();}
    public void ToggleUserVisibility(){visibilityPolicy.SetManualHidden(IsVisible);SyncVisibility();}
    private void SyncVisibility(){var show=visibilityPolicy.ShouldShow(state.Data.FullscreenDnd,Native.OtherAppFullscreen());if(show && !IsVisible)Show();else if(!show && IsVisible){transitions.Cancel();Hide();}}
    private void PersistPosition(){state.Data.Left=Left;state.Data.Top=Top;state.Save();}
    public void RefreshView()
    {
        if(lastBird!=state.Current.Id){var switched=lastBird.Length>0;brief=false;lastBird=state.Current.Id;transitions.ChangeBird(Images.Load(state.Current),switched);if(switched){switchText.Children.Clear();switchText.Children.Add(Ui.Text(state.Current.Name,16));switchText.Children.Add(Ui.Latin(state.Current.Scientific,13));transitions.ShowName();}}
        toolbar.Opacity=state.Data.ToolbarOpacity;
        Topmost=state.Data.Topmost;Width=Math.Max(330,state.Data.Size+30);Height=Math.Max(240,state.Data.Size)+98;bird.Width=state.Data.Size;bird.Height=state.Data.Size;
        previousBird.Width=state.Data.Size;previousBird.Height=state.Data.Size;
        bird.Source=Images.Load(state.Current);
        bird.Visibility=brief || bird.Source==null?Visibility.Collapsed:Visibility.Visible;
        card.Visibility=brief || bird.Source==null?Visibility.Visible:Visibility.Collapsed;
        cardText.Children.Clear();var heading=new DockPanel();var favorite=Ui.Button(state.Data.FavoriteIds.Contains(state.Current.Id)?"♥":"♡",()=>state.ToggleFavorite(state.Current.Id),"加入 / 取消收藏");favorite.Padding=new Thickness(8,2,8,2);favorite.SetResourceReference(Button.ForegroundProperty,"Accent");DockPanel.SetDock(favorite,Dock.Right);heading.Children.Add(favorite);heading.Children.Add(Ui.Text(state.Current.Name,23));cardText.Children.Add(heading);cardText.Children.Add(Ui.Latin(state.Current.Scientific));cardText.Children.Add(Ui.Text(bird.Source==null?"贴纸读取失败，请检查资源文件。":state.Current.Summary,14));cardText.Children.Add(Ui.Text($"{state.Current.FamilyZh}   {state.Current.RarityName}",12,"Muted"));cardText.Children.Add(Ui.Button("查看详细资料",state.OpenGuide));
        var name=new StackPanel();name.Children.Add(Ui.Text(state.Current.Name,17));name.Children.Add(Ui.Latin(state.Current.Scientific)); bird.ToolTip=new ToolTip{Content=name,Placement=System.Windows.Controls.Primitives.PlacementMode.Top};
        ToolTipService.SetInitialShowDelay(bird,180);ToolTipService.SetShowDuration(bird,60000);
        RefreshClock();if(IsLoaded){Native.Clamp(this);SyncVisibility();}
    }
    private void RefreshClock(){clock.Text=state.ClockText;chest.Content=state.Data.Chests>0?$"宝箱 {state.Data.Chests}":"宝箱";chest.ToolTip=state.Engine.Complete?"当前收藏已完成":state.Data.Chests>0?"打开一个宝箱":state.ClockText;}
}
