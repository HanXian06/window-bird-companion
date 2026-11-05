# Data

`species.json` is the complete dataset, one record per species, sorted in AviList v2026 sequence. The CSV files are flattened subsets for spreadsheets.

| File | Contents | License |
|---|---|---|
| `species.json` | Everything below except the Chinese field-guide text | MIT for our compilation; third-party fields keep their terms (see Sources) |
| `species.csv` | Taxonomy, main names, IDs, sticker and reference info | as above |
| `names.csv` | Names in 43 languages, one column per language | English: CC BY 4.0 (AviList); others: CC BY 3.0 (IOC) |
| `languages.json` | Language codes and the IOC multilingual column each comes from | MIT |
| `crosswalk-ioc15.2-avilist2026.csv` | How each IOC 15.2 species (the taxonomy of version 0.1) maps to AviList v2026 | MIT |
| `field-guide.zh-Hans.json` | Chinese field-guide notes for 1,596 taxa | CC BY-SA 4.0 |

## Taxonomy

Since version 2.0 the taxonomy is [AviList v2026](https://www.avilist.org/), the consensus global checklist that the IOC, Clements/eBird and BirdLife teams merged into in 2025. IOC 15.2 was the final IOC list. AviList updates once a year in October, and we will follow it.

Each row of the crosswalk has a `relation`:

| relation | Meaning | Count |
|---|---|---|
| `same` | Same scientific name. A few authorities were corrected by AviList | 10,891 |
| `renamed` | Moved to another genus (mostly buntings to *Schoeniclus* and *Melophus*), or a spelling fix | 82 |
| `lumped` | Now a subspecies of another species; its sticker is kept under that species' `forms` | 91 |
| `split_from` | A new species split off from this IOC species | 30 |
| `new` | Not in IOC 15.2 as a species (newly described, or missing from version 0.1) | 8 |

A same name does not always mean the same birds. Each record has `taxonConcept`: `same`, `broadened` (other species were merged into it), `narrowed` (species were split off it), `split` or `new`. Data that described the old set of birds is not carried over to a species whose boundaries changed: names in languages other than English and Simplified Chinese, Wikidata/GBIF/iNaturalist IDs, AVONET traits and body size. We left them empty rather than attach numbers that belong to a different set of birds; `dataNotes` says what was dropped and why.

For five split species we checked where the sticker's reference photo was taken. It turned out to show the population that is now the new species, so the sticker moved there and the parent species is waiting for a new drawing: *Macropygia doreya* → *M. albicapilla*, *Tringa semipalmata* → *T. inornata*, *Oenanthe lugens* → *O. halophila*, *Enicurus leschenaulti* → *E. sinensis*, *Cistothorus palustris* → *C. paludicola* (the last by location only).

## Fields in `species.json`

| Field | What it is | Source | Coverage |
|---|---|---|---|
| `id` | `Genus_species`, used for file names. Changes when AviList changes the scientific name; old IDs are in `previous` | — | all |
| `scientificName`, `authority` | | AviList v2026 | all |
| `taxonomy` | AviList sequence, order, family, family English name, genus | AviList v2026 | all |
| `names` | Common names keyed by language code. `en` is the AviList English name | AviList; IOC 15.2 multilingual | see main README |
| `otherEnglishNames` | Clements and BirdLife English names where they differ from AviList | AviList | where different |
| `ids` | eBird species code, Avibase ID, Wikidata, GBIF, iNaturalist IDs | AviList (eBird, Avibase); Wikidata; iNaturalist | eBird 10,983 |
| `range` | `text`: AviList range statement. `ioc`: IOC 15.2 breeding region and ranges, `zh-Hans`: Chinese summary of the IOC ranges (`zhSource`), both only where the species concept is unchanged | AviList v2026; IOC 15.2 | all; `ioc` 10,882 |
| `subspecies` | Subspecies with authority and range | AviList v2026 | species with subspecies |
| `extinctionStatus` | AviList's extinction flag, for the 43 species flagged extinct, extinct in the wild or possibly extinct | AviList v2026 | 43 |
| `traits` | Mass (g), wing, tail, tarsus, bill measurements (mm), hand-wing index, habitat, migration, trophic level and niche, primary lifestyle. `inferred: true` means AVONET imputed some traits from relatives | AVONET | 10,423 |
| `size` | Body length and wingspan in cm, as `[min, max]`, with where the length came from | Wikidata, Wikipedia text | about 4,700 |
| `conservation` | IUCN Red List category, CITES appendix, China's national protection class | AviList (BirdLife v10); Wikidata P7603; China's 2021 list of protected wild animals | IUCN 10,140 |
| `zh` | Pinyin, Chinese aliases, Chinese order and family names, Chinese Wikipedia page; `formerName` where the IOC Chinese name no longer fits | IOC 15.2 Chinese; Wikidata; Chinese Wikipedia | Chinese name 10,947 |
| `sticker` | File, kind, review score, whether it meets the quality bar, license, and the reference it was drawn from. `null` for the 38 species not drawn yet | this project | 10,973 |
| `forms` | Stickers of subspecies that were separate species in IOC 15.2, with their former names | this project | 91 stickers |
| `taxonConcept`, `previous`, `dataNotes` | What changed in the move to AviList; see Taxonomy above | this project | all |

Notes:

- `conservation.iucn` follows BirdLife v10 as given in AviList; BirdLife is still aligning parts of the Red List with AviList. Check <https://www.iucnredlist.org/> for the latest assessment.
- Where AviList gives no eBird code, eBird currently draws the species boundary differently.
- Chinese names: unchanged species keep the IOC 15.2 Chinese name. For species whose boundaries changed we checked each name by hand and used the name Chinese birders use for the population that occurs in China (e.g. *Apus affinis* 小白腰雨燕), or left it empty where no reliable Chinese name exists. We did not make up names.
- `zh.aliases` come from Wikidata and Chinese Wikipedia and have not been checked one by one; a few are names of other, similar species. Corrections welcome.
- Not included: per-province records and monthly occurrence derived from eBird data (we have not confirmed the terms for redistributing them), and Wikipedia article text (link only).

## Reference photos

Each AI sticker was drawn from one reference photo, or from a board of several images; 178 redrawn stickers were drawn from a written description alone. Its page, author and license are in `sticker.reference`; the photos themselves are not distributed. By sticker (including `forms`):

| Reference | Stickers |
|---|---|
| Openly licensed photo (CC0, public domain, CC BY, CC BY-SA) | 1,954 |
| Board of openly licensed images, or a written description only | 108 |
| Photo under a non-commercial or no-derivatives license (CC BY-NC, -ND, …) | 4,964 |
| Photo with all rights reserved, or under Macaulay Library terms | 1,449 |
| Photo whose license we did not record | 2,017 |
| Board mixing images under different terms | 394 |
| Redrawn from a written description, without a reference photo (`"reference": null`) | 178 |

The stickers are new drawings, not copies. Whether a drawing made from a photo is bound by that photo's license is not settled law; if it matters for your use, filter on `sticker.reference.license`.

## Sources

- AviList Core Team. 2026. AviList: The Global Avian Checklist, v2026. <https://doi.org/10.2173/avilist.v2026>. [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/).
- IOC World Bird List v15.2, Gill, Donsker & Rasmussen (Eds), [CC BY 3.0](https://creativecommons.org/licenses/by/3.0/). <https://worldbirdnames.org/>
- AVONET: Tobias, J. A. et al. (2022). AVONET: morphological, ecological and geographical data for all birds. *Ecology Letters* 25: 581–597. doi:[10.1111/ele.13898](https://doi.org/10.1111/ele.13898). [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/).
- Wikidata, [CC0](https://creativecommons.org/publicdomain/zero/1.0/).
- 国家重点保护野生动物名录 (2021), National Forestry and Grassland Administration and Ministry of Agriculture and Rural Affairs of China.
- Chinese field-guide notes are rewritten from Chinese Wikipedia and other sources and are shared under [CC BY-SA 4.0](https://creativecommons.org/licenses/by-sa/4.0/); `basedOn` gives the main page used.
