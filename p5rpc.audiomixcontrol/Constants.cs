/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using Reloaded.Hooks.Definitions.X64;

namespace p5rpc.audiomixcontrol;

internal static class Constants
{
    public const string VolumeUpdateSig = "48 83 EC 38 0F BE 05 ?? ?? ?? ?? B9 01 00 00 00";
    public const string AtomCatSetVolumeSig = "40 53 48 83 EC 30 8B D9 0F 29 74 24 ?? 33 C9 0F 28 F1";
    public const string ManaPlayerUpdateSig = "40 57 48 83 EC 40 48 8B F9 48 85 C9 75 09 8D 41 02";
    public const string SoundConfigPreviewSig = "40 53 48 83 EC 30 48 8B D9 85 D2 0F 84 98 00 00 00";
    public const string BgmPlaySig = "40 53 48 83 EC 30 89 CB 48 8D 05 ?? ?? ?? ?? 48 8D 0D";
    public const string BgmPlaySeekSig = "40 53 48 83 EC 30 8B D9 48 8D 05 ?? ?? ?? ?? 48 8D 0D";

    public const int VolumeGlobalsPtrOffset = 7;
    public const int ManaSetVolumePtrOffset = 807;
    public const int ManaDestroyPtrOffset = 105;

    public static readonly int[] CriCatLut = { 2, 1, 3 };

    [Function(CallingConventions.Microsoft)]
    public delegate void VolumeUpdate();

    [Function(CallingConventions.Microsoft)]
    public delegate void AtomCatSetVolume(int id, float volume);

    [Function(CallingConventions.Microsoft)]
    public delegate void ManaSetVolume(IntPtr player, float volume);

    [Function(CallingConventions.Microsoft)]
    public delegate void ManaDestroy(IntPtr player);

    [Function(CallingConventions.Microsoft)]
    public delegate void SoundConfigPreview(IntPtr thisPtr, int category, int volume);

    [Function(CallingConventions.Microsoft)]
    public delegate void BgmPlay(int cueId);

    [Function(CallingConventions.Microsoft)]
    public delegate void BgmPlaySeek(int cueId, long startTime);
}
