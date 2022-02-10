use interoptopus::util::NamespaceMappings;
use interoptopus::{Error, Interop};
use interoptopus_backend_csharp::{CSharpVisibility, Config, Generator, Unsafe};

fn main() -> Result<(), Error> {
    let library = multiplayer_native::inventory();

    Generator::new(
        Config {
            class: "Bindings".to_string(),
            dll_name: "multiplayer_native".to_string(),
            namespace_mappings: NamespaceMappings::new("JKMP.Plugin.Multiplayer.Native"),
            file_header_comment: "// Automatically generated, do not edit".to_string(),
            use_unsafe: Unsafe::UnsafePlatformMemCpy,
            visibility_types: CSharpVisibility::ForcePublic,
            ..Config::default()
        },
        library,
    )
    .write_file("bindings/Bindings.cs")?;

    Ok(())
}
