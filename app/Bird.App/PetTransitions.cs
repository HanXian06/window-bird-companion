using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Bird.App;

// Keeps visual transitions independent of the persisted current species.
public sealed class PetTransitions(AlphaImage bird,Image previous,Border label)
{
    private int birdGeneration,nameGeneration;
    public void ChangeBird(ImageSource? next,bool animate)
    {
        var generation=++birdGeneration;
        var snapshot=animate && bird.Source!=null?Snapshot():null;
        bird.BeginAnimation(UIElement.OpacityProperty,null);previous.BeginAnimation(UIElement.OpacityProperty,null);
        bird.Source=next;bird.Opacity=1;previous.Opacity=0;
        previous.Source=snapshot;previous.Visibility=snapshot==null?Visibility.Collapsed:Visibility.Visible;
        if(snapshot==null || next==null)return;
        var incoming=new DoubleAnimation(0,1,TimeSpan.FromMilliseconds(300)){EasingFunction=new QuadraticEase{EasingMode=EasingMode.EaseInOut},FillBehavior=FillBehavior.Stop};
        var outgoing=new DoubleAnimation(1,0,TimeSpan.FromMilliseconds(300)){EasingFunction=new QuadraticEase{EasingMode=EasingMode.EaseInOut},FillBehavior=FillBehavior.Stop};
        incoming.Completed+=(_,_)=>{if(generation!=birdGeneration)return;bird.BeginAnimation(UIElement.OpacityProperty,null);previous.BeginAnimation(UIElement.OpacityProperty,null);previous.Visibility=Visibility.Collapsed;previous.Source=null;};
        bird.BeginAnimation(UIElement.OpacityProperty,incoming);previous.BeginAnimation(UIElement.OpacityProperty,outgoing);
    }
    private BitmapSource Snapshot()
    {
        // Capture the visible blend, so rapid selection does not jump back to an older sticker.
        var size=Math.Max(1,(int)Math.Ceiling(bird.Width));var visual=new DrawingVisual();
        using(var drawing=visual.RenderOpen()) { Draw(drawing,bird.Source,bird.Opacity,size);if(previous.Visibility==Visibility.Visible)Draw(drawing,previous.Source,previous.Opacity,size); }
        var bitmap=new RenderTargetBitmap(size,size,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);bitmap.Freeze();return bitmap;
    }
    private static void Draw(DrawingContext drawing,ImageSource? source,double opacity,double size)
    {
        if(source==null || source.Width<=0 || source.Height<=0 || opacity<=0)return;
        var scale=Math.Min(size/source.Width,size/source.Height);var w=source.Width*scale;var h=source.Height*scale;
        drawing.PushOpacity(opacity);drawing.DrawImage(source,new Rect((size-w)/2,(size-h)/2,w,h));drawing.Pop();
    }
    public void ShowName()
    {
        var generation=++nameGeneration;label.BeginAnimation(UIElement.OpacityProperty,null);label.Opacity=0;label.Visibility=Visibility.Visible;
        var animation=new DoubleAnimationUsingKeyFrames{Duration=TimeSpan.FromSeconds(2),FillBehavior=FillBehavior.Stop};
        animation.KeyFrames.Add(new LinearDoubleKeyFrame(0,KeyTime.FromTimeSpan(TimeSpan.Zero)));
        animation.KeyFrames.Add(new EasingDoubleKeyFrame(1,KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(180))){EasingFunction=new QuadraticEase{EasingMode=EasingMode.EaseOut}});
        animation.KeyFrames.Add(new LinearDoubleKeyFrame(1,KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1750))));
        animation.KeyFrames.Add(new EasingDoubleKeyFrame(0,KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2))){EasingFunction=new QuadraticEase{EasingMode=EasingMode.EaseIn}});
        animation.Completed+=(_,_)=>{if(generation!=nameGeneration)return;label.Visibility=Visibility.Collapsed;label.BeginAnimation(UIElement.OpacityProperty,null);};
        label.BeginAnimation(UIElement.OpacityProperty,animation);
    }
    public void Cancel()
    {
        birdGeneration++;nameGeneration++;bird.BeginAnimation(UIElement.OpacityProperty,null);bird.Opacity=1;previous.BeginAnimation(UIElement.OpacityProperty,null);previous.Visibility=Visibility.Collapsed;previous.Source=null;label.BeginAnimation(UIElement.OpacityProperty,null);label.Visibility=Visibility.Collapsed;
    }
}
