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
    private float timeSinceTrialStart = 0f;

    public GameObject player;
    public string positionFileNameBase = "PositionData_trial";
    public string distanceFileNameBase = "DistanceData_trial";

    private float positionLogTimer = 0f;

    private string currentPositionFileName;
    private string currentDistanceFileName;

    public bool enableTotalDistance = true;
    public bool enableCoordinates = true;
    public bool enableSpeed = false;

    private int currentTrialNumber = 0;

    private string expDataDir; // <-- NEW
    private bool isCollecting = false;

    // Path trace support
    private List<Vector2> pathPoints = new List<Vector2>();
    public TreasureChestManager treasureChestManager; // optional, used for chest position
    public int traceImageSize = 1024;
    public int tracePaddingPx = 24;
    public int traceJpgQuality = 90;

    // Quadrant time tracking (Q1: x>=0,z>=0; Q2: x<0,z>=0; Q3: x<0,z<0; Q4: x>=0,z<0)
    private float[] timeInQuadrants = new float[4];

    void Start()
    {
        // Determine correct directory based on platform
#if UNITY_STANDALONE_OSX
        string exeFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
#elif UNITY_STANDALONE_WIN
        string exeFolder = Path.GetDirectoryName(Application.dataPath);
#else
        string exeFolder = Application.dataPath; // fallback
#endif
        expDataDir = Path.Combine(exeFolder, "ExpData");

        if (!Directory.Exists(expDataDir))
            Directory.CreateDirectory(expDataDir);

        Debug.Log("Saving experiment data to: " + expDataDir);

        positionLog = new List<string>();
        enabledColumns = new List<string>();
        pathPoints = new List<Vector2>();

        SetupEnabledColumns();

        lastPosition = player.transform.position;
        if (enableCoordinates)
        {
            LogPosition(lastPosition);
        }

        if (treasureChestManager == null)
        {
            treasureChestManager = FindObjectOfType<TreasureChestManager>();
        }
    }

    void Update()
    {
        if (!isCollecting) return;
        timeSinceTrialStart += Time.deltaTime;

        Vector3 currentPosition = player.transform.position;
        float distanceThisFrame = Vector3.Distance(lastPosition, currentPosition);
        totalDistance += distanceThisFrame;

        // Accumulate time in current quadrant
        int q = GetQuadrant(currentPosition);
        if (q >= 1 && q <= 4)
        {
            timeInQuadrants[q - 1] += Time.deltaTime;
        }

        positionLogTimer += Time.deltaTime;
        if (enableCoordinates && positionLogTimer >= 0.1f)
        {
            LogPosition(currentPosition);
            positionLogTimer = 0f;
        }

        lastPosition = currentPosition;
        totalTimeTaken += Time.deltaTime;
    }

    int GetQuadrant(Vector3 pos)
    {
        if (pos.x >= 0f && pos.z >= 0f) return 1;
        if (pos.x < 0f && pos.z >= 0f) return 2;
        if (pos.x < 0f && pos.z < 0f) return 3;
        return 4;
    }

    void SetupEnabledColumns()
    {
        enabledColumns.Add("time");
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
            string positionEntry = $"{timeSinceTrialStart:F3},{position.x},{position.z}";
            positionLog.Add(positionEntry);
            pathPoints.Add(new Vector2(position.x, position.z));
        }
    }

    public void StartNewTrial(int trialNumber)
    {
        timeSinceTrialStart = 0f;
        totalDistance = 0f;
        totalTimeTaken = 0f;
        numberOfIntersections = 0;
        success = false;
        positionLog.Clear();
        pathPoints.Clear();
        positionLogTimer = 0f;
        for (int i = 0; i < timeInQuadrants.Length; i++) timeInQuadrants[i] = 0f;

        lastPosition = player.transform.position;
        if (enableCoordinates)
        {
            LogPosition(lastPosition);
        }

        currentPositionFileName = $"PositionData_{GameSettings.participantID}.csv";
        currentDistanceFileName = $"DistanceData_{GameSettings.participantID}.csv";

        Debug.Log($"DataCollector initialized for Trial {currentTrialNumber}");
        currentTrialNumber = trialNumber;
        isCollecting = true;
    }

    public void ExportData()
    {
        string fullPath = Path.Combine(expDataDir, currentPositionFileName);
        bool writeHeader = !File.Exists(fullPath);

        using (StreamWriter writer = new StreamWriter(fullPath, append: true))
        {
            if (writeHeader)
            {
                writer.WriteLine("trialNumber," + string.Join(",", enabledColumns));
            }

            foreach (string position in positionLog)
            {
                writer.WriteLine($"{currentTrialNumber},{position}");
            }
        }

        Debug.Log($"Data saved to: {fullPath}");
    }

    public void ExportDistanceData()
    {
        if (!enableTotalDistance) return;

        string fullPath = Path.Combine(expDataDir, currentDistanceFileName);
        bool writeHeader = !File.Exists(fullPath);

        using (StreamWriter writer = new StreamWriter(fullPath, append: true))
        {
            if (writeHeader)
            {
                writer.WriteLine("trialNumber,totalDistance,totalTimeTaken");
            }

            writer.WriteLine($"{currentTrialNumber},{totalDistance},{totalTimeTaken}");
        }

        Debug.Log($"Distance data saved to: {fullPath}");
    }

    public void ExportQuadrantData()
    {
        // Determine chest quadrant
        int chestQuadrant = 0;
        if (treasureChestManager != null)
        {
            Vector3 c = treasureChestManager.ChestPosition;
            if (c != Vector3.zero)
            {
                chestQuadrant = GetQuadrant(c);
            }
        }

        string fileName = $"QuadrantData_{GameSettings.participantID}.csv";
        string fullPath = Path.Combine(expDataDir, fileName);
        bool writeHeader = !File.Exists(fullPath);

        float total = Mathf.Max(0.0001f, totalTimeTaken);
        float prop = (chestQuadrant >= 1 && chestQuadrant <= 4) ? (timeInQuadrants[chestQuadrant - 1] / total) : 0f;

        using (StreamWriter writer = new StreamWriter(fullPath, append: true))
        {
            if (writeHeader)
            {
                writer.WriteLine("trialNumber,chestQuadrant,timeQ1,timeQ2,timeQ3,timeQ4,proportionInChestQuadrant");
            }
            writer.WriteLine($"{currentTrialNumber},{chestQuadrant},{timeInQuadrants[0]:F3},{timeInQuadrants[1]:F3},{timeInQuadrants[2]:F3},{timeInQuadrants[3]:F3},{prop:F4}");
        }

        Debug.Log($"Quadrant data saved to: {fullPath}");
    }

    public void StopCollectionAndExport()
    {
        if (!isCollecting)
        {
            // Avoid duplicate exports
            return;
        }
        isCollecting = false;
        ExportData();
        ExportDistanceData();
        ExportQuadrantData();
        ExportPathTraceImage();
    }

    public void ExportPathTraceImage()
    {
        // Prepare canvas
        int size = Mathf.Max(128, traceImageSize);
        int pad = Mathf.Clamp(tracePaddingPx, 8, size / 4);
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGB24, false);
        Color32 bg = new Color32(255, 255, 255, 255);
        Color32 black = new Color32(0, 0, 0, 255);
        Color32 pathColor = new Color32(30, 144, 255, 255); // dodger blue
        Color32 startColor = new Color32(0, 150, 0, 255);   // green
        Color32 chestColor = new Color32(200, 0, 0, 255);   // red

        // Fill background
        var pixels = tex.GetPixels32();
        for (int i = 0; i < pixels.Length; i++) pixels[i] = bg;
        tex.SetPixels32(pixels);

        // Compute mapping
        float radius = GameSettings.circleRadius;
        // Use the arena center (world origin) as the image center
        Vector2 centerWorld = Vector2.zero;

        float scale = (size * 0.5f - pad) / Mathf.Max(0.001f, radius);
        Vector2 texCenter = new Vector2(size * 0.5f, size * 0.5f);

        // Helpers
        System.Action<int, int, Color32> SetPx = (x, y, c) =>
        {
            if (x >= 0 && x < size && y >= 0 && y < size) tex.SetPixel(x, y, c);
        };

        System.Action<int, int, int, int, Color32> DrawLine = (x0, y0, x1, y1, c) =>
        {
            int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy, e2;
            while (true)
            {
                SetPx(x0, y0, c);
                if (x0 == x1 && y0 == y1) break;
                e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        };

        System.Action<int, int, int, Color32> DrawCircle = (cx, cy, r, c) =>
        {
            int x = r;
            int y = 0;
            int err = 0;
            while (x >= y)
            {
                SetPx(cx + x, cy + y, c);
                SetPx(cx + y, cy + x, c);
                SetPx(cx - y, cy + x, c);
                SetPx(cx - x, cy + y, c);
                SetPx(cx - x, cy - y, c);
                SetPx(cx - y, cy - x, c);
                SetPx(cx + y, cy - x, c);
                SetPx(cx + x, cy - y, c);
                y++;
                if (err <= 0)
                {
                    err += 2 * y + 1;
                }
                if (err > 0)
                {
                    x--;
                    err -= 2 * x + 1;
                }
            }
        };

        // Draw boundary circle (thick)
        int rPx = Mathf.RoundToInt(radius * scale);
        int cxPx = Mathf.RoundToInt(texCenter.x);
        int cyPx = Mathf.RoundToInt(texCenter.y);
        for (int t = -1; t <= 1; t++) DrawCircle(cxPx, cyPx, rPx + t, black);

        // Draw quadrant axes (crosshair) within the circle
        Color32 axisColor = new Color32(180, 180, 180, 255);
        // Horizontal line: from (-radius,0) to (radius,0)
        Vector2 h0 = texCenter + new Vector2(-radius, 0f) * scale;
        Vector2 h1 = texCenter + new Vector2(radius, 0f) * scale;
        DrawLine(Mathf.RoundToInt(h0.x), Mathf.RoundToInt(h0.y), Mathf.RoundToInt(h1.x), Mathf.RoundToInt(h1.y), axisColor);
        // Vertical line: from (0,-radius) to (0,radius)
        Vector2 v0 = texCenter + new Vector2(0f, -radius) * scale;
        Vector2 v1 = texCenter + new Vector2(0f, radius) * scale;
        DrawLine(Mathf.RoundToInt(v0.x), Mathf.RoundToInt(v0.y), Mathf.RoundToInt(v1.x), Mathf.RoundToInt(v1.y), axisColor);

        // Draw path
        if (pathPoints != null && pathPoints.Count > 1)
        {
            for (int i = 1; i < pathPoints.Count; i++)
            {
                Vector2 p0 = pathPoints[i - 1];
                Vector2 p1 = pathPoints[i];
                Vector2 p0Img = texCenter + (p0 - centerWorld) * scale;
                Vector2 p1Img = texCenter + (p1 - centerWorld) * scale;
                DrawLine(Mathf.RoundToInt(p0Img.x), Mathf.RoundToInt(p0Img.y), Mathf.RoundToInt(p1Img.x), Mathf.RoundToInt(p1Img.y), pathColor);
            }
        }

        // Draw start 'O' at first point
        if (pathPoints != null && pathPoints.Count > 0)
        {
            Vector2 p = pathPoints[0];
            Vector2 pImg = texCenter + (p - centerWorld) * scale;
            int ox = Mathf.RoundToInt(pImg.x);
            int oy = Mathf.RoundToInt(pImg.y);
            for (int t = -1; t <= 1; t++) DrawCircle(ox, oy, 8 + t, startColor);
        }

        // Draw chest 'X' at chest position (fallback to last point)
        Vector2 chestWorld;
        if (treasureChestManager != null && treasureChestManager.ChestPosition != Vector3.zero)
            chestWorld = new Vector2(treasureChestManager.ChestPosition.x, treasureChestManager.ChestPosition.z);
        else if (pathPoints != null && pathPoints.Count > 0)
            chestWorld = pathPoints[pathPoints.Count - 1];
        else
            chestWorld = centerWorld;

        Vector2 chestImg = texCenter + (chestWorld - centerWorld) * scale;
        int cx = Mathf.RoundToInt(chestImg.x);
        int cy = Mathf.RoundToInt(chestImg.y);
        int half = 8;
        DrawLine(cx - half, cy - half, cx + half, cy + half, chestColor);
        DrawLine(cx - half, cy + half, cx + half, cy - half, chestColor);

        tex.Apply(false, false);

        string fileName = $"PathTrace_{GameSettings.participantID}_trial{currentTrialNumber}.jpg";
        string fullPath = Path.Combine(expDataDir, fileName);
        byte[] jpg = tex.EncodeToJPG(Mathf.Clamp(traceJpgQuality, 1, 100));
        File.WriteAllBytes(fullPath, jpg);
        Debug.Log($"Path trace image saved to: {fullPath}") ;

        // Cleanup
        Object.Destroy(tex);
    }
}
