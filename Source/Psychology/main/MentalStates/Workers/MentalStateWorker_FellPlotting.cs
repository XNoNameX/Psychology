﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class MentalStateWorker_FellPlotting : MentalStateWorker
    {
        public override bool StateCanOccur(Pawn pawn)
        {
            IEnumerable<Pawn> rivals = (from x in pawn.Map.mapPawns.FreeColonistsSpawned
                                        where pawn.relations.OpinionOf(x) < -20
                                        select x);
            return base.StateCanOccur(pawn) && !pawn.story.WorkTagIsDisabled(WorkTags.Violent) && rivals.Count() > 0;
        }
    }
}
