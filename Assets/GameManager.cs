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
    List<int> PressedAndshould;
    List<int> PressedAndshouldNot;
    List<int> NotPressedAndshould;
    List<List<int>> TimesOfShouldPress;
    QueueHandler startScreeningQueue;
    QueueHandler stopScreeningQueue;
    QueueHandler finishScreeningQueue;
    QueueHandler errorsQueue;
    bool readMessage = false;
    bool stopGame = false;
    bool isGameRunning = false;
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
        errorsQueue = new QueueHandler("ErrorsQueue");
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
        stopGame = false;
        patient = new Patient();
        disturbances.Stop();
        HeadTracker.StopTracking();
        readMessage = false;
        isGameRunning = false;
    }

    private void RestartSession()
    {
        InitLettrsData();
        PressedAndshould = new List<int>();
        PressedAndshouldNot = new List<int>();
        NotPressedAndshould = new List<int>();
    }

    // Update is called once per frame
    void Update()
    {
        if (readMessage && !isGameRunning)
        {
            board.text = "Press the button when you are ready!";
            button.ButtonPress.AddListener(StartGameAsync);
            readMessage = false;
            isGameRunning = true;
        }
    }

    public void StartGameAsync()
    {
        button.ButtonPress.RemoveListener(StartGameAsync);
        StartCoroutine(GameCoroutine());
    }

    IEnumerator GameCoroutine()
    {
        // Restart the first session
        Report report;
        try
        {
            RestartSession();
            report = new Report
            {
                Time = DateTime.Now.ToShortDateString() + " | " + DateTime.Now.ToShortTimeString(),
                PatientId = patient.EmailAddress,
                PatientName = patient.FirstName + " " + patient.LastName,
                SessionConfiguration = sessionConfiguration
            };
            board.text = "Lets Start...";
        }
        catch(Exception exc)
        {
            errorsQueue.SendMessageToQ(exc.ToString());
            RestartGame();
            yield break;
        }
        yield return new WaitForSecondsRealtime(3);
        // Start the first session:
        HeadTracker.StartTracking();
        yield return SessionCoroutine();
        try
        {
           
            // Update report with first session data:
            Session sessionWithout = new Session();
            UpdateSessionData(sessionWithout);
            report.SessionWithoutDisturbances = sessionWithout;
            board.text = "First session is done";
            // Restart the second session
            RestartSession();
        }
        catch (Exception exc)
        {
            errorsQueue.SendMessageToQ(exc.ToString());
            RestartGame();
            yield break;
        }
        yield return new WaitForSecondsRealtime(10f);//Decide the break time 
        board.text = "Second session is Starting...";
        yield return new WaitForSecondsRealtime(3);
        try
        {
            // Start the second session:
            disturbances.StartDisturbances(sessionConfiguration.DisturbanceTimeRangeMin, sessionConfiguration.DisturbanceTimeRangeMax);
            HeadTracker.StartTracking();
        }
        catch (Exception exc)
        {
            errorsQueue.SendMessageToQ(exc.ToString());
            RestartGame();
            yield break;
        }
        yield return SessionCoroutine();
        try
        {
            disturbances.Stop();
            board.text = "Second session is done";
            // Update report with second session data:
            Session sessionWith = new Session();
            UpdateSessionData(sessionWith);
            report.SessionWithDisturbances = sessionWith;

            for (int i = 0; i < disturbances.TimesOfDisturbances.Count; i++)
            {
                report.DisturbancesMetadata.Add(new DisturbanceMetadata(disturbances.TimesOfDisturbances[i], disturbances.disturbancesTypes[i]));
            }
            finishScreeningQueue.SendMessageToQ(JsonConvert.SerializeObject(report));
        }
        catch (Exception exc)
        {
            errorsQueue.SendMessageToQ(exc.ToString());
            RestartGame();
            yield break;
        }
        yield return new WaitForSecondsRealtime(2);
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
        session.TimesOfShouldPress = TimesOfShouldPress;
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
            bool shouldPress = ShouldPress(i);
            for (int j = 0; j < sessionConfiguration.LettersDelayInSec; j++)
            {
                yield return new WaitForSecondsRealtime(1);
                int currentTime = i * sessionConfiguration.LettersDelayInSec + j;
                if (pressed)
                {
                    if (shouldPress)
                    {
                        PressedAndshould.Add(currentTime);
                    }
                    else
                    {
                        PressedAndshouldNot.Add(currentTime);
                    }
                    pressed = false;
                }
            }
            if (!pressed && shouldPress)
            {
                NotPressedAndshould.Add(i * sessionConfiguration.LettersDelayInSec);
            }
        }
    }

    private bool ShouldPress(int i)
    {
        return i != 0 && lettersDataList[i] == 'X' && lettersDataList[i - 1] == 'A';
    }

    private void InitLettrsData()
    {
        int lettersAmount = (int)Math.Ceiling((decimal)(sessionConfiguration.SessionLengthInMin * 60) / sessionConfiguration.LettersDelayInSec);
        lettersDataList = new char[lettersAmount];
        for (int i = 0; i < lettersAmount; i++)
        {
            lettersDataList[i] = (char)('A' + Random.Range(0, 26));
        }
        int[] indexesArray = new int[sessionConfiguration.AmountOfShouldPress];
        Array.Fill(indexesArray, -1);
        for (int i = 0; i < sessionConfiguration.AmountOfShouldPress; i++)
        {
            int index = Random.Range(1, lettersAmount);
            while (indexesArray.Contains(index) || indexesArray.Contains(index + 1) || indexesArray.Contains(index - 1))
            {
                index = Random.Range(1, lettersAmount);
            }
            indexesArray[i] = index;
            lettersDataList[index] = 'X';
            lettersDataList[index - 1] = 'A';
        }
        TimesOfShouldPress = new List<List<int>>();
        for (int i = 0; i < indexesArray.Length; i++)
        {
            int X_index = indexesArray[i];
            var times = new List<int>();
            for (int j = 0; j < sessionConfiguration.LettersDelayInSec; j++)
            {
                times.Add(X_index * sessionConfiguration.LettersDelayInSec + j);
            }
            TimesOfShouldPress.Add(times);
        }
    }
}
