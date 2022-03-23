use crate::MyFFIError;
use audiopus::coder::{Decoder, Encoder};
use audiopus::packet::Packet;
use audiopus::{Application, Channels, MutSignals, SampleRate};
use interoptopus::patterns::slice::{FFISlice, FFISliceMut};
use interoptopus::{ffi_service, ffi_service_ctor, ffi_service_method, ffi_type, Error};

#[ffi_type(opaque)]
#[repr(C)]
pub struct OpusContext {
    encoder: Encoder,
    decoder: Decoder,
}

#[ffi_service(error = "MyFFIError", prefix = "opus_context_")]
impl OpusContext {
    /// Creates a new OpusContext.
    /// If the sample rate is unsupported, Unsupported is returned.
    #[ffi_service_ctor]
    pub fn new(sample_rate: u32) -> Result<Self, Error> {
        let sample_rate = convert_sample_rate(sample_rate);

        if sample_rate.is_err() {
            return Err(Error::Unsupported);
        }

        let sample_rate = sample_rate.unwrap();

        let encoder = Encoder::new(sample_rate, Channels::Mono, Application::Voip).unwrap();
        let decoder = Decoder::new(sample_rate, Channels::Mono).unwrap();
        Ok(Self { encoder, decoder })
    }

    /// Compresses the audio data. The audio data is assumed to be signed PCM 16-bit mono.
    /// The data in the callback is compressed using opus codec.
    /// Returns the number of bytes written to the buffer.
    /// If the output buffer is not large enough -1 is returned.
    /// If the sample rate is not supported -2 is returned.
    #[no_mangle]
    #[ffi_service_method(on_panic = "return_default")]
    pub fn compress(&mut self, audio_data: FFISlice<i16>, mut out_data: FFISliceMut<u8>) -> i32 {
        match self.encoder.encode(&audio_data, &mut out_data) {
            Ok(len) => {
                return len as i32;
            }
            Err(err) => {
                log::warn!("Failed to encode: {:?}", err);
            }
        }

        -1
    }

    /// Decompresses the compressed data. The data is assumed to be compressed using opus codec.
    /// The data is decompressed into the out_data_audio slice.
    /// Returns the number of bytes written to the buffer.
    /// If the output buffer is not large enough -1 is returned.
    /// If the input or output length exceeds the maximum value of int32, -2 is returned.
    #[no_mangle]
    #[ffi_service_method(on_panic = "return_default")]
    pub fn decompress(&mut self, data: FFISlice<u8>, mut out_audio_data: FFISliceMut<i16>) -> i32 {
        let packet = Packet::try_from(data.as_slice());

        if packet.is_err() {
            return -2;
        }

        let packet = packet.unwrap();
        let out_signal = MutSignals::try_from(out_audio_data.as_slice_mut());

        if out_signal.is_err() {
            return -2;
        }

        let out_signal = out_signal.unwrap();

        match self.decoder.decode(Some(packet), out_signal, false) {
            Ok(len) => {
                return len as i32;
            }
            Err(err) => {
                log::warn!("Failed to decode: {:?}", err);
            }
        }

        -1
    }
}

fn convert_sample_rate(sample_rate: u32) -> Result<SampleRate, anyhow::Error> {
    match sample_rate {
        8000 => Ok(SampleRate::Hz8000),
        12000 => Ok(SampleRate::Hz12000),
        16000 => Ok(SampleRate::Hz16000),
        24000 => Ok(SampleRate::Hz24000),
        48000 => Ok(SampleRate::Hz48000),
        _ => Err(anyhow::Error::msg("Unsupported sample rate")),
    }
}
