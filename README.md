# p5rpc.audiomixcontrol

Allows for user customization of the game's hardcoded audio channel volumes, which are applied on top of the in-game sliders.

Additionally, other mods can include a `BgmVolume.json` file in their directory containing volume multipliers for specific cue IDs, and this mod will scan for and apply those. For example:

```json
{
  "907": 2.0
}
```

... will make Take Over (the ambush battle theme) play twice as loud. Be sure to add this mod as a dependency when using this feature.
