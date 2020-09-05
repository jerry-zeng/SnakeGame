using System.Collections;
using System.Collections.Generic;
using Framework;

/// <summary>
/// CSV table container: storea a whole table values.
/// </summary>
public sealed class CSVTableContainer : IEnumerable
{
    private Dictionary<string, Dictionary<string, CSVValue>> _rootDictSingleKey = null;
    private Dictionary<string, Dictionary<string, Dictionary<string, CSVValue>>> _rootDictMultiKey = null;

    public CSVTableContainer(int hierarchyLevel)
    {
        if (hierarchyLevel == 1)
        {
            _rootDictSingleKey = new Dictionary<string, Dictionary<string, CSVValue>>();
        }
        else if (hierarchyLevel == 2)
        {
            _rootDictMultiKey = new Dictionary<string, Dictionary<string, Dictionary<string, CSVValue>>>();
        }
        else
        {
            Debuger.LogError("Unsupported keyword count " + hierarchyLevel.ToString());
        }
    }

    public void SetRow(string keyValue, Dictionary<string, CSVValue> dict)
    {
        _rootDictSingleKey[keyValue] = dict;
    }

    public void SetRow(string keyValue1, string keyValue2, Dictionary<string, CSVValue> dict)
    {
        if (!_rootDictMultiKey.ContainsKey(keyValue1))
        {
            _rootDictMultiKey[keyValue1] = new Dictionary<string, Dictionary<string, CSVValue>>();
        }
        _rootDictMultiKey[keyValue1][keyValue2] = dict;
    }

    public Dictionary<string, CSVValue> GetRow(string keyValue)
    {
        if (_rootDictSingleKey == null)
            return null;

        if (_rootDictSingleKey.ContainsKey(keyValue))
            return _rootDictSingleKey[keyValue];

        return null;
    }

    public Dictionary<string, CSVValue> GetRow(string keyValue1, string keyValue2)
    {
        if (_rootDictMultiKey == null)
            return null;

        if (_rootDictMultiKey.ContainsKey(keyValue1))
        {
            if (_rootDictMultiKey[keyValue1] != null && _rootDictMultiKey[keyValue1].ContainsKey(keyValue2))
                return _rootDictMultiKey[keyValue1][keyValue2];
        }

        return null;
    }

    public CSVValue GetValue(string keyValue, string columnName)
    {
        Dictionary<string, CSVValue> row = GetRow(keyValue);
        if (row != null && row.ContainsKey(columnName))
        {
            return row[columnName];
        }
        return null;
    }

    public CSVValue GetValue(string keyValue1, string keyValue2, string columnName)
    {
        Dictionary<string, CSVValue> row = GetRow(keyValue1, keyValue2);
        if (row != null && row.ContainsKey(columnName))
        {
            return row[columnName];
        }
        return null;
    }

    public bool IsKeyExist(string keyValue)
    {
        return _rootDictSingleKey.ContainsKey(keyValue);
    }
    public bool IsKeyExist(string keyValue1, string keyValue2)
    {
        return _rootDictMultiKey.ContainsKey(keyValue1) && _rootDictMultiKey[keyValue1].ContainsKey(keyValue2);
    }

    public IEnumerator GetEnumerator()
    {
        if (_rootDictSingleKey != null)
        {
            foreach (var item in _rootDictSingleKey)
            {
                yield return item.Value;
            }
        }
        else if (_rootDictMultiKey != null)
        {
            foreach (var firstItem in _rootDictMultiKey)
            {
                foreach (var secondItem in firstItem.Value)
                {
                    yield return secondItem.Value;
                }
            }
        }
        else
        {
            yield break;
        }
    }


    public List<string> GetSingleKeyList()
    {
        if (_rootDictSingleKey == null)
            return null;

        List<string> keys = new List<string>();
        foreach (var item in _rootDictSingleKey)
        {
            if (!string.IsNullOrEmpty(item.Key))
                keys.Add(item.Key);
        }
        return keys;
    }

    public List<KeyValuePair<string, string>> GetMultiKeyList()
    {
        if (_rootDictMultiKey == null)
            return null;

        List<KeyValuePair<string, string>> keys = new List<KeyValuePair<string, string>>();

        foreach (var firstItem in _rootDictMultiKey)
        {
            if (!string.IsNullOrEmpty(firstItem.Key))
            {
                foreach (var secondItem in firstItem.Value)
                {
                    if (!string.IsNullOrEmpty(secondItem.Key))
                        keys.Add(new KeyValuePair<string, string>(firstItem.Key, secondItem.Key));
                }
            }
        }

        return keys;
    }

}

