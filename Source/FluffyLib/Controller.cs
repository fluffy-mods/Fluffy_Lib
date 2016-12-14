using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace FluffyLib
{
    public class Controller : MonoBehaviour
    {
        public virtual void Start()
        {
            Log( "Starting" );

            // scan all mod assemblies for methods that should be detoured
            Detours.DoDetours();

            // scan all mod assemblies for mod bootstrappers
            Bootstrappers.DoBootstraps( On.GameInit );

            // we don't need this controller to tick.
            enabled = false;
        }

        public virtual void OnLevelWasLoaded( int level )
        {
            Log( "Map Loaded" );

            // scan all mod assemblies for mod bootstrappers
            Bootstrappers.DoBootstraps( On.MapLoaded );

            // scan all mod assemblies for mapcomponents that should be injected
            MapComponents.DoMapComponents();
        }

        public static void Log( string msg, string context = "" )
        {
            Verse.Log.Message( "FluffyLib :: " + ( !context.NullOrEmpty() ? context + " :: " : "" ) + msg );
        }
    }
}
