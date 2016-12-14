using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace FluffyLib
{
#if DEBUG
    public static class Log
    {
        public static void Success( string name, string subject = "Detours", string context = "Tests" )
        {
            Verse.Log.Message( " [ OK ] FluffyLib :: " + context + " :: " + subject + " :: " + name );
        }

        public static void Fail( string name, string subject = "Detours", string context = "Tests", string reason = "" )
        {
            Verse.Log.Message( " [ FAIL ] FluffyLib :: " + context + " :: " + subject + " :: " + name + ( reason.NullOrEmpty() ? "" : "\n" + reason ) );
        }
    }

    public class DetourTestSources
    {
        // simple methods
        public void PublicInstanceMethod()
        {
            Log.Fail( "public instance method" );
        }
        private void PrivateInstanceMethod()
        {
            Log.Fail( "private instance method" );
        }
        public static void PublicStaticMethod()
        {
            Log.Fail( "public static method" );
        }
        private static void PrivateStaticMethod()
        {
            Log.Fail( "private static method" );
        }
        
        // parameter overloads
        public void Overload( string asd, string qwe )
        {
            Log.Fail( "public overload string" );
        }
        public void Overload( int asd, int qwe )
        {
            Log.Fail( "public overload int"  );
        }

        // properties
        public string GetterOnly
        {
            get
            {
                Log.Fail( "public getterOnly getter"  );
                return "asd";
            }
            set
            {
                Log.Success( "public getterOnly setter" );
            }
        }

        public string SetterOnly
        {
            get
            {
                Log.Success( "public setterOnly getter" );
                return "asd";
            }
            set
            {
                Log.Fail( "public setterOnly setter" );
            }
        }
        public string Both
        {
            get
            {
                Log.Fail( "public both getter" );
                return "asd";
            }
            set
            {
                Log.Fail( "public both setter" );
            }
        }
    }

    public class DetourTestTargets
    {
        // simple methods
        [DetourMethod(typeof( DetourTestSources ), "PublicInstanceMethod")]
        public void PublicInstanceMethod()
        {
            Log.Success( "public instance method" );
        }

        [DetourMethod( typeof( DetourTestSources ), "PrivateInstanceMethod" )]
        private void PrivateInstanceMethod()
        {
            Log.Success( "private instance method" );
        }

        [DetourMethod( typeof( DetourTestSources ), "PublicStaticMethod" )]
        public static void PublicStaticMethod()
        {
            Log.Success( "public static method" );
        }
        
        [DetourMethod( typeof( DetourTestSources ), "PrivateStaticMethod" )]
        private static void PrivateStaticMethod()
        {
            Log.Success( "private static method" );
        }

        // parameter overloads
        [DetourMethod( typeof( DetourTestSources ), "Overload" )]
        public void Overload( string asd, string qwe )
        {
            Log.Success( "overload string" );
        }

        [DetourMethod( typeof( DetourTestSources ), "Overload" )]
        public void Overload( int asd, int qwe )
        {
            Log.Success( "overload int" );
        }

        // properties
        [DetourProperty(typeof( DetourTestSources ), "GetterOnly", DetourProperty.Getter)]
        public string GetterOnly
        {
            get
            {
                Log.Success( "public getterOnly getter" );
                return "asd";
            }
            set
            {
                Log.Fail( "public getterOnly setter" );
            }
        }

        [DetourProperty( typeof( DetourTestSources ), "SetterOnly", DetourProperty.Setter )]
        public string SetterOnly
        {
            get
            {
                Log.Fail( "public setterOnly getter" );
                return "asd";
            }
            set
            {
                Log.Success( "public setterOnly setter" );
            }
        }

        [DetourProperty( typeof( DetourTestSources ), "Both" )]
        public string Both
        {
            get
            {
                Log.Success( "public both getter" );
                return "asd";
            }
            set
            {
                Log.Success( "public both setter" );
            }
        }
    }

    public static class DetourTest
    {
        public static void RunTests()
        {
            // instance tests
            Verse.Log.Message( "FluffyLib :: Running tests..." );
            Type sourceType = typeof( DetourTestSources );
            DetourTestSources sources = new DetourTestSources();
            sources.PublicInstanceMethod();
            sourceType.GetMethod( "PrivateInstanceMethod", Detours.AllBindingFlags ).Invoke( sources, null );

            // static tests
            DetourTestSources.PublicStaticMethod();
            sourceType.GetMethod( "PrivateStaticMethod", Detours.AllBindingFlags ).Invoke( null, null );

            // overloads
            sources.Overload( 1, 1 );
            sources.Overload( "asd", "qwe" );

            // properties
            var x = sources.GetterOnly;
            sources.GetterOnly = "asd";
            x = sources.SetterOnly;
            sources.SetterOnly = "asd";
            x = sources.Both;
            sources.Both = "asd";
        }
    }
#endif
}
