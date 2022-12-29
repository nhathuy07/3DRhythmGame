using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SFB;
using System;
using Python.Runtime;


public class MapLoader : MonoBehaviour
{
    // Stopwatch
    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    // Random number generator
    private System.Random rng = new System.Random(Guid.NewGuid().GetHashCode());


    // Coin object
    [SerializeField] private GameObject CoinObj;
    [SerializeField] private float CoinScale;
    [SerializeField] private float SpawnThreshold;

    // Time
    public float elapsedTime = 0;

    // Song ended
    public bool finished = false;

    // Speed
    [SerializeField] public float BaseSpeed = 10;
    public float CalculatedSpeed;

    // Road config
    [SerializeField] public GameObject roadSegment;
    [SerializeField] public float roadSegmentLength;
    [SerializeField] public float roadLength;
    [SerializeField] public float roadSegmentCount;

    // Ocean config
    [SerializeField] public GameObject WaterBlock;

    // Toggles
    [SerializeField] public bool IsWorldReady = false;
    [SerializeField] public bool IsWorldBuilt = false;

    // Lanes
    [SerializeField] public Tuple<float, float, float> Lanes = Tuple.Create(-1.7f, 0f, 1.7f);

    public AudioSource audioSource;
    public AudioClip clip;
    public float songDuration;
    public string musicFolderLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
    public Tuple<int, List<float>> noteDetectionData = Tuple.Create(0, new List<float>());
    public ExtensionFilter[] filters = new[]
    {
        new ExtensionFilter("Audio files", "mp3", "wav", "ogg", "aif", "aiff")
    };

    // Temp
    private float currentLane;
    private float prevNoteLocation;



    Tuple<int, List<float>> GetNoteTimestamp(string file_path)
    {
        // Initialize embedded Python
        Debug.Log("Initializing Embedded Python");
        Runtime.PythonDLL = $"{Application.dataPath}/StreamingAssets/Embedded Python/python-3.7.0-embed-amd64/python37.dll";
        PythonEngine.Initialize();

        using (Py.GIL())
        {
            dynamic librosa = Py.Import("librosa");
            dynamic _song_data = librosa.load(file_path);
            int _bpm = (int)librosa.beat.tempo(_song_data[0], _song_data[1]);

            PyList _onset_data = librosa.frames_to_time(librosa.onset.onset_detect(_song_data[0], _song_data[1])).tolist();

            // Convert Python List to C# List
            List<float> _converted_onset_data = new List<float> { };
            foreach (PyObject item in _onset_data)
            {
                _converted_onset_data.Add(float.Parse(item.ToString(), System.Globalization.CultureInfo.InvariantCulture));
                
            }
            
            return Tuple.Create(_bpm, _converted_onset_data);
            
        }
        
    }

    IEnumerator GetAudioClip(string file_path)
    {
        // Load audio from local file
        string uri = $"file://{file_path}";
        AudioType audioType = AudioType.UNKNOWN;

        if (uri.EndsWith(".wav")) {
            audioType = AudioType.WAV;
        }
        else if (uri.EndsWith(".mp3"))
        {
            audioType = AudioType.MPEG;
        }
        else if (uri.EndsWith(".ogg"))
        {
            audioType = AudioType.OGGVORBIS;
        }
        else if (uri.EndsWith(".aiff") || uri.EndsWith(".aif"))
        {
            audioType = AudioType.AIFF;
        }

        using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(uri, audioType))
        {
            yield return req.SendWebRequest();
            Debug.Log(req.result);
            if (req.result != UnityWebRequest.Result.ConnectionError)
            {
                clip = DownloadHandlerAudioClip.GetContent(req);
                audioSource.clip = clip;
                songDuration = clip.length;
                yield break;
            }

        }

    }

    void BuildLevel()
    {

        CalculatedSpeed = BaseSpeed * noteDetectionData.Item1;
        roadLength = CalculatedSpeed * songDuration;
        roadSegmentCount = (float)(roadLength / 6.319 * 2);
        roadSegment.transform.localScale = new Vector3(roadSegmentCount, 1, 1);
        WaterBlock.transform.localScale = new Vector3((float)(roadLength / 20), 1, (float)(roadLength / 20));
        if (!IsWorldBuilt)
        {
            GameObject ClonedCoinObj;
            foreach (float timestamp in noteDetectionData.Item2)
            {
                Debug.Log(-CalculatedSpeed * timestamp - prevNoteLocation);
                if ((float)Math.Abs(-CalculatedSpeed * timestamp - prevNoteLocation) > SpawnThreshold)
                {
                    Debug.Log(timestamp);
                    int random = rng.Next(0, Lanes.GetType().GetGenericArguments().Length);
                    switch (random)
                    {
                        case 0:
                            ClonedCoinObj = Instantiate(CoinObj, new Vector3(Lanes.Item1, (float)-3.9, (float)(-CalculatedSpeed * timestamp)), new Quaternion());
                            Debug.Log($"Spawned object {CoinObj} at {-CalculatedSpeed * timestamp} at lane {Lanes.Item1}");
                            break;
                        case 1:
                            ClonedCoinObj = Instantiate(CoinObj, new Vector3(Lanes.Item2, (float)-3.9, (float)(-CalculatedSpeed * timestamp)), new Quaternion());
                            Debug.Log($"Spawned object {CoinObj} at {-CalculatedSpeed * timestamp} at lane {Lanes.Item2}");
                            break;
                        case 2:
                            ClonedCoinObj = Instantiate(CoinObj, new Vector3(Lanes.Item3, (float)-3.9, (float)(-CalculatedSpeed * timestamp)), new Quaternion());
                            Debug.Log($"Spawned object {CoinObj} at {-CalculatedSpeed * timestamp} at lane {Lanes.Item3}");
                            break;
                    }

                    prevNoteLocation = (float)-CalculatedSpeed * timestamp;
                }
            }
            IsWorldBuilt = true;
        }
        stopwatch.Stop();
        Debug.Log($"Map build took {stopwatch.Elapsed}");

    }

    // Start is called before the first frame update
    void Start()
    {

        CoinObj.transform.localScale = new Vector3(CoinScale, CoinScale, CoinScale);
        audioSource = gameObject.GetComponent(typeof(AudioSource)) as AudioSource;
        string[] filePath = StandaloneFileBrowser.OpenFilePanel("Choose a song...", musicFolderLocation, filters, false);
        if (filePath.Length > 0) 
        {
            StartCoroutine(GetAudioClip(filePath[0]));
        }
        else
        {
            Application.Quit();
        }

        stopwatch.Start();
        noteDetectionData = GetNoteTimestamp(filePath[0]);
        Debug.Log($"Note estimation took {stopwatch.Elapsed}");
        stopwatch.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime = audioSource.time;
        if (!IsWorldReady)
        {
            stopwatch.Restart();
            BuildLevel();
            
            if (CalculatedSpeed != 0 && roadLength != 0 && roadSegmentCount != 0)
            {
                IsWorldReady = true;


                Debug.Log($"Building map took {stopwatch.Elapsed}");
                stopwatch.Stop();
            }
            
        }
        
        if (!audioSource.isPlaying && IsWorldReady && !finished)
        {
            audioSource.Play();
           
        }
        if (audioSource.time >= (int)songDuration && audioSource.isPlaying)
        {
            audioSource.Stop();
            finished = true;
        }
    }
    void OnApplicationQuit()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        if (PythonEngine.IsInitialized)
        {
            Debug.Log("Shutting down Embedded Python");
            PythonEngine.Shutdown();
        }
    }
}
