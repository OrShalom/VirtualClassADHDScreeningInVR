using Assets.Model;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;


public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI board;
    public ButtonVR button;
    public DisturbancesManager disturbances;
    public HeadTracker HeadTracker;
    private bool pressed;
    char[] lettersDataList;
    float time;
    List<float> PressedAndshould;
    List<float> PressedAndshouldNot;
    List<float> NotPressedAndshould;
    QueueHandler startScreeningQueue;
    QueueHandler stopScreeningQueue;
    QueueHandler finishScreeningQueue;
    bool readMessage = false;
    bool stopGame = false;
    Patient patient;
    SessionConfiguration sessionConfiguration;


    // Start is called before the first frame update
    void Start()
    {
        button.ButtonPress.AddListener(() =>
        {
            pressed = true;
        });
        InitQueueHandlers();
        RestartGame();
    }

    private void InitQueueHandlers()
    {
        startScreeningQueue = new QueueHandler("StartScreening");
        stopScreeningQueue = new QueueHandler("StopScreening");
        finishScreeningQueue = new QueueHandler("FinishScreening");
        startScreeningQueue.SubscribeToQueue((model, ea) =>
        {
            try
            {
                var message = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());
                Debug.Log($" [x] Received {message}");

                var startSessionMessage = JsonConvert.DeserializeObject<StartSessionMessage>(message);
                patient = startSessionMessage.Patient;
                sessionConfiguration = startSessionMessage.SessionConfiguration;

                readMessage = true;
            }
            catch (Exception) { }
        });
        stopScreeningQueue.SubscribeToQueue((model, ea) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());

            Debug.Log($" [x] Received {message}");
            stopGame = true;
        });
    }

    void RestartGame()
    {
        board.text = "Welcome to Virtual Classroom\nWaiting for the screening to start";
        InitLettrsData();
        readMessage = false;
        stopGame = false;
        patient = new Patient();
        PressedAndshould = new List<float>();
        PressedAndshouldNot = new List<float>();
        NotPressedAndshould = new List<float>();
        disturbances.Stop();
        HeadTracker.StopTracking();
    }

    // Update is called once per frame
    void Update()
    {
        if (readMessage)
        {
            board.text = "Press the button when you are ready!";
            button.ButtonPress.AddListener(StartGameAsync);
            readMessage = false;
        }
    }

    public void StartGameAsync()
    {
        button.ButtonPress.RemoveListener(StartGameAsync);
        StartCoroutine(GameCoroutine());
    }

    IEnumerator GameCoroutine()
    {
        Report report = new Report
        {
            Time = DateTime.Now,
            PatientId = patient.EmailAddress
        };
        Session session= new Session();
        board.text = "Lets Start...";
        yield return new WaitForSecondsRealtime(4);
        HeadTracker.StartTracking();
        yield return StartSession();
        UpdateSessionData(session);
        report.SessionWithoutDisturbances = session;
        board.text = "First session has done";
        session = new Session();
        yield return new WaitForSecondsRealtime(10f);//Decide the break time 
        board.text = "Second session is Starting...";
        disturbances.StartDisturbances(sessionConfiguration.DisturbanceTimeRangeMin, sessionConfiguration.DisturbanceTimeRangeMax);
        HeadTracker.StartTracking();
        yield return StartSession();
        disturbances.Stop();
        UpdateSessionData(session);
        report.SessionWithDisturbances = session;
        board.text = "Second session has done";
        finishScreeningQueue.SendMessageToQ(JsonConvert.SerializeObject(report));
        board.text = "Screening Has Finished";
        yield return new WaitForSecondsRealtime(3f);
        RestartGame();
    }

    private void UpdateSessionData(Session session)
    {
        session.HeadRotation = HeadTracker.StopTracking();
        session.PressedAndshould = PressedAndshould;
        session.NotPressedAndshould = NotPressedAndshould;
        session.PressedAndshouldNot = PressedAndshouldNot;
    }

    private IEnumerator StartSession()
    {
        for (int i = 0; i < lettersDataList.Length; i++)  
        {
            if (stopGame)
            {
                board.text = "Screening Stopped";
                yield return new WaitForSecondsRealtime(4);
                RestartGame();
                yield break;
            }
            pressed = false;
            board.text = lettersDataList[i].ToString();
            yield return new WaitForSecondsRealtime(sessionConfiguration.LettersDelayInSec);
            if (pressed && ShouldPress(i))
            {
                time = (i * sessionConfiguration.LettersDelayInSec) / (60 / 1000);
                PressedAndshould.Add(time);
            }
            if (pressed && !ShouldPress(i))
            {
                time = (i * sessionConfiguration.LettersDelayInSec) / (60 / 1000);
                PressedAndshouldNot.Add(time);
            }
            if (!pressed && ShouldPress(i))
            {
                time = (i * sessionConfiguration.LettersDelayInSec) / (60 / 1000);
                NotPressedAndshould.Add(time);
            }
        }
    }

    private bool ShouldPress(int i)
    {
        return i != 0 && lettersDataList[i] == 'x' && lettersDataList[i - 1] == 'a';
    }

    private void InitLettrsData()
    {
        int lettersAmount = (int)Math.Ceiling((decimal)sessionConfiguration.SessionLengthInMin / sessionConfiguration.LettersDelayInSec);
        for (int i =0; i< lettersAmount; i++)
        {
            lettersDataList[i] = (char)('A' + Random.Range(0, 26));
        }
        int[] indexesArray = new int[sessionConfiguration.AmountOfShouldPress];
        for (int i = 0; i < sessionConfiguration.AmountOfShouldPress; i++)
        {
            int index = Random.Range(1, lettersAmount );
            while (indexesArray.Contains(index) || indexesArray.Contains(index-1))
            {
                index = Random.Range(1, lettersAmount );
            }
            indexesArray[i] = index;
            lettersDataList[index] = 'X';
            lettersDataList[index-1] = 'A';
        }
    }
}
