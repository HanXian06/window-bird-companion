using System.Text.Json;

namespace Bird.Core;
public sealed class ProgressStore(string path)
{
    public string Path {get;}=System.IO.Path.GetFullPath(path);
    public string? RecoveryMessage {get;private set;}
    public static readonly JsonSerializerOptions JsonOptions=new(){WriteIndented=true};
    public SaveData Load()
    {
        if(!File.Exists(Path) && !File.Exists(Path+".bak")) return new();
        Exception? original=null;
        foreach(var candidate in new[]{Path,Path+".bak"})
        {
            if(!File.Exists(candidate)) continue;
            try {
                var data=JsonSerializer.Deserialize<SaveData>(File.ReadAllText(candidate))??throw new JsonException("Empty save");
                if(data.Version!=1 || data.Unlocks==null || data.History==null ||
                   data.Unlocks.Any(x=>x==null || string.IsNullOrWhiteSpace(x.Id)) ||
                   data.History.Any(x=>x==null || x.Ids==null || x.Ids.Any(string.IsNullOrWhiteSpace)))
                    throw new JsonException("Invalid save version or collection");
                if(candidate!=Path) RecoveryMessage="进度文件读取失败，已从上一次备份恢复。损坏文件已保留。";
                return data;
            } catch(Exception e) when(e is JsonException or IOException) {
                original=e;
                if(candidate==Path) File.Move(Path,Path+".corrupt-"+DateTime.Now.ToString("yyyyMMddHHmmssfff"),true);
            }
        }
        throw new IOException("进度及备份均无法读取，请保留存档并检查文件；程序没有重置进度。",original);
    }
    public void Save(SaveData data)
    {
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
        var temp=Path+".tmp";
        using(var stream=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None)) { JsonSerializer.Serialize(stream,data,JsonOptions); stream.Flush(true); }
        if(File.Exists(Path)) File.Replace(temp,Path,Path+".bak"); else File.Move(temp,Path);
    }
    public void BackupBeforeReset() { if(File.Exists(Path)) File.Copy(Path,Path+".before-reset-"+DateTime.Now.ToString("yyyyMMddHHmmssfff"),true); }
}
