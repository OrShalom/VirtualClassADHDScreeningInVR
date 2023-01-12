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
                Debug.Log($" START MESSAGE [x] Received {message}");

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

            Debug.Log($" STOP MESSAGE [x] Received {message}");
            stopGame = true;
        });
    }

    private void OnApplicationQuit()
    {
        startScreeningQueue.Dispose();
        stopScreeningQueue.Dispose();
        finishScreeningQueue.Dispose();
    }

    void RestartGame()
    {
        board.text = "Welcome to Virtual Classroom\nWaiting for the screening to start";
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
        InitLettrsData();
        Report report = new Report
        {
            Time = DateTime.Now,
            PatientId = patient.EmailAddress
        };
        board.text = "Lets Start...";
        yield return new WaitForSecondsRealtime(3);
        HeadTracker.StartTracking();
        yield return SessionCoroutine();
        Session sessionWithout = new Session();
        UpdateSessionData(sessionWithout);
        report.SessionWithoutDisturbances = sessionWithout;
        board.text = "First session is done";
        InitLettrsData();
        yield return new WaitForSecondsRealtime(10f);//Decide the break time 
        board.text = "Second session is Starting...";
        yield return new WaitForSecondsRealtime(3);
        disturbances.StartDisturbances(sessionConfiguration.DisturbanceTimeRangeMin, sessionConfiguration.DisturbanceTimeRangeMax);
        HeadTracker.StartTracking();
        yield return SessionCoroutine();
        disturbances.Stop();
        Session sessionWith = new Session();
        UpdateSessionData(sessionWith);
        report.SessionWithDisturbances = sessionWith;
        try
        {
            for (int i = 0; i < disturbances.TimesOfDisturbances.Count; i++)
            {
                report.DisturbancesMetadata.Add(new DisturbanceMetadata(disturbances.TimesOfDisturbances[i], disturbances.disturbancesTypes[i]));
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        board.text = "Second session is done";
        yield return new WaitForSecondsRealtime(2);
        finishScreeningQueue.SendMessageToQ(JsonConvert.SerializeObject(report));
        board.text = "Screening Has Finished";
        yield return new WaitForSecondsRealtime(3f);
        RestartGame();
    }

    private void UpdateSessionData(Session session)
    {
        session.HeadRotation = HeadTracker.StopTracking();
        session.lettersDataList = lettersDataList;
        session.PressedAndshould = PressedAndshould;
        session.NotPressedAndshould = NotPressedAndshould;
        session.PressedAndshouldNot = PressedAndshouldNot;
    }

    private IEnumerator SessionCoroutine()
    {
        for (int i = 0; i < lettersDataList.Length; i++)  
        {
            if (stopGame)
            {
                board.text = "Screening is stopped";
                yield return new WaitForSecondsRealtime(4);
                RestartGame();
                yield break;
            }
            pressed = false;
            board.text = lettersDataList[i].ToString();
            yield return new WaitForSecondsRealtime(sessionConfiguration.LettersDelayInSec);
            if (pressed && ShouldPress(i))
            {
                time = ((i+1) * sessionConfiguration.LettersDelayInSec);
                PressedAndshould.Add(time);
            }
            if (pressed && !ShouldPress(i))
            {
                time = ((i+1) * sessionConfiguration.LettersDelayInSec);
                PressedAndshouldNot.Add(time);
            }
            if (!pressed && ShouldPress(i))
            {
                time = ((i+1) * sessionConfiguration.LettersDelayInSec);
                NotPressedAndshould.Add(time);
            }
        }
    }

    private bool ShouldPress(int i)
    {
        return i != 0 && lettersDataList[i] == 'X' && lettersDataList[i - 1] == 'A';
    }

    private void InitLettrsData()
    {
        int lettersAmount = (int)Math.Ceiling((decimal)(sessionConfiguration.SessionLengthInMin*60) / sessionConfiguration.LettersDelayInSec);
        lettersDataList = new char[lettersAmount];
        for (int i =0; i< lettersAmount; i++)
        {
            lettersDataList[i] = (char)('A' + Random.Range(0, 26));
        }
        int[] indexesArray = new int[sessionConfiguration.AmountOfShouldPress];
        Array.Fill(indexesArray,-1);
        for (int i = 0; i < sessionConfiguration.AmountOfShouldPress; i++)
        {
            int index = Random.Range(1, lettersAmount );
            while (indexesArray.Contains(index) || indexesArray.Contains(index+1) || indexesArray.Contains(index-1))
            {
                index = Random.Range(1, lettersAmount );
            }
            indexesArray[i] = index;
            lettersDataList[index] = 'X';
            lettersDataList[index-1] = 'A';
        }
    }
}
