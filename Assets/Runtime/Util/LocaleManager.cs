using LitJson;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LocaleManager 
{
	private Dictionary<string, Dictionary<string, string>> _localeStringMap;
	readonly static string REPLACE_DELIMITER = "#";
	private const string LOCALE_PATH = "Locale/locale";
	private const string LOCALE_BASE_PATH = "Locale";
	private const string TEXT_PATH = "text";
	public Locale Locale { get; private set; }
	public string LanguageCode 
	{ 
		get; 
		private set; 
	}

	public string DefaultLanguage
	{
		get;
		private set;
	}

	public signals.Signal<string> LanguageChanged;

	public LocaleManager()
	{

	}

	public void init()
    {
		_localeStringMap = new Dictionary<string, Dictionary<string, string>>();
		LanguageChanged = new signals.Signal<string>();

		TextAsset locale = Resources.Load<TextAsset>(LOCALE_PATH);
		Locale = JsonUtility.FromJson<Locale>(locale.text);
		LanguageCode = DefaultLanguage = Locale.languages[0].code;

		foreach (Language language in Locale.languages)
		{
			string basePath = LOCALE_BASE_PATH + "/" + language.code;
			TextAsset text = Resources.Load<TextAsset>(basePath + "/" + TEXT_PATH);
			addBundle(text.text, language.code);
		}
	}

	public void SetLanguage(string languageCode)
    {
		if (LanguageCode != languageCode)
        {
			LanguageCode = languageCode;
			LanguageChanged.Dispatch(languageCode);
		}
    }

	public void addBundle(string strings, string languageCode)
	{
		if (!_localeStringMap.ContainsKey(languageCode))
		{
			_localeStringMap[languageCode] = new Dictionary<string, string>();
		}

		Dictionary<string, string> newEntries = JsonMapper.ToObject<Dictionary<string, string>>(strings);

		// merge the new entries into the existing dictionary.  This will overwrite duplicates.
		newEntries.ToList().ForEach(x => _localeStringMap[languageCode][x.Key] = x.Value);
	}
		
	public string lookup(string key, Dictionary<string, string> replace = null, bool forceUppercase = true)
	{
		key = key.ToLower();

		if (!_localeStringMap[LanguageCode].ContainsKey(key))
		{
			Debug.LogError($"LocaleManager.lookup failed for key '{key}' in language code '{LanguageCode}'");
			return string.Empty;
		}

		string text = _localeStringMap[LanguageCode][key];

		if (replace != null)
		{
			foreach (KeyValuePair<string, string> replaceItem in replace)
			{
				string[] sep = new string[] { REPLACE_DELIMITER + replaceItem.Key + REPLACE_DELIMITER };
				string[] textArray = text.Split(sep, System.StringSplitOptions.None);
				text = string.Join(replace[replaceItem.Key], textArray);
			}
		}

		// convert new lines
		if (text != null)
		{
			text = text.Replace("<br>","\n");
		}

		// A hack to deal with fonts that only contain upper case letters.
		if (forceUppercase)
        {
			text = text.ToUpper();
        }
		return text;
	}
}

[System.Serializable]
public class Locale
{
	public List<Language> languages;
}

[System.Serializable]
public class Language
{
	public string label;
	public string code;
}
