use interoptopus::ffi_function;
use interoptopus::patterns::slice::{FFISlice, FFISliceMut};

#[ffi_function]
#[no_mangle]
/// Compresses the audio data. The audio data is assumed to be signed PCM 16-bit.
/// The data in the callback is compressed using opus codec with the specified parameters.
/// Returns the number of bytes written to the buffer. If the output buffer is not large enough -1 is returned.
pub extern "C" fn opus_compress(audio_data: FFISlice<i16>, out_data: FFISliceMut<u8>) -> i32 {
    todo!()
}

/// Decompresses the compresser data. The data is assumed to be compressed using opus codec.
/// The data is decompressed into the out_data_audio slice.
/// Returns the number of bytes written to the buffer. If the output buffer is not large enough -1 is returned.
#[ffi_function]
#[no_mangle]
pub extern "C" fn opus_decompress(data: FFISlice<u8>, out_audio_data: FFISliceMut<i16>) -> i32 {
    todo!()
}
