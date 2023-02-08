/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using Reloaded.Mod.Interfaces;

namespace p5rpc.audiomixcontrol.Configuration;

public class Configurator : IConfiguratorV2
{
    private string? _modDirectory;
    private string? _configDirectory;

    private IUpdatableConfigurable[]? _configurations;

    private IUpdatableConfigurable[] MakeConfigurations()
    {
        _configurations = new IUpdatableConfigurable[]
        {
            Configurable<Config>.FromFile(Path.Combine(_configDirectory!, "Config.json"), "General Config")
        };

        _configurations[0].ConfigurationUpdated += (config) =>
        {
            _configurations[0] = config;
        };

        return _configurations;
    }

    public Configurator() { }
    internal Configurator(string configDirectory) : this() { _configDirectory = configDirectory; }

    public IConfigurable[] GetConfigurations() => _configurations ?? MakeConfigurations();

    public void SetModDirectory(string modDirectory) { _modDirectory = modDirectory; }
    public void SetConfigDirectory(string configDirectory) { _configDirectory = configDirectory; }

    public bool TryRunCustomConfiguration() => false;

    public void Migrate(string oldDirectory, string newDirectory) { }
}
