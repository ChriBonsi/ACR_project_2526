using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SecurityRobot : Robot
{
    private Vector3 securedLocation = new(9, 1, 0);
    private bool isDestroying = false;
    protected override bool HandleSpecialObstacle(GameObject objectHit)
    {
        if (objectHit.CompareTag("UnattendedObstacle"))
        {
            if (Vector3.Distance(transform.position, objectHit.transform.position) < 0.5f)
            {
                if (!isPerformingTask)
                {
                    StartCoroutine(PickupRoutine(objectHit));
                }
                obstacleDetected = true;
            }
            ReportObstacle(objectHit);
            return true;
        }

        return false;
    }

    private IEnumerator PickupRoutine(GameObject unattended)
    {
        isPerformingTask = true;
        pathQueue.Clear();
        Debug.Log($"[SecurityRobot {robotId}] Clearing unattended obstacle at {unattended.transform.position}...");

        yield return new WaitForSeconds(2f);

        if (unattended != null)
        {
            unattended.transform.SetParent(transform);
            unattended.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        endX = securedLocation.x;
        endY = securedLocation.y;
        SendRequest();
    }

    protected override void UpdateTask()
    {
        if (!isPerformingTask) return;
        if (isPathRequestPending) return;

        if (pathQueue.Count > 0)
        {
            Vector3 target = pathQueue.Peek();
            Move(target);
            CheckIfQueuedPointReached(target);
        }
        else if (Vector3.Distance(transform.position, securedLocation) < 0.5f)
        {
            if (!isDestroying) StartCoroutine(DestroyUnattendedRoutine());
        }
    }

    private IEnumerator DestroyUnattendedRoutine()
    {
        isDestroying = true;

        yield return new WaitForSeconds(2f);
        
        foreach (Transform child in transform)
        {
            if (child.CompareTag("UnattendedObstacle"))
            {
                Destroy(child.gameObject);
            }
        }

        Debug.Log($"[SecurityRobot {robotId}] Package destroyed at {securedLocation}.");

        SetNextDestination();
        isPerformingTask = false;
        obstacleDetected = false;
        isDestroying = false;
    }

    private void SetNextDestination()
    {
        float minDistance = float.MaxValue;
        int closestIndex = 0;
        for (int i = 0; i < destinations.Count; i++)
        {
            float d = Vector3.Distance(transform.position, destinations[i]);
            if (d < minDistance)
            {
                minDistance = d;
                closestIndex = i;
            }
        }
        destinationIndex = closestIndex;
    }
}