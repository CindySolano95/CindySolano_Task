using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowCharacterPortrait : MonoBehaviour
{

    private Transform cameraTransfor;
    private Transform followTransfor;
    [SerializeField] Camera cameraCharacter;
    [SerializeField] Vector2 offset;
    public LayerMask layerToRender;

    private void Awake()
    {
        cameraTransfor = this.gameObject.transform;

        Debug.Log(cameraTransfor);

        cameraCharacter.cullingMask = layerToRender;
    }

    private void Update()
    {
        cameraTransfor.position = new Vector3(followTransfor.position.x + offset.x, followTransfor.position.y + offset.y, Camera.main.transform.position.z);
    
    }

    public void show(Transform followTransform)
    {
        gameObject.SetActive(true);
        this.followTransfor = followTransform;
    }

    public void Hide()
    {

        gameObject.SetActive(false);
    }

}
