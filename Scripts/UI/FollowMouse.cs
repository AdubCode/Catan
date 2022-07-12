using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    public GameObject trackingObject;

    public Vector3 startPosition;

    public bool isCard = true;

    public bool isTrackingUI = false;

    private bool isReturning = false;

    void Start()
    {
        Float fx = GetComponent<Float>();
        if (fx != null)
        {
            fx.enabled = false;
        }
    }

    public void Cancel()
    {
        isReturning = true;
    }

    public void Remove()
    {
        Destroy(gameObject);
    }

    void Update()
    {
        if (trackingObject == null) return;

        if (isReturning)
        {
            transform.position = Vector3.Lerp(transform.position, startPosition, Time.deltaTime * 10f);
            Vector3 dist = transform.position - startPosition;
            if (dist.magnitude < (isTrackingUI ? 20f : 0.5f))
            {
                Remove();
            }
            return;
        }

        Vector3 pos = Vector3.zero;

        if (isTrackingUI)
        {
            pos = Input.mousePosition;
        }
        else
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Camera.main.transform.position.y - (trackingObject.transform.position.y + trackingObject.transform.lossyScale.y) + (isCard ? 0f : 1f);

            pos = Camera.main.ScreenToWorldPoint(mousePosition);
            pos.y = 1f;
        }

        trackingObject.transform.position = Vector3.Lerp(trackingObject.transform.position, pos, Time.deltaTime * 10f);
    }

}
