using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;

public abstract class SavedObject : MonoBehaviour {

	[Tooltip("Use state shared between scenes for objects with this hierarchichal name.")]
	public bool useGlobalNamespace;

	private Dictionary<string, object> properties = new Dictionary<string, object>();

	bool hasSavedData => properties.Count > 0;

	Save save;

	void Start() {
		Load();
		Initialize();
		if (hasSavedData) LoadFromProperties();
	}

	void Load() {
		save = GameObject.FindObjectOfType<SaveManager>().save;
		properties = save.LoadAtPath(GetObjectPath());
	}

	public void BeforeSave() {
		SaveToProperties(ref properties);
		foreach (String s in properties.Keys.ToArray()) {
			if (properties[s] is Vector3) {
				SanitizeVector3(s, properties);
			}
		}
	}

	// putting a vanilla vec3 in a dict will result in a circular ref error
	// w.r.t. normalization
	void SanitizeVector3(string s, Dictionary<string, object> properties) {
		Vector3 v = (Vector3) properties[s];
		properties[s+"X"] = v.x;
		properties[s+"Y"] = v.y;
		properties[s+"Z"] = v.z;
		properties.Remove(s);
	}

	public void ReloadFromDisk() {
		Load();
		if (hasSavedData) LoadFromProperties();
	}

	protected abstract void SaveToProperties(ref Dictionary<string, object> properties);
	protected virtual void Initialize() {}
	protected abstract void LoadFromProperties();

	public string GetObjectPath() {
		if (useGlobalNamespace) return $"global/{name}/{GetType().Name}";
		return $"{SceneManager.GetActiveScene().name}/{gameObject.GetHierarchicalName()}/{GetType().Name}";
	}

	protected T Get<T>(string key) {	
		if (typeof(T).Equals(typeof(Vector3))) {
			return (T) Convert.ChangeType(GetVector3(key), typeof(T));
		}

		var v = properties[key];
		try {
			return (T) v;
		} catch (InvalidCastException) {
			// simple conversions that are serialized as C# types
			if (typeof(T).Equals(typeof(float))) {
				return (T) Convert.ChangeType(v, typeof(float));
			} else if (typeof(T).Equals(typeof(int))) {
				return (T) Convert.ChangeType(v, typeof(int));
			} else if (properties[key].GetType().Equals(typeof(JObject))) {
				return (properties[key] as JObject).ToObject<T>();
			}
		}
		throw new InvalidCastException("No valid cast for property "+key+"!");
	}

	protected List<T> GetList<T>(string key) {
		var v = properties[key];
		try {
			return (List<T>) v;
		} catch (InvalidCastException) {
			return (v as JArray).ToObject<List<T>>();
		}
	}

	Vector3 GetVector3(string key) {
		return new Vector3(
			Get<float>(key+"X"),
			Get<float>(key+"Y"),
			Get<float>(key+"Z")
		);
	}
}
