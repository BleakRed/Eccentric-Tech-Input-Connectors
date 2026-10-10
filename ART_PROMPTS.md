# Art prompts

The shipped textures are procedural placeholders. These prompts are for replacing them with real art **that matches Eccentric Tech**.

Drop replacements at the exact paths below, same filenames — no def changes needed.

---

## House style

Every value here was sampled from Spacer Furniture's own textures, not invented. Match it.

### Palette

| Role | Hex | Sampled from |
|---|---|---|
| Building body | `#808080` | autoloader, autofabricator |
| Body shade | `#707070` / `#606060` | autoloader |
| Conduit / light panel | `#A0A0A0` – `#B0B0B0` | supply conduit |
| Outline | `#000000` | everything |
| Accent, shade | `#508080` | repair cabinet active arm |
| Accent, mid | `#70B0B0` | supply conduit |
| Accent, core | `#A0FFFF` | supply conduit overlay |

### Three rules that matter more than the prompt text

1. **One accent hue for the whole mod line.** Eccentric uses a single desaturated pale cyan everywhere — even the repair cabinet's active arm glows cyan. There is no amber, no red-for-repair, no per-function colour coding. The only coloured body in the whole set is the autorefinery (`#A03030`), and that reads as painted machinery, not a status light.
2. **Buildings are told apart by form, not colour.** Since everything is grey and cyan, silhouette and the shape of the central feature carry all the identity. Two buildings in the same role should share a body and differ in their working part.
3. **Faces are restrained.** A large flat grey plate, a heavy pure-black outline, one thin inset seam, small quiet corner marks, and a single functional feature. Resist adding greebles, rivets, wear, text or logos — the originals are far emptier than instinct suggests.

### Prompt preamble

> Top-down orthographic game asset for RimWorld, viewed straight from directly above with no perspective. Flat 2D sprite, minimal shading, hard pure-black outline roughly 6 px thick at 256. Industrial spacer-tech, utilitarian and understated, matte not glossy. Flat neutral grey body `#808080` with a single pale desaturated cyan feature (`#70B0B0` with a `#A0FFFF` core). No text, no logos, no wear or grime. Transparent background. Must read cleanly at 32 px.

### Lore voice

Spacer Furniture's own descriptions are dry engineering copy — "a network pipe used to transport supplies", "this compact autoloader attaches to a reloadable or refuelable device". Deliberately unglamorous automation that removes tedium. Keep the art at that register: infrastructure, not a set piece.

---

## The shared plate

Both connectors use one plate, modelled on **RimIOT's input connector** — the closest existing art for "a thing you build underneath storage". Draw it once and change only the face.

> A square floor plate with generously rounded corners, seen from directly above. Heavy pure-black outline. Light grey face shading from `#CECECE` at the top to `#A6A6A6` at the bottom. A dark rounded block inset at each of the four corners. A pale-cyan bus bar runs along the top edge and another along the bottom edge, between the corner blocks — a dark teal base, mid-cyan body, bright `#A0FFFF` highlight line.

Only the centre of the face differs between the two.

---

## 1. Supply input connector

**Path:** `Textures/EccentricSupplyInput/SupplyInputConnector.png`
**Size:** 256×256, RGBA

> The shared plate with a smooth, unbroken face. A horizontal seam runs across the middle, stepping up into a shallow trapezoid at the centre like an access hatch, with a thin bright highlight line just below it. A small round port sits on the seam at dead centre: black rim, mid-cyan body, bright pale core.

**Notes:** Closely follows the IOT connector's stepped centre seam. The face stays smooth and empty — that emptiness is the whole contrast with the repair plate.

---

## 2. Repair input connector

**Path:** `Textures/EccentricSupplyInput/RepairInputConnector.png`
**Size:** 256×256, RGBA

> The shared plate, but the inner face is a recessed **fine mesh grid** — a pale grid of small square cells behind a black-outlined rounded window, with a scattering of cells filled slightly darker so the mesh looks worked rather than printed. Centred over the mesh is a servicing head: concentric black rim, dark teal, mid-cyan, then a dark recessed bore with a bright pale cyan point at its middle.

**Notes:** The mesh is taken from Spacer Furniture's **repair shelf**, which has exactly this speckled grid face — it is the mod's existing visual shorthand for "this surface works on the items placed on it". Keep the grid fine; at 32 px it should read as texture, not as individual cells.

Same hue as the supply plate by design — do not tint it warm. Identity comes from mesh-versus-smooth, which survives the zoom-out better than any centre detail.

---

## 3. Workshop preview

**Path:** `About/Preview.png`
**Size:** 640×360
**Status:** a procedural title card ships now. This prompt is for a proper replacement.

> A RimWorld colony storage room seen top-down. Rows of grey metal shelves stacked with crates, steel and weapons. Set into the floor beneath two shelf runs are two square armoured connector plates glowing pale cyan, with thin conduit lines running from them toward the edge of frame. A damaged rifle hovers in a soft cyan field above one plate. Cool neutral lighting, dark floor, restrained palette of greys and one cyan. Clean composition with empty space in the upper left for a title.

Overlay **"Input Connectors"** in the upper-left space, with **"Eccentric Tech add-on"** smaller beneath.

---

## Optional extras

Not required for v0.1.

- **Repair field effect** — a soft pale-cyan elliptical glow the overlay could draw beneath the hovering item, to separate it from whatever is stored under it. 64×64, RGBA, heavy alpha falloff, no hard rim.
- **Linked-cell overlay tile** — a translucent cyan square marking cells a connector has claimed, for a future selection highlight. 64×64, flat colour with a 2 px inner border, roughly 35% alpha.
