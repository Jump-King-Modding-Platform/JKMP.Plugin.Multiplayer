use crate::{audio_capture::device_information::DeviceInformation, MyFFIError};

use cpal::traits::{DeviceTrait, HostTrait};
use cpal::{Device, Host};

use interoptopus::{
    callback, ffi_service, ffi_service_ctor, ffi_type, patterns::slice::FFISlice, Error,
};

#[ffi_type(opaque)]
#[repr(C)]
pub struct AudioContext {
    host: Host,
    active_device: Option<Device>,
}

callback!(GetOutputDevicesCallback(
    devices: FFISlice<DeviceInformation>
));

#[allow(clippy::vec_init_then_push)]
#[ffi_service(error = "MyFFIError", prefix = "audio_context_")]
impl AudioContext {
    #[ffi_service_ctor]
    pub fn new() -> Result<Self, Error> {
        Ok(Self {
            host: cpal::default_host(),
            active_device: None,
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

            if let Ok(name) = name {
                names.push(name);
                let name = names.last().unwrap();
                result.push(DeviceInformation::new(name.as_bytes()));
            }
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

    fn set_active_device_internal(&mut self, device: Option<Device>) {
        if let Some(device) = device {
            println!("Setting active device to {}", device.name().unwrap());
            self.active_device = Some(device);
        }
    }
}
