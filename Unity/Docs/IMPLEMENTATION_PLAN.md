# Implementation plan

## Product decisions already made

- Engine: Unity 2022 LTS.
- Platforms: Android and iOS; optimize the experience for phones and test both platforms.
- Career loop: playable player-career matches **or** quick-sim, progress through a full season, and choose to simulate remaining fixtures to season end.
- Online: Unity Gaming Services is the preferred ecosystem. Multiplayer is in scope, but it follows the deterministic career/match prototype rather than being represented as finished in this scaffold.
- Character: use a properly licensed starter character; no asset was supplied yet.
- Match moments: user-selectable goal dances, a miss reaction, and a club-signing cinematic are desired.
- Real club names/branding: names already exist in the source data. Crests, kits, league marks, stadium likenesses, and other protected branding require rights; use neutral placeholder visuals until licensed.

## Phased delivery

### 0 — Project and integration baseline
- Open this Unity project in the selected 2022.3 LTS editor and let it generate metadata.
- Choose mobile orientation, minimum OS/device targets, and a starter asset after license review.
- Add EditMode tests for league schedule invariants and season progression before porting rules.
- Define versioned request/response DTOs. Treat the current ASP.NET API as a prototype only: career state is in-memory and PHP saves are not a secure cloud-account system.

**Exit checks:** clean Unity import; Android and iOS target switches work; assets and licenses tracked; existing web game still builds.

### 1 — Career hub and simulation loop
- Build start-career and player profile screens, league table, fixtures, result screen, season history, and transfer window.
- Support `Play Match`, `Quick Sim`, and `Sim to Season End` as explicit choices. Show a progress/loading state and a season summary for bulk simulation.
- Keep career simulation independent of 3D rendering so it can run quickly on a phone.
- Save a versioned career snapshot locally first; add account/cloud saves after backend ownership and conflict handling are defined.

**Exit checks:** a complete season can be played, quick-simmed, or bulk-simmed; league standings and player stats agree across paths within defined simulation rules; save/load survives app restart.

### 2 — 3D match vertical slice
- Import one licensed, humanoid-rigged starter character and a small set of compatible animations.
- Build one generic pitch, ball, follow camera, touch movement, pass, shot, and basic opponent behavior.
- Drive match result/stat events from the actual match simulation. Do not let a visual cutscene change authoritative score or career state.
- Profile on representative older iPhones and Android devices; use URP, LODs, baked lighting where appropriate, pooled effects, and limited real-time shadows.

**Exit checks:** playable short match with stable touch controls, match completion returns a result exactly once, and performance/memory budgets are recorded on real devices.

### 3 — Character creator and cinematic moments
- Apply face/skin/hair/facial-hair options supported by the selected asset; expose only compatible parts and materials.
- Persist selected customization and a celebration choice in the career profile.
- Implement animation state/Timeline hooks for `GoalCelebration`, `MissReaction`, and `ClubSigning`. Celebration style is selected by the user and mapped to clips the asset is licensed to use.
- Use Cinemachine/Timeline only after the actual model, clips, and camera shots exist; provide a skip button, avoid replaying a moment after scene reload, and keep gameplay results unchanged.
- Create a signing scene with generic contract presentation and club colors until branding is cleared.

**Exit checks:** all moments trigger only on their matching match/career event; selected dance persists; cutscenes can be skipped; no cutscene can duplicate a goal, signing, or save.

### 4 — Unity Gaming Services multiplayer
- First decide the multiplayer mode. The proposed first slice is a private 1v1 friendly, separate from solo career progression.
- Set up a Unity project organization/cloud project, Authentication, Lobby, Relay, and Netcode for GameObjects. Never commit service secrets or player data credentials.
- Use server-authoritative match outcomes (dedicated server or a trusted host design with validation); client-reported goals and ratings must not directly grant career progression or rewards.
- Add disconnect/reconnect, timeout, matchmaking, moderation/reporting, privacy, and service-cost plans before public release.
- Add cross-platform compatibility testing on iOS and Android.

**Exit checks:** invite/join works across platforms, match state is validated by authority, disconnects resolve safely, and no multiplayer result can alter a solo career without validation.

### 5 — Release hardening
- Accessibility, localization, device QA, privacy disclosures, age ratings, crash reporting, analytics consent, app-store signing, and tested backup/restore.
- Replace all placeholder art/audio with original or licensed content and retain the rights records.

## Architecture direction

```text
Unity client
  Presentation: menus, 3D pitch, UI, camera, cutscene playback
  Gameplay: touch input, player controller, ball, AI, match event generation
  Career: player profile, fixtures, quick simulation, progression, local snapshot
  Adapters: current ASP.NET API (temporary), local persistence, later UGS services

Existing ASP.NET/web project
  Source of current career/league behavior during prototype integration
  Not a trusted multiplayer authority or production persistence layer as currently implemented
```

Keep the `Career` and `Presentation` layers independent. A match produces one immutable result/event payload; the career layer applies it once; the presentation layer reacts to it. Multiplayer messages are validated by an authority, never trusted simply because the client animation showed a goal.

## Suggested directory layout

```text
Unity/
├── Assets/
│   ├── AddressableAssetsData/
│   ├── Art/{Characters,Kits,Pitch,Stadium,UI}/
│   ├── Audio/{Music,SFX,Ambience}/
│   ├── Data/{Clubs,Leagues,Animations}/
│   ├── Materials/
│   ├── Prefabs/{Characters,Match,UI}/
│   ├── Scenes/{Bootstrap,CareerHub,CharacterCreator,Match,Signing}/
│   ├── Scripts/{Career,Cutscenes,Match,Multiplayer,Networking,Player,Presentation,UI}/
│   └── Tests/{EditMode,PlayMode}/
├── Docs/
├── Packages/manifest.json
└── ProjectSettings/
```

Folders may be created in the Editor as needed. Do not add empty or fabricated asset files to simulate finished scenes or models.

## First playable milestone

A mobile vertical slice should let a player create a career, select a generic club, choose a licensed starter-character appearance and dance, play one short 3D match or quick-sim it, see the result and table update, and simulate to the end of the season. Goal/miss/signing animation hooks can be tested using placeholder clips. Multiplayer remains visibly marked as not configured until the service project and authoritative architecture are in place.
