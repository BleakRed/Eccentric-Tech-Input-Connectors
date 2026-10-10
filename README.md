# Eccentric Tech — Input Connectors

**Version 0.1** · RimWorld 1.6 · Unofficial add-on for [Eccentric Tech - Spacer Furniture](https://steamcommunity.com/sharedfiles/filedetails/?id=3118185542)

Two floor-level connectors that let an entire storage building or stockpile zone act as a single piece of supply-network infrastructure, instead of one tile at a time.

---

## The problem

Eccentric's supply network reads items off the **cells its provider buildings occupy**. The supply shelf is a 1×1 building, so it contributes only the three stacks that fit on its single tile. Covering a real warehouse means tiling supply shelves across it, each drawing its own power.

The repair shelf has the same shape of limitation: it only repairs gear sitting on its own tile.

These connectors keep the stock behaviour and widen the footprint. You build one on the floor, put a shelf or stockpile on top, and the whole slot group is covered.

---

## Buildings

### Supply input connector

Registers every storable item in the storage above it with the supply network. Autoloaders, repair shelves and autofabricators can then draw from it.

| | |
|---|---|
| Size | 1×1, floor level |
| Cost | Plasteel 50, Component ×5, Advanced component ×2 |
| Work | 1200 |
| Power | 50 W |
| Research | `Eccentric_SupplyNetworks` |
| Construction | 5 |

Also acts as a supply conduit, so a network can route straight through it.

### Repair input connector

Repairs damaged weapons and apparel held in the storage above it, pulling raw materials from the connected network. Shows the item currently being worked on hovering over the plate, and lists every active repair cycle in its **Repair** tab.

| | |
|---|---|
| Size | 1×1, floor level |
| Cost | Plasteel 75, Component ×8, Advanced component ×4 |
| Work | 2500 |
| Power | 100 W (draws nothing while idle) |
| Research | `Eccentric_AutomatedRepair` |
| Construction | 6 |

---

## Usage

1. Research the relevant project — supply networks for the supply connector, automated repair for the repair one.
2. Build the connector on the floor where your storage will sit. It is not an edifice and does not clear the build area, so it can go under existing shelves.
3. Build shelves on top of it, or paint a stockpile zone over the area.
4. Run a supply conduit from the connector to the rest of your network.

The connector reads whatever slot group covers its own tile, so it works with vanilla shelves, multi-cell shelves, stockpile zones, and modded storage such as deep-storage units — anything implementing `ISlotGroupParent`.

### Placement rules

You cannot place a connector within one tile of another of the same kind, or anywhere in a slot group already served by one. Two connectors over the same storage would each claim every item in it, which doubles reported supply counts and has two repair comps competing for the same gear.

Different kinds may overlap freely — a supply connector and a repair connector can serve the same stockpile.

---

## Settings

Mod options → **Eccentric Tech - Input Connectors**

- **Enable supply input connector**
- **Enable repair input connector**

Disabling a connector hides it from the build menu and stops existing ones working. Placed buildings are not destroyed; they go inert until re-enabled. Changes apply immediately, without a restart.

---

## Balance notes

Both connectors are deliberately expensive rather than gated behind deep research. Each sits on the same research as the building it scales up, because it extends that system rather than forming a new one — the supply connector alongside the supply shelf and conduit, the repair connector alongside the repair shelf and cabinet. The advanced-component cost is what keeps them a late-colony convenience rather than an early staple.

---

## Requirements

- RimWorld 1.6
- [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077)
- [Eccentric Tech - Spacer Furniture](https://steamcommunity.com/sharedfiles/filedetails/?id=3118185542)

Load after both.

---

## How it works

The mod adds no new network machinery. It reuses Spacer Furniture's own components and widens the set of cells they look at, through two Harmony postfixes:

- **`SupplyNetwork.CountAvailableSupply`** — after the base pass counts the provider's own cells, the postfix walks the slot group at the connector's position and adds the remaining storable items. The connector's own tile is skipped, because `AddSupplyItem` accumulates rather than deduplicating.
- **`CompRepairItems.SyncParentCells`** — the base method fills `parentCells` from the building's own footprint; the postfix adds the slot group's cells. Everything downstream (`ScanCells`, `ValidateRepairTarget`, material draw, repair cycles) then works unchanged over the wider area.

Two supporting details:

- `SupplyNetwork` only recounts when `supplyCountsDirty` is set, and the stock provider raises that flag only for changes on its own cell. The supply connector sets it on a rare tick so items moving elsewhere in the storage stay visible.
- `CompRepairItems.SyncParentCells` is called once, from `PostSpawnSetup`. A stockpile resized after the connector was built would never enter the repair area, so the repair connector re-invokes it on a rare tick.

The overlay is a new `AbstractOverlayComp` subclass. The repair cabinet's own overlay could not be reused — its component reference is typed to `CompRepairSleepers` and it draws a sleeping pawn's gear.

---

## Building from source

There is no `.csproj`; the single source file compiles directly.

```
csc.exe /target:library /optimize+ /out:Assemblies\EccentricSupplyInput.dll ^
  /reference:<Managed>\netstandard.dll ^
  /reference:<Managed>\Assembly-CSharp.dll ^
  /reference:<Managed>\UnityEngine.CoreModule.dll ^
  /reference:<Managed>\UnityEngine.IMGUIModule.dll ^
  /reference:<Harmony>\0Harmony.dll ^
  /reference:<SpacerFurniture>\1.6\Assemblies\EccentricFurniture.dll ^
  Source\SupplyInput.cs
```

`<Managed>` is `RimWorldWin64_Data\Managed`. Def and cost changes need only a restart, not a rebuild.

---

## Known limitations

- The repair overlay draws above the stored items, so it overlaps a full shelf. It is an activity indicator, not a diorama.
- Placement exclusion is checked when placing. Painting a stockpile across two existing connectors after the fact is not caught.
- Textures are procedural placeholders. See `ART_PROMPTS.md`.

---

## Credits

Supply network, repair system and all reused components are the work of **Aelanna**, from Eccentric Tech - Spacer Furniture. This add-on only widens which cells those components read.

Unofficial fan add-on, not affiliated with or endorsed by the original author.
