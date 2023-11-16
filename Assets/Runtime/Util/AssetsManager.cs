using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class AssetsManager 
{
	private Dictionary<string, string> _assetMap;

	public AssetsManager()
	{

	}

	public void parse(string assetsData)
	{
		_assetMap = new Dictionary<string, string>();

		XmlDocument xml = new XmlDocument();
		xml.LoadXml(assetsData);
		XmlNodeList assets = xml.SelectNodes(".//asset");

		foreach (XmlElement node in assets)
		{
			string id = node.GetAttribute("id");

			_assetMap[id] = getBaseUrl(node.ParentNode) + node.GetAttribute("url");

		}
	}

	private string getBaseUrl(XmlNode node, string baseUrl = "")
	{
		if (node != null && node.Name == "group")
		{
			baseUrl = getBaseUrl(node.ParentNode, node.Attributes.GetNamedItem("baseURL").Value) + baseUrl;
		}
			
		return baseUrl;
	}

	public string getUrl(string id)
	{
		if (_assetMap.ContainsKey(id))
		{
			return _assetMap[id];
		}

		return null;
	}
}
