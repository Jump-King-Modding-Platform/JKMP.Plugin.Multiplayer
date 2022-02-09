use audio_capture::{context::AudioContext, device_information::DeviceInformation};
use interoptopus::{ffi_type, patterns::result::FFIError, Error};

pub mod audio_capture;

#[ffi_type(patterns(ffi_error))]
#[repr(C)]
#[derive(Debug)]
pub enum MyFFIError {
    Ok = 0,
    NullPassed = 1,
    Panic = 2,
    OtherError = 3,
    InvalidParam = 4,
    InvalidUtf8,
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

interoptopus::inventory!(inventory, [], [], [DeviceInformation], [AudioContext]);
