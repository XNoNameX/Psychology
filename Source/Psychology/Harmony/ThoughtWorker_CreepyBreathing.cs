﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(ThoughtWorker_CreepyBreathing), "CurrentSocialStateInternal")]
    public static class ThoughtWorker_CreepyBreathingPatch
    {
        [HarmonyPostfix]
        public static void Disable(ref ThoughtState __result, Pawn pawn, Pawn other)
        {
            if (pawn is PsychologyPawn && other is PsychologyPawn)
            {
                __result = false;
            }
        }
    }
}
