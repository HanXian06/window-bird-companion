using System.Windows.Input;
using System.Windows.Markup;

namespace Bird.App;
public sealed class FavoriteCard
{
    public BirdInfo Bird {get;}
    public string Name=>Bird.Name;
    public string Scientific=>Bird.Scientific;
    public string English=>Bird.English;
    public string Status=>Bird.RarityName+" / 已解锁";
    public string Date {get;}
    public bool Compact {get;}
    public double TitleSize=>Compact?17:22;
    public double LatinSize=>Compact?12:14;
    public double ImageHeight=>Compact?120:180;
    public Thickness CardPadding=>new(Compact?10:16);
    public ImageSource? Image=>Images.Load(Bird);
    public ICommand Select {get;}
    public ICommand Remove {get;}
    public ICommand Details {get;}
    public FavoriteCard(BirdInfo bird,AppState state,bool compact=false)
    {
        Compact=compact;
        Bird=bird;Date="相遇于 "+state.Data.Unlocks.First(x=>x.Id==bird.Id).At.LocalDateTime.ToString("yyyy.MM.dd");
        Select=new ActionCommand(()=>state.Select(bird.Id));Remove=new ActionCommand(()=>state.ToggleFavorite(bird.Id));Details=new ActionCommand(()=>{state.Select(bird.Id);state.OpenGuide();});
    }
}
public sealed record FavoriteCardRow(List<FavoriteCard> Cards,int Columns);
public sealed class ActionCommand(Action action) : ICommand
{
    public bool CanExecute(object? parameter)=>true;
    public void Execute(object? parameter)=>action();
    public event EventHandler? CanExecuteChanged {add{} remove{}}
}
public sealed class FavoritesWindow : Window
{
    private readonly AppState state;
    private readonly ListBox cards=new(){Name="FavoriteGrid"};
    private readonly TextBlock count=Ui.Text("",13,"Muted");
    private readonly TextBlock empty=Ui.Text("还没有收藏小鸟。\n单击桌宠贴纸，在名字右侧点 ♡，把喜欢的小鸟留在这里。",17,"Muted");
    public FavoritesWindow(AppState state)
    {
        this.state=state;SetResourceReference(StyleProperty,typeof(Window));Title="窗边小鸟 · 收藏";Width=1260;Height=800;MinWidth=820;MinHeight=500;WindowStartupLocation=WindowStartupLocation.CenterScreen;
        var root=new DockPanel{Margin=new Thickness(20)};Content=root;
        var header=new DockPanel{Margin=new Thickness(8,4,8,18)};DockPanel.SetDock(header,Dock.Top);root.Children.Add(header);
        var actions=new StackPanel{Orientation=Orientation.Horizontal};DockPanel.SetDock(actions,Dock.Right);header.Children.Add(actions);actions.Children.Add(Ui.Button("图鉴",state.OpenGuide));actions.Children.Add(Ui.Button("设置",()=>state.OpenSettings()));
        var title=new StackPanel();title.Children.Add(Ui.Text("喜欢的小鸟",27));title.Children.Add(count);header.Children.Add(title);
        var content=new Grid();content.Children.Add(cards);empty.HorizontalAlignment=HorizontalAlignment.Center;empty.VerticalAlignment=VerticalAlignment.Center;content.Children.Add(empty);root.Children.Add(content);
        cards.ItemContainerStyle=new Style(typeof(ListBoxItem), (Style)Application.Current.FindResource(typeof(ListBoxItem)));cards.ItemContainerStyle.Setters.Add(new Setter(Control.PaddingProperty,new Thickness(0)));cards.ItemContainerStyle.Setters.Add(new Setter(FrameworkElement.MarginProperty,new Thickness(0)));cards.ItemContainerStyle.Setters.Add(new Setter(UIElement.FocusableProperty,false));
        cards.ItemTemplate=(DataTemplate)XamlReader.Parse("""
        <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"><ItemsControl ItemsSource="{Binding Cards}"><ItemsControl.ItemsPanel><ItemsPanelTemplate><UniformGrid Columns="{Binding Columns}"/></ItemsPanelTemplate></ItemsControl.ItemsPanel><ItemsControl.ItemTemplate><DataTemplate><Border Background="{DynamicResource Surface}" CornerRadius="12" Padding="{Binding CardPadding}" Margin="7" MinHeight="230"><Grid><Grid.RowDefinitions><RowDefinition Height="Auto"/><RowDefinition Height="Auto"/></Grid.RowDefinitions><Grid.ColumnDefinitions><ColumnDefinition Width="2*"/><ColumnDefinition Width="3*"/></Grid.ColumnDefinitions><Image x:Name="art" Source="{Binding Image}" Stretch="Uniform" Height="{Binding ImageHeight}" Margin="0,0,12,0"/><StackPanel x:Name="info" Grid.Column="1" VerticalAlignment="Center"><DockPanel><Button Content="♥" Command="{Binding Remove}" ToolTip="取消收藏" Padding="7,3" DockPanel.Dock="Right"/><TextBlock Text="{Binding Name}" FontSize="{Binding TitleSize}" TextWrapping="Wrap" ToolTip="{Binding Name}" VerticalAlignment="Center"/></DockPanel><TextBlock Text="{Binding Scientific}" FontFamily="Georgia" FontStyle="Italic" FontSize="{Binding LatinSize}" Foreground="{DynamicResource Muted}" TextWrapping="Wrap" Margin="0,6,0,4"/><TextBlock Text="{Binding English}" FontSize="12" Foreground="{DynamicResource Muted}" TextWrapping="Wrap" Margin="0,0,0,7"/><TextBlock Text="{Binding Status}" FontSize="12" Foreground="{DynamicResource Accent}" TextWrapping="Wrap"/><TextBlock Text="{Binding Date}" FontSize="11" Foreground="{DynamicResource Muted}" Margin="0,5,0,9" TextWrapping="Wrap"/><WrapPanel><Button Content="设为桌宠" Command="{Binding Select}" Padding="9,6"/><Button Content="详细资料" Command="{Binding Details}" Padding="9,6"/></WrapPanel></StackPanel></Grid></Border><DataTemplate.Triggers><DataTrigger Binding="{Binding Compact}" Value="True"><Setter TargetName="art" Property="Grid.ColumnSpan" Value="2"/><Setter TargetName="art" Property="Margin" Value="0,0,0,10"/><Setter TargetName="info" Property="Grid.Row" Value="1"/><Setter TargetName="info" Property="Grid.Column" Value="0"/><Setter TargetName="info" Property="Grid.ColumnSpan" Value="2"/></DataTrigger></DataTemplate.Triggers></DataTemplate></ItemsControl.ItemTemplate></ItemsControl></DataTemplate>
        """);
        state.Changed+=Refresh;SizeChanged+=(_,e)=>{if(e.WidthChanged)Refresh();};Closed+=(_,_)=>state.Changed-=Refresh;Refresh();
    }
    private void Refresh()
    {
        var favorites=state.Data.FavoriteIds.ToHashSet();var birds=state.Engine.UnlockedBirds.Where(b=>favorites.Contains(b.Id)).ToList();var columns=state.Data.FavoriteColumns;
        var compact=columns>=3 || (ActualWidth>0?ActualWidth:Width)/columns<420;
        cards.ItemsSource=birds.Chunk(columns).Select(group=>new FavoriteCardRow(group.Select(b=>new FavoriteCard(b,state,compact)).ToList(),columns)).ToList();
        count.Text=$"收藏了 {birds.Count} 位朋友    每行 {columns} 张";empty.Visibility=birds.Count==0?Visibility.Visible:Visibility.Collapsed;
    }
}
