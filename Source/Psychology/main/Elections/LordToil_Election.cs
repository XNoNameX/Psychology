﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using UnityEngine;

namespace Psychology
{
    public class LordToil_Election : LordToil
    {
        public LordToil_Election(IntVec3 spot)
        {
            this.spot = spot;
        }

        public override void UpdateAllDuties()
        {
            for (int i = 0; i < this.lord.ownedPawns.Count; i++)
            {
                this.lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOfPsychology.Vote, this.spot, -1f);
            }
        }
        
        public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
        {
            return DutyDefOfPsychology.Vote.hook;
        }

        public override void Notify_ReachedDutyLocation(Pawn pawn)
        {
            LordJob_Joinable_Election election = pawn.GetLord().LordJob as LordJob_Joinable_Election;
            PsychologyPawn voter = pawn as PsychologyPawn;
            if(election != null && voter != null && !election.voters.Contains(pawn.GetHashCode()))
            {
                election.voters.Add(pawn.GetHashCode());
                if(election.candidates.Find(c => c.pawn == voter) == null)
                {
                    List<Pair<PsychologyPawn, float>> possibleVotes = new List<Pair<PsychologyPawn, float>>();
                    foreach (Candidate candidate in election.candidates)
                    {
                        float issueWeighting = 0f;
                        candidate.nodes.ForEach(p => issueWeighting += Mathf.Pow((1f - Mathf.Abs(candidate.pawn.psyche.GetPersonalityRating(p) - voter.psyche.GetPersonalityRating(p))), 2) * Mathf.Pow(10, p.controversiality));
                        possibleVotes.Add(new Pair<PsychologyPawn, float>(candidate.pawn, issueWeighting+voter.relations.OpinionOf(candidate.pawn)));
                    }
                    IEnumerable<Pair<PsychologyPawn, float>> orderedPossibleVotes = (from v in possibleVotes
                                                                                     orderby v.Second descending
                                                                                     select v);
                    if (Prefs.DevMode && Prefs.LogVerbose)
                    {
                        StringBuilder voteString = new StringBuilder("[Psychology] Vote weights for " + voter.LabelShort + ": ");
                        foreach (Pair<PsychologyPawn, float> v in orderedPossibleVotes)
                        {
                            voteString.Append(v.First.LabelShort + " " + v.Second + " ");
                        }
                        Log.Message(voteString.ToString());
                    }
                    election.votes.Add(orderedPossibleVotes.First().First.LabelShort);
                }
                else
                {
                    election.votes.Add(voter.LabelShort);
                }
            }
        }

        private IntVec3 spot;
    }
}
