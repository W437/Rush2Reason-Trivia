using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Game
    public GameObject GamePanel;
    public Button Btn_PauseGame;


    // Game Over
    public GameObject GameOverPanel;
    public TextMeshProUGUI Txt_GameOverStats;
    public TextMeshProUGUI Txt_CurrentMode;
    public Button Btn_GameOver_Back;


    // Home 
    public GameObject HomePanel;
    public Button Btn_Blitz;
    public Button Btn_Rush;
    public Button Btn_Classic;
    public Button Btn_About;
    public Button Btn_LB;
    public TextMeshProUGUI Txt_FunFact;

    // About
    public GameObject AboutPanel;
    public Button Btn_About_Back;
    public TextMeshProUGUI Txt_QuestionsCount;
    public int QuestionsCount;

    public TextMeshProUGUI Txt_Question;
    public TextMeshProUGUI[] Txt_Answers;
    public TextMeshProUGUI Txt_Timer;
    public Button[] Btns_Answers_Prefabs;

    List<Button> Btns_Answers;  // list of buttons

    FirebaseFirestore db;

    // keep count of asked questions not to repeat them
    private List<int> askedQuestionsIds = new List<int>();
    private List<int> fetchedFunFactsIds = new List<int>();

    // Correct answer index in the shuffled list
    int correctAnswerIndex;

    // Timer fields
    public float timeLeft = 60f; // One minute timer
    private float totalTime;
    private bool timerIsRunning = false;
    private int questionsAnswered = 0;
    private int correctAnswers = 0;
    private int incorrectAnswers = 0;
    private string currentGameMode;
    public Slider timerSlider;
    public bool isPlaying = false;

    void Awake()
    {
        totalTime = timeLeft;
    }

    void Update()
    {
        // Update the slider value every frame
        if (timerIsRunning && isPlaying)
        {
            timerSlider.value = timeLeft / totalTime;
            Txt_Timer.text = timeLeft.ToString();
        }
        else if(!timerIsRunning && isPlaying)
        {
            OnGameEnd();
        }
    }

    void OnGameEnd()
    {
        if (!timerIsRunning)
        {
            AudioManager.Instance.PlaySFX(4);
            DisplayResults();
            GamePanel.SetActive(false);
            GameOverPanel.SetActive(true);
            GetRandomFunFact();
        }
    }

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        //GetRandomQuestion();
        GetRandomFunFact();

        Btns_Answers = new List<Button>(Btns_Answers_Prefabs);  // convert array to list

        // Attach event listeners to buttons
        for (int i = 0; i < Btns_Answers.Count; i++)
        {
            int index = i; // Copy i to preserve its value inside the listener
            Btns_Answers[i].onClick.AddListener(() => OnAnswerButtonClick(index));
        }

        // Start the timer when a button is clicked
        foreach (Button button in Btns_Answers)
        {
            button.onClick.AddListener(StartTimer);
        }

        Btn_Blitz.onClick.AddListener(() => StartGameMode("Blitz"));
        Btn_Rush.onClick.AddListener(() => StartGameMode("Rush"));
        Btn_Classic.onClick.AddListener(() => StartGameMode("Classic"));
        Btn_About.onClick.AddListener(() => ShowAboutScreen());
        Btn_About_Back.onClick.AddListener(() => ShowHomeScreen());
        Btn_GameOver_Back.onClick.AddListener(() => ShowHomeScreen());
        Btn_PauseGame.onClick.AddListener(() => PauseGame());

        Txt_Question.text = "";
        Txt_Answers[0].text = "";
        Txt_Answers[1].text = "";
        Txt_Answers[2].text = "";
        Txt_Answers[3].text = "";
    }

    void StartTimer()
    {
        if (!timerIsRunning)
        {
            timerIsRunning = true;
            StartCoroutine(CountdownTimer());
        }
    }

    IEnumerator CountdownTimer()
    {
        while (timeLeft > 0)
        {
            yield return new WaitForSeconds(1f);
            timeLeft--;
            // Update the slider here too to make sure it updates immediately when the timer decreases
            timerSlider.value = timeLeft / totalTime;
            Debug.Log("Timer:" + timeLeft);
        }

        timerIsRunning = false;
    }

    private void ShowAboutScreen()
    {
        AudioManager.Instance.PlaySFX(0);
        AboutPanel.SetActive(true);
        HomePanel.SetActive(false);
        GamePanel.SetActive(false);
        var count = QuestionsCount;
        Txt_QuestionsCount.text = "Total Questions in the Database: " + count;
    }

    private void PauseGame()
    {
        GetRandomFunFact();
        AudioManager.Instance.PlaySFX(0);
        GamePanel.SetActive(false);
        HomePanel.SetActive(true);
        ResetStats();
    }

    private void ShowHomeScreen()
    {
        GetRandomFunFact();
        AudioManager.Instance.PlaySFX(0);
        GamePanel.SetActive(false);
        GameOverPanel.SetActive(false);
        AboutPanel.SetActive(false);
        HomePanel.SetActive(true);
    }


    void DisplayResults()
    {
        string statsText = "Questions Answered: " + questionsAnswered + "\n\n" +
                           "Correct: " + correctAnswers + "\n" +
                           "Wrong: " + incorrectAnswers;

        Txt_GameOverStats.text = statsText;
        Txt_CurrentMode.text = currentGameMode;
        GetRandomFunFact();
        ResetStats();
    }

    void ResetStats()
    {
        questionsAnswered = 0;
        correctAnswers = 0;
        incorrectAnswers = 0;
        timerIsRunning = false;
        timeLeft = 0;
        isPlaying = false;
    }

    public void StartGameMode(string mode)
    {
        switch (mode)
        {
            case "Blitz":
                timeLeft = 60; // 60 seconds for Blitz
                break;
            case "Rush":
                timeLeft = 180; // 180 seconds for Rush
                break;
            case "Classic":
                timeLeft = 420; // 420 seconds for Classic
                break;
        }

        isPlaying = true;
        AudioManager.Instance.PlaySFX(3);
        currentGameMode = mode;
        totalTime = timeLeft;
        StartTimer();
        GamePanel.SetActive(true);
        HomePanel.SetActive(false);
        GetRandomQuestion();
    }

    IEnumerator FlashCorrectButton()
    {
        for (int i = 0; i < 2; i++)
        {
            Btns_Answers[correctAnswerIndex].GetComponent<Image>().color = ExtensionsHelpers.HexToColor("#00f6ff" + "FF");
            yield return new WaitForSeconds(0.155f);
            Btns_Answers[correctAnswerIndex].GetComponent<Image>().color = ExtensionsHelpers.HexToColor("#080808" + "FF");
            yield return new WaitForSeconds(0.155f);
        }
    }


    void OnAnswerButtonClick(int index)
    {
        // Increase the count of answered questions
        questionsAnswered++;

        // Disable all buttons temporarily to avoid multiple clicks and hide if the mode is "True or False"
        for (int i = 0; i < Btns_Answers.Count; i++)
        {
            Btns_Answers[i].interactable = false;

            // Check if the game mode is "True or False" and if the clicked button isn't the current one
            if (Txt_Answers.Length == 2 && i != index)
            {
                Btns_Answers[i].gameObject.SetActive(false);
            }
        }

        if (index == correctAnswerIndex)
        {
            AudioManager.Instance.PlaySFX(1);
            Debug.Log("Correct answer!");
            // Change the button color to green
            Btns_Answers[index].GetComponent<Image>().color = ExtensionsHelpers.HexToColor("#00f6ff" + "FF");
            correctAnswers++;
            // Optional delay before loading the next question
            StartCoroutine(DelayBeforeNextQuestion());
        }
        else
        {
            AudioManager.Instance.PlaySFX(2);
            Debug.Log("Incorrect answer.");
            // Change the button color to red
            Btns_Answers[index].GetComponent<Image>().color = ExtensionsHelpers.HexToColor("#ff0000" + "FF");
            incorrectAnswers++;
            // Start flashing the correct button
            StartCoroutine(FlashCorrectButton());
            // Optional delay before loading the next question
            StartCoroutine(DelayBeforeNextQuestion());
        }
    }


    IEnumerator DelayBeforeNextQuestion()
    {
        yield return new WaitForSeconds(0.5f);

        // Clear the button list for new answers
        Btns_Answers.Clear();

        // Fetch a new question after answering the current one
        GetRandomQuestion();

        // Reset buttons color and make them clickable again
        foreach (Button button in Btns_Answers)
        {
            button.GetComponent<Image>().color = ExtensionsHelpers.HexToColor("#080808" + "FF");
            button.interactable = true;
        }
    }

    private void GetRandomQuestion()
    {
        db.Collection("Questions").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log("Failed to get document count: " + task.Exception);
                return;
            }

            QuerySnapshot snapshot = task.Result;
            int count = snapshot.Count;
            QuestionsCount = count;
            if (count == 0)
            {
                Debug.Log("No documents found in the collection.");
                return;
            }

            Debug.Log("Snapshot Count: " + count);

            // Generate a random number within the range of documents
            int randomId;

            // Generate a random number within the range of documents,
            // but only from the ones that haven't been asked yet
            do
            {
                randomId = UnityEngine.Random.Range(0, count);
            } while (askedQuestionsIds.Contains(randomId));

            // Add the id of the chosen question to the list of asked questions
            askedQuestionsIds.Add(randomId);

            // Get the document with the random ID
            db.Collection("Questions").Document(randomId.ToString()).GetSnapshotAsync().ContinueWithOnMainThread(task1 =>
            {
                DocumentSnapshot questionSnapshot = task1.Result;
                if (!questionSnapshot.Exists)
                {
                    Debug.Log("Question does not exist!");
                    return;
                }

                string question = questionSnapshot.GetValue<string>("Question");
                Txt_Question.text = question;

                List<object> answers = questionSnapshot.GetValue<List<object>>("Answers");
                int originalCorrectIndex = questionSnapshot.GetValue<int>("CorrectIndex");

                List<string> stringAnswers = answers.Cast<string>().ToList();
                string correctAnswer = stringAnswers[originalCorrectIndex];
                stringAnswers.Shuffle();

                // Enable the buttons after the new question and answers are set
                for (int i = 0; i < Btns_Answers_Prefabs.Length; i++)
                {
                    if (i < stringAnswers.Count)
                    {
                        Btns_Answers_Prefabs[i].gameObject.SetActive(true);
                        Btns_Answers.Add(Btns_Answers_Prefabs[i]);
                    }
                    else
                    {
                        Btns_Answers_Prefabs[i].gameObject.SetActive(false);
                    }
                }

                for (int i = 0; i < stringAnswers.Count; i++)
                {
                    Txt_Answers[i].text = stringAnswers[i];
                    if (stringAnswers[i] == correctAnswer)
                    {
                        correctAnswerIndex = i;
                    }
                }

                // Add the buttons equivalent to the count of answers
                for (int i = 0; i < stringAnswers.Count; i++)
                {
                    Btns_Answers.Add(Btns_Answers_Prefabs[i]);
                }

                // Enable the buttons after the new question and answers are set
                foreach (Button button in Btns_Answers)
                {
                    button.GetComponent<Image>().color = ExtensionsHelpers.HexToColor("#080808" + "FF");
                    button.interactable = true;
                }
            });
        });
    }


    private void GetRandomFunFact()
    {
        db.Collection("FunFacts").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log("Failed to get document count: " + task.Exception);
                return;
            }

            QuerySnapshot snapshot = task.Result;
            int count = snapshot.Count;
            if (count == 0)
            {
                Debug.Log("No documents found in the collection.");
                return;
            }

            Debug.Log("Snapshot Count: " + count);

            // Generate a random number within the range of documents
            int randomId;

            // Generate a random number within the range of documents,
            // but only from the ones that haven't been fetched yet
            randomId = UnityEngine.Random.Range(0, count);

            // Get the document with the random ID
            db.Collection("FunFacts").Document(randomId.ToString()).GetSnapshotAsync().ContinueWithOnMainThread(task1 =>
            {
                DocumentSnapshot funFactSnapshot = task1.Result;
                if (!funFactSnapshot.Exists)
                {
                    Debug.Log("FunFact does not exist!");
                    return;
                }

                string funFact = funFactSnapshot.GetValue<string>("fact");
                Txt_FunFact.text = funFact;
            });
        });
    }



}


