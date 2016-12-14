using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using RimWorld;
using UnityEngine;

namespace FluffyLib
{
    public class Bootstrap : ITab
    {
        public Bootstrap() : base()
        {
            // this gets constructed upon game load, and will be used to create our main controller.
            // however, we can't construct gameObjects directly in the ITab's constructor function since A14,
            // as RimWorld will now load mods on separate threads, and the version of Unity used in RimWorld 
            // is single threaded. 
            // To avoid this, we queue up our initialization to be performed after all mods are loaded. 
            LongEventHandler.ExecuteWhenFinished( Initialize );
        }

        private void Initialize()
        {
            // First, create a GameObject instance...
            GameObject gameObject = new GameObject( "Fluffy Lib" );

            // Second, attach our logic to it... 
            gameObject.AddComponent<Controller>();

            // Finally, make sure Unity doesn't destroy this object on level changes.
            MonoBehaviour.DontDestroyOnLoad( gameObject );
        }

        protected override void FillTab()
        {
            // required implementation.
        }
    }
}
