using System;
using System.IO;
using HarmonyLib;

namespace JKMP.Plugin.Multiplayer.Memory
{
    internal static class MemoryExtensions
    {
        /// <summary>
        /// <para>
        /// Returns a readonly span pointing to the written data in the memory stream's buffer.
        /// Does not allocate a new array, unlike ToArray.
        /// If the array is not publicly accessible, we use reflection to access the private _buffer array.
        /// It's preferable to make the buffer publicly visible since reflection is a lot slower.
        /// </para>
        /// <para>
        /// If the stream is written to after this method is called, the returned span will not be updated to the new length. It will however still point to the same data,
        /// so if the position is reset and the stream is written to again, the data in the span will be updated.
        /// </para>
        /// </summary>
        /// <param name="stream"></param>
        /// <exception cref="IndexOutOfRangeException">Thrown if stream.Position > <see cref="int.MaxValue"/></exception>
        /// <returns></returns>
        public static ReadOnlySpan<byte> GetReadOnlySpan(this MemoryStream stream)
        {
            if (stream.Position > int.MaxValue)
                throw new IndexOutOfRangeException("The stream position is further than the maximum indexable value.");

            byte[] buffer;
            
            if (stream.CanWrite)
            {
                buffer = stream.GetBuffer();
            }
            else
            {
                buffer = (byte[])AccessTools.Field(typeof(MemoryStream), "_buffer").GetValue(stream);
            }
            
            var span = new ReadOnlySpan<byte>(buffer, 0, (int)stream.Position);
            return span;
        }
    }
}