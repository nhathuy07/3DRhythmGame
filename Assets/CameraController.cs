using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        offset = new Vector3((float)-2.334748, (float)(-1.59 + 2.77), (float)6.3);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = offset + new Vector3(0, 0, player.transform.position.z);
    }
}
