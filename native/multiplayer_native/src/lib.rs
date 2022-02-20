extern crate core;

use audio_capture::capture_context::AudioContext;
use audio_capture::opus_context::OpusContext;
use compression::{lz4_compress, lz4_decompress, lz4_get_maximum_output_size};

use interoptopus::patterns::slice::FFISlice;
use interoptopus::{
    ffi_function, ffi_service, ffi_service_ctor, ffi_type, function, pattern,
    patterns::result::FFIError, Error, Inventory, InventoryBuilder,
};

pub mod audio_capture;
pub mod compression;

#[ffi_type(patterns(ffi_error))]
#[repr(C)]
#[derive(Debug)]
pub enum MyFFIError {
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
}

impl FFIError for MyFFIError {
    const SUCCESS: Self = Self::Ok;
    const NULL: Self = Self::NullPassed;
    const PANIC: Self = Self::Panic;
}

impl From<Error> for MyFFIError {
    fn from(err: Error) -> Self {
        match err {
            Error::Null => Self::NullPassed,
            Error::Unsupported => Self::OtherError,
            Error::Ascii => Self::OtherError,
            Error::Format(_) => Self::OtherError,
            Error::IO(_) => Self::OtherError,
            Error::UTF8(_) => Self::InvalidUtf8,
            Error::FromUtf8(_) => Self::InvalidUtf8,
            Error::CommandNotFound => Self::OtherError,
            Error::TestFailed => Self::OtherError,
            Error::FileNotFound => Self::OtherError,
        }
    }
}

pub fn ffi_inventory() -> Inventory {
    InventoryBuilder::new()
        .register(pattern!(AudioContext))
        .register(pattern!(OpusContext))
        .register(function!(lz4_compress))
        .register(function!(lz4_decompress))
        .register(function!(lz4_get_maximum_output_size))
        .inventory()
}
