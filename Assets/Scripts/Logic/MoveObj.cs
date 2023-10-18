using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MoveObj : MonoBehaviour
{
    private Animator animator;
    public float speed;
    private List<Vector2Int> path;
    private int currentPos;

    private Vector3 targetPos;
    public int currentWayPoint;
    public int previousWayPoint;
    public float nextWaypointDistance = 0.1f;
    public Vector3 currentDirection;
    public Vector3 previouDirection;
    public bool changeDirection;
    private List<Vector2Int> _path = new List<Vector2Int>();

    public List<Vector2Int> Path()
    {
        return _path;
    }

    public int GetWaypointCount() { return _path.Count; }
    public void SetPath(List<Vector2Int> path)
    {
        _path.Clear();
        _path.AddRange(path);
        currentWayPoint = 1;
        previousWayPoint = 0;

        transform.position = new Vector3(path[0].x, path[0].y, 0);
        if (animator == null) {
            animator = GetComponentInChildren<Animator>();
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (_path == null || _path.Count == 0)
        {
            return;
        }

        if (currentWayPoint >= _path.Count)
        {
            OnEndPath();
            return;
        }

        UpdateMove(Time.deltaTime);
    }

    private void OnEndPath()
    {
        ObjectPoolManager.Unspawn(gameObject);
    }

    public void UpdateMove(float deltaTime)
    {
        if (_path == null || _path.Count == 0)
        {
            return;
        }

        if (currentWayPoint >= _path.Count)
            return;


        var curPOint = GetWorldPosInPath(currentWayPoint);
        if (previousWayPoint >= 0)
        {
            var prePoint = GetWorldPosInPath(previousWayPoint);
            Vector3 newDirection = curPOint - prePoint;
            if (newDirection != currentDirection)
            {
                previouDirection = currentDirection;
                currentDirection = newDirection;
                changeDirection = true;
            }
            else
            {
                changeDirection = false;
            }
        }
      
        if (changeDirection)
        {
            if(currentDirection.x < 0)
            {
                animator.SetTrigger(AnimID.leftAnimID);
            }
            else if (currentDirection.x > 0)
            {
                animator.SetTrigger(AnimID.rightAnimID);
            }
            else if (currentDirection.y < 0)
            {
                animator.SetTrigger(AnimID.downAnimID);
            }
            else if (currentDirection.y > 0)
            {
                animator.SetTrigger(AnimID.upAnimID);
            }
        }
        Move(curPOint, deltaTime);
        if (Vector3.Distance(transform.position, curPOint) < nextWaypointDistance)
        {
            previousWayPoint = currentWayPoint;
            currentWayPoint++;
        }
    }

    private void Move(Vector3 curPOint, float deltaTime)
    {
       var dir = ( curPOint - transform.position ).normalized;
        transform.position += dir * speed * deltaTime;
    }

    public Vector3 GetWorldPosInPath(int idx)
    {
        Vector2Int p = Vector2Int.zero;
        if (idx >= _path.Count)
        {
            p = _path[_path.Count - 1];
        }
        else if (idx < 0)
        {
            p = _path[0];
        }
        else
        {
            p = _path[idx];
        }
        return new Vector3(p.x, p.y, 0);
    }
}

public static class AnimID
{
    public static int upAnimID = Animator.StringToHash("mUp");
    public static int downAnimID = Animator.StringToHash("mDown");
    public static int leftAnimID = Animator.StringToHash("mLeft");
    public static int rightAnimID = Animator.StringToHash("mRight");
}
