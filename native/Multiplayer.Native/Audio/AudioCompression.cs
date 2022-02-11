using System;

namespace JKMP.Plugin.Multiplayer.Native.Audio
{
    public static class AudioCompression
    {
        public static unsafe int Compress(ReadOnlySpan<short> data, Span<byte> compressedOutput)
        {
            fixed (short* inPtr = &data.GetPinnableReference())
            {
                fixed (byte* outPtr = &compressedOutput.GetPinnableReference())
                {
                    Slicei16 inSlice = new((IntPtr)inPtr, (ulong)data.Length);
                    SliceMutu8 outSlice = new((IntPtr)outPtr, (ulong)compressedOutput.Length);

                    try
                    {
                        return Bindings.opus_compress(inSlice, outSlice);
                    }
                    catch (InteropException<MyFFIError> err)
                    {
                        if (err.Error == MyFFIError.OutputBufferTooSmall)
                        {
                            return -1;
                        }

                        throw;
                    }
                }
            }
        }

        public static unsafe int Decompress(ReadOnlySpan<byte> data, Span<short> decompressedOutput)
        {
            fixed (byte* inPtr = &data.GetPinnableReference())
            {
                fixed (short* outPtr = &decompressedOutput.GetPinnableReference())
                {
                    Sliceu8 inSlice = new((IntPtr)inPtr, (ulong)data.Length);
                    SliceMuti16 outSlice = new((IntPtr)outPtr, (ulong)decompressedOutput.Length);

                    try
                    {
                        return Bindings.opus_decompress(inSlice, outSlice);
                    }
                    catch (InteropException<MyFFIError> err)
                    {
                        if (err.Error == MyFFIError.OutputBufferTooSmall)
                        {
                            return -1;
                        }

                        throw;
                    }
                }
            }
        }
    }
}