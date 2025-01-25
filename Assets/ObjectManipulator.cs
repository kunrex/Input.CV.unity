using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManipulator : MonoBehaviour
{
    [SerializeField] private LayerMask layer;

    [SerializeField] private string scaleLabel;
    [SerializeField] private string selectLabel;
    [SerializeField] private string rotationLabel;
    [SerializeField] private string translationLabel;
    
    private Transform current;

    [SerializeField] private Transform camera;
    [SerializeField] private float translateFactor = 10;
    [SerializeField] private float scaleFactor = 0.1f;
    
    void Start()
    {
        InputConnection.Instance.AngleEvent += ProcessAngles;
        InputConnection.Instance.DeltaEvent += ProcessDelta;
        InputConnection.Instance.ScaleEvent += ProcessScale;
        InputConnection.Instance.LabelEvent += ProcessLabel;
    }
    
    private void SelectObject()
    {
        Debug.Log("hello");
        if (current != null)
            current = null;
        else if (Physics.Raycast(transform.position, camera.forward, out var hit, Mathf.Infinity, layer))
        {
            current = hit.transform;
            Debug.Log($"Object {current.gameObject.name} selected");
        }
    }

    private void ProcessAngles(float yaw, float pitch, float roll)
    {
        if (current == null)
            return;

        if (InputConnection.Instance.Label != rotationLabel)
            return;

        var final = Quaternion.Euler(new Vector3(yaw, pitch, roll));
        current.localRotation =  Quaternion.Lerp(current.localRotation, final, Time.deltaTime * 10);
    }

    private void ProcessDelta(Vector3 delta)
    {
        if (current == null)
            return;
        
        if (InputConnection.Instance.Label != translationLabel)
            return;
        
        if (delta.sqrMagnitude > 10)
            current.position = Vector3.Lerp(current.position, current.position + delta * translateFactor, Time.deltaTime);
    }

    private void ProcessScale(float delta)
    {
        if (current == null)
            return;
        
        if (InputConnection.Instance.Label != scaleLabel)
            return;

        current.localScale = Vector3.Lerp(current.localScale, current.localScale + Vector3.one.normalized * delta * scaleFactor, Time.deltaTime);
    }

    private void ProcessLabel(string newLabel)
    {
        Debug.Log(newLabel);
        if(newLabel == selectLabel)
            SelectObject();
    }
}
