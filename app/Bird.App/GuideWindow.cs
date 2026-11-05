using System.Text.Json;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Threading;

namespace Bird.App;
public sealed class BirdRow
{
    public BirdInfo Bird {get;init;}=null!;
    public bool Unlocked {get;init;}
    public string Name=>Bird.Name;
    public string Subtitle=>Unlocked?Bird.Scientific:Bird.Exclusion.Length>0?"未解锁 / "+Bird.Exclusion:"未解锁";
    public ImageSource? Thumbnail=>Unlocked?Images.Load(Bird):null;
    public string LockMark=>Unlocked?"":"◇";
}

public sealed class GuideWindow : Window
{
    private readonly AppState state;
    private readonly Grid layout=new();
    private readonly Border sidebar;
    private readonly TextBox search=new(){ToolTip="搜索中文名或拉丁学名"};
    private readonly ComboBox sorting=new();
    private readonly ListBox list=new();
    private readonly TreeView tree=new();
    private readonly TextBlock totals=Ui.Text("",12,"Muted");
    private readonly StackPanel detail=new(){Margin=new Thickness(32,12,32,32)};
    private readonly ScrollViewer detailScroll;
    private readonly DispatcherTimer debounce=new(){Interval=TimeSpan.FromMilliseconds(230)};
    private bool all, rebuilding;
    private BirdInfo selected;
    private string currentId;
    private HashSet<string> unlocked=[];
    public GuideWindow(AppState state)
    {
        SetResourceReference(StyleProperty,typeof(Window));
        this.state=state;selected=state.Current;currentId=state.Current.Id;
        Title="窗边小鸟 · 图鉴";Width=1080;Height=790;MinWidth=820;MinHeight=550;WindowStartupLocation=WindowStartupLocation.CenterScreen;
        var outer=new DockPanel();Content=outer;
        var header=new DockPanel{Margin=new Thickness(24,20,24,12)};DockPanel.SetDock(header,Dock.Top);outer.Children.Add(header);
        var actions=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right};DockPanel.SetDock(actions,Dock.Right);header.Children.Add(actions);
        actions.Children.Add(Ui.Button("收起 / 展开目录",()=>{var hide=sidebar!.Visibility==Visibility.Visible;sidebar.Visibility=hide?Visibility.Collapsed:Visibility.Visible;layout.ColumnDefinitions[0].Width=new GridLength(hide?0:290);}));
        actions.Children.Add(Ui.Button("收藏",state.OpenFavorites));actions.Children.Add(Ui.Button("解锁记录",state.ShowHistory));actions.Children.Add(Ui.Button("设置",()=>state.OpenSettings()));
        var brand=new StackPanel();brand.Children.Add(Ui.Text("窗边小鸟",25));brand.Children.Add(Ui.Text("一页图鉴，一次相遇",12,"Muted"));header.Children.Add(brand);
        outer.Children.Add(layout);layout.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(290)});layout.ColumnDefinitions.Add(new ColumnDefinition());
        var side=new DockPanel{Margin=new Thickness(16,8,14,16)};sidebar=new Border{Child=side};sidebar.SetResourceReference(Border.BackgroundProperty,"Surface");layout.Children.Add(sidebar);
        var controls=new StackPanel();DockPanel.SetDock(controls,Dock.Top);side.Children.Add(controls);
        controls.Children.Add(Ui.Text("鸟类图鉴",18));controls.Children.Add(search);
        var tabs=new StackPanel{Orientation=Orientation.Horizontal};var unlockedButton=Ui.Button("已解锁",()=>{all=false;Rebuild();});var allButton=Ui.Button("全部图鉴",()=>{all=true;Rebuild();});tabs.Children.Add(unlockedButton);tabs.Children.Add(allButton);controls.Children.Add(tabs);
        controls.Children.Add(Ui.Text("排序",12,"Muted"));foreach(var s in new[]{"解锁顺序","中文拼音","拉丁学名","界门纲目分类"})sorting.Items.Add(s);sorting.SelectedIndex=0;controls.Children.Add(sorting);
        totals.Margin=new Thickness(0,12,0,0);DockPanel.SetDock(totals,Dock.Bottom);side.Children.Add(totals);
        var directory=new Grid();directory.Children.Add(list);directory.Children.Add(tree);side.Children.Add(directory);
        list.ItemTemplate=(DataTemplate)XamlReader.Parse("""
        <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"><Grid><Grid.ColumnDefinitions><ColumnDefinition Width="54"/><ColumnDefinition Width="*"/></Grid.ColumnDefinitions><Grid Width="48" Height="50"><Image Source="{Binding Thumbnail}" Stretch="Uniform"/><TextBlock Text="{Binding LockMark}" HorizontalAlignment="Center" VerticalAlignment="Center" FontSize="25" Foreground="{DynamicResource Muted}"/></Grid><StackPanel Grid.Column="1" VerticalAlignment="Center" Margin="8,0,0,0"><TextBlock Text="{Binding Name}" FontSize="14" TextTrimming="CharacterEllipsis"/><TextBlock Text="{Binding Subtitle}" FontSize="11" Foreground="{DynamicResource Muted}" TextTrimming="CharacterEllipsis" Margin="0,4,0,0"/></StackPanel></Grid></DataTemplate>
        """);
        list.SelectionChanged+=(_,_)=>{if(!rebuilding && list.SelectedItem is BirdRow row) Choose(row.Bird);};
        tree.SelectedItemChanged+=(_,e)=>{if(e.NewValue is TreeViewItem item && item.Tag is BirdInfo b)Choose(b);};
        detailScroll=Ui.Scroll(detail);Grid.SetColumn(detailScroll,1);layout.Children.Add(detailScroll);
        search.TextChanged+=(_,_)=>{debounce.Stop();debounce.Start();};debounce.Tick+=(_,_)=>{debounce.Stop();Rebuild();};sorting.SelectionChanged+=(_,_)=>Rebuild();
        state.Changed+=RefreshState;Closed+=(_,_)=>{state.Changed-=RefreshState;debounce.Stop();};
        Closing+=(_,e)=>{if(!Application.Current.Dispatcher.HasShutdownStarted){e.Cancel=true;Hide();}};
        Rebuild();RenderDetail();
    }
    public void ShowCurrent(){selected=state.Current;currentId=state.Data.CurrentId;rebuilding=true;list.SelectedItem=list.Items.Cast<BirdRow>().FirstOrDefault(r=>r.Bird.Id==currentId);rebuilding=false;RenderDetail();detailScroll.ScrollToTop();}
    private void RefreshState(){if(currentId!=state.Data.CurrentId){currentId=state.Data.CurrentId;selected=state.Current;}Rebuild();RenderDetail();}
    private void Choose(BirdInfo b){selected=b;if(unlocked.Contains(b.Id))state.Select(b.Id);else RenderDetail();detailScroll.ScrollToTop();}
    private void Rebuild()
    {
        if(sidebar==null)return;
        rebuilding=true;unlocked=state.Engine.UnlockedIds;
        var query=search.Text.Trim();IEnumerable<BirdInfo> birds=state.Catalog.Birds.Where(b=>all||unlocked.Contains(b.Id));
        if(query.Length>0)birds=birds.Where(b=>b.Name.Contains(query,StringComparison.OrdinalIgnoreCase)||b.Scientific.Contains(query,StringComparison.OrdinalIgnoreCase)||b.English.Contains(query,StringComparison.OrdinalIgnoreCase));
        var sequences=state.Data.Unlocks.ToDictionary(x=>x.Id,x=>x.Sequence);
        birds=sorting.SelectedIndex switch {1=>birds.OrderBy(b=>b.Pinyin,StringComparer.OrdinalIgnoreCase).ThenBy(b=>b.Id,StringComparer.Ordinal),2=>birds.OrderBy(b=>b.Scientific,StringComparer.OrdinalIgnoreCase).ThenBy(b=>b.Id,StringComparer.Ordinal),3=>birds.OrderBy(b=>b.Sequence),_=>birds.OrderBy(b=>sequences.GetValueOrDefault(b.Id,long.MaxValue)).ThenBy(b=>b.Scientific,StringComparer.OrdinalIgnoreCase)};
        var items=birds.ToList();tree.Visibility=sorting.SelectedIndex==3?Visibility.Visible:Visibility.Collapsed;list.Visibility=sorting.SelectedIndex==3?Visibility.Collapsed:Visibility.Visible;
        if(sorting.SelectedIndex==3) BuildTree(items);else {list.ItemsSource=items.Select(b=>new BirdRow{Bird=b,Unlocked=unlocked.Contains(b.Id)}).ToList();list.SelectedItem=list.Items.Cast<BirdRow>().FirstOrDefault(x=>x.Bird.Id==selected.Id);}
        totals.Text=$"已解锁 {state.Engine.UnlockedBirds.Count():N0} / 可收集 {state.Catalog.Birds.Count(b=>b.Collectible):N0}\n全部图鉴 {state.Catalog.Birds.Count:N0} 种    当前显示 {items.Count:N0} 种";
        rebuilding=false;
    }
    private void BuildTree(List<BirdInfo> birds)
    {
        tree.Items.Clear();var kingdom=new TreeViewItem{Header="动物界",IsExpanded=true};var phylum=new TreeViewItem{Header="脊索动物门",IsExpanded=true};var aves=new TreeViewItem{Header="鸟纲",IsExpanded=true};kingdom.Items.Add(phylum);phylum.Items.Add(aves);tree.Items.Add(kingdom);
        foreach(var order in birds.GroupBy(b=>b.Order)) {
            var orderItem=new TreeViewItem{Header=$"{order.First().OrderZh}  ({order.Count()})"};aves.Items.Add(orderItem);
            foreach(var family in order.GroupBy(b=>b.Family)) {
                var familyBirds=family.ToList();var familyItem=new TreeViewItem{Header=$"{family.First().FamilyZh}  ({family.Count()})"};orderItem.Items.Add(familyItem);familyItem.Items.Add(new TreeViewItem{Header="展开查看"});
                familyItem.Expanded+=(_,e)=>{if(e.OriginalSource!=familyItem || familyItem.Tag is true)return;familyItem.Tag=true;familyItem.Items.Clear();foreach(var genus in familyBirds.GroupBy(b=>b.Genus)){var genusItem=new TreeViewItem{Header=genus.Key,IsExpanded=true};familyItem.Items.Add(genusItem);foreach(var b in genus)genusItem.Items.Add(new TreeViewItem{Header=(unlocked.Contains(b.Id)?"":"◇ ")+b.Name,Tag=b,ToolTip=b.Scientific});}};
            }
        }
    }
    private void RenderDetail()
    {
        detail.Children.Clear();var b=selected;var isUnlocked=unlocked.Contains(b.Id);
        var hero=new Grid{Margin=new Thickness(0,0,0,16)};hero.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(245)});hero.ColumnDefinitions.Add(new ColumnDefinition());
        if(isUnlocked)hero.Children.Add(new Image{Source=Images.Load(b),Width=230,Height=235,Stretch=Stretch.Uniform});
        else {var placeholder=Ui.Text("◇",80,"Muted");placeholder.VerticalAlignment=VerticalAlignment.Center;placeholder.HorizontalAlignment=HorizontalAlignment.Center;hero.Children.Add(placeholder);}
        var name=new StackPanel{VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(18,0,0,0)};Grid.SetColumn(name,1);hero.Children.Add(name);
        name.Children.Add(Ui.Text(b.Name,32));if(isUnlocked)name.Children.Add(Ui.Button(state.Data.FavoriteIds.Contains(b.Id)?"♥ 已收藏":"♡ 加入收藏",()=>state.ToggleFavorite(b.Id)));name.Children.Add(Ui.Latin(b.Scientific,18));name.Children.Add(Ui.Text(b.English,14,"Muted"));name.Children.Add(Ui.Text(isUnlocked?b.RarityName+" / 已解锁":"尚未解锁",14,"Accent"));
        if(isUnlocked){var when=state.Data.Unlocks.FirstOrDefault(x=>x.Id==b.Id);if(when!=null)name.Children.Add(Ui.Text("相遇于 "+when.At.LocalDateTime.ToString("yyyy.MM.dd"),12,"Muted"));}
        detail.Children.Add(hero);
        if(!isUnlocked){detail.Children.Add(Ui.Panel(Ui.Text(b.Exclusion.Length>0?$"{b.Exclusion}。当前暂不加入宝箱奖池。":"让小鸟在桌面陪伴你，每半小时获得一个宝箱，慢慢遇见新朋友。")));Ui.Heading(detail,"分类");detail.Children.Add(Ui.Text(b.Taxonomy));return;}
        detail.Children.Add(Ui.Panel(Ui.Text(b.Summary,16),22));
        if(b.Notes.ValueKind==JsonValueKind.Object){Ui.Heading(detail,"认识这位朋友");foreach(var field in b.Notes.EnumerateObject()){if(field.Name=="impression")continue;var label=field.Name switch{"marks"=>"辨识特征","sexAge"=>"雌雄与幼鸟","similar"=>"相似鸟种","funFact"=>"一个小知识","call"=>"声音描述","basedOn"=>"文字依据",_=>field.Name};AddField(label,Value(field.Value));}}
        else {Ui.Heading(detail,"辨识介绍");detail.Children.Add(Ui.Text("该鸟种暂无中文辨识文字，下方保留已有结构化资料与原文。",14,"Muted"));}
        Ui.Heading(detail,"分类与分布");AddField("分类",b.Taxonomy);var range=b.Raw.Field("range");AddField("中文分布",range.At("zh-Hans"));AddField("分布原文",range.At("text"));
        Ui.Heading(detail,"形态与生态");var traits=b.Raw.Field("traits");
        foreach(var (key,label,unit) in new[]{("massG","体重"," g"),("wingMm","翼长"," mm"),("tailMm","尾长"," mm"),("tarsusMm","跗蹠长"," mm"),("beakCulmenMm","嘴峰长"," mm"),("habitat","栖息地",""),("migration","迁徙",""),("trophicLevel","营养级",""),("trophicNiche","食性",""),("primaryLifestyle","生活方式","")}){var value=traits.At(key);AddField(label,value.Length>0?Translate(value)+unit:"");}
        Ui.Heading(detail,"保护信息");var con=b.Raw.Field("conservation");AddField("IUCN",Iucn(con.At("iucn")));AddField("CITES",con.At("cites"));AddField("中国保护字段",con.At("chinaProtection") switch{"Class I"=>"国家一级","Class II"=>"国家二级",var v=>v});
        detail.Children.Add(Ui.Text("收藏稀有度为本软件游戏分级；保护信息为资源快照，请以对应机构现行资料为准。",12,"Muted"));
        Ui.Heading(detail,"图像与资料来源");detail.Children.Add(Ui.Text($"huashu-bird-sticker v{state.Catalog.Version} / 花叔\n鸟类图像为 AI 插画，按原图展示。贴纸 CC BY 4.0；中文图鉴文字 CC BY-SA 4.0。",12,"Muted"));
        var source=b.Raw.Field("sticker").Field("reference");AddField("参考作者",source.At("author"));AddField("参考页面",source.At("page"));AddField("文字来源",b.Notes.At("basedOn"));
        var links=new WrapPanel();links.Children.Add(Ui.Button("打开资源仓库",()=>OpenLink("https://github.com/alchaincyf/huashu-bird-sticker")));links.Children.Add(Ui.Button("来源与许可",()=>state.OpenSettings()));detail.Children.Add(links);
        var rawText=JsonSerializer.Serialize(new {species=b.Raw,fieldGuide=b.Notes.ValueKind==JsonValueKind.Undefined?(JsonElement?)null:b.Notes},ProgressStore.JsonOptions);
        var raw=new TextBox{Text=rawText,IsReadOnly=true,TextWrapping=TextWrapping.Wrap,FontSize=12,MaxHeight=420,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
        detail.Children.Add(new Expander{Header="完整原始数据（可选中复制）",Content=raw});
    }
    private void AddField(string label,string value){var row=new StackPanel{Margin=new Thickness(0,0,0,10)};row.Children.Add(Ui.Text(label,12,"Muted"));row.Children.Add(Ui.Text(string.IsNullOrWhiteSpace(value)?"暂无资料":value));detail.Children.Add(row);}
    private static string Value(JsonElement e)=>e.ValueKind==JsonValueKind.Array?string.Join("\n",e.EnumerateArray().Select(Value)):e.ValueKind==JsonValueKind.Object?JsonSerializer.Serialize(e,ProgressStore.JsonOptions):e.Text();
    private static string Translate(string v)=>v switch{"sedentary"=>"留鸟","migratory"=>"候鸟","partially migratory"=>"部分迁徙","Woodland"=>"疏林","Forest"=>"森林","Wetland"=>"湿地","Grassland"=>"草地","Granivore"=>"食种子","Invertivore"=>"食无脊椎动物","Omnivore"=>"杂食","Herbivore"=>"植食","Carnivore"=>"肉食","Terrestrial"=>"地面活动","Insessorial"=>"栖枝活动",_=>v};
    private static string Iucn(string v)=>v switch{"LC"=>"LC / 无危","NT"=>"NT / 近危","VU"=>"VU / 易危","EN"=>"EN / 濒危","CR"=>"CR / 极危","DD"=>"DD / 数据缺乏",_=>v};
    public static void OpenLink(string url){try{System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url){UseShellExecute=true});}catch(Exception e){MessageBox.Show("无法打开链接。\n"+e.Message,"打开链接");}}
}
