using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluffyLib
{
    public enum On
    {
        GameInit,
        MapLoaded
    }

    public abstract class Bootstrapper
    {
        public abstract void Bootstrap();
        public virtual On On => On.MapLoaded;
    }
}
