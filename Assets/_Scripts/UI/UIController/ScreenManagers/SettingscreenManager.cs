﻿using UnityEngine;
using UnityEngine.UI;

public class SettingscreenManager : UIManager 
{
	public static SettingscreenManager singleton;

	[SerializeField]private Button backButton;
	
	protected override void Awake()
	{
		if(singleton != null && singleton != this)
			Destroy(this);
		singleton = this;
		
		this.screenType = ScreenType.SETTINGSSCREEN;
	}

	protected override void OnScreenEnabled()
	{
		backButton.onClick.AddListener(() => UIController.singleton.GoToScreen(ScreenType.STARTSCREEN));
	}

	protected override void OnScreenDisabled()
	{
		backButton.onClick.RemoveAllListeners();
	}
}
