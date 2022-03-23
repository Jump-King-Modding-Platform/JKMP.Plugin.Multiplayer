use interoptopus::patterns::slice::FFISlice;
use interoptopus::{callback, ffi_function, ffi_type};
use log::{LevelFilter, Metadata, Record};

// Note that this enum needs to match serilog's enum, otherwise casting will not work correctly
#[repr(C)]
#[ffi_type]
pub enum LogLevel {
    Verbose,
    Debug,
    Info,
    Warning,
    Error,
    Fatal,
}

callback!(OnLogCallback(log_level: LogLevel, message: FFISlice<u8>));

#[ffi_function]
#[no_mangle]
pub extern "C" fn initialize_logging(on_log: OnLogCallback) -> bool {
    log::set_boxed_logger(Box::new(CallbackLogger {
        on_log_callback: on_log,
    }))
    .map(|()| log::set_max_level(LevelFilter::Trace))
    .is_ok()
}

struct CallbackLogger {
    on_log_callback: OnLogCallback,
}

impl log::Log for CallbackLogger {
    fn enabled(&self, _: &Metadata) -> bool {
        true
    }

    fn log(&self, record: &Record) {
        let log_level = match record.level() {
            log::Level::Error => LogLevel::Error,
            log::Level::Warn => LogLevel::Warning,
            log::Level::Info => LogLevel::Info,
            log::Level::Debug => LogLevel::Debug,
            log::Level::Trace => LogLevel::Verbose,
        };
        let mut message = String::new();

        message.push_str(format!("{}", record.args()).as_str());

        self.on_log_callback
            .call(log_level, message.as_bytes().into());
    }

    fn flush(&self) {}
}
