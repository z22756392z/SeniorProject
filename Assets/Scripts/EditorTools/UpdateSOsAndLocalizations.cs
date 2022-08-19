
using UnityEngine;
using System;
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
public class UpdateSOsAndLocalizations
{
#if UNITY_EDITOR
    private static string AcupuncturePointDBPath = "Assets/DataBase/acupuncture_point.db";
    private static string AcupuncturePointDBConnectionName
    {
        get
        {
            if (!File.Exists(AcupuncturePointDBPath))
            {
                Debug.LogError("Can't find database in Path:" + AcupuncturePointDBPath);
                return default;
            }
            return "URI=file:" + AcupuncturePointDBPath;
        }
    }

    private static string QuestionDBPath = "Assets/DataBase/queation_ans.db";
    private static string QuestionDBConnectionName
    {
        get
        {
            if (!File.Exists(QuestionDBPath))
            {
                Debug.LogError("Can't find database in Path:" + QuestionDBPath);
                return default;
            }
            return "URI=file:" + QuestionDBPath;
        }
    }

    private static GameObject _acupuncturePointPrefab;
    private static GameObject AcupuncturePointPrefab { get { return _acupuncturePointPrefab ??= GetGameObject("Assets/Prefab/MediaPipe/Point Annotation.prefab"); } }

    private static ItemTypeSO _acupuncturePoint;
    private static ItemTypeSO AcupuncturePoint { get { return _acupuncturePoint ??= GetType("ItemAcupuncturePointType"); } }
    

    private static StringTable _acupuncturePointChineseTable;
    private static StringTable AcupuncturePointChineseTable
    {
        get
        {
            var collection = LocalizationEditorSettings.GetStringTableCollection("AcupuncturePoint");
            return _acupuncturePointChineseTable ??= GetStringTable(collection, "zh-TW");
        }
    }
    private static StringTable _acupuncturePointEnglishTable;
    private static StringTable AcupuncturePointEnglishTable
    {
        get
        {
            var collection = LocalizationEditorSettings.GetStringTableCollection("AcupuncturePoint");
            return _acupuncturePointEnglishTable ??= GetStringTable(collection, "en");
        }
    }

    private static StringTable _questionChineseTable;
    private static StringTable QuestionPointChineseTable
    {
        get
        {
            var collection = LocalizationEditorSettings.GetStringTableCollection("Question");
            return _questionChineseTable ??= GetStringTable(collection, "zh-TW");
        }
    }
    private static StringTable _questionEnglishTable;
    private static StringTable QuestionEnglishTable
    {
        get
        {
            var collection = LocalizationEditorSettings.GetStringTableCollection("Question");
            return _questionEnglishTable ??= GetStringTable(collection, "en");
        }
    }

    [MenuItem("SeniorProject/AcupunturePointSOs Update")]
    public static void UpdateAcupuncturePointSOs()
    {
        ReadAcupunturePointDB("acupuncture_point", true);
        //ReadAcupunturePointDB("EN_acupuncture_point", false);
    }

    [MenuItem("SeniorProject/QuestionLocals Update")]
    public static void UpdateQuestionLocals()
    {
        ReadQuestionDB("questioon", true);
    }

