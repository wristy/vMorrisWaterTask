using UnityEngine;

public static class QuadrantUtility
{
    public const int MinQuadrants = 1;
    public const int MaxQuadrants = 10;
    private const float Tau = Mathf.PI * 2f;

    public static int ClampCount(int count)
    {
        return Mathf.Clamp(count, MinQuadrants, MaxQuadrants);
    }

    public static int GetQuadrant(Vector3 position, int quadrantCount)
    {
        quadrantCount = Mathf.Max(MinQuadrants, quadrantCount);

        if (quadrantCount == 1)
        {
            return 1;
        }

        float angle = Mathf.Atan2(position.z, position.x);
        if (angle < 0f)
        {
            angle += Tau;
        }

        float sectorSize = Tau / quadrantCount;
        int index = Mathf.FloorToInt(angle / sectorSize);
        if (index >= quadrantCount)
        {
            index = quadrantCount - 1;
        }

        return index + 1;
    }
}
