﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;

namespace Psychology.Harmony.Optional
{
    [StaticConstructorOnStartup]
    class EdBPrepareCarefully
    {
        static EdBPrepareCarefully()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.psychology.prepare_carefully_patch");
            #region prepareCarefully
            {
                try
                {
                    ((Action)(() =>
                    {
                        if (AccessTools.Method(typeof(EdB.PrepareCarefully.PrepareCarefully), nameof(EdB.PrepareCarefully.PrepareCarefully.Initialize)) != null)
                        {
                            harmony.Patch(AccessTools.Method(typeof(EdB.PrepareCarefully.PanelBackstory), "DrawPanelContent"), null, new HarmonyMethod(typeof(PanelBackstoryPatch), "AddPsycheEditButton"));
                            harmony.Patch(AccessTools.Method(typeof(EdB.PrepareCarefully.PresetLoaderVersion3), "LoadPawn", new Type[] { typeof(EdB.PrepareCarefully.SaveRecordPawnV3) }), null, new HarmonyMethod(typeof(PresetLoaderPatch), "AddPsyche"));
                            harmony.Patch(AccessTools.Method(typeof(EdB.PrepareCarefully.PresetSaver), "SaveToFile", new Type[] { typeof(EdB.PrepareCarefully.PrepareCarefully), typeof(string) }), null, null, new HarmonyMethod(typeof(PresetSaverPatch), "SavePawnRef"));
                            harmony.Patch(AccessTools.Method(typeof(EdB.PrepareCarefully.ColonistSaver), "SaveToFile", new Type[] { typeof(EdB.PrepareCarefully.CustomPawn), typeof(string) }), null, null, new HarmonyMethod(typeof(PresetSaverPatch), "SavePawnRef"));
                            harmony.Patch(AccessTools.Method(typeof(EdB.PrepareCarefully.SaveRecordPawnV3), "ExposeData"), null, new HarmonyMethod(typeof(SaveRecordPawnV3Patch), "ExposePsycheData"));
                        }
                    }))();
                }
                catch (TypeLoadException) { }
            }
            #endregion
        }
    }
}
