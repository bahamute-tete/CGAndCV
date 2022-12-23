using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TriggerForSound : MonoBehaviour
{
    AudioSource audioSource;
    [SerializeField] AudioClip audioClip;
    [SerializeField] ParticleSystem impactEffectPrefab;
    GameObject parciles;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClip;
        parciles = Instantiate(impactEffectPrefab.gameObject, gameObject.transform);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        audioSource.Play();
        parciles.GetComponent<ParticleSystem>().Play();
    }


}
 
