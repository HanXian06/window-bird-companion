using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Bird.Core;

public enum Rarity { Common, Rare, Ultra, Pending }
public static class JsonFields
{
    public static JsonElement Field(this JsonElement e, string key) => e.ValueKind==JsonValueKind.Object && e.TryGetProperty(key,out var v)?v:default;
    public static string Text(this JsonElement e) => e.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined?"":e.ValueKind==JsonValueKind.String?e.GetString()??"":e.ToString();
    public static string At(this JsonElement e, string key) => e.Field(key).Text();
}

public sealed class BirdInfo
{
    public string Id {get;init;}="";
    public string Name {get;init;}="";
    public string Scientific {get;init;}="";
    public string English {get;init;}="";
    public string Order {get;init;}="";
    public string Family {get;init;}="";
    public string Genus {get;init;}="";
    public string OrderZh {get;init;}="";
    public string FamilyZh {get;init;}="";
    public string Pinyin {get;init;}="";
    public string StickerPath {get;init;}="";
    public int Sequence {get;init;}
    public bool HasSticker {get;init;}
    public bool Collectible {get;init;}
    public Rarity Rarity {get;init;}
    public string Exclusion {get;init;}="";
    public JsonElement Raw {get;init;}
    public JsonElement Notes {get;init;}
    public string RarityName => Rarity switch { Rarity.Common=>"普通", Rarity.Rare=>"稀有", Rarity.Ultra=>"极其稀有", _=>"分级待核" };
    public string Taxonomy => $"动物界 / 脊索动物门 / 鸟纲 / {OrderZh} / {FamilyZh} / {Genus} / {Scientific}";
    public string Summary => Notes.At("impression") is {Length:>0} impression?impression:$"{OrderZh}，{FamilyZh}。";
    public override string ToString()=>Name;
}

public sealed class BirdCatalog
{
    private readonly JsonDocument speciesDocument, notesDocument;
    public string Root {get;}
    public string Version {get;}
    public List<BirdInfo> Birds {get;}=[];
    public Dictionary<string,BirdInfo> ById {get;}=new(StringComparer.Ordinal);
    private BirdCatalog(string root)
    {
        Root=root;
        speciesDocument=JsonDocument.Parse(File.ReadAllBytes(Path.Combine(root,"data","species.json")));
        notesDocument=JsonDocument.Parse(File.ReadAllBytes(Path.Combine(root,"data","field-guide.zh-Hans.json")));
        Version=speciesDocument.RootElement.At("version");
        var notes=notesDocument.RootElement.Field("species");
        foreach(var r in speciesDocument.RootElement.GetProperty("species").EnumerateArray())
        {
            var id=r.At("id"); var names=r.Field("names"); var taxonomy=r.Field("taxonomy"); var zh=r.Field("zh"); var conservation=r.Field("conservation");
            var name=names.At("zh-Hans"); var sci=r.At("scientificName"); var iucn=conservation.At("iucn"); var china=conservation.At("chinaProtection"); var cites=conservation.At("cites");
            var rarity = iucn is "CR" or "EN" || china=="Class I"?Rarity.Ultra:
                iucn is "VU" or "NT" || china=="Class II" || cites is "I" or "II"?Rarity.Rare:iucn=="LC"?Rarity.Common:Rarity.Pending;
            var relative=r.Field("sticker").At("file");
            var sticker=string.IsNullOrWhiteSpace(relative)?"":Path.GetFullPath(Path.Combine(root,relative));
            var hasSticker=sticker.StartsWith(root+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase) && File.Exists(sticker);
            var extinct=r.At("extinctionStatus");
            var excluded=extinct.Length>0 && !extinct.StartsWith("previously",StringComparison.OrdinalIgnoreCase);
            var initial=ProgressEngine.InitialIds.Contains(id);
            var b=new BirdInfo {
                Id=id, Name=name.Length>0?name:sci, Scientific=sci, English=names.At("en"), Order=taxonomy.At("order"), Family=taxonomy.At("family"), Genus=taxonomy.At("genus"),
                OrderZh=zh.At("order") is {Length:>0} o?o:taxonomy.At("order"), FamilyZh=zh.At("family") is {Length:>0} f?f:taxonomy.At("family"),
                Pinyin=name.Length==0?"~"+sci:StripAccents(zh.At("pinyin") is {Length:>0} p?p:name),
                Sequence=taxonomy.Field("seq").TryGetInt32(out var seq)?seq:0, StickerPath=sticker, HasSticker=hasSticker,
                Collectible=hasSticker && (initial || (!excluded && rarity!=Rarity.Pending)), Rarity=rarity, Raw=r, Notes=notes.Field(id),
                Exclusion=!hasSticker?"资源待补充":excluded?"非首版收藏范围":rarity==Rarity.Pending && !initial?"分级待核":""
            };
            Birds.Add(b); ById.Add(id,b);
        }
    }
    public static BirdCatalog Load(string root)=>new(Path.GetFullPath(root));
    private static string StripAccents(string value)=>new(value.Normalize(NormalizationForm.FormD).Where(c=>CharUnicodeInfo.GetUnicodeCategory(c)!=UnicodeCategory.NonSpacingMark && !char.IsWhiteSpace(c)).ToArray());
}
