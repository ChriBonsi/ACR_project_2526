using UnityEngine;
using System.Collections;

public class SecurityRobot : Robot
{
    private Vector3 securedLocation = new(9, 1, 0);
    protected override bool HandleSpecialObstacle(GameObject objectHit)
    {
        if (objectHit.CompareTag("UnattendedObstacle"))
        {
            if (Vector3.Distance(transform.position, objectHit.transform.position) < 0.5f)
            {
                if (!isPerformingTask)
                {
                    StartCoroutine(ClearUnattendedRoutine(objectHit));
                }
                obstacleDetected = true;
            }
            return true;
        }

        return false;
    }

    private IEnumerator ClearUnattendedRoutine(GameObject unattended)
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
        if(!isPerformingTask) return;
        if (isPathRequestPending) return;

        if (pathQueue.Count > 0)
        {
            Vector3 target = pathQueue.Peek();
            Move(target);
            CheckIfQueuedPointReached(target);
        }
        else if (Vector3.Distance(transform.position, securedLocation) < 0.5f)
        {
            foreach (Transform child in transform)
            {
                if (child.CompareTag("UnattendedObstacle"))
                {
                    Destroy(child.gameObject);
                }
            }
            
            Debug.Log($"[SecurityRobot {robotId}] Package destroyed at (9,1).");
            
            isPerformingTask = false;
            obstacleDetected = false;
        }
    }
}