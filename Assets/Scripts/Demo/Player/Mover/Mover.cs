using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class Mover : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navMesh;
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float maxRayDistance = 1000f;
    [SerializeField] private float navMeshSampleDistance = 2f;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        TryMoveToMouseClick();
    }
    #region Movement
    private void TryMoveToMouseClick()
    {
        if (navMesh == null || Mouse.current == null)
        {
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }

        if (!Mouse.current.rightButton.wasPressedThisFrame)
        {
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, groundMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            Plane fallbackPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
            if (!fallbackPlane.Raycast(ray, out float enter))
            {
                return;
            }

            targetPoint = ray.GetPoint(enter);
        }

        if (NavMesh.SamplePosition(targetPoint, out NavMeshHit navHit, navMeshSampleDistance, NavMesh.AllAreas))
        {
            navMesh.SetDestination(navHit.position);
        }
    }
    #endregion
}
