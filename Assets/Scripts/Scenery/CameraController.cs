using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private static GameObject _gameObject;

    [SerializeField] private Camera mainCamera;
    private float minZoomNumber = 3f;
    private float maxZoomNumber = 10f;

    private Vector3 offset;
    private Vector3 previousMousePosition;
    private Vector3 nextCameraPosition;

    private void OnEnable()
    {
        PlayerController.OnPlayerCharacterChanged += (_) => CenterCameraOnPlayer();
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerCharacterChanged -= (_) => CenterCameraOnPlayer();
    }

    private void Awake()
    {
        _gameObject = gameObject;
    }

    private void Update()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            CameraZoom();
        }
        else if (Input.GetMouseButtonDown(2))
        {
            previousMousePosition = GetMouseWorldPosition();
            offset = transform.position - previousMousePosition;
        }
        else if (Input.GetMouseButton(2))
        {
            if (Vector3.Distance(GetMouseWorldPosition(), previousMousePosition) >= 0.25f)
            {
                nextCameraPosition = GetMouseWorldPosition() + offset;
                previousMousePosition = GetMouseWorldPosition();
            }
            if (nextCameraPosition != transform.position)
            {
                transform.position = Vector3.Lerp(transform.position, nextCameraPosition, Time.deltaTime);
            }
        }
    }

    public static void CenterCameraOnPlayer()
    {
        _gameObject.transform.position = PlayerController.PC.transform.position;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Physics.Raycast(ray, out RaycastHit hit);
        Vector3 newPosition = new(hit.point.x, 0f, hit.point.z);
        return newPosition;
    }

    private void CameraZoom()
    {
        mainCamera.orthographicSize -= Input.mouseScrollDelta.y * 0.25f;
        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minZoomNumber, maxZoomNumber);
    }

    public void RotateClockwise()
    {
        transform.Rotate(new(0f,-90f,0f), Space.World);
    }

    public void RotateCounterclockwise()
    {
        transform.Rotate(new(0f,90f,0f), Space.World);
    }
}