using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CategoryEntryView : MonoBehaviour 
{
	public GameObject totalNewContainer;
    public TextMeshProUGUI totalNewText;
	public TextMeshProUGUI labelText; 
	public Image iconImage;
	public Button button;
	public Image backgroundImage;
	public Sprite lightBackgroundImage;
	public Sprite darkBackgroundImage; 

	public void Awake()
	{
		hideTotalNew();
	}

	public void showTotalNew(int total)
	{
		totalNewContainer.SetActive(true);
		totalNewText.text = total.ToString();
	}

	public void hideTotalNew()
	{
		totalNewContainer.SetActive(false);
		totalNewText.text = "";
	}
}
