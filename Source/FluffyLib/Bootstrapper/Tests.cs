using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace FluffyLib
{
#if DEBUG
    public class BootstrapTestsGameInit : Bootstrapper
    {
        public override On On => On.GameInit;
        public override void Bootstrap() { Log.Success( "GameInit", "Bootstrappers" ); }
    }

    public class BootstrapTestsMapLoaded : Bootstrapper
    {
        public override On On => On.MapLoaded;
        public override void Bootstrap() { Log.Success( "MapLoaded", "Bootstrappers" ); }
    }
#endif
}
