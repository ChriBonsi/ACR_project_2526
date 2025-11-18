using UnityEngine;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

public class WaypointConfigGenerator : MonoBehaviour
{
    public GameObject waypointGroup;
    public bool closeLoop = false; // optional: connect first<->last if within distance
    public float maxNeighborDistance = 1.1f; // tune to your waypoint spacing (e.g., 1 unit)

    void Start()
    {
        if (waypointGroup == null)
        {
            Debug.LogWarning("waypointGroup not assigned.");
            return;
        }

        Transform parent = waypointGroup.transform;
        int count = parent.childCount;
        if (count == 0)
        {
            Debug.LogWarning("No children under waypointGroup.");
            return;
        }

        // Collect 2D positions (x,y)
        var positions = new Vector2[count];
        for (int i = 0; i < count; i++)
        {
            var p = parent.GetChild(i).position;
            positions[i] = new Vector2(p.x, p.y);
        }

        // Build adjacency by distance (bidirectional), only immediate neighbors (<= maxNeighborDistance)
        var neighbors = new List<int>[count];
        for (int i = 0; i < count; i++) neighbors[i] = new List<int>();

        for (int i = 0; i < count; i++)
        {
            for (int j = i + 1; j < count; j++)
            {
                float d = Vector2.Distance(positions[i], positions[j]);
                if (d > 0f && d <= maxNeighborDistance)
                {
                    neighbors[i].Add(j);
                    neighbors[j].Add(i);
                }
            }
        }

        // Optional loop only if endpoints are within the threshold
        if (closeLoop && count > 1)
        {
            if (Vector2.Distance(positions[0], positions[count - 1]) <= maxNeighborDistance)
            {
                neighbors[0].Add(count - 1);
                neighbors[count - 1].Add(0);
            }
        }

        // Sort neighbor lists for stable YAML
        for (int i = 0; i < count; i++) neighbors[i].Sort();

        var sb = new StringBuilder();
        sb.AppendLine("nodes:");
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = positions[i];
            sb.AppendLine($"  - id: {i}");
            sb.AppendLine($"    x: {pos.x.ToString(CultureInfo.InvariantCulture)}");
            sb.AppendLine($"    y: {pos.y.ToString(CultureInfo.InvariantCulture)}");
        }

        sb.AppendLine("adjacency:");
        for (int i = 0; i < count; i++)
        {
            if (neighbors[i].Count == 0)
            {
                sb.AppendLine($"  {i}: []");
            }
            else
            {
                sb.AppendLine($"  {i}: [{string.Join(", ", neighbors[i])}]");
            }
        }

        string path = Path.Combine(Application.persistentDataPath, "waypoints.yaml");
        File.WriteAllText(path, sb.ToString());
        Debug.Log($"Waypoint YAML written to: {path}");
    }

    void Update() { }
}