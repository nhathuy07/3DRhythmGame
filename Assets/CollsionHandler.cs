using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollsionHandler : MonoBehaviour
{
    public GameObject player;
    private ParticleSystem hitFx;
    // Start is called before the first frame update


    // Update is called once per frame
    void Start()
    {
        player = GameObject.Find("lowpoly_car");
        print(player);
        hitFx = player.transform.Find("Cube/HitEffect").gameObject.GetComponent<ParticleSystem>();
        if (hitFx == null)
        {
            Debug.LogError("Couldn't find HitEffect particle");
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.parent.gameObject == player)
        {
            hitFx.Clear();
            hitFx.Emit(1000);
        };

    }
}
