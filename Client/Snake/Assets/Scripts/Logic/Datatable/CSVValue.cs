using System.Collections;
using System.Collections.Generic;
using Framework;

/// <summary>
/// CSV value, stores one of values: string, stringArray, int, intArray, float or floatArray.
/// The type string uses: S, SA, I, IA, F, FA
/// </summary>
public sealed class CSVValue : System.IDisposable
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
        get { return _valueType; }
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

    public void Dispose()
    {
        _stringValue = null;

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
        get {
            if (!CheckType(ValueType.StringArray))
                return null;

            if (_stringArrayValue == null)
            {
                _stringArrayValue = _stringValue.Split(';');
            }

            return _stringArrayValue;
        }
    }

    private int _intValue;
    public int IntValue
    {
        get {
            if (!CheckType(ValueType.Int))
                return 0;

            if (_intValue == int.MinValue)
                int.TryParse(_stringValue, out _intValue);

            return _intValue;
        }
    }

    private int[] _intArrayValue;
    public int[] IntArrayValue
    {
        get {
            if (!CheckType(ValueType.IntArray))
                return null;

            if (_intArrayValue == null)
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
        get {
            if (!CheckType(ValueType.Float))
                return 0f;

            if (_floatValue == float.MinValue)
                float.TryParse(_stringValue, out _floatValue);

            return _floatValue;
        }
    }

    private float[] _floatArrayValue;
    public float[] FloatArrayValue
    {
        get {
            if (!CheckType(ValueType.FloatArray))
                return null;

            if (_floatArrayValue == null)
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

        if (str[0] == 'K' || str[0] == 'k')
            str = str.Substring(1, str.Length - 1);

        switch (str)
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
        if (_valueType != type)
        {
            Debuger.LogError("CSVValue", "The raw value type is {0}, but you are trying to convert it to {1}. '{2}'", _valueType.ToString(), type.ToString(), _stringValue);
            return false;
        }
        return true;
    }
}

