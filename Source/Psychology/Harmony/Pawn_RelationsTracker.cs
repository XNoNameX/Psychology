﻿using System;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(Pawn_RelationsTracker), "Notify_RescuedBy")]
    public static class Notify_RescuedBy_BleedingHeartPatch
    {
        [HarmonyPostfix]
        public static void AddBleedingHeartThought(Pawn_RelationsTracker __instance, Pawn rescuer)
        {
            if (rescuer.RaceProps.Humanlike && __instance.canGetRescuedThought)
            {
                rescuer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.RescuedBleedingHeart, Traverse.Create(__instance).Field("pawn").GetValue<Pawn>());
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_RelationsTracker), "SecondaryRomanceChanceFactor")]
    public static class Pawn_RelationsTracker_RomanceChancePatch
    {
        [HarmonyPostfix]
        public static void PsychologyFormula(Pawn_RelationsTracker __instance, ref float __result, Pawn otherPawn)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            PsychologyPawn realPawn = pawn as PsychologyPawn;
            if (realPawn != null)
            {
                if (pawn.def != otherPawn.def || pawn == otherPawn)
                {
                    __result = 0f;
                    return;
                }
                /* Throw away the existing result and substitute our own formula. */
                float ageFactor = 1f;
                float sexualityFactor = 1f;
                float ageBiologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
                float ageBiologicalYearsFloat2 = otherPawn.ageTracker.AgeBiologicalYearsFloat;
                if (PsychologyBase.ActivateKinsey() && realPawn.sexuality != null)
                {
                    float kinsey = 3 - realPawn.sexuality.kinseyRating;
                    float homo = (pawn.gender == otherPawn.gender) ? 1f : -1f;
                    sexualityFactor = Mathf.InverseLerp(3f, 0f, kinsey * homo);
                }
                else if (Rand.ValueSeeded(pawn.thingIDNumber ^ 3273711) >= 0.015f)
                {
                    if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOf.Gay))
                    {
                        if (otherPawn.gender != pawn.gender)
                        {
                            __result = 0f;
                            return;
                        }
                    }
                    else if (otherPawn.gender == pawn.gender)
                    {
                        __result = 0f;
                        return;
                    }
                }
                if (pawn.gender == Gender.Male)
                {
                    if (ageBiologicalYearsFloat2 < 16f)
                    {
                        __result = 0f;
                        return;
                    }
                    float min = Mathf.Max(16f, ageBiologicalYearsFloat - 30f);
                    float lower = Mathf.Max(20f, ageBiologicalYearsFloat - 10f);
                    ageFactor = GenMath.FlatHill(0.15f, min, lower, ageBiologicalYearsFloat, ageBiologicalYearsFloat + 10f, 0.15f, ageBiologicalYearsFloat2);
                }
                else if (pawn.gender == Gender.Female)
                {
                    if (ageBiologicalYearsFloat2 < 16f)
                    {
                        __result = 0f;
                        return;
                    }
                    if ((ageBiologicalYearsFloat2 < ageBiologicalYearsFloat - 10f) && (!pawn.story.traits.HasTrait(TraitDefOfPsychology.OpenMinded)))
                    {
                        ageFactor *= 0.15f;
                    }
                    if (ageBiologicalYearsFloat2 < ageBiologicalYearsFloat - 3f)
                    {
                        ageFactor *= Mathf.InverseLerp(ageBiologicalYearsFloat - 10f, ageBiologicalYearsFloat - 3f, ageBiologicalYearsFloat2) * 0.3f;
                    }
                    else
                    {
                        ageFactor *= GenMath.FlatHill(0.3f, ageBiologicalYearsFloat - 3f, ageBiologicalYearsFloat, ageBiologicalYearsFloat + 10f, ageBiologicalYearsFloat + 30f, 0.15f, ageBiologicalYearsFloat2);
                    }
                }
                ageFactor = Mathf.Lerp(ageFactor, (1.6f - ageFactor), realPawn.psyche.GetPersonalityRating(PersonalityNodeDefOf.Experimental));
                float disabilityFactor = 1f;
                disabilityFactor *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Talking));
                disabilityFactor *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation));
                disabilityFactor *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving));
                disabilityFactor = Mathf.Lerp(disabilityFactor, (1.6f - disabilityFactor), realPawn.psyche.GetPersonalityRating(PersonalityNodeDefOf.Experimental));
                if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOfPsychology.OpenMinded))
                {
                    ageFactor = 1f;
                    disabilityFactor = 1f;
                }
                float relationFactor = 1f;
                foreach (PawnRelationDef current in pawn.GetRelations(otherPawn))
                {
                    relationFactor *= current.attractionFactor;
                }
                int beauty = 0;
                if (otherPawn.RaceProps.Humanlike)
                {
                    beauty = otherPawn.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
                }
                if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOfPsychology.OpenMinded))
                {
                    beauty = 0;
                }
                float beautyFactor = 1f;
                if (beauty < 0)
                {
                    beautyFactor = 0.3f;
                }
                else if (beauty > 0)
                {
                    beautyFactor = 2.3f;
                }
                if (pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight) < 1f)
                {
                    /* Pawns who can't see as well can't determine beauty as well. */
                    beautyFactor = Mathf.Pow(beautyFactor, pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight));
                }
                if (realPawn != null && PsychologyBase.ActivateKinsey() && realPawn.sexuality != null && realPawn.sexuality.AdjustedSexDrive < 1f)
                {
                    /* Pawns with low sex drive will care about physical features less. */
                    beautyFactor = Mathf.Pow(beautyFactor, realPawn.sexuality.AdjustedSexDrive);
                    ageFactor = Mathf.Pow(ageFactor, realPawn.sexuality.AdjustedSexDrive);
                    disabilityFactor = Mathf.Pow(disabilityFactor, realPawn.sexuality.AdjustedSexDrive);
                }
                float initiatorYouthFactor = Mathf.InverseLerp(15f, 18f, ageBiologicalYearsFloat);
                float recipientYouthFactor = Mathf.InverseLerp(15f, 18f, ageBiologicalYearsFloat2);
                __result = 1f * sexualityFactor * ageFactor * disabilityFactor * relationFactor * beautyFactor * initiatorYouthFactor * recipientYouthFactor;
            }
        }
    }
}
