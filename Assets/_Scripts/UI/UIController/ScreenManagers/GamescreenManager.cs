﻿using UnityEngine;
using UnityEngine.UI;

public class GamescreenManager : UIManager 
{
    public static GamescreenManager singleton;

	[Header("Master version")]
    [SerializeField]private GameObject masterView;
    [SerializeField]private Text feedbackMaster;
    [SerializeField]private Text promptMaster;
    [SerializeField]private Text[] answersMaster;
    [SerializeField]private Text timer;

    [Space(10)]
    [Header("Client version")]
    [SerializeField]private GameObject clientView;
    [SerializeField]private Text clientName;
    [SerializeField]private Text promptClient;
    [SerializeField]private InputField answerfieldClient;
    [SerializeField]private Button submitClient;
    [SerializeField]private GameObject answerSection;
    [SerializeField]private GameObject voteSection;
    [SerializeField]private Button[] answersClient;
    [SerializeField]private Text[] answersClientLabel;
    public static System.Action<string> OnSubmitAnswer;

    [Space(10)]
    [SerializeField]private Text score1;
    [SerializeField]private Text score2;
    
    protected override void Awake()
    {
        if(singleton != null && singleton != this)
            Destroy(this);
        singleton = this;
        
        this.screenType = ScreenType.GAMESCREEN;
    }

    protected override void SetScreenForComputer()
    {
        this.clientView.SetActive(false);
        this.clientView = null;

        this.clientName.gameObject.SetActive(false);
        this.clientName = null;

        this.promptClient.gameObject.SetActive(false);
        this.promptClient = null;

        this.answerfieldClient.gameObject.SetActive(false);
        this.answerfieldClient = null;

        this.answerSection.SetActive(false);
        this.answerSection = null;

        this.voteSection.SetActive(false);
        this.voteSection = null;

        this.submitClient.gameObject.SetActive(false);
        this.submitClient = null;

        this.masterView.SetActive(true);

        this.feedbackMaster.gameObject.SetActive(true);
        this.feedbackMaster.text = "Answer the questions on your phone";

        this.promptMaster.gameObject.SetActive(true);
        this.promptMaster.text = string.Empty;

        this.timer.gameObject.SetActive(false);
        this.timer.text = string.Empty;

        this.answersMaster[0].gameObject.SetActive(false);
        this.answersMaster[1].gameObject.SetActive(false);

        this.answersMaster[0].transform.parent.gameObject.SetActive(false);
        this.answersMaster[1].transform.parent.gameObject.SetActive(false);
    }

    protected override void SetScreenForMobile()
    {
        this.masterView.SetActive(false);
        this.masterView = null;

        this.feedbackMaster.text = string.Empty;
        this.feedbackMaster.gameObject.SetActive(false);
        this.feedbackMaster = null;

        this.promptMaster.text = string.Empty;
        this.promptMaster.gameObject.SetActive(false);
        this.promptMaster = null;

        this.timer.text = string.Empty;
        this.timer.gameObject.SetActive(false);
        this.timer = null;

        this.answersMaster = null;

        this.clientView.SetActive(true);
        this.clientName.gameObject.SetActive(true);
        this.promptClient.gameObject.SetActive(true);

        this.answerfieldClient.Clear();
        this.answerfieldClient.gameObject.SetActive(true);
        this.submitClient.gameObject.SetActive(true);
        this.answerSection.SetActive(true);
        this.voteSection.SetActive(false);

        this.SetQuestion("Waiting for a prompt", false);
    }

    protected override void OnScreenEnabled()
    {
        this.score1.text = string.Empty;
        this.score2.text = string.Empty;

        if(this.clientView != null)
        {
            this.clientName.text = JoinscreenManager.singleton.Name;
            submitClient.onClick.AddListener(()=> this.OnSubmit());
        }

        if(this.masterView != null)
        {
            Host.singleton.UpdateRound(0);
        }
    }

    public void SetQuestion(string promptClient, bool removeInput)
    {
        this.promptClient.text = promptClient;
        this.answerfieldClient.gameObject.SetActive(!removeInput);
        this.submitClient.gameObject.SetActive(!removeInput);
    }

    public void StartWaitForAnswers()
    {
        StartCoroutine("WaitForAnswers");
    }

    private System.Collections.IEnumerator WaitForAnswers()
    {
        float waitTime = 90.0f;
        float timer = waitTime;
        this.timer.gameObject.SetActive(true);
        this.feedbackMaster.text = "Answer the prompts on you phone.";
        while(timer > 0.0f)
        {
            this.timer.text = Mathf.Round(timer).ToString("00");
            yield return new WaitForSeconds(1.0f);
            timer--;
        }
        this.timer.gameObject.SetActive(false);
    }