    public static void ReadAcupunturePointDB(string TableName, bool IsChinese)
    {
        IDbConnection connection;
        IDbCommand command;

        using (connection = new SqliteConnection(AcupuncturePointDBConnectionName))
        {
            connection.Open();

            string name;
            using (command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM " + TableName;
                
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!Read(reader, "id",out int id)) continue;
                        if (!Read(reader, "title", out string title)) continue;
                        if (!Read(reader, "content", out string content)) continue;
                        if (!Read(reader, "disease_type", out string disease)) continue;
                        if (!Read(reader, "rel_position", out int rel_position)) continue;
                        if (!Read(reader, "offset_x", out int offset_x)) continue;
                        if (!Read(reader, "offset_y", out int offset_y)) continue;
                        if (!Read(reader, "customize", out int customize)) continue;

                        name = title + " " + id.ToString();

                        UpdateAcupuncturePointSO(name, title, content, disease, offset_x, offset_y, rel_position, customize, IsChinese);
                    }

                }
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
        Debug.Log("Acupunture Point SOs Update completed");
    }

    public static void ReadQuestionDB(string TableName, bool IsChinese)
    {
        IDbConnection connection;
        IDbCommand command;

        using (connection = new SqliteConnection(QuestionDBConnectionName))
        {
            connection.Open();

            int questionIndex = 0; string name;
            using (command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM " + TableName;

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!Read(reader, "queation", out string question)) continue;
                        if (!Read(reader, "A", out string A)) continue;
                        if (!Read(reader, "B", out string B)) continue;
                        if (!Read(reader, "C", out string C)) continue;
                        if (!Read(reader, "D", out string D)) continue;
                        if (!Read(reader, "ANS", out string ANS)) continue;

                        //TODO: base on database set dialogue , line, choice and so on...
                        int lineIndex = 0, choiceIndex = 0;
                        questionIndex++;

                        name = "Q" + questionIndex + "-" + "Default";
                        lineIndex++;
                        choiceIndex++;
                        string Lkey = "L" + lineIndex + "-" + name;
                        string Ckey = "C" + choiceIndex + "-" + name;
                        


                        if (IsChinese)
                        {
                            UpdateStringTableEntry(QuestionPointChineseTable, Lkey, question);
                            UpdateStringTableEntry(QuestionPointChineseTable, "A1-"+ name, ANS);
                            UpdateStringTableEntry(QuestionPointChineseTable, "C1-" + name, A);
                            UpdateStringTableEntry(QuestionPointChineseTable, "C2-" + name, B);
                            UpdateStringTableEntry(QuestionPointChineseTable, "C3-" + name, C);
                            UpdateStringTableEntry(QuestionPointChineseTable, "C4-" + name, D);
                        }
                        else
                        {
                            UpdateStringTableEntry(QuestionEnglishTable, Lkey, question);
                            UpdateStringTableEntry(QuestionEnglishTable, "A1-" + name, ANS);
                            UpdateStringTableEntry(QuestionEnglishTable, "C1-" + name, A);
                            UpdateStringTableEntry(QuestionEnglishTable, "C2-" + name, B);
                            UpdateStringTableEntry(QuestionEnglishTable, "C3-" + name, C);
                            UpdateStringTableEntry(QuestionEnglishTable, "C4-" + name, D);
                        }
                    }

                }
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
        Debug.Log("Question Update completed");
    }

    private static bool Read<T>(IDataReader reader,string id,out T value)
    {
        string content = reader[id].ToString();
        if (content.Length == 0)
        {
            value = default;
            return false;
        }

        value = (T)Convert.ChangeType(content, typeof(T));
        return true;
    }

    private static void UpdateAcupuncturePointSO(string name, string title, string content, string disease, float offset_x, float offset_y, int rel_position,float customize, bool IsChinese)
    {
        string titleKeyID = name + "_title";
        string descritpionKeyID = name + "_description";
        if(IsChinese)
        {
            UpdateStringTableEntry(AcupuncturePointChineseTable, titleKeyID, title);
            UpdateStringTableEntry(AcupuncturePointChineseTable, descritpionKeyID, content);
        }
        else
        {
            UpdateStringTableEntry(AcupuncturePointEnglishTable, titleKeyID, title);
            UpdateStringTableEntry(AcupuncturePointEnglishTable, descritpionKeyID, content);
        }


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

    public static void UpdateStringTableEntry(StringTable StringTable,string KeyID, string value)
    {
            StringTableEntry TitleEntry = StringTable.GetEntry(KeyID);
            UpdateLocalizeEntry(StringTable, TitleEntry, KeyID, value);
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