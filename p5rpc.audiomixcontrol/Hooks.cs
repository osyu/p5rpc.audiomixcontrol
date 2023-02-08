/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.Sources;
using static p5rpc.audiomixcontrol.Constants;

namespace p5rpc.audiomixcontrol;

internal class Hooks
{
    public IntPtr VolumeGlobalsAddress;
    public VolumeUpdate VolumeUpdateWrapper = null!;
    public IHook<AtomCatSetVolume> AtomCatSetVolumeHook = null!;
    public IHook<ManaSetVolume> ManaSetVolumeHook = null!;
    public IHook<ManaDestroy> ManaDestroyHook = null!;
    public IHook<SoundConfigPreview> SoundConfigPreviewHook = null!;
    public IHook<BgmPlay> BgmPlayHook = null!;
    public IHook<BgmPlaySeek> BgmPlaySeekHook = null!;

    public Dictionary<int, float> BgmVolumeMultipliers = new Dictionary<int, float>();

    private Config _config;
    private Memory _memory;

    private IntPtr _currentManaPlayer = IntPtr.Zero;

    private byte[] _userVolumes = new byte[5];
    private bool _initialized = false;

    private bool _volumeOverridden = false;
    private float _currentMultiplier;

    public Hooks(Config config, Memory memory)
    {
        _config = config;
        _memory = memory;
    }

    public void ConfigurationUpdated(Config config)
    {
        _config = config;

        VolumeUpdateWrapper();

        if (_currentManaPlayer != IntPtr.Zero)
        {
            ManaSetVolumeImpl(_currentManaPlayer, 1f);
        }
    }

    public void AtomCatSetVolumeImpl(int id, float volume)
    {
        if (!_initialized)
        {
            _memory.SafeReadRaw((UIntPtr)VolumeGlobalsAddress, out _userVolumes, _userVolumes.Length);
            _initialized = true;
        }

        if (volume != 0f)
        {
            volume = _userVolumes[CriCatLut[id]] * _userVolumes[0] * GetCatVolume(id) / 10000;

            if (id == 1 && _volumeOverridden)
            {
                volume *= _currentMultiplier;
            }

            volume = Math.Max(0f, volume);

            if (!_config.NoClamp)
            {
                volume = Math.Min(volume, 1f);
            }
        }

        AtomCatSetVolumeHook.OriginalFunction(id, volume);
    }

    public void ManaSetVolumeImpl(IntPtr player, float volume)
    {
        _currentManaPlayer = player;

        volume = _userVolumes[4] * _userVolumes[0] * _config.MovieVolume / 10000;

        ManaSetVolumeHook.OriginalFunction(player, volume);
    }

    public void ManaDestroyImpl(IntPtr player)
    {
        _currentManaPlayer = IntPtr.Zero;
        ManaDestroyHook.OriginalFunction(player);
    }

    public void SoundConfigPreviewImpl(IntPtr thisPtr, int category, int volume)
    {
        _userVolumes[category] = (byte)volume;
        SoundConfigPreviewHook.OriginalFunction(thisPtr, category, volume);
    }

    public void BgmPlayImpl(int cueId)
    {
        BgmPlaySeekImpl(cueId, -1);
    }

    public void BgmPlaySeekImpl(int cueId, long startTime)
    {
        if (BgmVolumeMultipliers.ContainsKey(cueId))
        {
            _volumeOverridden = true;
            _currentMultiplier = Math.Max(0f, BgmVolumeMultipliers[cueId]);
            VolumeUpdateWrapper();
        }
        else if (_volumeOverridden)
        {
            _volumeOverridden = false;
            VolumeUpdateWrapper();
        }

        BgmPlaySeekHook.OriginalFunction(cueId, startTime);
    }

    private float GetCatVolume(int id)
    {
        switch (id)
        {
            case 0:
                return _config.SeVolume;
            case 1:
                return _config.BgmVolume;
            case 2:
                return _config.VoiceVolume;
            default:
                return 0;
        }
    }
}
