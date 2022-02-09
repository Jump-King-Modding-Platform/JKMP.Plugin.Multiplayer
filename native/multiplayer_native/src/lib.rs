use interoptopus::ffi_function;

#[no_mangle]
#[ffi_function]
pub extern "C" fn test() -> u32 {
    32
}

interoptopus::inventory!(inventory, [], [test], [], []);
