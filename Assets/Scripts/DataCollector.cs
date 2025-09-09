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
    private string currentTrialSummaryFileName;

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

    // Fractal complexity tuning
    // Maximum grid resolution (power of two) used in box-counting (e.g., 256 => smallest cell ~ (2R/256)).
    public int fractalMaxGrid = 256;
    // Optional resampling step (world units). If 0, auto = max((2R)/fractalMaxGrid, R/512).
    public float fractalResampleStep = 0f;

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
            enabledColumns.Add("HeadingDeg");
            enabledColumns.Add("DistanceToChest");
        }
    }

    void LogPosition(Vector3 position)
    {
        if (enableCoordinates)
        {
            // Heading bearing (degrees) from Unity transform orientation (yaw from transform.forward)
            string headingStr = GetPlayerHeadingDeg().ToString("F3");

            // Compute distance to chest in XZ plane
            string distChestStr = "NaN";
            if (treasureChestManager != null)
            {
                Vector3 c = treasureChestManager.ChestPosition;
                if (c != Vector3.zero)
                {
                    float d = Mathf.Sqrt((position.x - c.x) * (position.x - c.x) + (position.z - c.z) * (position.z - c.z));
                    distChestStr = d.ToString("F3");
                }
            }

            string positionEntry = $"{timeSinceTrialStart:F3},{position.x},{position.z},{headingStr},{distChestStr}";
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

        // Do not log yet; wait until player is repositioned by GameManager
        lastPosition = player.transform.position;

        currentPositionFileName = $"PositionData_{GameSettings.participantID}.csv";
        currentDistanceFileName = $"DistanceData_{GameSettings.participantID}.csv";
        currentTrialSummaryFileName = $"TrialData_{GameSettings.participantID}.csv";

        currentTrialNumber = trialNumber;
        Debug.Log($"DataCollector initialized for Trial {currentTrialNumber}");
        // Defer collection until the player is reset to the start position
        isCollecting = false;
    }

    // Call this AFTER GameManager resets the player's position at trial start.
    public void InitializeTrialStartingPosition()
    {
        Vector3 pos = player.transform.position;
        lastPosition = pos;
        // Force the first sample at time 0 to the correct start position with heading from transform
        timeSinceTrialStart = 0f;
        if (enableCoordinates)
        {
            string headingStr = GetPlayerHeadingDeg().ToString("F3");
            string distChestStr = "NaN";
            if (treasureChestManager != null)
            {
                Vector3 c = treasureChestManager.ChestPosition;
                if (c != Vector3.zero)
                {
                    float d = Mathf.Sqrt((pos.x - c.x) * (pos.x - c.x) + (pos.z - c.z) * (pos.z - c.z));
                    distChestStr = d.ToString("F3");
                }
            }
            string entry = $"{0f:F3},{pos.x},{pos.z},{headingStr},{distChestStr}";
            if (positionLog.Count == 0)
            {
                positionLog.Add(entry);
            }
            else
            {
                // Replace any premature sample if present
                positionLog[0] = entry;
            }
            if (pathPoints.Count == 0)
                pathPoints.Add(new Vector2(pos.x, pos.z));
            else
                pathPoints[0] = new Vector2(pos.x, pos.z);
        }
        totalTimeTaken = 0f;
        isCollecting = true;
    }

    private float GetPlayerHeadingDeg()
    {
        if (player == null) return float.NaN;
        Vector3 fwd = player.transform.forward;
        // Project to XZ plane in case of tilt
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 1e-12f) return float.NaN;
        fwd.Normalize();
        float headingDeg = Mathf.Atan2(fwd.z, fwd.x) * Mathf.Rad2Deg; // 0 deg along +X, 90 deg along +Z
        if (headingDeg < 0f) headingDeg += 360f;
        return headingDeg;
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
        ExportTrialSummaryData();
        ExportPathTraceImage();
    }

    public void ExportTrialSummaryData()
    {
        // Determine chest quadrant first
        int chestQuadrant = 0;
        if (treasureChestManager != null)
        {
            Vector3 c = treasureChestManager.ChestPosition;
            if (c != Vector3.zero)
            {
                chestQuadrant = GetQuadrant(c);
            }
        }

        string fullPath = Path.Combine(expDataDir, currentTrialSummaryFileName);
        bool writeHeader = !File.Exists(fullPath);

        float total = Mathf.Max(0.0001f, totalTimeTaken);
        float prop = (chestQuadrant >= 1 && chestQuadrant <= 4) ? (timeInQuadrants[chestQuadrant - 1] / total) : 0f;

        using (StreamWriter writer = new StreamWriter(fullPath, append: true))
        {
            if (writeHeader)
            {
                writer.WriteLine("trialNumber,totalDistance,totalTimeTaken,chestQuadrant,timeQ1,timeQ2,timeQ3,timeQ4,proportionInChestQuadrant,fractalDimensionBox,straightnessIndex,tortuosityRadPerMeter,angularEntropyBits,crossings");
            }
            float fd = EstimateFractalDimensionBoxCounting(pathPoints, GameSettings.circleRadius);
            string fdStr = float.IsNaN(fd) ? "NaN" : fd.ToString("F4");
            float si = ComputeStraightnessIndex(pathPoints);
            string siStr = float.IsNaN(si) ? "NaN" : si.ToString("F4");
            float tort = ComputeTortuosity(pathPoints);
            string tortStr = float.IsNaN(tort) ? "NaN" : tort.ToString("F4");
            float angEnt = ComputeAngularEntropy(pathPoints, 18);
            string angEntStr = float.IsNaN(angEnt) ? "NaN" : angEnt.ToString("F4");
            int crossings = CountSelfCrossings(pathPoints);
            writer.WriteLine($"{currentTrialNumber},{totalDistance},{totalTimeTaken},{chestQuadrant},{timeInQuadrants[0]:F3},{timeInQuadrants[1]:F3},{timeInQuadrants[2]:F3},{timeInQuadrants[3]:F3},{prop:F4},{fdStr},{siStr},{tortStr},{angEntStr},{crossings}");
        }

        Debug.Log($"Trial summary data saved to: {fullPath}");
    }

    // Estimates the planar fractal dimension of the trajectory via box-counting.
    // Uses grids with subdivisions per axis: 2,4,8,... and counts occupied boxes.
    // Returns NaN if insufficient data.
    private float EstimateFractalDimensionBoxCounting(List<Vector2> pts, float arenaRadius)
    {
        if (pts == null || pts.Count < 2) return float.NaN;
        float R = Mathf.Max(0.001f, arenaRadius);

        // Compute total path length to detect degenerate paths
        float pathLen = 0f;
        for (int i = 1; i < pts.Count; i++) pathLen += Vector2.Distance(pts[i - 1], pts[i]);
        if (pathLen < 1e-3f) return float.NaN; // essentially stationary

        // Build a densified copy of the path for finer coverage
        float dsAuto = Mathf.Max((2f * R) / Mathf.Max(4, Mathf.NextPowerOfTwo(Mathf.Clamp(fractalMaxGrid, 4, 1024))), R / 512f);
        float ds = (fractalResampleStep > 0f) ? fractalResampleStep : dsAuto;
        List<Vector2> dense = new List<Vector2>(pts.Count * 2);
        dense.Add(pts[0]);
        const int maxDense = 50000;
        for (int i = 1; i < pts.Count; i++)
        {
            Vector2 a = dense[dense.Count - 1];
            Vector2 b = pts[i];
            float segLen = Vector2.Distance(a, b);
            if (segLen <= 1e-6f)
            {
                continue;
            }
            int steps = Mathf.Clamp(Mathf.FloorToInt(segLen / ds), 0, 10000);
            if (steps > 0)
            {
                Vector2 dir = (b - a) / (steps + 1);
                for (int s = 1; s <= steps; s++)
                {
                    dense.Add(a + dir * s);
                    if (dense.Count >= maxDense) break;
                }
                if (dense.Count >= maxDense) break;
            }
            dense.Add(b);
            if (dense.Count >= maxDense) break;
        }

        // Grid subdivisions (powers of two) up to configured max
        int maxM = Mathf.NextPowerOfTwo(Mathf.Clamp(fractalMaxGrid, 4, 1024));
        List<int> ms = new List<int>();
        for (int m = 4; m <= maxM; m <<= 1) ms.Add(m);

        List<float> xs = new List<float>(); // log(m)
        List<float> ys = new List<float>(); // log(N)

        foreach (int m in ms)
        {
            float cell = (2f * R) / m;
            if (cell <= 0f) continue;

            var occupied = new HashSet<long>();

            // Mark cells traversed by each polyline segment using Bresenham in grid space
            for (int i = 1; i < dense.Count; i++)
            {
                Vector2 a = dense[i - 1];
                Vector2 b = dense[i];

                // Map to grid indices
                int ax = Mathf.Clamp(Mathf.FloorToInt((a.x + R) / cell), 0, m - 1);
                int ay = Mathf.Clamp(Mathf.FloorToInt((a.y + R) / cell), 0, m - 1);
                int bx = Mathf.Clamp(Mathf.FloorToInt((b.x + R) / cell), 0, m - 1);
                int by = Mathf.Clamp(Mathf.FloorToInt((b.y + R) / cell), 0, m - 1);

                MarkLineCells(ax, ay, bx, by, occupied);
            }

            int N = occupied.Count;
            // Skip degenerate scales (no occupancy or full saturation)
            if (N <= 0 || N >= m * m) continue;

            xs.Add(Mathf.Log(m));
            ys.Add(Mathf.Log((float)N));
        }

        int n = xs.Count;
        if (n < 3) return float.NaN;

        // Least-squares slope
        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
        for (int i = 0; i < n; i++)
        {
            double x = xs[i];
            double y = ys[i];
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
        }
        double denom = n * sumX2 - sumX * sumX;
        if (denom <= 1e-12) return float.NaN;
        double slope = (n * sumXY - sumX * sumY) / denom;
        
        float D = (float)slope;
        if (float.IsNaN(D)) return float.NaN;
        return D;
    }

    private void MarkLineCells(int x0, int y0, int x1, int y1, HashSet<long> occ)
    {
        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy, e2;
        while (true)
        {
            occ.Add((((long)x0) << 32) ^ (uint)y0);
            if (x0 == x1 && y0 == y1) break;
            e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    private float ComputePathLength(List<Vector2> pts)
    {
        if (pts == null || pts.Count < 2) return 0f;
        float len = 0f;
        for (int i = 1; i < pts.Count; i++) len += Vector2.Distance(pts[i - 1], pts[i]);
        return len;
    }

    private float ComputeStraightnessIndex(List<Vector2> pts)
    {
        if (pts == null || pts.Count < 2) return float.NaN;
        float pathLen = ComputePathLength(pts);
        if (pathLen <= 1e-6f) return float.NaN;
        float disp = Vector2.Distance(pts[0], pts[pts.Count - 1]);
        float si = disp / pathLen;
        return Mathf.Clamp01(si);
    }

    private float ComputeTortuosity(List<Vector2> pts)
    {
        if (pts == null || pts.Count < 3) return float.NaN;
        float pathLen = ComputePathLength(pts);
        if (pathLen <= 1e-6f) return float.NaN;
        // headings per segment
        List<float> headings = new List<float>(pts.Count - 1);
        for (int i = 1; i < pts.Count; i++)
        {
            Vector2 d = pts[i] - pts[i - 1];
            if (d.sqrMagnitude < 1e-10f) continue; // skip near-zero step
            float h = Mathf.Atan2(d.y, d.x); // radians
            headings.Add(h);
        }
        if (headings.Count < 2) return float.NaN;
        float totalTurn = 0f;
        for (int i = 1; i < headings.Count; i++)
        {
            float dh = AngleDiffRad(headings[i - 1], headings[i]);
            totalTurn += Mathf.Abs(dh);
        }
        return totalTurn / pathLen; // rad per meter
    }

    private float AngleDiffRad(float a, float b)
    {
        float d = b - a;
        while (d > Mathf.PI) d -= 2f * Mathf.PI;
        while (d < -Mathf.PI) d += 2f * Mathf.PI;
        return d;
    }

    private float ComputeAngularEntropy(List<Vector2> pts, int bins)
    {
        if (pts == null || pts.Count < 3) return float.NaN;
        if (bins <= 1) bins = 12;
        int[] hist = new int[bins];
        int segCount = 0;
        for (int i = 1; i < pts.Count; i++)
        {
            Vector2 d = pts[i] - pts[i - 1];
            if (d.sqrMagnitude < 1e-10f) continue;
            float h = Mathf.Atan2(d.y, d.x); // [-pi, pi]
            float norm = (h + Mathf.PI) / (2f * Mathf.PI); // [0,1)
            int bin = Mathf.Clamp(Mathf.FloorToInt(norm * bins), 0, bins - 1);
            hist[bin]++;
            segCount++;
        }
        if (segCount == 0) return float.NaN;
        float H = 0f;
        for (int i = 0; i < bins; i++)
        {
            if (hist[i] == 0) continue;
            float p = (float)hist[i] / segCount;
            H -= p * Mathf.Log(p, 2f);
        }
        return H; // bits
    }

    private int CountSelfCrossings(List<Vector2> pts)
    {
        if (pts == null || pts.Count < 4) return 0;
        int m = pts.Count - 1; // segments
        int crossings = 0;
        for (int i = 0; i < m; i++)
        {
            Vector2 a1 = pts[i];
            Vector2 a2 = pts[i + 1];
            for (int j = i + 2; j < m; j++)
            {
                // Skip adjacent segments and shared endpoints
                if (j == i) continue;
                if (j == i + 1) continue;
                Vector2 b1 = pts[j];
                Vector2 b2 = pts[j + 1];
                if (SegmentsIntersectProper(a1, a2, b1, b2)) crossings++;
            }
        }
        return crossings;
    }

    private bool SegmentsIntersectProper(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        // Proper intersection excluding collinear-overlap and endpoints
        float o1 = Orient(p1, p2, q1);
        float o2 = Orient(p1, p2, q2);
        float o3 = Orient(q1, q2, p1);
        float o4 = Orient(q1, q2, p2);
        if (o1 == 0 || o2 == 0 || o3 == 0 || o4 == 0)
        {
            // handle near-collinear/endpoints with epsilon: treat as non-crossing
            const float eps = 1e-6f;
            if (Mathf.Abs(o1) < eps || Mathf.Abs(o2) < eps || Mathf.Abs(o3) < eps || Mathf.Abs(o4) < eps)
                return false;
        }
        return (o1 > 0f) != (o2 > 0f) && (o3 > 0f) != (o4 > 0f);
    }

    private float Orient(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
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
        Debug.Log($"Path trace image saved to: {fullPath}");

        // Cleanup
        Object.Destroy(tex);
    }
}
