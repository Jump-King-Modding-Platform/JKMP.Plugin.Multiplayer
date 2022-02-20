use interoptopus::util::NamespaceMappings;
use interoptopus::{Error, Interop};
use interoptopus_backend_csharp::{overloads, CSharpVisibility, Config, Generator, Unsafe};

fn main() -> Result<(), Error> {
    let inventory = multiplayer_native::ffi_inventory();

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
        inventory,
    )
    .add_overload_writer(overloads::DotNet::new())
    .write_file("bindings/Bindings.cs")?;

    Ok(())
}
