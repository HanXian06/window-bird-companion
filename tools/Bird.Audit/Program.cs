using Bird.Core;
using System.Text;
var catalog=BirdCatalog.Load(Path.GetFullPath("huashu-bird-sticker"));
string Csv(string value)=>"\""+value.Replace("\"","\"\"")+"\"";
Directory.CreateDirectory("docs");
File.WriteAllLines("docs/rarity-audit.csv",new[]{"Id,中文名,学名,游戏分级,可收藏,排除原因,贴纸"}.Concat(catalog.Birds.Select(b=>string.Join(",",new[]{b.Id,b.Name,b.Scientific,b.RarityName,b.Collectible?"是":"否",b.Exclusion,b.Raw.Field("sticker").At("file")}.Select(Csv)))),new UTF8Encoding(true));
foreach(var group in catalog.Birds.GroupBy(b=>b.Rarity))Console.WriteLine($"{group.Key}: {group.Count()} total, {group.Count(b=>b.Collectible)} collectible");
Console.WriteLine($"Total: {catalog.Birds.Count}; collectible: {catalog.Birds.Count(b=>b.Collectible)}");
