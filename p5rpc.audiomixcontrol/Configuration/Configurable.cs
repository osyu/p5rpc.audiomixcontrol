/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Reloaded.Mod.Interfaces;

namespace p5rpc.audiomixcontrol.Configuration;

public class Configurable<T> : IUpdatableConfigurable where T : Configurable<T>, new()
{
    [Browsable(false)]
    [JsonIgnore]
    public string? ConfigName { get; private set; }

    [Browsable(false)]
    [JsonIgnore]
    public Action? Save { get; private set; }

    [Browsable(false)]
    public event Action<IUpdatableConfigurable>? ConfigurationUpdated;

    private static readonly object _readLock = new object();

    private string? _filePath;
    private FileSystemWatcher? _configWatcher;

    private void Initialize(string filePath, string configName)
    {
        _filePath = filePath;
        ConfigName = configName;

        _configWatcher = new FileSystemWatcher(Path.GetDirectoryName(_filePath)!, Path.GetFileName(_filePath)!);
        _configWatcher.Changed += OnChanged;
        _configWatcher.EnableRaisingEvents = true;

        Save = OnSave;
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (Monitor.TryEnter(_readLock, 0))
        {
            try
            {
                using FileStream? fs = WaitForFile(_filePath!, FileMode.Open, FileAccess.Read);

                T newConfig = fs != null ? FromStream(fs) : new T();

                newConfig.Initialize(_filePath!, ConfigName!);
                newConfig.ConfigurationUpdated = ConfigurationUpdated;

                _configWatcher?.Dispose();
                ConfigurationUpdated = null;

                newConfig.ConfigurationUpdated?.Invoke(newConfig);
            }
            finally
            {
                Monitor.Exit(_readLock);
            }
        }
    }

    private void OnSave()
    {
        using FileStream fs = new FileStream(_filePath!, FileMode.Create, FileAccess.Write);
        JsonSerializer.Serialize(new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true }), (T)this);
    }

    internal static T FromFile(string filePath, string configName)
    {
        T config;

        if (File.Exists(filePath))
        {
            using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            config = FromStream(fs);
        }
        else
        {
            config = new T();
        }

        config.Initialize(filePath, configName);
        return config;
    }

    private static T FromStream(FileStream fs)
    {
        try
        {
            JsonDocument doc = JsonDocument.Parse(fs);
            return JsonSerializer.Deserialize<T>(doc)!;
        }
        catch (JsonException)
        {
            return new T();
        }
    }

    private static FileStream? WaitForFile(string filePath, FileMode mode, FileAccess access)
    {
        while (true)
        {
            FileStream? fs = null;

            try
            {
                fs = new FileStream(filePath, mode, access);
                return fs;
            }
            catch (IOException)
            {
                if (!File.Exists(filePath))
                {
                    return null;
                }

                fs?.Dispose();
                Thread.Sleep(50);
            }
        }
    }
}
