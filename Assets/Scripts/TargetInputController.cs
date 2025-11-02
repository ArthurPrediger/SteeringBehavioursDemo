using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TargetInputController : MonoBehaviour
{
    private GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        target = gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetMouseButton((int)MouseButton.Right)) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000f, LayerMask.GetMask("Ground")))
        {
            target.transform.position = hitInfo.point + Vector3.up * target.GetComponent<Renderer>().bounds.extents.y;
        }
    }
}
