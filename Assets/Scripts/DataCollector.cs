using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataCollector : MonoBehaviour
{
    private float totalDistance = 0f;
    private Vector3 lastPosition;
    private List<string> positionLog;
    private List<string> enabledColumns;
    private int numberOfIntersections = 0;
    private bool success = false;
    private float totalTimeTaken = 0f;

    public GameObject player;
    public string positionFileName = "PositionData.csv";
    public string distanceFileName = "DistanceData.csv";

    private float positionLogTimer = 0f; 

    public bool enableTotalDistance = true;
    public bool enableCoordinates = true;
    public bool enableSpeed = false;         

    void Start()
    {
        positionLog = new List<string>();
        enabledColumns = new List<string>();

        SetupEnabledColumns();

        lastPosition = player.transform.position;
        if (enableCoordinates)
        {
            LogPosition(lastPosition);
        }
        
    }

    void Update()
    {
        Vector3 currentPosition = player.transform.position;
        float distanceThisFrame = Vector3.Distance(lastPosition, currentPosition);
        totalDistance += distanceThisFrame;

        positionLogTimer += Time.deltaTime;
        if (enableCoordinates && positionLogTimer >= 0.1f)
        {
            LogPosition(currentPosition);
            positionLogTimer = 0f;
        }

        lastPosition = currentPosition;
        totalTimeTaken += Time.deltaTime;
    }
    
    void SetupEnabledColumns()
    {
        if (enableCoordinates)
        {
            enabledColumns.Add("PositionX");
            enabledColumns.Add("PositionZ");
        }
        
    }

    void LogPosition(Vector3 position)
    {
        if (enableCoordinates)
        {
            // Log the position in CSV format (x,z)
            string positionEntry = $"{position.x},{position.z}";
            positionLog.Add(positionEntry);
        }
    }

    public void ExportData()
    {
        string fullPath = Path.Combine(Application.dataPath, positionFileName);
        using (StreamWriter writer = new StreamWriter(fullPath))
        {
            writer.WriteLine(string.Join(", ", enabledColumns));

            foreach (string position in positionLog)
            {
                writer.WriteLine(position);
            }

        }
        Debug.Log("Data exported successfully.");
    }

    public void ExportDistanceData()
    {
        if (enableTotalDistance)
        {
            string fullPath = Path.Combine(Application.dataPath, distanceFileName);
         
            using (StreamWriter writer = new StreamWriter(fullPath))
            {
                writer.WriteLine("tot_distance,number_of_intersections,success,total_time_taken");
                writer.WriteLine($"{totalDistance},{numberOfIntersections},{success},{totalTimeTaken}");
            }
            Debug.Log("Distance data exported successfully.");
            }
    }

 void OnApplicationQuit()
    {
        ExportData();
        ExportDistanceData();
    }
}
