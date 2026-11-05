using SkiaSharp;
using System.Windows.Media.Imaging;

namespace Bird.App;
public static class Images
{
    private static readonly Dictionary<string,BitmapSource> cache=[];
    private static readonly Queue<string> order=[];
    public static BitmapSource? Load(BirdInfo bird)
    {
        if(!bird.HasSticker) return null;
        if(cache.TryGetValue(bird.Id,out var cached)) return cached;
        try {
            using var original=SKBitmap.Decode(bird.StickerPath);
            if(original==null) return null;
            var scale=Math.Min(1,640.0/Math.Max(original.Width,original.Height));
            using var bitmap=new SKBitmap(Math.Max(1,(int)(original.Width*scale)),Math.Max(1,(int)(original.Height*scale)),SKColorType.Bgra8888,SKAlphaType.Premul);
            using(var canvas=new SKCanvas(bitmap)) { canvas.Clear(SKColors.Transparent); using var paint=new SKPaint{IsAntialias=true,FilterQuality=SKFilterQuality.High}; canvas.DrawBitmap(original,new SKRect(0,0,bitmap.Width,bitmap.Height),paint); }
            var source=BitmapSource.Create(bitmap.Width,bitmap.Height,96,96,PixelFormats.Pbgra32,null,bitmap.GetPixels(),bitmap.ByteCount,bitmap.RowBytes); source.Freeze();
            cache[bird.Id]=source; order.Enqueue(bird.Id);
            while(order.Count>24) cache.Remove(order.Dequeue());
            return source;
        } catch(Exception e) when(e is IOException or ArgumentException) { return null; }
    }
}

public sealed class AlphaImage : Image
{
    protected override HitTestResult? HitTestCore(PointHitTestParameters p)
    {
        if(Source is not BitmapSource bitmap || ActualWidth<=0 || ActualHeight<=0) return null;
        var scale=Math.Min(ActualWidth/bitmap.PixelWidth,ActualHeight/bitmap.PixelHeight);
        var x=(int)((p.HitPoint.X-(ActualWidth-bitmap.PixelWidth*scale)/2)/scale);
        var y=(int)((p.HitPoint.Y-(ActualHeight-bitmap.PixelHeight*scale)/2)/scale);
        if(x<0 || y<0 || x>=bitmap.PixelWidth || y>=bitmap.PixelHeight) return null;
        var pixel=new byte[4]; bitmap.CopyPixels(new Int32Rect(x,y,1,1),pixel,4,0);
        return pixel[3]>16?new PointHitTestResult(this,p.HitPoint):null;
    }
}
