# Football Career Unity mobile prototype

This folder starts a Unity client alongside the existing ASP.NET/web game. It is a scaffold and implementation guide—not yet a complete playable game. Existing C# game rules remain the behavioral reference while the Unity client is built.

## Target

- Unity **2022.3 LTS** (the checked-in editor version is 2022.3.20f1; update it if your installed 2022.3 patch differs).
- Android and iOS, with a mobile-first UI and performance budget. Validate on real devices; iOS is a priority but Android remains a supported target.
- Universal Render Pipeline, Input System, Addressables-ready asset organization.
- Unity Gaming Services for authentication, Lobby, Relay, and Netcode for GameObjects. Multiplayer is a later milestone; configure services and secure server-authoritative game outcomes before public matchmaking.

## Open in Unity

1. Install Unity Hub and Unity **2022.3 LTS** with Android Build Support (SDK/NDK + OpenJDK) and iOS Build Support. Building/signing iOS requires macOS and Xcode.
2. In Unity Hub, choose **Add > Add project from disk** and select this `Unity` directory.
3. Let Package Manager resolve `Packages/manifest.json`. If resolution fails, use the Unity Editor version in `ProjectSettings/ProjectVersion.txt`, then resolve packages from Package Manager.
4. Open `Assets/Scenes/Bootstrap.unity` after it has been created in the Editor. For the initial scaffold, create a scene named `Bootstrap` under `Assets/Scenes` and save it as the first Unity Editor setup step.
5. Set Android/iOS in **File > Build Settings**. Set portrait UI for menus and landscape orientation for the match scene; confirm final orientation preference before production.

> Unity-generated `.meta` files are source assets and must be committed. Do not commit `Library/`, `Temp/`, `Obj/`, or local build output.

## Current scaffold

- `Assets/Scripts/Career/CareerModels.cs`: serializable client-side career data contracts.
- `Assets/Scripts/Player/CustomizationProfile.cs`: customization selections that can be saved and applied to a rigged starter character.
- `Assets/Scripts/Match/MatchModels.cs`: match result and match-moment contracts.
- `Assets/Scripts/Presentation/CutsceneDirector.cs`: Animator hook for goal celebration, missed chance, and signing moments, including a player-selected celebration style.
- `Assets/Scenes/README.md`: scene responsibilities and setup order.
- `Docs/IMPLEMENTATION_PLAN.md`: staged plan, architecture, acceptance checks, and asset/IP notes.

These are foundations only. There are no included 3D models, animation clips, stadium assets, production scenes, complete gameplay, backend credentials, or multiplayer server configuration yet.

## Existing game integration

The existing web game has C# career, league, progression, transfer, and save rules at the repository root. The ASP.NET project explicitly excludes `Unity/**` from its compilation. For the prototype, port or extract the pure game rules into a shared .NET Standard-compatible assembly only after adding automated parity tests; Unity must not reference the ASP.NET host project directly. The first client can use the current API as a temporary integration boundary, but that server currently stores careers in memory and is not production-ready for accounts or multiplayer.

## Assets and branding

Use a starter character that explicitly permits commercial game use, redistribution in a compiled game, and the intended modification/customization. Verify rig, animation, platform, and attribution terms before importing. Keep the starter asset's license and attribution beside the asset. Do not use extracted FIFA assets, unlicensed music, team crests, league marks, or player likenesses. The current simulator's club names can remain as data for development, but use generic badges/kits until rights for real-world branding are secured.

## Music

Ship no commercial soundtrack by default. Use original compositions or tracks licensed for interactive games on both app stores, and retain license documentation. Add independent music, ambience, and effects volume settings; support mute and audio-focus interruptions.
