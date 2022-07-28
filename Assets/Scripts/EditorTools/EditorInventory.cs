using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
#endif

public class EditorInventory
{
#if UNITY_EDITOR
	private const string AcupunturePointSOsPath = "Assets/ScriptableObjects/Inventory/Item/AcupuncturePoint";
	private const string InventoryPath = "Assets/ScriptableObjects/Inventory/Inventories/Inventory.asset";

	[MenuItem("SeniorProject/Add all acupunture point to default Inventory")]
	public static void AddAllSpotsToDefaultInventory()
	{
		InventorySO inventory = GetInventory(InventoryPath);
		ItemAcupuncturePointSO[] spots = GetSpotSos(AcupunturePointSOsPath);

		List<ItemStack> spotStacks = new List<ItemStack>();
		foreach (var spot in spots)
		{
			spotStacks.Add(new ItemStack(spot, 1));
		}
		inventory.AddDefaultAndItems(spotStacks);

		Debug.Log("Add all spots to default Inventory complete");
		EditorUtility.FocusProjectWindow();
	}

	private static InventorySO GetInventory(string path)
	{
		InventorySO Inventory = (InventorySO)AssetDatabase.LoadAssetAtPath(path, typeof(InventorySO));
		if (Inventory == default)
		{
			Debug.LogError("Can't find prefab in Path: " + path);
			return default;
		}
		return Inventory;
	}

	private static ItemAcupuncturePointSO[] GetSpotSos(string path)
	{
		string[] GUIDs = AssetDatabase.FindAssets("t:ItemAcupuncturePointSO",
			new string[] { path });
		ItemAcupuncturePointSO[] SOs = new ItemAcupuncturePointSO[GUIDs.Length];

		for (int i = 0; i < GUIDs.Length; i++)
		{
			string _path = AssetDatabase.GUIDToAssetPath(GUIDs[i]);
			SOs[i] = (ItemAcupuncturePointSO)AssetDatabase.LoadAssetAtPath(_path, typeof(ItemAcupuncturePointSO));
		}
		return SOs;
	}
#endif
}