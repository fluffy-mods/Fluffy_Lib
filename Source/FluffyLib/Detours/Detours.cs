// #define DEBUG_SPAMMY_DETOURS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Text;
using Verse;

namespace FluffyLib
{        
    /// <summary>
    /// =======================
    /// The following functionality is largely taken from CCL. This implementation uses only the core detour functionality
    /// by RawCode and 1000101, detouring by attribute has been implemented by Fluffy, gratefully repurposing
    /// code snippets contributed to CCL by Zhentar.
    /// 
    /// The implementation here is simpler, and assumes the user knows what he/she is doing. It doesn't do many of the 
    /// safety and sanity checks that CCL implements, which has the benefit of being easier to maintain - but should 
    /// be used at your own risk!
    /// =======================
    /// 
    /// The basic implementation of the IL method 'hooks' (detours) made possible by RawCode's work (32-bit);
    /// https://ludeon.com/forums/index.php?topic=17143.0
    ///
    /// Additional implementation features(64-bit, error checking, method gathering, method validation, etc)
    /// are coded by and based on research done by 1000101.
    ///
    /// Method parameter list matching for initial gathering purposes supplied by Zhentar.
    ///
    /// Performs detours, spits out basic logs and warns if a method is detoured multiple times.
    ///
    /// Remember when stealing...err...copying free code to make sure the proper people get proper credit.
    /// </summary>
    public static class Detours
    {
        internal static BindingFlags AllBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                                       BindingFlags.NonPublic;

        private static Dictionary<MethodInfo, MethodInfo> detours = new Dictionary<MethodInfo, MethodInfo>();

        internal static void DoDetours()
        {
            // provide some info
            Verse.Log.Message( "FluffyLib :: Detours :: Scanning mod assemblies for detour requests..." );

            // loop over all methods and properties in all mod's assemblies
            foreach ( ModContentPack mod in LoadedModManager.RunningMods )
            {
#if DEBUG_SPAMMY_DETOURS
                Verse.Log.Message( mod.Name );
#endif
                foreach ( Assembly assembly in mod.assemblies.loadedAssemblies )
                {
#if DEBUG_SPAMMY_DETOURS
                    Verse.Log.Message( "\t" + assembly.FullName );
#endif
                    foreach ( Type type in assembly.GetTypes() )
                    {
#if DEBUG_SPAMMY_DETOURS
                        Verse.Log.Message( "\t\t" + type.FullName );
#endif
                        foreach ( MethodInfo methodInfo in type.GetMethods( AllBindingFlags ) )
                        {
#if DEBUG_SPAMMY_DETOURS
                            Verse.Log.Message( "\t\t\t" + methodInfo.Name );
#endif
                            if ( methodInfo.HasAttribute<DetourMethodAttribute>() )
                            {
#if DEBUG_SPAMMY_DETOURS
                                Verse.Log.Message( "\t\t\t\t" + "DING!" );
#endif
                                // if attribute is defined, do the detour
                                DetourMethodAttribute detourAttribute = methodInfo.GetCustomAttributes( typeof( DetourMethodAttribute ), false ).First() as DetourMethodAttribute;
                                HandleDetour( detourAttribute, methodInfo );
                            }
                        }
                        foreach ( PropertyInfo propertyInfo in type.GetProperties( ( AllBindingFlags ) ) )
                        {
#if DEBUG_SPAMMY_DETOURS
                            Verse.Log.Message( "\t\t\t" + propertyInfo.Name );
#endif
                            if ( propertyInfo.HasAttribute<DetourPropertyAttribute>() )
                            {
#if DEBUG_SPAMMY_DETOURS
                                Verse.Log.Message( "\t\t\t\t" + "DING!" );
#endif
                                // if attribute is defined, do the detour
                                DetourPropertyAttribute detourAttribute = propertyInfo.GetCustomAttributes( typeof( DetourPropertyAttribute ), false ).First() as DetourPropertyAttribute;
                                HandleDetour( detourAttribute, propertyInfo );
                            }
                        }
                    }
                }
            }

#if DEBUG
            DetourTest.RunTests();
#endif
        }

        private static void HandleDetour( DetourMethodAttribute sourceAttribute, MethodInfo targetInfo )
        {
            // we need to get the method info of the source (usually, vanilla) method. 
            // if it was specified in the attribute, this is easy. Otherwise, we'll have to do some digging.
            MethodInfo sourceInfo = sourceAttribute.WasSetByMethodInfo
                                        ? sourceAttribute.sourceMethodInfo
                                        : GetMatchingMethodInfo( sourceAttribute, targetInfo );

            // make sure we've got what we wanted.
            if ( sourceInfo == null )
                throw new NullReferenceException( "sourceMethodInfo could not be found based on attribute" );
            if ( targetInfo == null )
                throw new ArgumentNullException( nameof( targetInfo ) );

            // call the actual detour
            TryDetourFromTo( sourceInfo, targetInfo );
        }

