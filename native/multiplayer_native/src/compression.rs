use interoptopus::ffi_function;
use interoptopus::patterns::slice::{FFISlice, FFISliceMut};

use lz4_flex::block::{compress_into, decompress_into, DecompressError};

/// Compresses the input bytes into the output buffer.
/// The returned value is the number of bytes written to the output buffer,
/// unless the output buffer is too small. In that case, the function returns -1.
/// You can use get_maximum_output_size(input_len) to know the maximum size of the output buffer.
#[ffi_function]
#[no_mangle]
pub extern "C" fn lz4_compress(input: FFISlice<u8>, mut output: FFISliceMut<u8>) -> i32 {
    let result = compress_into(&input, &mut output);

    match result {
        Ok(len) => len as i32,
        Err(_) => -1,
    }
}

/// Decompresses the input bytes into the output buffer.
/// The returned value is the number of bytes written to the output buffer.
/// If the output buffer is too small the function returns -1.
/// If the uncompressed size differs from the expected output length the function returns -2.
/// Any other error will cause the function to return -3.
/// Errors are written to stdout.
#[ffi_function]
#[no_mangle]
pub extern "C" fn lz4_decompress(input: FFISlice<u8>, mut output: FFISliceMut<u8>) -> i32 {
    let result = decompress_into(&input, &mut output);

    match result {
        Ok(len) => len as i32,
        Err(e) => match e {
            DecompressError::UncompressedSizeDiffers { expected, actual } => {
                println!(
                    "Failed to decompress, uncompressed size differs: expected {}, actual {}",
                    expected, actual
                );

                -2
            }
            DecompressError::OutputTooSmall { expected, actual } => {
                println!(
                    "Failed to decompress, output buffer too small: expected {}, actual {}",
                    expected, actual
                );

                -1
            }
            _ => {
                println!("Failed to decompress: {:?}", e);

                -3
            }
        },
    }
}

#[ffi_function]
#[no_mangle]
pub extern "C" fn lz4_get_maximum_output_size(input_len: u64) -> u64 {
    lz4_flex::block::get_maximum_output_size(input_len as usize) as u64
}
