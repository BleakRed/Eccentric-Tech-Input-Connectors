using System.Collections.Generic;
using System.Reflection;
using EccentricFurniture;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace EccentricSupplyInput
{
    // ---------------------------------------------------------------- settings

    public class SupplyInputSettings : ModSettings
    {
        public bool supplyConnectorEnabled = true;
        public bool repairConnectorEnabled = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref supplyConnectorEnabled, "supplyConnectorEnabled", true, false);
            Scribe_Values.Look(ref repairConnectorEnabled, "repairConnectorEnabled", true, false);
            base.ExposeData();
        }
    }

    public class SupplyInputMod : Mod
    {
        public static SupplyInputSettings Settings;

        public SupplyInputMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<SupplyInputSettings>();
        }

        public override string SettingsCategory()
        {
            return "Eccentric Tech - Input Connectors";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);
            list.CheckboxLabeled(
                "Enable supply input connector",
                ref Settings.supplyConnectorEnabled,
                "Registers the storage above the connector with the supply network. Disabling hides it from the build menu and stops existing ones contributing.");
            list.Gap(6f);
            list.CheckboxLabeled(
                "Enable repair input connector",
                ref Settings.repairConnectorEnabled,
                "Repairs weapons and apparel held in the storage above the connector. Disabling hides it from the build menu and stops existing ones widening their repair area.");
            list.Gap(12f);
            list.Label("Changes apply immediately. Buildings already placed are not removed - they simply stop working until re-enabled.");
            list.End();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            BuildMenu.Apply();
        }
    }

    // Toggling designationCategory and re-resolving is what actually removes an entry from the
    // architect menu; hiding it any other way leaves a buildable-but-inert ghost in the list.
    [StaticConstructorOnStartup]
    public static class BuildMenu
    {
        // ResolveDesignators is non-public in 1.6, but it is the only thing that rebuilds
        // resolvedDesignators after designationCategory changes.
        private static readonly MethodInfo ResolveDesignators =
            AccessTools.Method(typeof(DesignationCategoryDef), "ResolveDesignators", null, null);

        static BuildMenu()
        {
            Apply();
        }

        public static void Apply()
        {
            SetVisible("Eccentric_SupplyInputConnector", SupplyInputMod.Settings.supplyConnectorEnabled);
            SetVisible("Eccentric_SupplyRepairConnector", SupplyInputMod.Settings.repairConnectorEnabled);

            if (ResolveDesignators == null)
            {
                return;
            }
            List<DesignationCategoryDef> cats = DefDatabase<DesignationCategoryDef>.AllDefsListForReading;
            for (int i = 0; i < cats.Count; i++)
            {
                ResolveDesignators.Invoke(cats[i], null);
                cats[i].DirtyCache();
            }
        }

        private static void SetVisible(string defName, bool visible)
        {
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (def == null)
            {
                return;
            }
            def.designationCategory = visible
                ? DefDatabase<DesignationCategoryDef>.GetNamedSilentFail("Eccentric_Tech")
                : null;
        }
    }

    // ---------------------------------------------------------------- shared

    public static class StorageLink
    {
        public static List<IntVec3> LinkedCells(ThingWithComps parent)
        {
            Map map = parent.Map;
            if (map == null)
            {
                return null;
            }
            // SlotGroupAt covers shelves, stockpile zones and any modded ISlotGroupParent in one call,
            // so deep storage and oversized shelves need no special handling.
            SlotGroup group = map.haulDestinationManager.SlotGroupAt(parent.Position);
            if (group == null)
            {
                return null;
            }
            return group.CellsList;
        }
    }

    // ---------------------------------------------------------------- supply connector

    public class CompProperties_StorageInput : CompProperties
    {
        public CompProperties_StorageInput()
        {
            compClass = typeof(CompStorageInput);
        }
    }

    public class CompStorageInput : ThingComp
    {
        private CompSupplyProvider providerComp;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            providerComp = parent.GetComp<CompSupplyProvider>();
        }

        // SupplyNetwork only recounts when supplyCountsDirty is set, and the stock provider raises it
        // solely from changes to its own cell. Items moving anywhere else in the linked slot group
        // would otherwise stay invisible to the network until something unrelated dirtied it.
        public override void CompTickRare()
        {
            if (providerComp != null && providerComp.network != null)
            {
                providerComp.network.supplyCountsDirty = true;
            }
        }
    }

    // ---------------------------------------------------------------- repair connector

    public class CompProperties_StorageRepair : CompProperties
    {
        public CompProperties_StorageRepair()
        {
            compClass = typeof(CompStorageRepair);
        }
    }

    public class CompStorageRepair : ThingComp
    {
        private static readonly MethodInfo SyncParentCells =
            AccessTools.Method(typeof(CompRepairItems), "SyncParentCells", null, null);

        private CompRepairItems repairComp;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            repairComp = parent.GetComp<CompRepairItems>();
        }

        // CompRepairItems only calls SyncParentCells once, in PostSpawnSetup, so a stockpile that is
        // resized or a shelf built later would never enter the repair area. Re-running it through
        // reflection also re-triggers the postfix below, which is what pulls the slot group back in.
        public override void CompTickRare()
        {
            if (repairComp == null || SyncParentCells == null)
            {
                return;
            }
            SyncParentCells.Invoke(repairComp, null);
            repairComp.dirty = true;
        }
    }

    // ---------------------------------------------------------------- repair overlay

    public class CompProperties_RepairOverlay : CompProperties
    {
        public CompProperties_RepairOverlay()
        {
            compClass = typeof(CompRepairOverlay);
        }
    }

    // AbstractRepairComp picks this up via GetComp<AbstractOverlayComp>() in its PostSpawnSetup, the
    // same hook the repair cabinet's overlay uses. CompRepairCabinetOverlay itself is not reusable -
    // its repairComp field is typed CompRepairSleepers and it draws a pawn's gear in a bed.
    public class CompRepairOverlay : AbstractOverlayComp
    {
        private const float BaseScale = 0.62f;
        private const float BobHeight = 0.07f;
        private const float BobTicks = 110f;

        private CompRepairItems repairComp;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            repairComp = parent.GetComp<CompRepairItems>();
        }

        public override void AnimationTick()
        {
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (repairComp == null || !repairComp.RepairActive())
            {
                return;
            }
            List<RepairCycle> cycles = repairComp.ActiveRepairCycles();
            if (cycles == null || cycles.Count == 0)
            {
                return;
            }
            Thing target = cycles[0].target;
            if (target == null || target.Graphic == null)
            {
                return;
            }

            Material mat = target.Graphic.MatSingleFor(target);
            if (mat == null)
            {
                return;
            }

            float bob = Mathf.Sin(Find.TickManager.TicksGame / BobTicks * Mathf.PI * 2f) * BobHeight;
            Vector3 pos = parent.DrawPos;
            pos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
            pos.z += 0.18f + bob;

            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(pos, Quaternion.identity, new Vector3(BaseScale, 1f, BaseScale));
            Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
        }
    }

    // ---------------------------------------------------------------- placement

    public class PlaceWorker_ConnectorExclusion : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            ThingDef def = checkingDef as ThingDef;
            if (def == null)
            {
                return true;
            }

            // Two connectors over one storage would each claim every item in it - doubled supply
            // counts, or two repair comps fighting over the same gear.
            SlotGroup group = map.haulDestinationManager.SlotGroupAt(loc);
            if (group != null)
            {
                List<IntVec3> cells = group.CellsList;
                for (int i = 0; i < cells.Count; i++)
                {
                    if (Occupied(cells[i], map, def, thingToIgnore))
                    {
                        return new AcceptanceReport("Another " + def.label + " already serves this storage.");
                    }
                }
            }

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(loc, 1.5f, true))
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }
                if (Occupied(cell, map, def, thingToIgnore))
                {
                    return new AcceptanceReport("Too close to another " + def.label + ".");
                }
            }
            return true;
        }

        private static bool Occupied(IntVec3 cell, Map map, ThingDef def, Thing thingToIgnore)
        {
            List<Thing> things = cell.GetThingList(map);
            for (int i = 0; i < things.Count; i++)
            {
                Thing t = things[i];
                if (t == thingToIgnore)
                {
                    continue;
                }
                // catches the building itself plus any blueprint or frame for one
                if (t.def == def || t.def.entityDefToBuild == def)
                {
                    return true;
                }
            }
            return false;
        }
    }

    // ---------------------------------------------------------------- patches

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("bleakred.eccentrictech.supplyinput");

            harmony.Patch(
                AccessTools.Method(typeof(SupplyNetwork), "CountAvailableSupply", null, null),
                null,
                new HarmonyMethod(typeof(HarmonyPatches), "CountAvailableSupply_Postfix", null));

            harmony.Patch(
                AccessTools.Method(typeof(CompRepairItems), "SyncParentCells", null, null),
                null,
                new HarmonyMethod(typeof(HarmonyPatches), "SyncParentCells_Postfix", null));
        }

        public static void CountAvailableSupply_Postfix(SupplyNetwork __instance, CompSupplyProvider provider)
        {
            if (!SupplyInputMod.Settings.supplyConnectorEnabled)
            {
                return;
            }
            if (provider.parent.GetComp<CompStorageInput>() == null || !provider.IsActive)
            {
                return;
            }
            Map map = provider.parent.Map;
            if (map == null)
            {
                return;
            }
            List<IntVec3> cells = StorageLink.LinkedCells(provider.parent);
            if (cells == null)
            {
                return;
            }

            IntVec3 origin = provider.parent.Position;
            for (int i = 0; i < cells.Count; i++)
            {
                IntVec3 cell = cells[i];
                // The vanilla pass already counted the connector's own cell; AddSupplyItem does not
                // deduplicate, so counting it again would double the stack totals.
                if (cell == origin)
                {
                    continue;
                }
                List<Thing> things = cell.GetThingList(map);
                for (int j = 0; j < things.Count; j++)
                {
                    Thing thing = things[j];
                    if (thing is Building)
                    {
                        continue;
                    }
                    if (!thing.def.EverStorable(false))
                    {
                        continue;
                    }
                    __instance.AddSupplyItem(thing);
                }
            }
        }

        public static void SyncParentCells_Postfix(CompRepairItems __instance, HashSet<IntVec3> ___parentCells)
        {
            if (!SupplyInputMod.Settings.repairConnectorEnabled)
            {
                return;
            }
            if (__instance.parent.GetComp<CompStorageRepair>() == null)
            {
                return;
            }
            List<IntVec3> cells = StorageLink.LinkedCells(__instance.parent);
            if (cells == null)
            {
                return;
            }
            for (int i = 0; i < cells.Count; i++)
            {
                ___parentCells.Add(cells[i]);
            }
        }
    }
}
