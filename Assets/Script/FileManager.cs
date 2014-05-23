//--------------------------------------------------------------------------------
// Author	   : 
// Date		   : 
// Copyright   : 2011-2012 Hansung Univ. Robots in Education & Entertainment Lab.
//
// Description : 
//
//--------------------------------------------------------------------------------


using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class FileManager : MonoBehaviour
{
	string SaveDirectoryName = "SaveFiles/";
	string SaveDirectoryFullPath;
	string SaveFileName;

	void Awake()
	{
#if UNITY_ANDROID
		SaveDirectoryFullPath = Path.Combine(Application.persistentDataPath, SaveDirectoryName);
#else
		SaveDirectoryFullPath = Path.Combine(Application.dataPath, SaveDirectoryName);
#endif
		if (!Directory.Exists(SaveDirectoryFullPath))
			Directory.CreateDirectory(SaveDirectoryFullPath);
	}

	public void Save(string fileName, string text)
	{
		_saveFileName = fileName;
		File.WriteAllText(_projectFullName, text);
		Debug.Log(_projectFullName);
	}

	public void Load(string fileName, out string text)
	{
		_saveFileName = fileName;
		text = File.ReadAllText(_projectFullName);
	}

	public string GetDirectoryPath()
	{
		return SaveDirectoryFullPath;
	}

	public string _saveFileName
	{
		set { SaveFileName = value; }
	}

	public string _saveDirectoryPath
	{
		get
		{
			return SaveDirectoryFullPath;
		}
		set
		{
			SaveDirectoryName = value;
		}
	}

	public bool _isNotExistFile()
	{
		return string.IsNullOrEmpty(SaveFileName);
	}

	public string _projectFullName
	{
		get
		{
			if (_isNotExistFile())
				throw new ArgumentNullException("Project file name is null or empty!");
			else
				return Path.Combine(_saveDirectoryPath, SaveFileName);
		}
	}


	// ΩÃ±€≈Ê
	private static FileManager _instance = null;
	public static FileManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType(typeof(FileManager)) as FileManager;
				if (_instance == null)
					Debug.LogError("There needs to be one active FileManager script on a GameObject in your scene.");

			}
			return _instance;
		}
	}

}
