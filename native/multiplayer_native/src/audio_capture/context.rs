use crate::MyFFIError;

use cpal::traits::{DeviceTrait, HostTrait, StreamTrait};
use cpal::{Device, Host, Stream, StreamError, SupportedStreamConfig};

use interoptopus::{
    callback, ffi_service, ffi_service_ctor, ffi_type, patterns::slice::FFISlice, Error,
};

#[ffi_type(opaque)]
#[repr(C)]
pub struct AudioContext {
    host: Host,
    active_device: Option<Device>,
    input_stream: Option<Stream>,
    config: Option<SupportedStreamConfig>,
}

#[ffi_type]
#[repr(C)]
pub enum CaptureError {
    DeviceNotAvailable,
    BackendSpecific,
}

impl From<StreamError> for CaptureError {
    fn from(err: StreamError) -> Self {
        match err {
            StreamError::DeviceNotAvailable => CaptureError::DeviceNotAvailable,
            StreamError::BackendSpecific { .. } => CaptureError::BackendSpecific,
        }
    }
}

#[ffi_type]
#[repr(C)]
pub struct DeviceInformation {
    pub name_utf8: *const u8,
    pub name_len: i32,
    pub default_config: DeviceConfig,
}

impl DeviceInformation {
    pub fn new(name_bytes: &[u8], default_config: DeviceConfig) -> Self {
        Self {
            name_utf8: name_bytes.as_ptr(),
            name_len: name_bytes.len() as i32,
            default_config,
        }
    }
}

#[ffi_type]
#[repr(C)]
pub struct DeviceConfig {
    /// The number of channels, usually 1 (mono) or 2 (stereo).
    pub channels: u16,

    /// The sample rate, in Hz. For example, 44100 Hz.
    pub sample_rate: u32,
}

impl DeviceConfig {
    pub fn new(channels: u16, sample_rate: u32) -> Self {
        Self {
            channels,
            sample_rate,
        }
    }
}

impl From<SupportedStreamConfig> for DeviceConfig {
    fn from(config: SupportedStreamConfig) -> Self {
        DeviceConfig::new(config.channels(), config.sample_rate().0)
    }
}

callback!(GetOutputDevicesCallback(
    devices: FFISlice<DeviceInformation>
));
callback!(OnCapturedDataCallback(data: FFISlice<i16>));
callback!(OnCaptureErrorCallback(error: CaptureError));
callback!(GetActiveDeviceCallback(device: Option<&DeviceInformation>));

#[allow(clippy::vec_init_then_push)]
#[ffi_service(error = "MyFFIError", prefix = "audio_context_")]
impl AudioContext {
    #[ffi_service_ctor]
    pub fn new() -> Result<Self, Error> {
        Ok(Self {
            host: cpal::default_host(),
            active_device: None,
            input_stream: None,
            config: None,
        })
    }

    pub fn get_output_devices(&self, callback: GetOutputDevicesCallback) -> Result<(), MyFFIError> {
        let mut result = Vec::<DeviceInformation>::new();

        // Store name strings temporarily so they don't get deallocated before the callback has been called
        let mut names = Vec::<String>::new();

        let devices = self.host.input_devices();

        if devices.is_err() {
            callback.call(FFISlice::empty());
            return Ok(());
        }

        let devices = devices.unwrap();

        for device in devices {
            let name = device.name();
            let config = device.default_input_config();

            if name.is_err() || config.is_err() {
                continue;
            }

            let name = name.unwrap();
            let config = config.unwrap();

            names.push(name);
            let name = names.last().unwrap();
            result.push(DeviceInformation::new(name.as_bytes(), config.into()));
        }

        callback.call(result.as_slice().into());
        Ok(())
    }

    pub fn set_active_device(&mut self, device_name_utf8: FFISlice<u8>) -> Result<(), MyFFIError> {
        let device_name = std::str::from_utf8(&device_name_utf8);

        if device_name.is_err() {
            return Err(MyFFIError::InvalidUtf8);
        }

        let device_name = device_name.unwrap();

        let devices = self.host.input_devices();

        if devices.is_err() {
            return Err(MyFFIError::OtherError);
        }

        let devices = devices.unwrap();
        let mut new_device: Option<Device> = None;

        for device in devices {
            if let Ok(name) = device.name() {
                if name == device_name {
                    new_device = Some(device);
                    break;
                }
            }
        }

        if new_device.is_none() {
            return Err(MyFFIError::OtherError);
        }

        let new_device = new_device.unwrap();

        self.set_active_device_internal(Some(new_device));

        Ok(())
    }

    pub fn set_active_device_to_default(&mut self) -> Result<(), MyFFIError> {
        let device = self.host.default_input_device();
        self.set_active_device_internal(device);

        Ok(())
    }

    pub fn get_active_device(&self, callback: GetActiveDeviceCallback) -> Result<(), MyFFIError> {
        let device_info = match self.active_device.as_ref() {
            Some(device) => {
                let name = device.name().unwrap();
                let config = device.default_input_config().unwrap();
                Some(DeviceInformation::new(name.as_bytes(), config.into()))
            }
            None => None,
        };

        callback.call(device_info.as_ref());

        Ok(())
    }

    pub fn start_capture(
        &mut self,
        on_data: OnCapturedDataCallback,
        on_error: OnCaptureErrorCallback,
    ) -> Result<(), MyFFIError> {
        if self.active_device.is_none() || self.input_stream.is_some() {
            return Err(MyFFIError::InvalidState);
        }

        let device = self.active_device.as_ref().unwrap();
        let config = device.default_input_config().unwrap().config();

        println!("{:?}", config);

        let stream = device.build_input_stream(
            &config,
            move |data: &[i16], _| {
                on_data.call(FFISlice::from_slice(data));
            },
            move |err: StreamError| {
                on_error.call(CaptureError::from(err));
            },
        );

        if stream.is_err() {
            return Err(MyFFIError::OtherError);
        }

        self.input_stream = Some(stream.unwrap());

        if self.input_stream.as_ref().unwrap().play().is_err() {
            return Err(MyFFIError::OtherError);
        }

        Ok(())
    }

    pub fn stop_capture(&mut self) -> Result<(), MyFFIError> {
        if self.active_device.is_none() {
            return Err(MyFFIError::InvalidState);
        }

        if self.input_stream.is_none() {
            return Ok(());
        }

        let _ = self.input_stream.as_ref().unwrap().pause();
        self.input_stream = None;

        Ok(())
    }

    fn set_active_device_internal(&mut self, device: Option<Device>) {
        if self.input_stream.is_some() {
            self.stop_capture().unwrap();
        }

        let device_name = device.as_ref().map(|device| device.name().unwrap());

        self.config = device
            .as_ref()
            .map(|device| device.default_input_config().unwrap());
        self.active_device = device;

        println!("Active device set to {:?}, {:?}", device_name, self.config);
    }
}
