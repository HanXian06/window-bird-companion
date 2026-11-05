# Bird stickers for the world's living bird species

<img src="previews/header.webp" alt="huashu-bird-sticker: specimen drawer, body-size lineup and family plates" width="100%">

**花叔开源的全球鸟类贴纸库：按全球统一鸟类名录AviList整理的11011种现生鸟，每种一张白边贴纸（38种新拆分的种还在画），附多语种鸟名和分布、形态、保护等级数据。贴纸出自花叔做的观鸟App「鸣迹」。[中文说明 →](README.zh-Hans.md)**

**English** · [简体中文](README.zh-Hans.md) · [繁體中文](README.zh-Hant.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Español](README.es.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Português](README.pt.md) · [Русский](README.ru.md) · [العربية](README.ar.md)

<a href="https://birdalbum.com/?utm_source=github&utm_medium=readme&utm_campaign=bird-sticker"><img src="previews/birdalbum.webp" alt="Bird Album: every bird in the world, one sticker each. birdalbum.com" width="100%"></a>

**See the whole library in use at [birdalbum.com](https://birdalbum.com/?utm_source=github&utm_medium=readme&utm_campaign=bird-sticker).** Bird Album is a free bird guide built on these stickers: look up any species by name in 15 languages, see where it lives and which months it is around, and open a map of 8,171 birding hotspots that shows what people are seeing at each one this month.

By [Huashu (花叔)](https://github.com/alchaincyf), from the bird-watching app Mingji (鸣迹). 11,011 bird species across 45 orders and 249 families, following [AviList v2026](https://www.avilist.org/), the unified global checklist that replaced the IOC and Clements lists in 2025. Every living species is included, plus 43 that AviList flags as extinct, extinct in the wild or possibly extinct but that already had a sticker (see `extinctionStatus`). 10,973 species have a sticker; the other 38 were recently split and are being drawn. Each species comes with structured data: taxonomy, names in up to 43 languages, cross-database IDs, and where every sticker came from.

> **Version 2.0 moved the taxonomy from IOC 15.2 to AviList v2026.** 82 species changed their scientific name (and so their `id` and sticker path), 91 former species are now subspecies of another species, and 30 new species were split off. Every change is listed in [`data/crosswalk-ioc15.2-avilist2026.csv`](data/crosswalk-ioc15.2-avilist2026.csv), and each record keeps its old IOC ID under `previous`. See [CHANGELOG](CHANGELOG.md).

![40 stickers from 15 bird orders](previews/hero.webp)

**License in short:** the stickers are licensed under [CC BY 4.0](LICENSE.md): use them for anything, including commercial work, editing and redistribution, as long as you credit huashu-bird-sticker with a link to this repository. The data is MIT, and third-party parts are listed in [NOTICE](NOTICE.md).

## Gallery

Eighteen of the most colourful and distinctive groups, 24 stickers each, picked so that no two look alike. Every sticker shown here meets the quality bar.

### Hummingbirds · *Trochilidae* — 361 species

![Hummingbirds](previews/families/trochilidae.webp)

### Parrots & cockatoos · *Psittaciformes* — 391 species

![Parrots & cockatoos](previews/families/parrots.webp)

### Kingfishers · *Alcedinidae* — 123 species

![Kingfishers](previews/families/alcedinidae.webp)

### Bee-eaters, rollers & motmots · *Meropidae* · *Coraciidae* · *Brachypteraciidae* · *Momotidae* · *Todidae* — 69 species

![Bee-eaters, rollers & motmots](previews/families/coraciiformes.webp)

### Toucans & barbets · *Ramphastidae* · *Capitonidae* · *Semnornithidae* · *Megalaimidae* · *Lybiidae* — 132 species

![Toucans & barbets](previews/families/toucans.webp)

### Birds-of-paradise & bowerbirds · *Paradisaeidae* · *Ptilonorhynchidae* — 67 species

![Birds-of-paradise & bowerbirds](previews/families/paradisaeidae.webp)

### Pittas · *Pittidae* — 46 species

![Pittas](previews/families/pittidae.webp)

### Tanagers · *Thraupidae* — 392 species

![Tanagers](previews/families/thraupidae.webp)

### Sunbirds & flowerpeckers · *Nectariniidae* · *Dicaeidae* — 207 species

![Sunbirds & flowerpeckers](previews/families/nectariniidae.webp)

### Cotingas & manakins · *Cotingidae* · *Pipridae* — 120 species

![Cotingas & manakins](previews/families/cotingas.webp)

### Pheasants & partridges · *Phasianidae* — 185 species

![Pheasants & partridges](previews/families/phasianidae.webp)

### Ducks, geese & swans · *Anatidae* — 164 species

![Ducks, geese & swans](previews/families/anatidae.webp)

### Pigeons & doves · *Columbidae* — 338 species

![Pigeons & doves](previews/families/columbidae.webp)

### Waxbills & munias · *Estrildidae* — 138 species

![Waxbills & munias](previews/families/estrildidae.webp)

### Jays & magpies · *Corvidae* — 135 species

![Jays & magpies](previews/families/corvidae.webp)

### New World orioles & oropendolas · *Icteridae* — 107 species

![New World orioles & oropendolas](previews/families/icteridae.webp)

### Hornbills · *Bucerotidae* — 64 species

![Hornbills](previews/families/bucerotidae.webp)

### Starlings & mynas · *Sturnidae* — 118 species

![Starlings & mynas](previews/families/sturnidae.webp)

### One from each order

![One from each order](previews/orders.webp)

## What's inside

| | |
|---|---|
| `stickers/<Order>/<Family>/<Genus_species>.webp` | One transparent sticker per species with a white outline, about 1,000 px on the long side. `<Genus_species>.<subspecies>.webp` are 91 extra stickers of subspecies that used to be separate species |
| `data/species.json` | One record per species, the complete dataset: taxonomy, names, IDs, range and subspecies, body measurements and ecology, conservation status, sticker and its source. Field reference: [data/README.md](data/README.md) |
| `data/crosswalk-ioc15.2-avilist2026.csv` | How every IOC 15.2 species maps to AviList v2026: same, renamed, lumped, split |
| `data/species.csv` | The same, flattened for spreadsheets |
| `data/names.csv` | Names in 43 languages (46 columns: Chinese, French and Portuguese each have two variants) |
| `data/languages.json` | Language codes and the IOC multilingual column each one comes from |
| `data/field-guide.zh-Hans.json` | Chinese field-guide notes (look, field marks, sexes and ages, similar species) for 1,596 species, CC BY-SA 4.0 |

A record looks like this (names shortened):

```json
{
  "id": "Accipiter_nisus",
  "scientificName": "Accipiter nisus",
  "authority": "(Linnaeus, 1758)",
  "taxonomy": {"system": "AviList v2026", "seq": 3097, "order": "Accipitriformes", "family": "Accipitridae", "genus": "Accipiter"},
  "names": {"en": "Eurasian Sparrowhawk", "zh-Hans": "雀鹰", "ja": "ハイタカ", "es": "Gavilán común", "fr": "Épervier d'Europe", "…": "…"},
  "ids": {"ebird": "eurspa1", "avibase": "avibase-DB376A66", "wikidata": "Q25380", "gbif": "2480637", "inaturalist": 5106},
  "range": {"text": "lowland to treeline montane forest, open woodland, and urban and suburban parks, from Europe eastward to …", "…": "…"},
  "subspecies": [{"name": "nisus", "authority": "(Linnaeus, 1758)", "range": "breeds Europe to Türkiye and Siberia; winters to Africa"}, "…"],
  "traits": {"massG": 220.8, "wingMm": 207.4, "tailMm": 155.5, "habitat": "Forest", "migration": "partially migratory", "trophicNiche": "Vertivore", "…": "…"},
  "conservation": {"iucn": "LC", "cites": "II", "chinaProtection": "Class II"},
  "taxonConcept": "same",
  "sticker": {
    "file": "stickers/Accipitriformes/Accipitridae/Accipiter_nisus.webp",
    "kind": "ai-illustration",
    "reviewScore": 91,
    "meetsQualityBar": true,
    "license": "CC-BY-4.0",
    "reference": {"type": "photo", "page": "https://commons.wikimedia.org/wiki/File:…", "author": "Dion Art", "license": "CC BY-SA 4.0"}
  }
}
```

## Coverage

- Names: English (AviList) covers every species. Other languages come from the IOC multilingual list and are kept only where the species concept did not change; for the 129 species whose boundaries changed (lumps and splits) or that are new, an old name may refer to a different set of birds, so we left it out rather than guess. Simplified Chinese covers 10,947 species (each changed species was checked by hand); French, Spanish, German, Russian, Japanese and Traditional Chinese cover over 94%. Korean, Arabic, Thai, Persian and some others cover only a few hundred to about a thousand species, because that is what the IOC list has. See `data/names.csv`.
- IDs: eBird 10,983 and Avibase 11,011, both taken from AviList; Wikidata 10,757, GBIF 10,477.
- Stickers: 10,973 of 11,011 species, plus 91 subspecies stickers. The 38 species without one are listed with `"sticker": null`.

## How the stickers were made, and how good they are

All 11,064 sticker files are AI illustrations. For each species we found a reference photo (or, for 453 species where no single photo was good enough, a board of several images plus a written description), drew the sticker with GPT-image, upscaled it, and cut it out.

Every AI sticker was then scored by a separate reviewer against the reference for field marks: bill, eye, legs and feet, plumage pattern and colour. In the last round, 852 borderline stickers (80–82) also got a second, blind review, and 211 of them failed it and were redrawn. Borderline stickers from earlier rounds did not get this second review. In November 2026 we re-checked the stickers to a stricter standard, as plates for a field guide, and redrew 4,299 of them. A redrawn sticker passes only if identification, plumage, bill and eye, legs and feet, and proportions each score at least 3 out of 4, identification scores 4, and the reviewer would accept it as a field-guide plate.

- **10,677 meet our quality bar** (`"meetsQualityBar": true`): stickers that passed the stricter review above, or scored 80 or above in the earlier review.
- **387 do not.** We kept the best-scoring version so that no species is left without a picture: 375 score 70–79 and 12 score below 70. We are redrawing these; see [issue #1](../../issues/1) and [issue #3](../../issues/3) for the kinds of error we are fixing.

These are illustrations, not field-guide plates. Treat them as a friendly picture of the bird, and check a field guide before relying on a fine detail. If you spot a mistake, please [open an issue](../../issues/new/choose) — corrections from birders are the most valuable contribution this project can get.

## About the reference photos

**What a reference is for.** A sticker shows what a species looks like: its plumage colours and markings, bill, eye, legs and feet, and proportions. These are facts about the bird, the same things a field guide describes in words. A written description alone is often not enough for the image model to get them right, so we use reference photos.

**How we use it.** For each species, the image model is shown one or more reference photos together with a written description of the field marks, and draws a new illustration. We do not use a photo's composition, background or lighting: stickers redrawn since November 2026 all use the same standard side-on pose that we set, on a plain background, whatever the bird is doing in the photo. A separate reviewer then checks the drawing against the field marks. From now on we use photos from several observers for each species where we can, so that a sticker reflects the species rather than any one picture. No reference photo is included in this repository.

**Credit and removal.** The reference photos behind each sticker (page, author and license) are listed in `sticker.reference` in `data/species.json`, so every sticker can be traced. A breakdown by reference license is in [data/README.md](data/README.md#reference-photos). If one of your photos was used as a reference and you would rather it weren't, open an issue or email alchaincyf@gmail.com and we will replace the sticker within 7 days.

## Use it

Raw files can be loaded directly:

```
https://raw.githubusercontent.com/alchaincyf/huashu-bird-sticker/main/stickers/Accipitriformes/Accipitridae/Accipiter_nisus.webp
```

```python
import json
species = json.load(open("data/species.json"))["species"]
by_name = {s["scientificName"]: s for s in species}
print(by_name["Pica pica"]["names"]["ja"], by_name["Pica pica"]["sticker"]["file"])
```

The repository is about 700 MB. To fetch only the data and a few stickers:

```bash
git clone --filter=blob:none --sparse https://github.com/alchaincyf/huashu-bird-sticker
cd huashu-bird-sticker && git sparse-checkout set data stickers/Strigiformes
```

## Credits

- Taxonomy, English names, ranges, eBird and Avibase IDs: AviList Core Team. 2026. AviList: The Global Avian Checklist, v2026. https://doi.org/10.2173/avilist.v2026, CC BY 4.0.
- Names in other languages: IOC World Bird List v15.2, Gill, Donsker & Rasmussen (Eds), CC BY 3.0.
- Other cross-database IDs: Wikidata (CC0).
- Body measurements and ecology: AVONET, Tobias et al. 2022, *Ecology Letters*, CC BY 4.0.
- Reference photographers: credited per species in `data/species.json`. Reference photos are not included in this repository.
- Stickers drawn for 鸣迹 (Mingji), a bird-watching notebook app for iOS.

If one of your photos was used as a reference for a sticker and you would rather it weren't, open an issue or email alchaincyf@gmail.com and we will replace the sticker within 7 days.
