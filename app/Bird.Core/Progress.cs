namespace Bird.Core;

public sealed class Unlock { public string Id {get;set;}=""; public long Sequence {get;set;} public DateTimeOffset At {get;set;}=DateTimeOffset.Now; }
public sealed class ChestRecord { public DateTimeOffset At {get;set;}=DateTimeOffset.Now; public List<string> Ids {get;set;}=[]; }
public sealed class SaveData
{
    public int Version {get;set;}=1;
    public string ResourceVersion {get;set;}="";
    public List<Unlock> Unlocks {get;set;}=[];
    public List<ChestRecord> History {get;set;}=[];
    public string CurrentId {get;set;}=ProgressEngine.InitialIds[0];
    public long Chests {get;set;}
    public double Seconds {get;set;}
    public double ChestMinutes {get;set;}=30;
    public double SavedChestIntervalSeconds {get;set;}=1800;
    public bool Dark {get;set;}
    public bool Topmost {get;set;}=true;
    public bool FullscreenDnd {get;set;}=true;
    public bool AutoOpen {get;set;}
    public List<string> FavoriteIds {get;set;}=[];
    public bool FavoritesOnly {get;set;}
    public double RotationMinutes {get;set;}
    public double RotationSeconds {get;set;}
    public double ToolbarOpacity {get;set;}=1;
    public int FavoriteColumns {get;set;}=2;
    public double Size {get;set;}=220;
    public double? Left {get;set;}
    public double? Top {get;set;}
}

