
using UnityEngine;
using System;
using UnityEngine.Localization.Metadata;
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

    private static AssetTable _acupPointSpriteChineseTable;
    private static AssetTable AcupPointSpriteChineseTable
    {
        get
        {
            var collection = LocalizationEditorSettings.GetAssetTableCollection("AcpunturePointSprite");
            return _acupPointSpriteChineseTable ??= GetTable<AssetTable>(collection, "zh-TW");
        }
    }
    private static AssetTable _acupPointSpriteEnglishTable;
    private static AssetTable AcupPointSpriteEnglishTable
    {
        get
        {
            var collection = LocalizationEditorSettings.GetAssetTableCollection("AcpunturePointSprite");
            return _acupPointSpriteEnglishTable ??= GetTable<AssetTable>(collection, "en");
        }
    }

    [MenuItem("SeniorProject/LocalizationAndSO/AcupunturePointSOs Update")]
    public static void UpdateAcupuncturePointSOs()
    {
        
        //Debug.Log(GetTable<AssetTable>(LocalizationEditorSettings.GetAssetTableCollection("UISprite"), "en").GetEntry("AR mdoe"));
        ReadAcupunturePointDB("acupuncture_point", true);
        ReadAcupunturePointDB("EN_acupuncture_point", false);
    }

    [MenuItem("SeniorProject/LocalizationAndSO/Question Locals Update")]
    public static void UpdateQuestionLocals()
    {
        ReadQuestionDB("questioon", true);
        ReadQuestionDB("EN_questioon", false);
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
                    int index = 0;
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
                        string spriteName;
                        name = "AcupuncturePoint" + id.ToString();
                        if (IsChinese)
                        {
                            spriteName = "Acupuncture" + index.ToString();
                            UpdateAcupuncturePointSO(id,AcupuncturePointChineseTable,AcupPointSpriteChineseTable ,name, title, content, disease, offset_x, offset_y, rel_position, customize, spriteName,index);
                        }   
                        else
                        {
                            spriteName = "AcupunctureE" + index.ToString();
                            
                            UpdateAcupuncturePointSO(id,AcupuncturePointEnglishTable, AcupPointSpriteEnglishTable,name, title, content, disease, offset_x, offset_y, rel_position, customize, spriteName,index);
                        }

                        index++;
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
                        int lineIndex = 0, choiceIndex = 0,dialogueIndex = 0;
                        questionIndex++;
                        lineIndex++;
                        choiceIndex++;
                        dialogueIndex++;

                        name = "D" + dialogueIndex + "-" + "Q" + questionIndex;
                        
                        string Lkey = "L" + lineIndex + "-" + name;
                        string Ckey = "C" + choiceIndex + "-" + name;
                        
                        if(IsChinese)
                            UpdateQuestionLocalization(QuestionPointChineseTable,"D", Lkey, name, question, ANS, A, B, C, D);
                        else
                            UpdateQuestionLocalization(QuestionEnglishTable, "D", Lkey, name, question, ANS, A, B, C, D);

                        UpdateQuestionSOs(questionIndex);
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

    private static void UpdateAcupuncturePointSO(int id,StringTable stringTable,AssetTable assetTable ,string name, string title, string content, string disease, float offset_x, float offset_y, int rel_position, float customize,string spriteName,int index)
    {
        string titleKeyID = name  + "_title";
        string descritpionKeyID = name + "_description";
        string diseaseKeyID = name + "_disease";

        UpdateStringTableEntry(stringTable, titleKeyID, title);
        UpdateStringTableEntry(stringTable, descritpionKeyID, content);
        UpdateStringTableEntry(stringTable, diseaseKeyID, disease);
        
        Sprite sprite =Resources.Load<Sprite>("Art/AcupuncturePoint/" + spriteName);
        //Debug.Log(spriteName);
        //Debug.Log(sprite);
        var path = AssetDatabase.GetAssetPath(sprite);
        string spriteGuid = AssetDatabase.AssetPathToGUID(path);
        UpdateAssetTableEntry(assetTable, name, spriteGuid);
        
        if (!File.Exists(name))
        {
            CreateAcupunturePointSO(name, titleKeyID, descritpionKeyID, diseaseKeyID, offset_x, offset_y, rel_position, customize, name);
            return;
        }
        ItemAcupuncturePointSO acupuncturePoint = (ItemAcupuncturePointSO)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Spots/Spot", typeof(ItemAcupuncturePointSO));
        acupuncturePoint.Setup(titleKeyID, descritpionKeyID, AcupuncturePoint, diseaseKeyID, offset_x, offset_y, rel_position, customize, AcupuncturePointPrefab, name);
    }

    public static void UpdateQuestionSOs(int questionIndex)
    {
        string name = "Q" + questionIndex.ToString();
        if (!File.Exists(name))
        {
            CreateDialogueDataSO(name);
        }

    }

    private static void CreateAcupunturePointSO(string name, string titleKeyID, string descritpionKeyID, string disease, float offset_x, float offset_y, int rel_position, float customize,string spriteName)
    {
        ItemAcupuncturePointSO asset = ScriptableObject.CreateInstance<ItemAcupuncturePointSO>();


        asset.Setup(titleKeyID, descritpionKeyID, AcupuncturePoint, disease, offset_x, offset_y,rel_position,customize, AcupuncturePointPrefab, spriteName);

        AssetDatabase.CreateAsset(asset, "Assets/ScriptableObjects/Inventory/Item/AcupuncturePoint/" + name + ".asset");  // creat new scriptable
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }

    private static void CreateDialogueDataSO(string name)
    {
        DialogueDataSO asset = ScriptableObject.CreateInstance<DialogueDataSO>();

        AssetDatabase.CreateAsset(asset, "Assets/ScriptableObjects/Dialogue/Question/" + name + ".asset");  // creat new scriptable
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }

    private static void UpdateQuestionLocalization(StringTable stringTable, string actor, string Lkey, string name, string question, string ANS, string A, string B, string C, string D)
    {
        UpdateStringTableEntryANDComment(stringTable, Lkey, question, actor);
        UpdateStringTableEntryANDComment(stringTable, "C1-" + name, A, IsANS(ANS, "A"));
        UpdateStringTableEntryANDComment(stringTable, "C2-" + name, B, IsANS(ANS, "B"));
        UpdateStringTableEntryANDComment(stringTable, "C3-" + name, C, IsANS(ANS, "C"));
        UpdateStringTableEntryANDComment(stringTable, "C4-" + name, D, IsANS(ANS, "D"));
    }

    private static string IsANS(string ans,string opt)
    {
        return ans == opt ? "WinningChoice" : "LosingChoice";
    }

    public static void UpdateStringTableEntryANDComment(StringTable StringTable, string KeyID, string entryValue,string commentValue)
    {
        StringTableEntry TitleEntry = StringTable.GetEntry(KeyID);
        UpdateLocalizeEntry(StringTable, TitleEntry, KeyID, entryValue);
        UpdateEntryComment(StringTable,KeyID ,commentValue);
    }

    public static void UpdateStringTableEntry(StringTable StringTable,string KeyID, string value)
    {
            StringTableEntry TitleEntry = StringTable.GetEntry(KeyID);
            UpdateLocalizeEntry(StringTable, TitleEntry, KeyID, value);
    }

    public static void UpdateAssetTableEntry(AssetTable AssetTalbe, string KeyID, string value)
    {
        AssetTableEntry TitleEntry = AssetTalbe.GetEntry(KeyID);
        UpdateAssetLocalizeEntry(AssetTalbe, TitleEntry,KeyID, value);
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

    public static void UpdateAssetLocalizeEntry(AssetTable table, AssetTableEntry Entry, string keyID, string value)
    {
        if (Entry == default)
        {
            //create new entry
            table.AddEntry(keyID, value);
        }
        else
        {
            //update entry
            Entry.Guid = value;
        }
    }

    public static void UpdateEntryComment(StringTable stringTable,string KeyID ,string actor)
    {
        Comment comment = stringTable.SharedData.GetEntry(KeyID).Metadata.GetMetadata<Comment>();
        if (comment == null) 
        {
            Comment newComment = new Comment();
            newComment.CommentText = actor;
            stringTable.SharedData.GetEntry(KeyID).Metadata.AddMetadata(newComment);
        }

        else
        {
            comment.CommentText = actor;
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

    private static string GetGuid(string name)
    {
        IEnumerable<string> GUIDs = AssetDatabase.FindAssets(name + " t:Sprite", new string[] { "Assets/Art/UI" }).Where((guid) => Path.GetFileName(AssetDatabase.GUIDToAssetPath(guid)) == name + ".meta");
        if (GUIDs.Count() == 0)
        {
            Debug.LogError("Can't find Sprite named: " + name + " in Assets/Art/UI folder");
            return default;
        }
        return GUIDs.First();
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

    private static T GetTable<T>(AssetTableCollection collection, string tableName) where T : LocalizationTable
    {
        var Table = collection.GetTable(tableName) as T;

        if (Table == null)
        {
            Debug.LogError("Can't find " + tableName + " table in Spot string table collection");
            return default;
        }
        return Table;
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