﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;

namespace Psychology.Harmony
{
	[HarmonyPatch(typeof(InteractionWorker_DeepTalk), "RandomSelectionWeight")]
	public static class InteractionWorker_DeepTalk_SelectionWeightPatch
	{
		[HarmonyPrefix]
		public static bool PsychologyException(InteractionWorker_DeepTalk __instance, Pawn initiator, Pawn recipient)
		{
			return !(initiator is PsychologyPawn || recipient is PsychologyPawn);
		}
	}
}
