using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using UnityEngine;

public class GameData : BaseBehaviour
{
	public float DDamageMod   				{ get; set; }		//difficulty damage modifier
	public int CurrentScore          	{ get; set; }
	public int LastSavedScore        	{ get; set; }
	public int Lives              		{ get; set; }
	public int CurrentLevel          	{ get; set; }

	//only called once since this is a singleton
	void Awake()
	{
		DDamageMod 				= 1f;
		CurrentScore         = 0;
		LastSavedScore       = 0;
		Lives                = 3;
		CurrentLevel         = 1;
	}

	public void Save()
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(Application.persistentDataPath + "/GameData.dat");
		GameDataContainer container = new GameDataContainer();

		container.dDamageMod 			 = DDamageMod;
		container.currentScore         = CurrentScore;
		container.lastSavedScore       = LastSavedScore;
		container.lives                = Lives;
		container.currentLevel         = CurrentLevel;

		bf.Serialize(file, container);
		file.Close();
	}

	public void Load()
	{
		if(File.Exists(Application.persistentDataPath + "/GameData.dat"))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + "/GameData.dat", FileMode.Open);
			GameDataContainer container = (GameDataContainer)bf.Deserialize(file);
			file.Close();

			DDamageMod 			   = container.dDamageMod;
			CurrentScore         = container.currentScore;
			LastSavedScore       = container.lastSavedScore;
			Lives                = container.lives;
			CurrentLevel         = container.currentLevel;
		}
	}

	void OnSaveGameData(bool status)
	{
		Save();
	}

	void OnLoadGameData(bool status)
	{
		Load();
	}

	void OnEnable()
	{
		EventKit.Subscribe<bool>("save game data", OnSaveGameData);
		EventKit.Subscribe<bool>("load game data", OnLoadGameData);
	}

	void OnDestroy()
	{
		EventKit.Unsubscribe<bool>("save game data", OnSaveGameData);
		EventKit.Unsubscribe<bool>("load game data", OnLoadGameData);
	}
}

[Serializable]
class GameDataContainer
{
	public float 	dDamageMod;
	public int 		currentScore;
	public int 		lastSavedScore;
	public int 		lives;
	public int 		currentLevel;
}
