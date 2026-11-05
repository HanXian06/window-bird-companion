using System.Windows.Markup;

namespace Bird.App;
public static class Theme
{
    public static void Install()
    {
        var styles=(ResourceDictionary)XamlReader.Parse("""
        <ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
          <Style TargetType="Window"><Setter Property="Icon" Value="pack://application:,,,/BirdCompanion;component/Assets/bird.ico"/><Setter Property="FontFamily" Value="Microsoft YaHei UI"/><Setter Property="FontSize" Value="14"/><Setter Property="Foreground" Value="{DynamicResource Ink}"/><Setter Property="Background" Value="{DynamicResource Paper}"/></Style>
          <Style TargetType="Button">
            <Setter Property="Foreground" Value="{DynamicResource Ink}"/><Setter Property="Background" Value="{DynamicResource Wash}"/><Setter Property="BorderThickness" Value="0"/><Setter Property="Padding" Value="14,9"/><Setter Property="Margin" Value="0,3,7,3"/><Setter Property="Cursor" Value="Hand"/><Setter Property="MinHeight" Value="36"/>
            <Setter Property="Template"><Setter.Value><ControlTemplate TargetType="Button"><Border x:Name="shell" Background="{TemplateBinding Background}" CornerRadius="8" Padding="{TemplateBinding Padding}"><ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/></Border><ControlTemplate.Triggers><Trigger Property="IsMouseOver" Value="True"><Setter TargetName="shell" Property="Background" Value="{DynamicResource Hover}"/></Trigger><Trigger Property="IsKeyboardFocused" Value="True"><Setter TargetName="shell" Property="BorderBrush" Value="{DynamicResource Accent}"/><Setter TargetName="shell" Property="BorderThickness" Value="2"/></Trigger><Trigger Property="IsEnabled" Value="False"><Setter Property="Opacity" Value="0.45"/></Trigger><Trigger Property="IsPressed" Value="True"><Setter TargetName="shell" Property="Opacity" Value="0.7"/></Trigger></ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter>
          </Style>
          <Style TargetType="TextBox"><Setter Property="Foreground" Value="{DynamicResource Ink}"/><Setter Property="Background" Value="{DynamicResource Surface}"/><Setter Property="CaretBrush" Value="{DynamicResource Ink}"/><Setter Property="BorderBrush" Value="{DynamicResource Line}"/><Setter Property="Padding" Value="9,8"/><Setter Property="Margin" Value="0,4,0,8"/></Style>
          <Style TargetType="CheckBox"><Setter Property="Foreground" Value="{DynamicResource Ink}"/><Setter Property="Margin" Value="0,10,0,10"/><Setter Property="VerticalContentAlignment" Value="Center"/></Style>
          <Style TargetType="ComboBoxItem"><Setter Property="Foreground" Value="{DynamicResource Ink}"/><Setter Property="Padding" Value="9,8"/><Setter Property="Template"><Setter.Value><ControlTemplate TargetType="ComboBoxItem"><Border x:Name="option" Background="{DynamicResource Surface}" Padding="{TemplateBinding Padding}"><ContentPresenter/></Border><ControlTemplate.Triggers><Trigger Property="IsHighlighted" Value="True"><Setter TargetName="option" Property="Background" Value="{DynamicResource Hover}"/></Trigger></ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter></Style>
          <Style TargetType="ComboBox"><Setter Property="Foreground" Value="{DynamicResource Ink}"/><Setter Property="Margin" Value="0,4,0,8"/><Setter Property="Template"><Setter.Value><ControlTemplate TargetType="ComboBox"><Grid><Border Background="{DynamicResource Wash}" CornerRadius="7" Padding="10"><DockPanel><TextBlock Text="⌄" DockPanel.Dock="Right" Margin="10,0,0,0"/><ContentPresenter Content="{TemplateBinding SelectionBoxItem}"/></DockPanel></Border><ToggleButton IsChecked="{Binding IsDropDownOpen, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}" Focusable="False"><ToggleButton.Template><ControlTemplate TargetType="ToggleButton"><Border Background="Transparent"/></ControlTemplate></ToggleButton.Template></ToggleButton><Popup x:Name="PART_Popup" IsOpen="{TemplateBinding IsDropDownOpen}" Placement="Bottom" AllowsTransparency="True" Focusable="False"><Border Background="{DynamicResource Surface}" BorderBrush="{DynamicResource Line}" BorderThickness="1" MinWidth="{Binding ActualWidth, RelativeSource={RelativeSource TemplatedParent}}"><ScrollViewer MaxHeight="280"><ItemsPresenter/></ScrollViewer></Border></Popup></Grid></ControlTemplate></Setter.Value></Setter></Style>
          <Style TargetType="ListBox"><Setter Property="Background" Value="Transparent"/><Setter Property="Foreground" Value="{DynamicResource Ink}"/><Setter Property="BorderThickness" Value="0"/><Setter Property="ScrollViewer.HorizontalScrollBarVisibility" Value="Disabled"/><Setter Property="VirtualizingPanel.IsVirtualizing" Value="True"/><Setter Property="VirtualizingPanel.VirtualizationMode" Value="Recycling"/><Setter Property="ScrollViewer.CanContentScroll" Value="True"/></Style>
          <Style TargetType="ListBoxItem"><Setter Property="Padding" Value="10,9"/><Setter Property="Margin" Value="0,2,4,2"/><Setter Property="HorizontalContentAlignment" Value="Stretch"/><Setter Property="Template"><Setter.Value><ControlTemplate TargetType="ListBoxItem"><Border x:Name="row" Background="Transparent" Padding="{TemplateBinding Padding}" CornerRadius="8"><ContentPresenter/></Border><ControlTemplate.Triggers><Trigger Property="IsSelected" Value="True"><Setter TargetName="row" Property="Background" Value="{DynamicResource Hover}"/></Trigger><Trigger Property="IsMouseOver" Value="True"><Setter TargetName="row" Property="Background" Value="{DynamicResource Wash}"/></Trigger><Trigger Property="IsKeyboardFocused" Value="True"><Setter TargetName="row" Property="BorderThickness" Value="1"/><Setter TargetName="row" Property="BorderBrush" Value="{DynamicResource Accent}"/></Trigger></ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter></Style>
          <Style TargetType="TreeView"><Setter Property="Background" Value="Transparent"/><Setter Property="Foreground" Value="{DynamicResource Ink}"/><Setter Property="BorderThickness" Value="0"/></Style>
          <Style TargetType="TreeViewItem"><Setter Property="Foreground" Value="{DynamicResource Ink}"/><Setter Property="Padding" Value="3"/></Style>
          <Style TargetType="Expander"><Setter Property="Foreground" Value="{DynamicResource Ink}"/><Setter Property="Margin" Value="0,12,0,4"/></Style>
          <Style TargetType="ToolTip"><Setter Property="Background" Value="{DynamicResource Surface}"/><Setter Property="Foreground" Value="{DynamicResource Ink}"/><Setter Property="BorderBrush" Value="{DynamicResource Line}"/><Setter Property="Padding" Value="12"/></Style>
        </ResourceDictionary>
        """);
        Application.Current.Resources.MergedDictionaries.Add(styles);
    }
    public static void Apply(bool dark)
    {
        var colors=dark?new[]{("Paper","#202A27"),("Surface","#2B3731"),("Ink","#E9EFE4"),("Muted","#B4C1B5"),("Accent","#BCD1B7"),("Wash","#344239"),("Hover","#465B49"),("Line","#4A5A4E")}:new[]{("Paper","#F7F8F2"),("Surface","#FFFFFF"),("Ink","#29382E"),("Muted","#657367"),("Accent","#526A56"),("Wash","#E7EDE3"),("Hover","#D5E1CE"),("Line","#D8E0D4")};
        foreach(var (key,hex) in colors) Application.Current.Resources[key]=new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
    }
}

