using System.Threading;
using System.Diagnostics;
using Forms=System.Windows.Forms;

namespace Bird.App;
public static class Program
{
    [STAThread]
    public static int Main()
    {
        using var mutex=new Mutex(true,"Local\\WindowBirdCompanion",out var first);
        using var signal=new EventWaitHandle(false,EventResetMode.AutoReset,"Local\\WindowBirdCompanionShow");
        if(!first) { signal.Set(); return 0; }
        var app=new Application{ShutdownMode=ShutdownMode.OnExplicitShutdown};
        Theme.Install(); Theme.Apply(false);
        AppState? state=null; Forms.NotifyIcon? tray=null;
        try {
            var resources=FindResources();
            var savePath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"WindowBirdCompanion","progress.json");
            state=new AppState(resources,savePath);
            var pet=new PetWindow(state); state.Pet=pet; app.MainWindow=pet;
            var menu=new Forms.ContextMenuStrip();
            void Menu(string label,Action action) { menu.Items.Add(label,null,(_,_)=>app.Dispatcher.Invoke(action)); }
            Menu("显示 / 隐藏小鸟",()=>{pet.ToggleUserVisibility();});
            Menu("打开图鉴",state.OpenGuide); Menu("设置",()=>state.OpenSettings());
            var topItem=new Forms.ToolStripMenuItem("置顶"){Checked=state.Data.Topmost,CheckOnClick=false};
            topItem.Click+=(_,_)=>app.Dispatcher.Invoke(()=>state.Commit(()=>state.Data.Topmost=!state.Data.Topmost)); menu.Items.Add(topItem);
            menu.Items.Add(new Forms.ToolStripSeparator());
            Menu("退出观鸟桌宠",()=>{if(state.Save()) app.Shutdown();});
            tray=new Forms.NotifyIcon{Text="窗边小鸟 · 观鸟桌宠",Icon=CreateTrayIcon(),Visible=true,ContextMenuStrip=menu};
            tray.DoubleClick+=(_,_)=>app.Dispatcher.Invoke(()=>{pet.ToggleUserVisibility();});
            state.Changed+=()=>topItem.Checked=state.Data.Topmost;
            var listener=new Thread(()=>{while(true){ signal.WaitOne(); if(app.Dispatcher.HasShutdownStarted) break; app.Dispatcher.BeginInvoke(()=>{pet.ShowByUser();Native.Clamp(pet);}); }}){IsBackground=true}; listener.Start();
            app.Exit+=(_,_)=>{tray.Visible=false;tray.Dispose();state.Stop();};
            app.DispatcherUnhandledException+=(_,e)=>{MessageBox.Show("操作未完成，请重启桌宠。\n"+e.Exception.Message,"观鸟桌宠",MessageBoxButton.OK,MessageBoxImage.Error); e.Handled=true;};
            pet.Show(); state.Start();
            if(state.Store.RecoveryMessage!=null) MessageBox.Show(state.Store.RecoveryMessage,"进度恢复");
            return app.Run();
        } catch(Exception e) { tray?.Dispose(); MessageBox.Show("无法启动观鸟桌宠。\n"+e.Message,"观鸟桌宠",MessageBoxButton.OK,MessageBoxImage.Error); return 1; }
        finally { if(first) mutex.ReleaseMutex(); }
    }
    public static string FindResources()
    {
        var directory=new DirectoryInfo(AppContext.BaseDirectory);
        while(directory!=null) { var path=Path.Combine(directory.FullName,"huashu-bird-sticker"); if(File.Exists(Path.Combine(path,"data","species.json"))) return path; directory=directory.Parent; }
        throw new FileNotFoundException("找不到 huashu-bird-sticker 资源目录。请将程序与资源库放在同一个项目文件夹内。");
    }
    private static System.Drawing.Icon CreateTrayIcon()
    {
        using var stream=Application.GetResourceStream(new Uri("pack://application:,,,/BirdCompanion;component/Assets/bird.ico")).Stream;
        using var icon=new System.Drawing.Icon(stream);
        return new System.Drawing.Icon(icon,new System.Drawing.Size(32,32));
    }
}
