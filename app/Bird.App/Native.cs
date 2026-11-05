using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Bird.App;
public static class Native
{
    [StructLayout(LayoutKind.Sequential)] private struct WindowRect {public int Left,Top,Right,Bottom;}
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr handle,out WindowRect rect);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr handle,out uint processId);
    [DllImport("user32.dll")] private static extern bool IsIconic(IntPtr handle);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] private static extern int GetClassName(IntPtr handle,System.Text.StringBuilder name,int count);
    [DllImport("user32.dll",EntryPoint="GetWindowLongW")] private static extern int GetWindowLong(IntPtr handle,int index);
    public static bool OtherAppFullscreen()
    {
        var handle=GetForegroundWindow();if(handle==IntPtr.Zero || IsIconic(handle))return false;
        GetWindowThreadProcessId(handle,out var pid);if(pid==Environment.ProcessId)return false;
        var name=new System.Text.StringBuilder(256);GetClassName(handle,name,name.Capacity);
        if(name.ToString() is "Progman" or "WorkerW" or "Shell_TrayWnd" or "Shell_SecondaryTrayWnd")return false;
        // A regular maximized title-bar window is not a fullscreen session.
        if((GetWindowLong(handle,-16)&0x00C00000)!=0 || !GetWindowRect(handle,out var rect))return false;
        var screen=System.Windows.Forms.Screen.FromHandle(handle).Bounds;
        return CoversMonitor(rect.Left,rect.Top,rect.Right,rect.Bottom,screen.Left,screen.Top,screen.Right,screen.Bottom);
    }
    public static bool CoversMonitor(int left,int top,int right,int bottom,int monitorLeft,int monitorTop,int monitorRight,int monitorBottom)=>left<=monitorLeft+2 && top<=monitorTop+2 && right>=monitorRight-2 && bottom>=monitorBottom-2;
    [DllImport("kernel32.dll")] private static extern bool QueryUnbiasedInterruptTime(out ulong time);
    public static double ActiveSeconds {get { if(!QueryUnbiasedInterruptTime(out var time)) throw new InvalidOperationException("无法读取运行计时器"); return time/10000000.0; }}
    public static void Clamp(Window window)
    {
        if(!double.IsFinite(window.Left)) window.Left=100;
        if(!double.IsFinite(window.Top)) window.Top=100;
        var handle=new WindowInteropHelper(window).Handle;
        var screen=handle==IntPtr.Zero?System.Windows.Forms.Screen.PrimaryScreen!:System.Windows.Forms.Screen.FromHandle(handle);
        var area=screen.WorkingArea;
        var dpi=VisualTreeHelper.GetDpi(window);
        var left=area.Left/dpi.DpiScaleX; var top=area.Top/dpi.DpiScaleY;
        window.Left=Math.Clamp(window.Left,left,Math.Max(left,area.Right/dpi.DpiScaleX-window.Width));
        window.Top=Math.Clamp(window.Top,top,Math.Max(top,area.Bottom/dpi.DpiScaleY-window.Height));
    }
}
