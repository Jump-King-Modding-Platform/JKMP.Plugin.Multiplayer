// This file exists to extend the generated code in Bindings.cs.
// The writer that generates C# code is not perfect after all.

using System;

namespace JKMP.Plugin.Multiplayer.Native
{
    public partial struct Slicei16
    {
        public ReadOnlySpan<short> AsReadOnlySpan()
        {
            if (len == 0)
                return ReadOnlySpan<short>.Empty;
            
            unsafe
            {
                return new ReadOnlySpan<short>(data.ToPointer(), (int)len);
            }
        }
    }
}