using System;
using static JKMP.Plugin.Multiplayer.Native.Bindings;

namespace JKMP.Plugin.Multiplayer.Native.Compression
{
    public static class LZ4
    {
        /// <summary>
        /// Compresses the input bytes into the output buffer.
        /// The returned value is the number of bytes written to the output buffer,
        /// unless the output buffer is too small. In that case, the function returns -1.
        /// You can use <see cref="GetMaximumOutputSize(ulong)"/> to know the maximum size of the output buffer.
        /// </summary>
        public static unsafe int Compress(ReadOnlySpan<byte> input, Span<byte> output)
        {
            fixed (byte* inputPtr = &input.GetPinnableReference())
            {
                fixed (byte* outputPtr = &output.GetPinnableReference())
                {
                    var inputSlice = new Sliceu8((IntPtr)inputPtr, (ulong)input.Length);
                    var outputSlice = new SliceMutu8((IntPtr)outputPtr, (ulong)output.Length);

                    return lz4_compress(inputSlice, outputSlice);
                }
            }
        }

        /// <summary>
        /// Decompresses the input bytes into the output buffer.
        /// The returned value is the number of bytes written to the output buffer.
        /// If the output buffer is too small or the uncompressed size differs from the output buffer length, the function returns -1.
        /// Any other error will cause the function to return -2. Error descriptions are written to stdout.
        /// </summary>
        public static unsafe int Decompress(ReadOnlySpan<byte> input, Span<byte> output)
        {
            fixed (byte* inputPtr = &input.GetPinnableReference())
            {
                fixed (byte* outputPtr = &output.GetPinnableReference())
                {
                    var inputSlice = new Sliceu8((IntPtr)inputPtr, (ulong)input.Length);
                    var outputSlice = new SliceMutu8((IntPtr)outputPtr, (ulong)output.Length);

                    return lz4_decompress(inputSlice, outputSlice);
                }
            }
        }

        /// <summary>
        /// Returns the maximum compressed size for the given input span's length.
        /// </summary>
        public static ulong GetMaximumOutputSize(Span<byte> input) => GetMaximumOutputSize((ulong)input.Length);
        
        /// <summary>
        /// Returns the maximum compressed size for the given input size.
        /// </summary>
        public static ulong GetMaximumOutputSize(ulong inputLength)
        {
            return lz4_get_maximum_output_size(inputLength);
        }
    }
}