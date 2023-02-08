/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System.ComponentModel;
using p5rpc.audiomixcontrol.Configuration;

namespace p5rpc.audiomixcontrol;

public class Config : Configurable<Config>
{
    [DisplayName("BGM Mix Volume %")]
    [Description("Volume percentage for the 'bgm' CRI Atom category.")]
    [DefaultValue(24f)]
    public float BgmVolume { get; set; } = 24f;

    [DisplayName("SE Mix Volume %")]
    [Description("Volume percentage for the 'se' CRI Atom category.")]
    [DefaultValue(30f)]
    public float SeVolume { get; set; } = 30f;

    [DisplayName("Voice Mix Volume %")]
    [Description("Volume percentage for the 'voice' CRI Atom category.")]
    [DefaultValue(30f)]
    public float VoiceVolume { get; set; } = 30f;

    [DisplayName("Movie Mix Volume %")]
    [Description("Volume percentage for CRI Sofdec playback.")]
    [DefaultValue(80f)]
    public float MovieVolume { get; set; } = 80f;

    [DisplayName("Allow volumes above 100%")]
    [Description("Don't cap volumes at 100%. Does not affect movie audio.")]
    [DefaultValue(false)]
    public bool NoClamp { get; set; } = false;
}
