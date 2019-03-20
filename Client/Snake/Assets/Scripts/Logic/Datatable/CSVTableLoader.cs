using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Framework;

/// <summary>
/// CSV value, stores one of values: string, stringArray, int, intArray, float or floatArray.
/// The type string uses: S, SA, I, IA, F, FA
/// </summary>
public sealed class CSVValue
{
    public enum ValueType
    {
        String,
        StringArray,
        Int,
        IntArray,
        Float,
        FloatArray
    }

    private string _stringValue;

    private ValueType _valueType;
    public ValueType Type
    { 
        get{ return _valueType; } 
    }


    public CSVValue(string value, string typeStr)
    {
        _stringValue = value;
        _valueType = PauseValueType(typeStr);

        _stringArrayValue = null;
        _intValue = int.MinValue;
        _intArrayValue = null;
        _floatValue = float.MinValue;
        _floatArrayValue = null;
    }


    public string StringValue 
    { 
        get { return _stringValue; } 
    }

    private string[] _stringArrayValue;
    public string[] StringArrayValue
    {
        get
        {
            if( !CheckType(ValueType.StringArray) )
                return null;

            if( _stringArrayValue == null ){
                _stringArrayValue = _stringValue.Split(';');
            }

            return _stringArrayValue;
        }
    }

    private int _intValue;
    public int IntValue
    { 
        get
        {
            if( !CheckType(ValueType.Int) )
                return 0;

            if( _intValue == int.MinValue )
                int.TryParse(_stringValue, out _intValue);

            return _intValue;
        } 
    }

    private int[] _intArrayValue;
    public int[] IntArrayValue
    {
        get
        {
            if( !CheckType(ValueType.IntArray) )
                return null;

            if( _intArrayValue == null )
            {
                string[] splitStrs = _stringValue.Split(';');
                _intArrayValue = new int[splitStrs.Length];

                for (int i = 0; i < splitStrs.Length; i++)
                    int.TryParse(splitStrs[i], out _intArrayValue[i]);
            }

            return _intArrayValue;
        }
    }

    private float _floatValue;
    public float FloatValue
    {
        get
        {
            if( !CheckType(ValueType.Float) )
                return 0f;

            if(_floatValue == float.MinValue)
                float.TryParse(_stringValue, out _floatValue);

            return _floatValue;
        }
    }

    private float[] _floatArrayValue;
    public float[] FloatArrayValue
    {
        get
        {
            if( !CheckType(ValueType.FloatArray) )
                return null;

            if( _floatArrayValue == null )
            {
                string[] splitStrs = _stringValue.Split(';');
                _floatArrayValue = new float[splitStrs.Length];

                for (int i = 0; i < splitStrs.Length; i++)
                    float.TryParse(splitStrs[i], out _floatArrayValue[i]);
            }

            return _floatArrayValue;
        }
    }

    public static ValueType PauseValueType(string typeStr)
    {
        string str = typeStr;

        if( str[0] == 'K' || str[0] == 'k' )
            str = str.Substring(1, str.Length-1);

        switch(str)
        {
        case "S": return ValueType.String;
        case "SA": return ValueType.StringArray;
        case "I": return ValueType.Int;
        case "IA": return ValueType.IntArray;
        case "F": return ValueType.Float;
        case "FA": return ValueType.FloatArray;
        default:
            Debuger.LogError("CSVValue", "Unknown type string {0}({1})", str, typeStr);
            return ValueType.String;
        }
    }

    bool CheckType(ValueType type)
    {
        if( _valueType != type ){
            Debuger.LogError("CSVValue", "The raw value type is {0}, but you are trying to convert it to {1}. '{2}'", _valueType.ToString(), type.ToString(), _stringValue);
            return false;
        }
        return true;
    }
}

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
            Debuger.LogError( "Unsupported keyword count " + hierarchyLevel.ToString() );
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
        if( _rootDictSingleKey == null )
            return null;

        if(_rootDictSingleKey.ContainsKey(keyValue))
            return _rootDictSingleKey[keyValue];

        return null;
    }

    public Dictionary<string, CSVValue> GetRow(string keyValue1, string keyValue2)
    {
        if(_rootDictMultiKey == null)
            return null;

        if( _rootDictMultiKey.ContainsKey(keyValue1) )
        {
            if( _rootDictMultiKey[keyValue1] != null && _rootDictMultiKey[keyValue1].ContainsKey(keyValue2) )
                return _rootDictMultiKey[keyValue1][keyValue2];
        }

        return null;
    }

    public CSVValue GetValue(string keyValue, string columnName)
    {
        Dictionary<string, CSVValue> row = GetRow(keyValue);
        if( row != null && row.ContainsKey(columnName) )
        {
            return row[columnName];
        }
        return null;
    }

    public CSVValue GetValue(string keyValue1, string keyValue2, string columnName)
    {
        Dictionary<string, CSVValue> row = GetRow(keyValue1, keyValue2);
        if( row != null && row.ContainsKey(columnName) )
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
        if( _rootDictSingleKey != null )
        {
            foreach( var item in _rootDictSingleKey )
            {
                yield return item.Value;
            }
        }
        else if( _rootDictMultiKey != null )
        {
            foreach( var firstItem in _rootDictMultiKey )
            {
                foreach( var secondItem in firstItem.Value )
                {
                    yield return secondItem.Value;
                }
            }
        }
        else{
            yield break;
        }
    }


    public List<string> GetSingleKeyList()
    {
        if( _rootDictSingleKey == null )
            return null;

        List<string> keys = new List<string>();
        foreach(var item in _rootDictSingleKey)
        {
            if(!string.IsNullOrEmpty(item.Key))
                keys.Add( item.Key );
        }
        return keys;
    }

    public List<KeyValuePair<string, string>> GetMultiKeyList()
    {
        if( _rootDictMultiKey == null )
            return null;

        List<KeyValuePair<string, string>> keys = new List<KeyValuePair<string, string>>();

        foreach(var firstItem in _rootDictMultiKey)
        {
            if( !string.IsNullOrEmpty(firstItem.Key) )
            {
                foreach( var secondItem in firstItem.Value )
                {
                    if( !string.IsNullOrEmpty(secondItem.Key) )
                        keys.Add(new KeyValuePair<string, string>(firstItem.Key, secondItem.Key));
                }
            }
        }

        return keys;
    }

}

