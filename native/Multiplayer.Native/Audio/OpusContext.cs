using System;

namespace JKMP.Plugin.Multiplayer.Native.Audio
{
    public class OpusContext : IDisposable
    {
        private readonly Native.OpusContext context;

        public OpusContext(uint sampleRate)
        {
            context = Native.OpusContext.New(sampleRate);
        }

        public void Dispose()
        {
            context.Dispose();
        }

        public unsafe int Compress(ReadOnlySpan<short> audioData, Span<byte> compressedOutputData)
        {
            fixed (short* inPtr = &audioData.GetPinnableReference())
            {
                fixed (byte* outPtr = &compressedOutputData.GetPinnableReference())
                {
                    Slicei16 inSlice = new((IntPtr)inPtr, (ulong)audioData.Length);
                    SliceMutu8 outSlice = new((IntPtr)outPtr, (ulong)compressedOutputData.Length);

                    try
                    {
                        return context.Compress(inSlice, outSlice);
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

        public unsafe int Decompress(ReadOnlySpan<byte> data, Span<short> decompressedOutputData)
        {
            fixed (byte* inPtr = &data.GetPinnableReference())
            {
                fixed (short* outPtr = &decompressedOutputData.GetPinnableReference())
                {
                    Sliceu8 inSlice = new((IntPtr)inPtr, (ulong)data.Length);
                    SliceMuti16 outSlice = new((IntPtr)outPtr, (ulong)decompressedOutputData.Length);

                    try
                    {
                        return context.Decompress(inSlice, outSlice);
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