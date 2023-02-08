/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System.Diagnostics;
using System.Text.Json;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.Sources;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using p5rpc.audiomixcontrol.Configuration;
using static p5rpc.audiomixcontrol.Constants;

namespace p5rpc.audiomixcontrol;

public class Mod : IMod
{
    private IModLoader _modLoader = null!;
    private ILogger _logger = null!;
    private IReloadedHooks _reloadedHooks = null!;
    private IStartupScanner _scanner = null!;

    private Memory _memory = new Memory();

    private Hooks _hooks = null!;

    public void StartEx(IModLoaderV1 loaderApi, IModConfigV1 modConfig)
    {
        _modLoader = (IModLoader)loaderApi;
        _logger = (ILogger)_modLoader.GetLogger();
        _modLoader.GetController<IReloadedHooks>().TryGetTarget(out _reloadedHooks!);
        _modLoader.GetController<IStartupScanner>().TryGetTarget(out _scanner!);

        _modLoader.ModLoading += OnModLoading;
        _modLoader.OnModLoaderInitialized += OnModLoaderInitialized;

        Configurator configurator = new Configurator(_modLoader.GetModConfigDirectory(modConfig.ModId));
        Config config = (Config)configurator.GetConfigurations()[0];
        config.ConfigurationUpdated += OnConfigurationUpdated;

        _hooks = new Hooks(config, _memory);

        OnModLoading(this, modConfig);

        IntPtr baseAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        AddSigScan(baseAddress, VolumeUpdateSig, "VolumeUpdate", (address) =>
        {
            _hooks.VolumeGlobalsAddress = ReadDisplacement(address, VolumeGlobalsPtrOffset);
            _hooks.VolumeUpdateWrapper = _reloadedHooks.CreateWrapper<VolumeUpdate>(address, out _);
        });

        AddSigScan(baseAddress, AtomCatSetVolumeSig, "criAtomExCategory_SetVolumeById", (address) =>
        {
            _hooks.AtomCatSetVolumeHook = _reloadedHooks.CreateHook<AtomCatSetVolume>(_hooks.AtomCatSetVolumeImpl, address).Activate();
        });

        AddSigScan(baseAddress, ManaPlayerUpdateSig, "ManaPlayerUpdate", (address) =>
        {
            _hooks.ManaSetVolumeHook = _reloadedHooks.CreateHook<ManaSetVolume>(_hooks.ManaSetVolumeImpl, ReadDisplacement(address, ManaSetVolumePtrOffset)).Activate();
            _hooks.ManaDestroyHook = _reloadedHooks.CreateHook<ManaDestroy>(_hooks.ManaDestroyImpl, ReadDisplacement(address, ManaDestroyPtrOffset)).Activate();
        });

        AddSigScan(baseAddress, SoundConfigPreviewSig, "SoundConfigPreview", (address) =>
        {
            _hooks.SoundConfigPreviewHook = _reloadedHooks.CreateHook<SoundConfigPreview>(_hooks.SoundConfigPreviewImpl, address).Activate();
        });

        AddSigScan(baseAddress, BgmPlaySig, "BgmPlay", (address) =>
        {
            _hooks.BgmPlayHook = _reloadedHooks.CreateHook<BgmPlay>(_hooks.BgmPlayImpl, address).Activate();
        });

        AddSigScan(baseAddress, BgmPlaySeekSig, "BgmPlaySeek", (address) =>
        {
            _hooks.BgmPlaySeekHook = _reloadedHooks.CreateHook<BgmPlaySeek>(_hooks.BgmPlaySeekImpl, address).Activate();
        });
    }

    public void Suspend() { }
    public void Resume() { }
    public void Unload() { }

    public bool CanUnload() => false;
    public bool CanSuspend() => false;

    public Action Disposing => () => { };

    private void OnModLoading(IModV1 mod, IModConfigV1 modConfig)
    {
        string filePath = Path.Combine(_modLoader.GetDirectoryForModId(modConfig.ModId), "BgmVolume.json");

        if (File.Exists(filePath))
        {
            Dictionary<int, float> multipliers;

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    JsonDocument doc = JsonDocument.Parse(fs);
                    multipliers = JsonSerializer.Deserialize<Dictionary<int, float>>(doc)!;
                }
                catch (JsonException ex)
                {
                    _logger.WriteLine($"[AudioMixControl] Failed to parse BgmVolume for {modConfig.ModId}: {ex.Message}", _logger.ColorRed);
                    return;
                }
            }

            foreach (KeyValuePair<int, float> mult in multipliers)
            {
                _hooks.BgmVolumeMultipliers[mult.Key] = mult.Value;
            }
        }
    }

    private void OnModLoaderInitialized()
    {
        _modLoader.ModLoading -= OnModLoading;
        _modLoader.OnModLoaderInitialized -= OnModLoaderInitialized;
    }

    private void OnConfigurationUpdated(IConfigurable config)
    {
        _hooks.ConfigurationUpdated((Config)config);
    }

    private void AddSigScan(IntPtr baseAddress, string pattern, string name, Action<IntPtr> action)
    {
        _scanner.AddMainModuleScan(pattern, (result) =>
        {
            if (!result.Found)
            {
                throw new Exception(name + " function signature not found.");
            }

            action(baseAddress + result.Offset);
        });
    }

    private IntPtr ReadDisplacement(IntPtr address, int offset)
    {
        _memory.SafeRead((UIntPtr)(address + offset), out int ptr);
        return address + (offset + 4) + ptr;
    }
}