public sealed class ProgressEngine
{
    public static readonly string[] InitialIds=["Spilopelia_chinensis","Pycnonotus_sinensis","Nycticorax_nycticorax","Passer_montanus"];
    public BirdCatalog Catalog {get;}
    public SaveData Data {get;private set;}
    private readonly Random random;
    public IEnumerable<BirdInfo> UnlockedBirds=>Data.Unlocks.OrderBy(x=>x.Sequence).Where(x=>Catalog.ById.TryGetValue(x.Id,out var b) && b.HasSticker).Select(x=>Catalog.ById[x.Id]);
    public HashSet<string> UnlockedIds=>Data.Unlocks.Select(x=>x.Id).ToHashSet(StringComparer.Ordinal);
    public bool Complete=>Catalog.Birds.Where(x=>x.Collectible).All(x=>unlocked.Contains(x.Id));
    public int UnlockedCount=>Data.Unlocks.Count(x=>Catalog.ById.ContainsKey(x.Id));
    public bool CanCustomizeChestInterval=>UnlockedCount>50;
    public double ChestIntervalSeconds=>UnlockedCount<50?600:CanCustomizeChestInterval?Data.ChestMinutes*60:1800;
    private void ReconcileChestInterval(){var old=Data.SavedChestIntervalSeconds;var fraction=double.IsFinite(old)&&old>0?Math.Clamp(Data.Seconds/old,0,0.999999):0;Data.SavedChestIntervalSeconds=ChestIntervalSeconds;Data.Seconds=fraction*ChestIntervalSeconds;}
    public void SetChestMinutes(double minutes){if(!CanCustomizeChestInterval)return;if(!double.IsFinite(minutes)||minutes<0.1||minutes>10080)throw new ArgumentOutOfRangeException(nameof(minutes));Data.ChestMinutes=minutes;ReconcileChestInterval();}
    private HashSet<string> unlocked=[];
    public ProgressEngine(BirdCatalog catalog, SaveData data, Random? random=null) { Catalog=catalog; Data=data; this.random=random??Random.Shared; Normalize(); }
    public void Restore(SaveData data) { Data=data; Normalize(); }
    private void Normalize()
    {
        Data.Unlocks ??=[]; Data.History??=[];
        Data.Unlocks=Data.Unlocks.Where(x=>!string.IsNullOrWhiteSpace(x.Id)).DistinctBy(x=>x.Id).OrderBy(x=>x.Sequence).ToList();
        unlocked=Data.Unlocks.Select(x=>x.Id).ToHashSet(StringComparer.Ordinal);
        foreach(var id in InitialIds) if(unlocked.Add(id)) Data.Unlocks.Add(new Unlock { Id=id,Sequence=Data.Unlocks.Count==0?0:Data.Unlocks.Max(x=>x.Sequence)+1 });
        if(!unlocked.Contains(Data.CurrentId) || !Catalog.ById.TryGetValue(Data.CurrentId,out var current) || !current.HasSticker) Data.CurrentId=InitialIds[0];
        Data.Seconds=double.IsFinite(Data.Seconds)?Math.Max(Data.Seconds,0):0;
        Data.ChestMinutes=double.IsFinite(Data.ChestMinutes)&&Data.ChestMinutes>=0.1&&Data.ChestMinutes<=10080?Data.ChestMinutes:30;
        ReconcileChestInterval();
        Data.Chests=Math.Max(Data.Chests,0);
        Data.Size=double.IsFinite(Data.Size)?Math.Clamp(Data.Size,80,600):220;
        Data.FavoriteIds=(Data.FavoriteIds??[]).Where(id=>id!=null && unlocked.Contains(id) && Catalog.ById.TryGetValue(id,out var b) && b.HasSticker).Distinct().ToList();
        Data.RotationMinutes=double.IsFinite(Data.RotationMinutes)?Math.Clamp(Data.RotationMinutes,0,10080):0;
        Data.RotationSeconds=double.IsFinite(Data.RotationSeconds) && Data.RotationMinutes>0?Math.Clamp(Data.RotationSeconds,0,Data.RotationMinutes*60):0;
        Data.ToolbarOpacity=double.IsFinite(Data.ToolbarOpacity)?Math.Clamp(Data.ToolbarOpacity,0.1,1):1;
        Data.FavoriteColumns=Math.Clamp(Data.FavoriteColumns,1,4);
        Data.ResourceVersion=Catalog.Version;
    }
    public void Advance(double seconds)
    {
        if(!double.IsFinite(seconds) || seconds<=0 || Complete) return;
        ReconcileChestInterval();
        var total=Data.Seconds+seconds;
        var earned=(long)Math.Floor(Math.Min(total/ChestIntervalSeconds,long.MaxValue-Data.Chests));
        Data.Chests+=earned; Data.Seconds=total%ChestIntervalSeconds;
    }
    public List<BirdInfo> OpenChest()
    {
        if(Data.Chests<=0) return [];
        var pools=Catalog.Birds.Where(x=>x.Collectible && !unlocked.Contains(x.Id)).GroupBy(x=>x.Rarity).ToDictionary(g=>g.Key,g=>g.ToList());
        if(pools.Count==0) return [];
        double Weight(Rarity r)=>r switch{Rarity.Common=>70,Rarity.Rare=>25,Rarity.Ultra=>5,_=>0};
        var roll=random.NextDouble()*pools.Keys.Sum(Weight); var tier=pools.Keys.First();
        foreach(var r in new[]{Rarity.Common,Rarity.Rare,Rarity.Ultra}) { if(!pools.ContainsKey(r)) continue; tier=r; roll-=Weight(r); if(roll<0) break; }
        var candidates=pools[tier]; var n=Math.Min(candidates.Count,tier==Rarity.Common?5:tier==Rarity.Rare?2:1); var result=new List<BirdInfo>();
        for(var i=0;i<n;i++) { var index=random.Next(candidates.Count); var b=candidates[index]; candidates.RemoveAt(index); result.Add(b); AddUnlock(b.Id); }
        Data.Chests--; Data.History.Add(new ChestRecord { Ids=result.Select(x=>x.Id).ToList() });
        if(Data.History.Count>200) Data.History.RemoveRange(0,Data.History.Count-200);
        ReconcileChestInterval();
        return result;
    }
    private void AddUnlock(string id) { if(unlocked.Add(id)) Data.Unlocks.Add(new Unlock { Id=id,Sequence=Data.Unlocks.Max(x=>x.Sequence)+1 }); }
    public void UnlockAll() { foreach(var b in Catalog.Birds.Where(x=>x.Collectible).OrderBy(x=>x.Sequence)) AddUnlock(b.Id);ReconcileChestInterval(); }
    public void Reset() { Data.Unlocks=[]; Data.History=[]; Data.Chests=0; Data.Seconds=0; Data.FavoriteIds=[];Data.RotationSeconds=0;Data.CurrentId=InitialIds[0]; Normalize(); }
    public void Select(string id) { if(unlocked.Contains(id) && Catalog.ById.TryGetValue(id,out var b) && b.HasSticker) Data.CurrentId=id; }
    public void Move(int direction) { var birds=UnlockedBirds.ToList(); var index=birds.FindIndex(x=>x.Id==Data.CurrentId); Select(birds[(index+direction+birds.Count)%birds.Count].Id); }
    public void ToggleFavorite(string id) { if(!unlocked.Contains(id) || !Catalog.ById.TryGetValue(id,out var b) || !b.HasSticker)return; if(!Data.FavoriteIds.Remove(id))Data.FavoriteIds.Add(id); }
    public void RandomBird() { var candidates=UnlockedBirds.Where(x=>x.Id!=Data.CurrentId && (!Data.FavoritesOnly || Data.FavoriteIds.Contains(x.Id))).ToArray(); if(candidates.Length>0) Select(candidates[random.Next(candidates.Length)].Id); }
    public bool AdvanceRotation(double seconds)
    {
        if(Data.RotationMinutes<=0 || !double.IsFinite(seconds) || seconds<=0)return false;
        Data.RotationSeconds+=seconds;var interval=Data.RotationMinutes*60;
        if(Data.RotationSeconds<interval)return false;
        Data.RotationSeconds%=interval;var previous=Data.CurrentId;RandomBird();return Data.CurrentId!=previous;
    }
}
