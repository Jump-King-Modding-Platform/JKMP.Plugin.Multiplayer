use interoptopus::util::NamespaceMappings;
use interoptopus::{Error, Interop};
use interoptopus_backend_csharp::{Config, Generator};

fn main() -> Result<(), Error> {
    let library = multiplayer_native::inventory();

    Generator::new(
        Config {
            class: "Bindings".to_string(),
            visibility_types: interoptopus_backend_csharp::CSharpVisibility::ForceInternal,
            rename_symbols: true,
            dll_name: "multiplayer_native".to_string(),
            namespace_mappings: NamespaceMappings::new("Multiplayer.Native"),
            file_header_comment: "// Automatically generated, do not edit".to_string(),
            ..Config::default()
        },
        library,
    )
    .write_file("bindings/Bindings.cs")?;

    Ok(())
}
