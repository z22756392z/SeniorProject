
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;
using System.Linq;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;
using System.Collections.Generic;
#endif

public class UpdateAcupuncturePoint
{
#if UNITY_EDITOR
    private static string dbPath = "Assets/DataBase/acupuncture_point.db";
    private static string dbConnectionName = "URI=file:Assets/DataBase/acupuncture_point.db";

    private static GameObject _acupuncturePointPrefab;
    private static GameObject AcupuncturePointPrefab
    {
        get
        {
            return _acupuncturePointPrefab ??= GetGameObject("Assets/Prefab/MediaPipe/Point Annotation.prefab");
        }
    }

    private static ItemTypeSO _acupuncturePoint;
    private static ItemTypeSO AcupuncturePoint { get { return _acupuncturePoint ??= GetType("ItemAcupuncturePointType"); } }
    

    private static StringTable _chineseTable;
    private static StringTable ChineseTable
    {
        get
        {
            var collection = LocalizationEditorSettings.GetStringTableCollection("AcupuncturePoint");
            return _chineseTable ??= GetStringTable(collection, "zh-TW");
        }
    }
    private static StringTable _englishTable;
    private static StringTable EnglishTable
    {
        get
        {
            var collection = LocalizationEditorSettings.GetStringTableCollection("AcupuncturePoint");
            return _englishTable ??= GetStringTable(collection, "en");
        }
    }

    [MenuItem("SeniorProject/Update acupunture point ScriptableObjects")]
    public static void UpdateSpotSOs()
    {
        UpdateSpots("acupuncture_point", true);
        //UpdateSpots("EN_Spots", false);
    }

    public static void UpdateSpots(string Spots, bool IsChinese)
    {
        if (!File.Exists(dbPath))
        {
            Debug.LogError("Can't find database in Path:" + dbPath);
            return;
        }

        IDbConnection connection;
        IDbCommand command;

        using (connection = new SqliteConnection(dbConnectionName))
        {
            connection.Open();
            using (command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM " + Spots;
                int id, rel_position;
                string name, title, content, disease;
                float offset_x, offset_y, customize;
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        id = int.Parse(reader["id"].ToString());
                        title = reader["title"].ToString();
                        content = reader["content"].ToString();
                        disease = reader["disease_type"].ToString();
                        rel_position = int.Parse(reader["rel_position"].ToString());
                        offset_x = float.Parse(reader["offset_x"].ToString());
                        offset_y = float.Parse(reader["offset_y"].ToString());
                        customize = float.Parse(reader["customize"].ToString());

                        name = "AcupuncturePoint_" + id.ToString();

                        UpdateAcupuncturePointSO(name, title, content, disease, offset_x, offset_y, rel_position, customize, IsChinese);
                    }

                }
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
        Debug.Log("Update DateBase Sciptable Object.");
    }

    private static void UpdateAcupuncturePointSO(string name, string title, string content, string disease, float offset_x, float offset_y, int rel_position,float customize, bool IsChinese)
    {
        string titleKeyID = name + "_title";
        string descritpionKeyID = name + "_description";
        UpdateLocalizeEntrys(titleKeyID, descritpionKeyID, title, content, IsChinese);

        if (!File.Exists(name))
        {
            CreateAcupunturePointSO(name, titleKeyID, descritpionKeyID, disease, offset_x, offset_y,rel_position, customize);
            return;
        }

        ItemAcupuncturePointSO acupuncturePoint = (ItemAcupuncturePointSO)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Spots/Spot", typeof(ItemAcupuncturePointSO));
        acupuncturePoint.Setup(titleKeyID, descritpionKeyID,AcupuncturePoint,disease, offset_x, offset_y,rel_position, customize,AcupuncturePointPrefab);
    }

    private static void CreateAcupunturePointSO(string name, string titleKeyID, string descritpionKeyID, string disease, float offset_x, float offset_y, int rel_position, float customize)
    {
        ItemAcupuncturePointSO asset = ScriptableObject.CreateInstance<ItemAcupuncturePointSO>();


        asset.Setup(titleKeyID, descritpionKeyID, AcupuncturePoint, disease, offset_x, offset_y,rel_position,customize, AcupuncturePointPrefab);

        AssetDatabase.CreateAsset(asset, "Assets/ScriptableObjects/Inventory/Item/AcupuncturePoint/" + name + ".asset");  // creat new scriptable
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }

    public static void UpdateLocalizeEntrys(string titleKeyID, string descritpionKeyID, string title, string description, bool IsChinese)
    {
        if (IsChinese)
        {
            StringTableEntry chineseTitleEntry = ChineseTable.GetEntry(titleKeyID);
            UpdateLocalizeEntry(ChineseTable, chineseTitleEntry, titleKeyID, title);

            StringTableEntry chineseDescriptionEntry = ChineseTable.GetEntry(descritpionKeyID);
            UpdateLocalizeEntry(ChineseTable, chineseDescriptionEntry, descritpionKeyID, description);
        }
        else
        {
            StringTableEntry englishTitleEntry = EnglishTable.GetEntry(titleKeyID);
            UpdateLocalizeEntry(EnglishTable, englishTitleEntry, titleKeyID, title);

            StringTableEntry englishDescriptionEntry = EnglishTable.GetEntry(descritpionKeyID);
            UpdateLocalizeEntry(EnglishTable, englishDescriptionEntry, descritpionKeyID, description);
        }
    }

    public static void UpdateLocalizeEntry(StringTable table, StringTableEntry Entry, string keyID, string value)
    {
        if (Entry == default)
        {
            //create new entry
            table.AddEntry(keyID, value);
        }
        else
        {
            //update entry
            Entry.Value = value;
        }
    }

    private static ItemTypeSO GetType(string name)
    {
        IEnumerable<string> GUIDs = AssetDatabase.FindAssets(name + " t:ScriptableObject", new string[] { "Assets/ScriptableObjects" }).Where((guid) => Path.GetFileName(AssetDatabase.GUIDToAssetPath(guid)) == name + ".asset");
        if (GUIDs.Count() == 0)
        {
            Debug.LogError("Can't find ScriptableObject named: " + name + "in Assets/ScriptableObjects folder");
            return default;
        }

        ItemTypeSO SO;
        string path = AssetDatabase.GUIDToAssetPath(GUIDs.First());
        SO = (ItemTypeSO)AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject));
        return SO;
    }

    private static StringTable GetStringTable(StringTableCollection collection, string tableName)
    {
        var StringTable = collection.GetTable(tableName) as StringTable;

        if (StringTable == null)
        {
            Debug.LogError("Can't find " + tableName + " table in Spot string table collection");
            return default;
        }
        return StringTable;
    }

    private static GameObject GetGameObject(string path)
    {
        GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
        if (prefab == default)
        {
            Debug.LogError("Can't find prefab in Path: " + path);
            return default;
        }
        return prefab;
    }
#endif
}