//#define DEBUG_SPAMMY_BOOTSTRAPPERS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;


namespace FluffyLib
{
    public static class Bootstrappers
    {
        public static void DoBootstraps( On on )
        {
            // provide some info
            Verse.Log.Message( "FluffyLib :: Bootstrappers :: Scanning mod assemblies for bootstrapping requests..." );

            // loop over all methods and properties in all mod's assemblies
            foreach ( ModContentPack mod in LoadedModManager.RunningMods )
            {
#if DEBUG_SPAMMY_BOOTSTRAPPERS
                Verse.Log.Message( mod.Name );
#endif
                foreach ( Assembly assembly in mod.assemblies.loadedAssemblies )
                {
#if DEBUG_SPAMMY_BOOTSTRAPPERS
                    Verse.Log.Message( "\t" + assembly.FullName );
#endif
                    // loop over all types that inherit from the our bootstrapper, and call their bootstrap method.
                    foreach ( Type type in assembly.GetTypes().Where( t => typeof( Bootstrapper ).IsAssignableFrom( t ) && !t.IsAbstract ) )
                    {
#if DEBUG_SPAMMY_BOOTSTRAPPERS
                        Verse.Log.Message( "\t\t" + type.FullName );
#endif
                        // instantiate object
                        Bootstrapper bootstrapper = type.GetConstructor( Type.EmptyTypes )?.Invoke( null ) as Bootstrapper;

                        // check if that worked
                        if ( bootstrapper == null )
                        {
                            Verse.Log.Error( "FluffyLib :: Failed to instantiate bootstrapper " + type.FullName + " for " + mod.Name );
                            continue;
                        }

                        // check if we should call this bootstrapper now
                        if ( bootstrapper.On != on )
                            continue;

                        // do whatever it is it does
                        Verse.Log.Message( "FluffyLib :: Running " + bootstrapper.GetType().FullName + " for " + mod.Name );
                        bootstrapper.Bootstrap();
                    }
                }
            }
        }
    }
}
