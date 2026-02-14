# Tale of Immortal Discord Rich Presence (Vietnamese)

A Discord Rich Presence mod for "Tale of Immortal" (Quỷ Cốc Bát Hoang) localized in Vietnamese.

## Features
- Displays character name accurately (joins Il2CppStringArray).
- Displays cultivation realm in Vietnamese (Luyện Khí Cảnh, Trúc Cơ Cảnh, v.v.).
- Supports both standard and offset GradeID mapping for different save versions.
- Displays in-game year and month in Vietnamese.
- Optimized performance, lag-free.

## Files
- `TOI_RPC.dll`: The main mod assembly (Place in `Mods` folder).
- `discord-rpc.dll`: Native Discord library (Place in game root or `Mods` folder if needed).
- `Main.cs`: Source code for the mod.
- `DiscordRpc.cs`: Native wrapper.
- `TOI_RPC.csproj`: Visual Studio Project file.

## Installation
1. Install [MelonLoader](https://melonwiki.xyz/).
2. Place `TOI_RPC.dll` into the `Mods` directory of your game.
3. Ensure `discord-rpc.dll` is available (usually sits in the game root).

Developed by Dutch25.
