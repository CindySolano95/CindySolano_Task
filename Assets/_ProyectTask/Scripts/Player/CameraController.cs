using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    [SerializeField] float smoothing;
    [SerializeField] Vector2 minPos, maxPos;
    [SerializeField] private WindowCharacterPortrait windowCharacterPortrait;


    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindWithTag("Player").transform;
        windowCharacterPortrait.show(target);
    }

    // Update is called once per frame
    void Update()
    {

        //transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);

        if (transform.position != target.position)
        {
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);

            targetPosition.x = Mathf.Clamp(targetPosition.x, minPos.x, maxPos.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minPos.y, maxPos.y);

            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing);
        }
    }
}
