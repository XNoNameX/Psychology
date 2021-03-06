﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI.Group;
using UnityEngine;

namespace Psychology
{
    public class InteractionWorker_HangOut : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if(initiator.guest.IsPrisoner || recipient.guest.IsPrisoner)
            {
                return 0f;
            }
            PsychologyPawn realRecipient = recipient as PsychologyPawn;
            PsychologyPawn realInitiator = initiator as PsychologyPawn;
            if (realRecipient == null || realInitiator == null)
            {
                return 0f;
            }
            if (initiator.GetLord() != null || recipient.GetLord() != null)
            {
                return 0f;
            }
            if (initiator.Drafted || recipient.Drafted)
            {
                return 0f;
            }
            if (!PartyUtility.AcceptableGameConditionsToStartParty(initiator.Map))
            {
                return 0f;
            }
            if (initiator.health.summaryHealth.SummaryHealthPercent < 1f || recipient.health.summaryHealth.SummaryHealthPercent < 1f)
            {
                return 0f;
            }
            if (initiator.Faction != recipient.Faction)
            {
                return 0f;
            }
            float initiatorFactor = 0f;
            float recipientFactor = 0f;
            if (initiator.relations.OpinionOf(recipient) > -20)
            {
                initiatorFactor = realInitiator.psyche.GetPersonalityRating(PersonalityNodeDefOf.Extroverted) + 0.15f + Mathf.InverseLerp(0f, 100f, initiator.relations.OpinionOf(recipient));
                recipientFactor = realRecipient.psyche.GetPersonalityRating(PersonalityNodeDefOf.Friendly);
            }
            else if(initiator.relations.OpinionOf(recipient) <= -20)
            {
                initiatorFactor = Mathf.InverseLerp(0f, 0.4f, realInitiator.psyche.GetPersonalityRating(PersonalityNodeDefOf.Empathetic)-0.6f);
                recipientFactor = realRecipient.psyche.GetPersonalityRating(PersonalityNodeDefOf.Trusting);
            }
            float scheduleFactor = 0f;
            if (initiator.GetTimeAssignment() == TimeAssignmentDefOf.Anything)
            {
                scheduleFactor = 0.33f;
            }
            else if (initiator.GetTimeAssignment() == TimeAssignmentDefOf.Joy)
            {
                scheduleFactor = 1f;
            }
            if (initiator.mindState.IsIdle && recipient.mindState.IsIdle && initiator.GetTimeAssignment() != TimeAssignmentDefOf.Work && recipient.GetTimeAssignment() != TimeAssignmentDefOf.Work)
            {
                scheduleFactor = 5f;
            }
            return 0.05f * initiatorFactor * recipientFactor * scheduleFactor * RendezvousUtility.ColonySizeFactor(initiator);
        }

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
        {
            initiator.jobs.StopAll();
            recipient.jobs.StopAll();
            Lord meeting = LordMaker.MakeNewLord(initiator.Faction, new LordJob_HangOut(initiator, recipient), initiator.Map, new Pawn[] { initiator, recipient });
        }
        
    }
}
