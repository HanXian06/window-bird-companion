namespace Bird.App;
public sealed class SettingsWindow : Window
{
    private readonly AppState state;
    private readonly StackPanel developer=new();
    public SettingsWindow(AppState state)
    {
        SetResourceReference(StyleProperty,typeof(Window));
        this.state=state;Title="窗边小鸟 · 设置";Width=520;Height=730;MinWidth=420;MinHeight=450;WindowStartupLocation=WindowStartupLocation.CenterScreen;
        var p=new StackPanel{Margin=new Thickness(30,24,30,30)};Content=Ui.Scroll(p);
        p.Children.Add(Ui.Text("让陪伴刚刚好",27));p.Children.Add(Ui.Text("调整小鸟在桌面的样子",13,"Muted"));
        Ui.Heading(p,"桌面与外观");
        var top=new CheckBox{Content="让小鸟始终置顶",IsChecked=state.Data.Topmost};top.Click+=(_,_)=>{if(!state.Commit(()=>state.Data.Topmost=top.IsChecked==true))top.IsChecked=state.Data.Topmost;};p.Children.Add(top);
        var dnd=new CheckBox{Content="全屏免打扰（全屏时自动隐藏小鸟）",IsChecked=state.Data.FullscreenDnd};dnd.Click+=(_,_)=>{if(!state.Commit(()=>state.Data.FullscreenDnd=dnd.IsChecked==true))dnd.IsChecked=state.Data.FullscreenDnd;};p.Children.Add(dnd);
        p.Children.Add(Ui.Text("界面主题",13,"Muted"));var themes=new StackPanel{Orientation=Orientation.Horizontal};
        themes.Children.Add(Ui.Button("浅色 · 晨光",()=>SetTheme(false)));themes.Children.Add(Ui.Button("深色 · 夜林",()=>SetTheme(true)));p.Children.Add(themes);
        var sizeLabel=Ui.Text($"贴纸大小  {state.Data.Size:0} px",13,"Muted");sizeLabel.Margin=new Thickness(0,15,0,8);p.Children.Add(sizeLabel);
        var size=new Slider{Minimum=80,Maximum=600,Value=state.Data.Size,TickFrequency=10,IsSnapToTickEnabled=true,Margin=new Thickness(0,5,0,12)};size.ValueChanged+=(_,_)=>{sizeLabel.Text=$"贴纸大小  {size.Value:0} px";};
        size.PreviewMouseLeftButtonUp+=(_,_)=>state.Pet?.SetSize(size.Value);size.KeyUp+=(_,_)=>state.Pet?.SetSize(size.Value);p.Children.Add(size);
        var restore=new WrapPanel();restore.Children.Add(Ui.Button("恢复默认大小",()=>{state.Pet?.SetSize(220);size.Value=state.Data.Size;}));restore.Children.Add(Ui.Button("找回桌面小鸟",()=>state.Pet?.RestorePosition()));p.Children.Add(restore);
        Ui.Heading(p,"个性化陪伴");
        p.Children.Add(Ui.Text("每隔多少分钟随机切换（0 表示关闭）",13,"Muted"));
        var rotation=new TextBox{Text=state.Data.RotationMinutes.ToString(System.Globalization.CultureInfo.CurrentCulture),ToolTip="0 关闭，或 0.1–10080 分钟"};p.Children.Add(rotation);
        var feedback=Ui.Text("",12,"Muted");
        p.Children.Add(Ui.Button("应用切换间隔",()=>{if(!double.TryParse(rotation.Text,out var minutes)||!double.IsFinite(minutes)||(minutes!=0&&(minutes<0.1||minutes>10080))){feedback.Text="请填写 0，或 0.1–10080 之间的分钟数。";return;}if(state.Commit(()=>{state.Data.RotationMinutes=minutes;state.Data.RotationSeconds=0;}))feedback.Text=minutes==0?"已关闭定时切换":$"已设置：每 {minutes} 分钟切换一次";else{rotation.Text=state.Data.RotationMinutes.ToString();feedback.Text="未能保存，已恢复原间隔。";}}));p.Children.Add(feedback);
        p.Children.Add(Ui.Text("随机切换范围（手动与定时共用）",13,"Muted"));var reverting=false;var range=new ComboBox();range.Items.Add("全部已解锁");range.Items.Add("收藏的小鸟");range.SelectedIndex=state.Data.FavoritesOnly?1:0;range.SelectionChanged+=(_,_)=>{if(reverting)return;if(!state.Commit(()=>state.Data.FavoritesOnly=range.SelectedIndex==1)){reverting=true;range.SelectedIndex=state.Data.FavoritesOnly?1:0;reverting=false;}};p.Children.Add(range);
        p.Children.Add(Ui.Text("收藏为空或仅有当前小鸟时保持不变。切换后短暂展示名字与学名。",12,"Muted"));
        var opacityLabel=Ui.Text($"按钮框透明度  {state.Data.ToolbarOpacity:P0}",13,"Muted");p.Children.Add(opacityLabel);
        var opacity=new Slider{Minimum=0.1,Maximum=1,Value=state.Data.ToolbarOpacity,TickFrequency=0.05,IsSnapToTickEnabled=true,ToolTip="仅调整小鸟下方按钮框"};opacity.ValueChanged+=(_,_)=>opacityLabel.Text=$"按钮框透明度  {opacity.Value:P0}";void ApplyOpacity(){if(!state.Commit(()=>state.Data.ToolbarOpacity=opacity.Value))opacity.Value=state.Data.ToolbarOpacity;}opacity.PreviewMouseLeftButtonUp+=(_,_)=>ApplyOpacity();opacity.KeyUp+=(_,_)=>ApplyOpacity();p.Children.Add(opacity);
        p.Children.Add(Ui.Text("收藏页每行卡片数量",13,"Muted"));var columns=new ComboBox();foreach(var number in new[]{1,2,3,4})columns.Items.Add(number);columns.SelectedItem=state.Data.FavoriteColumns;columns.SelectionChanged+=(_,_)=>{if(reverting)return;if(columns.SelectedItem is int number && !state.Commit(()=>state.Data.FavoriteColumns=number)){reverting=true;columns.SelectedItem=state.Data.FavoriteColumns;reverting=false;}};p.Children.Add(columns);p.Children.Add(Ui.Button("打开收藏",state.OpenFavorites));
        Ui.Heading(p,"宝箱与收集");var auto=new CheckBox{Content="获得宝箱时自动开启",IsChecked=state.Data.AutoOpen};auto.Click+=(_,_)=>{if(!state.Commit(()=>state.Data.AutoOpen=auto.IsChecked==true))auto.IsChecked=state.Data.AutoOpen;};p.Children.Add(auto);
        var chestSpeed=new ChestSpeedSettings(state);p.Children.Add(chestSpeed);state.Changed+=chestSpeed.Refresh;Closed+=(_,_)=>state.Changed-=chestSpeed.Refresh;
        p.Children.Add(Ui.Text("普通 70%（5 种） / 稀有 25%（2 种） / 极其稀有 5%（1 种）",12,"Muted"));p.Children.Add(Ui.Button("查看最近解锁",state.ShowHistory));
        p.Children.Add(developer);RefreshDeveloper();
        Ui.Heading(p,"关于与来源");p.Children.Add(Ui.Text($"窗边小鸟 1.2\n资源版本 {state.Catalog.Version} / AviList v2026\n贴纸：花叔 huashu-bird-sticker，CC BY 4.0\n中文图鉴文字：CC BY-SA 4.0\n鸟名、分类、生态资料保留各自第三方许可。\n原始鸟类图像未修改，均为 AI 插画。",12,"Muted"));
        var sourceLinks=new WrapPanel();sourceLinks.Children.Add(Ui.Button("资源仓库",()=>GuideWindow.OpenLink("https://github.com/alchaincyf/huashu-bird-sticker")));sourceLinks.Children.Add(Ui.Button("许可与第三方说明",()=>ShowLicenses()));p.Children.Add(sourceLinks);
        p.Children.Add(Ui.Text("操作：单击贴纸翻看简介；拖动移动；Ctrl + 滚轮等比缩放；悬停显示鸟名、学名和工具栏。关闭此窗口后小鸟仍在托盘运行。",12,"Muted"));
        var bottom=new WrapPanel{Margin=new Thickness(0,18,0,0)};bottom.Children.Add(Ui.Button("完成",Close));bottom.Children.Add(Ui.Button("重试保存",()=>state.Save()));bottom.Children.Add(Ui.Button("退出桌宠",()=>{if(state.Save())Application.Current.Shutdown();}));p.Children.Add(bottom);
    }
    private void SetTheme(bool dark){if(state.Commit(()=>state.Data.Dark=dark))Theme.Apply(dark);}
    public void RefreshDeveloper()
    {
        developer.Children.Clear();developer.Visibility=state.Developer?Visibility.Visible:Visibility.Collapsed;if(!state.Developer)return;
        Ui.Heading(developer,"开发者模式");developer.Children.Add(Ui.Text("本次运行有效。以下操作会改变收集进度。",12,"Muted"));
        developer.Children.Add(Ui.Button("解锁全部可收藏鸟种",()=>{if(MessageBox.Show("解锁全部有贴纸且属于当前收藏范围的鸟种？此操作会保存进度。","解锁全部",MessageBoxButton.OKCancel,MessageBoxImage.Question)==MessageBoxResult.OK)state.Commit(state.Engine.UnlockAll);}));
        developer.Children.Add(Ui.Button("重置为初始四种",()=>{if(MessageBox.Show("保留珠颈斑鸠、白头鹎、夜鹭、麻雀，清空其他解锁、宝箱、计时和开箱记录。偏好设置保留，重置前备份存档。确定继续？","重置进度",MessageBoxButton.OKCancel,MessageBoxImage.Warning)==MessageBoxResult.OK)state.Commit(()=>{state.Store.BackupBeforeReset();state.Engine.Reset();});}));
    }
    private void ShowLicenses()
    {
        var p=new StackPanel{Margin=new Thickness(24)};
        foreach(var file in new[]{"LICENSE.md","NOTICE.md"}) {Ui.Heading(p,file=="LICENSE.md"?"资源许可":"第三方来源");p.Children.Add(new TextBox{Text=File.ReadAllText(Path.Combine(state.Catalog.Root,file)),IsReadOnly=true,TextWrapping=TextWrapping.Wrap,FontSize=12});}
        Ui.Dialog("许可与来源",Ui.Scroll(p),750,700).Show();
    }
}
