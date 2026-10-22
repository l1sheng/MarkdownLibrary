using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MarkdownLibrary.Library.Helpers;

namespace MarkdownLibrary.Library.Services;

public static class PreferenceKeys
{
    public const string LibraryFolder = "library.folder";
}

/// <summary>
/// 轻量偏好存储：JSON 文件，读写都小，不需要额外依赖。
/// </summary>
public class JsonPreferenceStorage : IPreferenceStorage
{
    private readonly string _path;
    private readonly object _gate = new();
    private Dictionary<string, string> _values;

    public JsonPreferenceStorage()
    {
        _path = PathHelper.PreferencePath;
    }

    public string Get(string key, string defaultValue = null)
    {
        lock (_gate)
        {
            EnsureLoaded();
            return _values.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }

    public void Set(string key, string value)
    {
        lock (_gate)
        {
            EnsureLoaded();
            _values[key] = value;
            Save();
        }
    }

    private void EnsureLoaded()
    {
        if (_values is not null)
        {
            return;
        }

        if (!File.Exists(_path))
        {
            _values = new Dictionary<string, string>();
            return;
        }

        try
        {
            var json = File.ReadAllText(_path);
            _values = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                      ?? new Dictionary<string, string>();
        }
        catch
        {
            _values = new Dictionary<string, string>();
        }
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, JsonSerializer.Serialize(_values,
            new JsonSerializerOptions { WriteIndented = true }));
    }
}
