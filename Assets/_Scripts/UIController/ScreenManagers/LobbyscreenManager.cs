﻿using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

public class LobbyscreenManager : UIManager 
{
    public static LobbyscreenManager singleton;

    [Header("Computer layout ")]
    [SerializeField] private Text roomCode;
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private GameObject wheel;
    [SerializeField] private Button playButton;
    private List<GameObject> iconHolders;

    [Space(10)]
    [Header("Mobile layout")]
    [SerializeField] private Text feedback;

    private int amountOfPlayers;

    protected override void Awake()
    {
        if(singleton != null && singleton != this)
            Destroy(this.gameObject);
        singleton = this;
        this.screenType = ScreenType.LOBBYSCREEN;
        base.Awake();
    }

    private void Start() 
    {
        amountOfPlayers = 0;
    }

    protected override void OnScreenEnabled()
    {
        this.playButton.gameObject.SetActive(false);
        this.playButton.onClick.AddListener(()=>this.OnStartGame());
    }

    public void ShowStartbutton(bool value)
    {
        this.playButton.gameObject.SetActive(value);
    }

    private void OnStartGame()
    {
        PhotonNetwork.room.IsOpen = false;
        RPC.singleton.CallGoToGame();
    }

    protected override void SetScreenForComputer()
    {
        feedback.gameObject.SetActive(false);
        feedback = null;

        wheel.SetActive(true);
        roomCode.gameObject.SetActive(true);

        iconHolders = new List<GameObject>(Data.MAX_PLAYERS);

        this.CreateIconHolders();
    }

    protected override void SetScreenForMobile()
    {
        roomCode.gameObject.SetActive(false);
        roomCode = null;
        
        iconPrefab = null;

        wheel.gameObject.SetActive(false);
        wheel = null;

        iconHolders = null;

        feedback.gameObject.SetActive(true);
        this.SetFeedback("Waiting for the host...");
    }

    public void SetFeedback(string feedback)
    {
        this.feedback.text = feedback;
    }

    private void CreateIconHolders()
    {
        byte len = Data.MAX_PLAYERS;
        float maxItemSize = MathHelper.GetDegreesPerSegment(len);

        for(byte i = 0; i < len; i++)
        {
            GameObject iconHolder = GameObject.Instantiate(this.iconPrefab);
            iconHolder.transform.SetParent(wheel.transform);

            GameObject icon = iconHolder.transform.GetChild(1).gameObject;
            icon.SetActive(false);

            Vector3 pos = MathHelper.PlaceOnCircle(wheel.transform.position, 220, maxItemSize*i);
            iconHolder.transform.localPosition = pos;
            iconHolder.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            this.iconHolders.Add(iconHolder);
        }
    }

    public void SetRoomcode(string code)
    {
        this.roomCode.text = "Roomcode: " + code;
    }

    public void AddPlayer(string name)
    {
        if(this.iconHolders.Count <= 0)
            return;
        
        GameObject holder = this.iconHolders[0];    
        this.iconHolders.RemoveAt(0);

        GameObject textObject = holder.transform.GetChild(0).gameObject;
        Text text = textObject.GetComponent<Text>();
        text.text = name;
        text.color = Color.white;
        textObject.GetComponent<Transitions.MoveTo>().Move(null);

        GameObject iconObject = holder.transform.GetChild(1).gameObject;
        iconObject.GetComponent<Image>().sprite = (Sprite)Data.PLAYER_ICONS[PhotonNetwork.room.PlayerCount-1];
        iconObject.SetActive(true);
    }

    protected override void OnScreenDisabled()
    {
        this.playButton.onClick.RemoveAllListeners();
    }
}