        private static MethodInfo GetMatchingMethodInfo( DetourMethodAttribute sourceAttribute, MethodInfo targetInfo )
        {
            // we should only ever get here in case the attribute was not defined with a sourceMethodInfo, but let's check just in case.
            if ( sourceAttribute.WasSetByMethodInfo )
                return sourceAttribute.sourceMethodInfo;

            // aight, let's search by name
            MethodInfo[] candidates =
                sourceAttribute.sourceType.GetMethods( AllBindingFlags )
                               .Where( mi => mi.Name == sourceAttribute.sourceMethodName ).ToArray();

            // if we only get one result, we've got our method info - if the length is zero, the method doesn't exist.
            if (candidates.Length == 0)
                return null;
            if (candidates.Length == 1)
                return candidates.First();

            // this is where things get slightly complicated, we'll have to search by parameters.
            candidates = candidates.Where( mi =>
                                           mi.ReturnType == targetInfo.ReturnType &&
                                           mi.GetParameters()
                                             .Select( pi => pi.ParameterType )
                                             .SequenceEqual( targetInfo.GetParameters().Select( pi => pi.ParameterType ) ) )
                                   .ToArray();

            // if we only get one result, we've got our method info - if the length is zero, the method doesn't exist.
            if ( candidates.Length == 0 )
                return null;
            if ( candidates.Length == 1 )
                return candidates.First();
            
            // if we haven't returned anything by this point there were still multiple candidates. This is theoretically impossible,
            // unless I missed something.
            return null;
        }

        private static void HandleDetour( DetourPropertyAttribute sourceAttribute, PropertyInfo targetInfo )
        {
            // first, lets get the source propertyInfo - there's no ambiguity here.
            PropertyInfo sourceInfo = sourceAttribute.sourcePropertyInfo;

            // do our detours
            // if getter was flagged (so Getter | Both )
            if ( ( sourceAttribute.detourProperty & DetourProperty.Getter ) == DetourProperty.Getter )
                TryDetourFromTo( sourceInfo.GetGetMethod( true ), targetInfo.GetGetMethod( true ) );

            // if setter was flagged
            if ( ( sourceAttribute.detourProperty & DetourProperty.Setter ) == DetourProperty.Setter )
                TryDetourFromTo( sourceInfo.GetSetMethod( true ), targetInfo.GetSetMethod( true ) );
        }

        private static string FullName( this MethodInfo methodInfo )
        {
            return methodInfo.DeclaringType.FullName + "." + methodInfo.Name;
        }
        
        private static unsafe void TryDetourFromTo( MethodInfo sourceMethod, MethodInfo destinationMethod )
        {
            // check if already detoured, if so - error out.
            if ( detours.ContainsKey( sourceMethod ) )
            {
                Verse.Log.Error( "FluffyLib :: " + sourceMethod.FullName() + " was already detoured to " +
                           detours[sourceMethod].FullName() + "! Doing nothing." );
                return;
            }
              
            // do the detour and log it.
            detours.Add( sourceMethod, destinationMethod );
            Verse.Log.Message( "FluffyLib :: Detouring " + sourceMethod.FullName() + " to " + destinationMethod.FullName() );

            // Now the meat!  Do the machine-word size appropriate detour (32/64-bit)
            if ( IntPtr.Size == sizeof( Int64 ) )
            {
                // 64-bit systems use 64-bit absolute address and jumps
                // 12 byte destructive

                // Get function pointers
                long sourceMethodPtr = sourceMethod.MethodHandle.GetFunctionPointer().ToInt64();
                long destinationMethodPtr = destinationMethod.MethodHandle.GetFunctionPointer().ToInt64();

                // Native source address
                byte* sourceMethodRawPtr = (byte*)sourceMethodPtr;

                // Pointer to insert jump address into native code
                long* jumpInstructionAddressPtr = (long*)( sourceMethodRawPtr + 0x02 );

                // Insert 64-bit absolute jump into native code (address in rax)
                // mov rax, immediate64
                // jmp [rax]
                *( sourceMethodRawPtr + 0x00 ) = 0x48;
                *( sourceMethodRawPtr + 0x01 ) = 0xB8;
                *jumpInstructionAddressPtr = destinationMethodPtr; // ( sourceMethodRawPtr + 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 )
                *( sourceMethodRawPtr + 0x0A ) = 0xFF;
                *( sourceMethodRawPtr + 0x0B ) = 0xE0;
            }
            else
            {
                // 32-bit systems use 32-bit relative offset and jump
                // 5 byte destructive

                // Get function pointers
                int sourceMethodPtr = sourceMethod.MethodHandle.GetFunctionPointer().ToInt32();
                int destinationMethodPtr = destinationMethod.MethodHandle.GetFunctionPointer().ToInt32();

                // Native source address
                byte* sourceMethodRawPtr = (byte*)sourceMethodPtr;

                // Pointer to insert jump address into native code
                int* jumpInstructionAddressPtr = (int*)( sourceMethodRawPtr + 1 );

                // Jump offset (less instruction size)
                int relativeJumpOffset = ( destinationMethodPtr - sourceMethodPtr ) - 5;

                // Insert 32-bit relative jump into native code
                *sourceMethodRawPtr = 0xE9;
                *jumpInstructionAddressPtr = relativeJumpOffset;
            }
        }
    }
}
