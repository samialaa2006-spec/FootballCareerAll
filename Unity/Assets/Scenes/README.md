# Scene setup

Create these scenes in the Unity Editor and add them to Build Settings in this order:

1. `Bootstrap` — initializes configuration, local save loading, and later service adapters.
2. `CareerHub` — player dashboard, next fixture, league table, Play / Quick Sim / Sim Season actions.
3. `CharacterCreator` — licensed rig preview and supported appearance/celebration choices.
4. `Match` — landscape playable 3D match, HUD, pause/skip controls, and event-driven cutscene hooks.
5. `Signing` — transfer/signing presentation; return to CareerHub after confirmation or skip.

Use a temporary empty scene for early code work; do not check in a hand-authored scene file until the required GameObjects and references can be verified in the Editor. Keep menus portrait-friendly, and decide final match orientation with device testing.