public static class Ui
{
    public static TextBlock Text(string text,double size=14,string color="Ink") { var t=new TextBlock { Text=text,FontSize=size,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,7) }; t.SetResourceReference(TextBlock.ForegroundProperty,color); return t; }
    public static TextBlock Latin(string value,double size=15) { var t=Text(value,size,"Muted"); t.FontFamily=new FontFamily("Georgia"); t.FontStyle=FontStyles.Italic; return t; }
    public static Button Button(string text,Action action,string? tip=null) { var b=new Button{Content=text,ToolTip=tip}; b.Click+=(_,_)=>action(); System.Windows.Automation.AutomationProperties.SetName(b,text); return b; }
    public static Border Panel(UIElement child,double pad=20) { var b=new Border{Child=child,Padding=new Thickness(pad),CornerRadius=new CornerRadius(14)}; b.SetResourceReference(Border.BackgroundProperty,"Surface"); return b; }
    public static void Heading(Panel p,string title) { var t=Text(title,18); t.FontWeight=FontWeights.SemiBold; t.Margin=new Thickness(0,22,0,12); p.Children.Add(t); }
    public static ScrollViewer Scroll(UIElement child)=>new(){Content=child,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled};
    public static Window Dialog(string title,UIElement content,double width=520,double height=650)=>new(){Title=title,Content=content,Width=width,Height=height,MinWidth=380,MinHeight=300,WindowStartupLocation=WindowStartupLocation.CenterScreen};
}
