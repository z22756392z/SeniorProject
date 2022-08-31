using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 序列化，讓資料(位元組流)可以在網路上傳送 // 可以序列化的類需要用這個屬性標記
[System.Serializable]  // 使用自訂一類型數據就要使用，可以直接在script上調整
public class UI_Dialogue : MonoBehaviour
{
    public string Name;

    [TextArea(4, 10)] // 輸入範圍加大
    public string[] sentences;
}
