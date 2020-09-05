using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Framework;


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

        TextAsset ta = Resources.Load<TextAsset>("DataTable/" + fileName);
        string strs = ta.text;

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
