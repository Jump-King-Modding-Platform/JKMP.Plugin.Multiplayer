// Automatically generated, do not edit

#pragma warning disable 0105
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using JKMP.Plugin.Multiplayer.Native;
#pragma warning restore 0105

namespace JKMP.Plugin.Multiplayer.Native
{
    public static partial class Bindings
    {
        public const string NativeLib = "multiplayer_native";

        static Bindings()
        {
        }


        /// Destroys the given instance.
        ///
        /// # Safety
        ///
        /// The passed parameter MUST have been created with the corresponding init function;
        /// passing any other value results in undefined behavior.
        [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "audio_context_destroy")]
        public static extern MyFFIError audio_context_destroy(ref IntPtr context);

        public static void audio_context_destroy_checked(ref IntPtr context) {
            var rval = audio_context_destroy(ref context);;
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
        }

        [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "audio_context_new")]
        public static extern MyFFIError audio_context_new(ref IntPtr context);

        public static void audio_context_new_checked(ref IntPtr context) {
            var rval = audio_context_new(ref context);;
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
        }

        [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "audio_context_get_input_devices")]
        public static extern MyFFIError audio_context_get_input_devices(IntPtr context, GetOutputDevicesCallback callback);

        public static void audio_context_get_input_devices_checked(IntPtr context, GetOutputDevicesCallback callback) {
            var rval = audio_context_get_input_devices(context, callback);;
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
        }

        [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "audio_context_set_active_device")]
        public static extern MyFFIError audio_context_set_active_device(IntPtr context, Sliceu8 device_name_utf8);

        public static void audio_context_set_active_device(IntPtr context, byte[] device_name_utf8) {
            unsafe
            {
                fixed (void* ptr_device_name_utf8 = device_name_utf8)
                {
                    var device_name_utf8_slice = new Sliceu8(new IntPtr(ptr_device_name_utf8), (ulong) device_name_utf8.Length);
                    var rval = audio_context_set_active_device(context, device_name_utf8_slice);;
                    if (rval != MyFFIError.Ok)
                    {
                        throw new InteropException<MyFFIError>(rval);
                    }
                }
            }
        }

        [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "audio_context_set_active_device_to_default")]
        public static extern MyFFIError audio_context_set_active_device_to_default(IntPtr context);

        public static void audio_context_set_active_device_to_default_checked(IntPtr context) {
            var rval = audio_context_set_active_device_to_default(context);;
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
        }

        [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "audio_context_get_active_device")]
        public static extern MyFFIError audio_context_get_active_device(IntPtr context, GetDeviceCallback callback);

        public static void audio_context_get_active_device_checked(IntPtr context, GetDeviceCallback callback) {
            var rval = audio_context_get_active_device(context, callback);;
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
        }

        [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "audio_context_get_default_device")]
        public static extern MyFFIError audio_context_get_default_device(IntPtr context, GetDeviceCallback callback);

        public static void audio_context_get_default_device_checked(IntPtr context, GetDeviceCallback callback) {
            var rval = audio_context_get_default_device(context, callback);;
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
        }

        /// Starts capturing audio data. The data in the callback is always 48kHz mono 16bit PCM regardless
        /// of the number of channels or sample rate on the input device.
        [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "audio_context_start_capture")]
        public static extern MyFFIError audio_context_start_capture(IntPtr context, OnCapturedDataCallback on_data, OnCaptureErrorCallback on_error);

        public static void audio_context_start_capture_checked(IntPtr context, OnCapturedDataCallback on_data, OnCaptureErrorCallback on_error) {
            var rval = audio_context_start_capture(context, on_data, on_error);;
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
        }

        [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "audio_context_stop_capture")]
        public static extern MyFFIError audio_context_stop_capture(IntPtr context);

        public static void audio_context_stop_capture_checked(IntPtr context) {
            var rval = audio_context_stop_capture(context);;
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
        }

        [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "audio_context_is_capturing")]
        public static extern bool audio_context_is_capturing(IntPtr context);


        /// Sets the volume of the input device.
        /// Input value is clamped between 0 and 2.5.
        /// A value of 0 would be silence, a value of 1 would be the default volume, and a value of 2.5 would be 250% volume.
        [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "audio_context_set_volume")]
        public static extern MyFFIError audio_context_set_volume(IntPtr context, double volume);

        public static void audio_context_set_volume_checked(IntPtr context, double volume) {
            var rval = audio_context_set_volume(context, volume);;
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
        }

        [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "audio_context_get_volume")]
        public static extern double audio_context_get_volume(IntPtr context);


        /// Destroys the given instance.
        ///
        /// # Safety
        ///
        /// The passed parameter MUST have been created with the corresponding init function;
        /// passing any other value results in undefined behavior.
        [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_context_destroy")]
        public static extern MyFFIError opus_context_destroy(ref IntPtr context);

        public static void opus_context_destroy_checked(ref IntPtr context) {
            var rval = opus_context_destroy(ref context);;
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
        }

        /// Creates a new OpusContext.
        /// If the sample rate is unsupported, Unsupported is returned.
        [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_context_new")]
        public static extern MyFFIError opus_context_new(ref IntPtr context, uint sample_rate);

        public static void opus_context_new_checked(ref IntPtr context, uint sample_rate) {
            var rval = opus_context_new(ref context, sample_rate);;
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
        }

        /// Compresses the audio data. The audio data is assumed to be signed PCM 16-bit mono.
        /// The data in the callback is compressed using opus codec.
        /// Returns the number of bytes written to the buffer.
        /// If the output buffer is not large enough -1 is returned.
        /// If the sample rate is not supported -2 is returned.
        [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_context_compress")]
        public static extern int opus_context_compress(IntPtr context, Slicei16 audio_data, SliceMutu8 out_data);

        public static int opus_context_compress(IntPtr context, short[] audio_data, byte[] out_data) {
            unsafe
            {
                fixed (void* ptr_audio_data = audio_data)
                {
                    var audio_data_slice = new Slicei16(new IntPtr(ptr_audio_data), (ulong) audio_data.Length);
                    fixed (void* ptr_out_data = out_data)
                    {
                        var out_data_slice = new SliceMutu8(new IntPtr(ptr_out_data), (ulong) out_data.Length);
                        return opus_context_compress(context, audio_data_slice, out_data_slice);;
                    }
                }
            }
        }

        /// Decompresses the compressed data. The data is assumed to be compressed using opus codec.
        /// The data is decompressed into the out_data_audio slice.
        /// Returns the number of bytes written to the buffer.
        /// If the output buffer is not large enough -1 is returned.
        /// If the input or output length exceeds the maximum value of int32, -2 is returned.
        [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_context_decompress")]
        public static extern int opus_context_decompress(IntPtr context, Sliceu8 data, SliceMuti16 out_audio_data);

        public static int opus_context_decompress(IntPtr context, byte[] data, short[] out_audio_data) {
            unsafe
            {
                fixed (void* ptr_data = data)
                {
                    var data_slice = new Sliceu8(new IntPtr(ptr_data), (ulong) data.Length);
                    fixed (void* ptr_out_audio_data = out_audio_data)
                    {
                        var out_audio_data_slice = new SliceMuti16(new IntPtr(ptr_out_audio_data), (ulong) out_audio_data.Length);
                        return opus_context_decompress(context, data_slice, out_audio_data_slice);;
                    }
                }
            }
        }

    }

    public enum CaptureError
    {
        DeviceNotAvailable = 0,
        BackendSpecific = 1,
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct DeviceConfig
    {
        /// The number of channels, usually 1 (mono) or 2 (stereo).
        public ushort channels;
        /// The sample rate, in Hz. For example, 44100 Hz.
        public uint sample_rate;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct DeviceInformation
    {
        public Sliceu8 name_utf8;
        public DeviceConfig default_config;
    }

    public enum MyFFIError
    {
        /// Returned when the function executed successfully.
        Ok = 0,
        /// Returned when the passed context is null.
        NullPassed = 1,
        /// Returned when the function panicked.
        Panic = 2,
        /// Returned for any other error.
        OtherError = 3,
        /// Returned when a function receives an invalid parameter (such as passing a u32 when the function expects i32).
        InvalidParam = 4,
        /// Returned when a utf8 byte array is not a valid utf8 format.
        InvalidUtf8 = 5,
        /// Returned when the state of the given context or parameter is invalid.
        /// For example, if you try to start capturing audio from an AudioContext without selecting an input device first.
        InvalidState = 6,
        /// Returned when an input buffer is too small.
        InputBufferTooSmall = 7,
        /// Returned when an output buffer is too small.
        OutputBufferTooSmall = 8,
        /// Returned when a selected or specified device was disconnected.
        DeviceLost = 9,
        /// Returned when a specified device could not be found.
        DeviceNotFound = 10,
        /// Returned when an input parameter value is not supported
        Unsupported = 11,
    }

    ///A pointer to an array of data someone else owns which may not be modified.
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct SliceDeviceInformation
    {
        ///Pointer to start of immutable data.
        IntPtr data;
        ///Number of elements.
        ulong len;
    }

    public partial struct SliceDeviceInformation : IEnumerable<DeviceInformation>
    {
        public SliceDeviceInformation(GCHandle handle, ulong count)
        {
            this.data = handle.AddrOfPinnedObject();
            this.len = count;
        }
        public SliceDeviceInformation(IntPtr handle, ulong count)
        {
            this.data = handle;
            this.len = count;
        }
        public ReadOnlySpan<DeviceInformation> ReadOnlySpan
        {
            get
            {
                unsafe
                {
                    return new ReadOnlySpan<DeviceInformation>(this.data.ToPointer(), (int) this.len);
                }
            }
        }
        public DeviceInformation this[int i]
        {
            get
            {
                if (i >= Count) throw new IndexOutOfRangeException();
                var size = Marshal.SizeOf(typeof(DeviceInformation));
                var ptr = new IntPtr(data.ToInt64() + i * size);
                return Marshal.PtrToStructure<DeviceInformation>(ptr);
            }
        }
        public DeviceInformation[] Copied
        {
            get
            {
                var rval = new DeviceInformation[len];
                for (var i = 0; i < (int) len; i++) {
                    rval[i] = this[i];
                }
                return rval;
            }
        }
        public int Count => (int) len;
        public IEnumerator<DeviceInformation> GetEnumerator()
        {
            for (var i = 0; i < (int)len; ++i)
            {
                yield return this[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }


    ///A pointer to an array of data someone else owns which may not be modified.
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct Slicei16
    {
        ///Pointer to start of immutable data.
        IntPtr data;
        ///Number of elements.
        ulong len;
    }

    public partial struct Slicei16 : IEnumerable<short>
    {
        public Slicei16(GCHandle handle, ulong count)
        {
            this.data = handle.AddrOfPinnedObject();
            this.len = count;
        }
        public Slicei16(IntPtr handle, ulong count)
        {
            this.data = handle;
            this.len = count;
        }
        public ReadOnlySpan<short> ReadOnlySpan
        {
            get
            {
                unsafe
                {
                    return new ReadOnlySpan<short>(this.data.ToPointer(), (int) this.len);
                }
            }
        }
        public short this[int i]
        {
            get
            {
                if (i >= Count) throw new IndexOutOfRangeException();
                unsafe
                {
                    var d = (short*) data.ToPointer();
                    return d[i];
                }
            }
        }
        public short[] Copied
        {
            get
            {
                var rval = new short[len];
                unsafe
                {
                    fixed (void* dst = rval)
                    {
                        #if __INTEROPTOPUS_NEVER
                        #elif NETCOREAPP
                        Unsafe.CopyBlock(dst, data.ToPointer(), (uint)len);
                        #else
                        for (var i = 0; i < (int) len; i++) {
                            rval[i] = this[i];
                        }
                        #endif
                    }
                }
                return rval;
            }
        }
        public int Count => (int) len;
        public IEnumerator<short> GetEnumerator()
        {
            for (var i = 0; i < (int)len; ++i)
            {
                yield return this[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }


    ///A pointer to an array of data someone else owns which may not be modified.
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct Sliceu8
    {
        ///Pointer to start of immutable data.
        IntPtr data;
        ///Number of elements.
        ulong len;
    }

    public partial struct Sliceu8 : IEnumerable<byte>
    {
        public Sliceu8(GCHandle handle, ulong count)
        {
            this.data = handle.AddrOfPinnedObject();
            this.len = count;
        }
        public Sliceu8(IntPtr handle, ulong count)
        {
            this.data = handle;
            this.len = count;
        }
        public ReadOnlySpan<byte> ReadOnlySpan
        {
            get
            {
                unsafe
                {
                    return new ReadOnlySpan<byte>(this.data.ToPointer(), (int) this.len);
                }
            }
        }
        public byte this[int i]
        {
            get
            {
                if (i >= Count) throw new IndexOutOfRangeException();
                unsafe
                {
                    var d = (byte*) data.ToPointer();
                    return d[i];
                }
            }
        }
        public byte[] Copied
        {
            get
            {
                var rval = new byte[len];
                unsafe
                {
                    fixed (void* dst = rval)
                    {
                        #if __INTEROPTOPUS_NEVER
                        #elif NETCOREAPP
                        Unsafe.CopyBlock(dst, data.ToPointer(), (uint)len);
                        #else
                        for (var i = 0; i < (int) len; i++) {
                            rval[i] = this[i];
                        }
                        #endif
                    }
                }
                return rval;
            }
        }
        public int Count => (int) len;
        public IEnumerator<byte> GetEnumerator()
        {
            for (var i = 0; i < (int)len; ++i)
            {
                yield return this[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }


    ///A pointer to an array of data someone else owns which may be modified.
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct SliceMuti16
    {
        ///Pointer to start of mutable data.
        IntPtr data;
        ///Number of elements.
        ulong len;
    }

    public partial struct SliceMuti16 : IEnumerable<short>
    {
        public SliceMuti16(GCHandle handle, ulong count)
        {
            this.data = handle.AddrOfPinnedObject();
            this.len = count;
        }
        public SliceMuti16(IntPtr handle, ulong count)
        {
            this.data = handle;
            this.len = count;
        }
        public ReadOnlySpan<short> ReadOnlySpan
        {
            get
            {
                unsafe
                {
                    return new ReadOnlySpan<short>(this.data.ToPointer(), (int) this.len);
                }
            }
        }
        public Span<short> Span
        {
            get
            {
                unsafe
                {
                    return new Span<short>(this.data.ToPointer(), (int) this.len);
                }
            }
        }
        public short this[int i]
        {
            get
            {
                if (i >= Count) throw new IndexOutOfRangeException();
                unsafe
                {
                    var d = (short*) data.ToPointer();
                    return d[i];
                }
            }
            set
            {
                if (i >= Count) throw new IndexOutOfRangeException();
                unsafe
                {
                    var d = (short*) data.ToPointer();
                    d[i] = value;
                }
            }
        }
        public short[] Copied
        {
            get
            {
                var rval = new short[len];
                unsafe
                {
                    fixed (void* dst = rval)
                    {
                        #if __FALSE
                        #elif NETCOREAPP
                        Unsafe.CopyBlock(dst, data.ToPointer(), (uint)len);
                        #else
                        for (var i = 0; i < (int) len; i++) {
                            rval[i] = this[i];
                        }
                        #endif
                    }
                }
                return rval;
            }
        }
        public int Count => (int) len;
        public IEnumerator<short> GetEnumerator()
        {
            for (var i = 0; i < (int)len; ++i)
            {
                yield return this[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }


    ///A pointer to an array of data someone else owns which may be modified.
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct SliceMutu8
    {
        ///Pointer to start of mutable data.
        IntPtr data;
        ///Number of elements.
        ulong len;
    }

    public partial struct SliceMutu8 : IEnumerable<byte>
    {
        public SliceMutu8(GCHandle handle, ulong count)
        {
            this.data = handle.AddrOfPinnedObject();
            this.len = count;
        }
        public SliceMutu8(IntPtr handle, ulong count)
        {
            this.data = handle;
            this.len = count;
        }
        public ReadOnlySpan<byte> ReadOnlySpan
        {
            get
            {
                unsafe
                {
                    return new ReadOnlySpan<byte>(this.data.ToPointer(), (int) this.len);
                }
            }
        }
        public Span<byte> Span
        {
            get
            {
                unsafe
                {
                    return new Span<byte>(this.data.ToPointer(), (int) this.len);
                }
            }
        }
        public byte this[int i]
        {
            get
            {
                if (i >= Count) throw new IndexOutOfRangeException();
                unsafe
                {
                    var d = (byte*) data.ToPointer();
                    return d[i];
                }
            }
            set
            {
                if (i >= Count) throw new IndexOutOfRangeException();
                unsafe
                {
                    var d = (byte*) data.ToPointer();
                    d[i] = value;
                }
            }
        }
        public byte[] Copied
        {
            get
            {
                var rval = new byte[len];
                unsafe
                {
                    fixed (void* dst = rval)
                    {
                        #if __FALSE
                        #elif NETCOREAPP
                        Unsafe.CopyBlock(dst, data.ToPointer(), (uint)len);
                        #else
                        for (var i = 0; i < (int) len; i++) {
                            rval[i] = this[i];
                        }
                        #endif
                    }
                }
                return rval;
            }
        }
        public int Count => (int) len;
        public IEnumerator<byte> GetEnumerator()
        {
            for (var i = 0; i < (int)len; ++i)
            {
                yield return this[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }


    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void GetDeviceCallback(ref DeviceInformation x0);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void GetOutputDevicesCallback(SliceDeviceInformation x0);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void OnCaptureErrorCallback(CaptureError x0);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void OnCapturedDataCallback(Slicei16 x0, float x1);


    public partial class AudioContext : IDisposable
    {
        private IntPtr _context;

        private AudioContext() {}

        public static AudioContext New()
        {
            var self = new AudioContext();
            var rval = Bindings.audio_context_new(ref self._context);
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
            return self;
        }

        public void Dispose()
        {
            var rval = Bindings.audio_context_destroy(ref _context);
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
        }

        public void GetInputDevices(GetOutputDevicesCallback callback)
        {
            var rval = Bindings.audio_context_get_input_devices(_context, callback);
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
        }

        public void SetActiveDevice(Sliceu8 device_name_utf8)
        {
            var rval = Bindings.audio_context_set_active_device(_context, device_name_utf8);
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
        }

        public void SetActiveDevice(byte[] device_name_utf8)
        {
            Bindings.audio_context_set_active_device(_context, device_name_utf8);
        }

        public void SetActiveDeviceToDefault()
        {
            var rval = Bindings.audio_context_set_active_device_to_default(_context);
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
        }

        public void GetActiveDevice(GetDeviceCallback callback)
        {
            var rval = Bindings.audio_context_get_active_device(_context, callback);
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
        }

        public void GetDefaultDevice(GetDeviceCallback callback)
        {
            var rval = Bindings.audio_context_get_default_device(_context, callback);
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
        }

        /// Starts capturing audio data. The data in the callback is always 48kHz mono 16bit PCM regardless
        /// of the number of channels or sample rate on the input device.
        public void StartCapture(OnCapturedDataCallback on_data, OnCaptureErrorCallback on_error)
        {
            var rval = Bindings.audio_context_start_capture(_context, on_data, on_error);
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
        }

        public void StopCapture()
        {
            var rval = Bindings.audio_context_stop_capture(_context);
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
        }

        public bool IsCapturing()
        {
            return Bindings.audio_context_is_capturing(_context);
        }

        /// Sets the volume of the input device.
        /// Input value is clamped between 0 and 2.5.
        /// A value of 0 would be silence, a value of 1 would be the default volume, and a value of 2.5 would be 250% volume.
        public void SetVolume(double volume)
        {
            var rval = Bindings.audio_context_set_volume(_context, volume);
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
        }

        public double GetVolume()
        {
            return Bindings.audio_context_get_volume(_context);
        }

        public IntPtr Context => _context;
    }


    public partial class OpusContext : IDisposable
    {
        private IntPtr _context;

        private OpusContext() {}

        /// Creates a new OpusContext.
        /// If the sample rate is unsupported, Unsupported is returned.
        public static OpusContext New(uint sample_rate)
        {
            var self = new OpusContext();
            var rval = Bindings.opus_context_new(ref self._context, sample_rate);
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
            return self;
        }

        public void Dispose()
        {
            var rval = Bindings.opus_context_destroy(ref _context);
            if (rval != MyFFIError.Ok)
            {
                throw new InteropException<MyFFIError>(rval);
            }
        }

        /// Compresses the audio data. The audio data is assumed to be signed PCM 16-bit mono.
        /// The data in the callback is compressed using opus codec.
        /// Returns the number of bytes written to the buffer.
        /// If the output buffer is not large enough -1 is returned.
        /// If the sample rate is not supported -2 is returned.
        public int Compress(Slicei16 audio_data, SliceMutu8 out_data)
        {
            return Bindings.opus_context_compress(_context, audio_data, out_data);
        }

        public int Compress(short[] audio_data, byte[] out_data)
        {
            return Bindings.opus_context_compress(_context, audio_data, out_data);
        }

        /// Decompresses the compressed data. The data is assumed to be compressed using opus codec.
        /// The data is decompressed into the out_data_audio slice.
        /// Returns the number of bytes written to the buffer.
        /// If the output buffer is not large enough -1 is returned.
        /// If the input or output length exceeds the maximum value of int32, -2 is returned.
        public int Decompress(Sliceu8 data, SliceMuti16 out_audio_data)
        {
            return Bindings.opus_context_decompress(_context, data, out_audio_data);
        }

        public int Decompress(byte[] data, short[] out_audio_data)
        {
            return Bindings.opus_context_decompress(_context, data, out_audio_data);
        }

        public IntPtr Context => _context;
    }



    public class InteropException<T> : Exception
    {
        public T Error { get; private set; }

        public InteropException(T error): base($"Something went wrong: {error}")
        {
            Error = error;
        }
    }

}
