use interoptopus::ffi_type;

#[ffi_type]
#[repr(C)]
pub struct DeviceInformation {
    pub name_utf8: *const u8,
    pub name_len: i32,
}

impl DeviceInformation {
    pub fn new(name_bytes: &[u8]) -> Self {
        Self {
            name_utf8: name_bytes.as_ptr(),
            name_len: name_bytes.len() as i32,
        }
    }
}