public sealed class CSVTableLoader
{
    public static string FileExtension = ".txt";

    private static Dictionary<string, CSVTableContainer> _CSVTableCache = new Dictionary<string, CSVTableContainer>();

    public static bool loadAsync = false;


    public static void RequestDataTable(string fileName)
    {
        if( _CSVTableCache.ContainsKey(fileName) )
            return;

        //string filePath = "CSV/" + fileName + ".csv";

        //AssetManager.Instance.RequestAssets( filePath, OnRequestOk );
    }
    public static void OnRequestOk(string filePath, string strs)
    {
        string dd = filePath.Substring( filePath.IndexOf("/")+1 );
        string fileName = dd.Substring(0, dd.LastIndexOf(".") );

        OnLoadCSVFile(strs, fileName);
    }


    public static void LoadDataTable(string fileName)
    {
        if( _CSVTableCache.ContainsKey(fileName) )
            return;

        string strs = "";

#if UNITY_EDITOR
        string filePath = Application.streamingAssetsPath + "/" + ("DataTable/" + fileName + FileExtension);
        strs = File.ReadAllText(filePath);
#else
        string filePath = AssetUrlUtils.GetStreamingAssetURL( "DataTable/" + fileName + FileExtension );

        WWW www = new WWW(filePath);
        while( !www.isDone ){}
        strs = www.text;
        www.Dispose();
#endif

        OnLoadCSVFile(strs, fileName);
    }


    static void OnLoadCSVFile(string strs, string fileName)
    {
        string[] contents = strs.Replace("\r\n", "\r").Split('\r');

        //line 1, for word value type defined
        string[] wordTypeStr = contents[0].Split(',');
        //line 2, for word key defined
        string[] wordKeyNameStr = contents[1].Split(',');

        if(wordTypeStr.Length != wordKeyNameStr.Length)
        {
            Debuger.LogError( "CSVTableLoader", "Load {0} csv file failed, because of word type length not equal to word key length", fileName);
            return;
        }

        //search the key word
        int keyWordHierarchyLevel = 0;
        foreach(string str in wordTypeStr)
        {
            if( str[0] == 'K' || str[0] == 'k' )
                keyWordHierarchyLevel = keyWordHierarchyLevel + 1;
        }

        //hierarchy level should not be bigger than 2
        if(keyWordHierarchyLevel == 0 || keyWordHierarchyLevel > 2)
        {
            Debuger.LogError( "CSVTableLoader", "Load {0} csv file failed, because of the incorrect key word count", fileName);
            return;
        }


        CSVTableContainer csvContainer = new CSVTableContainer(keyWordHierarchyLevel);
        //entry content
        for(int i = 2; i < contents.Length; i++)
        {
            if( string.IsNullOrEmpty( contents[i] )) 
                continue;

            Dictionary<string, CSVValue> rowDict = new Dictionary<string, CSVValue>();

            string[] rowContent = contents[i].Split(',');
            for(int j = 0; j < rowContent.Length; j++)
            {
                // column name as key                     //content string  //value type string
                rowDict[ wordKeyNameStr[j] ] = new CSVValue(rowContent[j], wordTypeStr[j]);
            }

            //store the entry to root dict
            if(keyWordHierarchyLevel == 1) {
                csvContainer.SetRow(rowContent[0], rowDict);
            } 
            else if(keyWordHierarchyLevel == 2) {
                csvContainer.SetRow(rowContent[0], rowContent[1], rowDict);
            }
        }

        _CSVTableCache[fileName] = csvContainer;
    }

    public static CSVValue GetCSVValue(string fileName, string key, string name)
    {
        CSVTableContainer container = GetTableContainer(fileName);
        if( container == null )
            return null;
        else
            return container.GetValue(key, name);
    }

    public static CSVValue GetCSVValue(string fileName, string key1, string key2, string name)
    {
        CSVTableContainer container = GetTableContainer(fileName);
        if( container == null )
            return null;
        else
            return container.GetValue(key1, key2, name);
    }

    public static CSVTableContainer GetTableContainer(string fileName)
    {
        if(!_CSVTableCache.ContainsKey(fileName))
        {
            if( loadAsync )
            {
                RequestDataTable(fileName);
                return null;
            }
            else
            {
                LoadDataTable(fileName);
            }
        }
        return _CSVTableCache[fileName];
    }

    public static void RemoveTable(string fileName)
    {
        Debuger.LogWarning("CSVTableLoader", "You must make sure you actually need remove the table: " + fileName);
    
        if( _CSVTableCache.ContainsKey(fileName) )
        {
            _CSVTableCache.Remove(fileName);
        }
    }
}
