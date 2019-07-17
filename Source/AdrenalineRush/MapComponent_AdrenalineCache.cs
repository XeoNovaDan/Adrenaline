using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using RimWorld.Planet;

namespace Adrenaline
{

    public class MapComponent_AdrenalineCache : MapComponent
    {

        private bool cacheSet;
        private const int AllPotentialHostileThingsUpdateInterval = 120;

        public MapComponent_AdrenalineCache(Map map) : base(map)
        {
            this.map = map;
        }

        public override void MapComponentTick()
        {
            // For save compatibility, but also because getting the list to save is a PITA
            if (!cacheSet)
            {
                ResetCachedPotentialHostileThings();
                cacheSet = true;
            }

            if (Find.TickManager.TicksGame % AllPotentialHostileThingsUpdateInterval == 0)
            {
                allPotentialHostileThings = map.mapPawns.AllPawnsSpawned.ToArray().Concat(cachedPotentialHostileThings).ToList();
            }
        }

        public void TryAddToCache(Thing t)
        {
            if (!cachedPotentialHostileThings.Contains(t))
            {
                Log.Message($"Adding {t} to cache");
                cachedPotentialHostileThings.Add(t);
            }    
        }

        public void TryRemoveFromCache(Thing t)
        {
            if (cachedPotentialHostileThings.Contains(t))
            {
                Log.Message($"Removing {t} from cache");
                cachedPotentialHostileThings.Remove(t);
            } 
        }

        private void ResetCachedPotentialHostileThings()
        {
            Log.Message("Resetting cache");
            cachedPotentialHostileThings.Clear();
            foreach (var thing in map.listerThings.AllThings)
                if (thing.IsPotentialPerceivableThreat())
                    TryAddToCache(thing);
        }

        public override void ExposeData()
        {
            //Scribe_Values.Look(ref cacheSet, "cacheSet");
            //Scribe_Collections.Look(ref cachedPotentialHostileThings, "cachedPotentialHostileThings", LookMode.Deep);
        }

        private List<Thing> cachedPotentialHostileThings = new List<Thing>();

        public List<Thing> allPotentialHostileThings = new List<Thing>();


    }

}