    public void StartVoting(Question[] data)
    {
        StopCoroutine("WaitForAnswers");
        this.feedbackMaster.text = "Vote for your favourite answer on your phone:";
        this.answersMaster[0].gameObject.SetActive(true);
        this.answersMaster[1].gameObject.SetActive(true);
        this.answersMaster[0].transform.parent.gameObject.SetActive(true);
        this.answersMaster[1].transform.parent.gameObject.SetActive(true);
        StartCoroutine(Vote(data, null));
    }

    private System.Collections.IEnumerator Vote(Question[] data, System.Action callback)
    {
        float waitTime = 20.0f;
        float timer = waitTime;

        int promptIndex = 0;
        int len = data.Length;
        this.timer.gameObject.SetActive(true);
        for(byte i = 0; i < len; i++)
        {   
            int currID = data[i].prompts[promptIndex].id;
            string prompt = data[i].prompts[promptIndex].text;
            Answer answer1 = data[i].answers[promptIndex];
            Answer answer2 = default(Answer);

            for(int j = 0; j < len*2; j++)
            {
                if(i == j) continue;

                if(data[j].prompts[0].id == currID)
                {
                    answer2 = data[j].answers[0];
                    break;
                }

                if(data[j].prompts[1].id == currID)
                {
                    answer2 = data[j].answers[1];
                    break;
                }
            }

            Host.singleton.SetVotables(answer1, answer2);

            string answer1Text = answer1.text;
            string answer2Text = answer2.text;
        
            this.promptMaster.text = prompt;
            this.answersMaster[0].text = answer1Text;
            this.answersMaster[1].text = answer2Text;
            RPC.singleton.CallVote(prompt, answer1Text, answer2Text);

            timer = waitTime;
            while(timer >= 0.0f)
            {
                this.timer.text = Mathf.Round(timer).ToString("00");
                yield return new WaitForSeconds(1.0f);
                timer--;
            }

            yield return StartCoroutine(ShowScores());
        }
        
        this.timer.gameObject.SetActive(false);
        this.feedbackMaster.text = string.Empty;
        
        this.answersMaster[0].transform.parent.gameObject.SetActive(false);
        this.answersMaster[1].transform.parent.gameObject.SetActive(false);

        if(callback != null)
            callback();
        yield return null;
    }

    public void ShowClientVote(string prompt, string answer1, string answer2)
    {
        this.promptClient.text = prompt;
        this.answersClientLabel[0].text = answer1;
        this.answersClientLabel[1].text = answer2;

        this.answersClient[0].gameObject.SetActive(true);
        this.answersClient[1].gameObject.SetActive(true);

        this.answersClient[0].onClick.RemoveAllListeners();
        this.answersClient[1].onClick.RemoveAllListeners();

        this.answersClient[0].onClick.AddListener(()=>
        {
            RPC.singleton.SendVote(0);
            this.answersClient[0].gameObject.SetActive(false);
            this.answersClient[1].gameObject.SetActive(false);
        });

        this.answersClient[1].onClick.AddListener(()=> 
        
        {
            RPC.singleton.SendVote(1);
            this.answersClient[0].gameObject.SetActive(false);
            this.answersClient[1].gameObject.SetActive(false);
        });

        this.answerSection.SetActive(false);
        this.voteSection.SetActive(true);
    }

    private System.Collections.IEnumerator ShowScores()
    {
        int[] data = Host.singleton.GetVotePercentage();
        this.score1.text = data[0]+"%";
        this.score2.text = data[1]+"%";

        yield return new WaitForSeconds(2.0f);

        int maxScore = Data.ROUNDS_DATA.rounds[Host.singleton.CurrentRound].maxScore;
        int score1 = (int)Mathf.Round(data[0] * maxScore/100);
        int score2 = maxScore-score1;
        this.score1.text =  score1.ToString();
        this.score2.text = score2.ToString();

        yield return new WaitForSeconds(3.0f);

        this.score1.text = string.Empty;
        this.score2.text = string.Empty;

        yield return null;
    }

    private void OnSubmit()
    {
        string answer = this.answerfieldClient.text;
        if(!this.ValidateAnswer(answer))
            return;

        this.answerfieldClient.Clear();

        if(OnSubmitAnswer != null)
            OnSubmitAnswer(answer);
    }

    private bool ValidateAnswer(string text)
    {
        return true;
    }

    protected override void OnScreenDisabled()
    {
        if(this.submitClient != null)
            submitClient.onClick.RemoveAllListeners();
    }
}