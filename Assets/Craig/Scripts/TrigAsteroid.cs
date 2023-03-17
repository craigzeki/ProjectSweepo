using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrigAsteroid : MonoBehaviour
{
    [SerializeField] private ParticleSystem _explosionParticleSystem;
    [SerializeField] private float _destroyDelay;

    private Renderer _myRenderer;
    // Start is called before the first frame update
    void Awake()
    {
        List<ParticleSystem> particleSystems = new List<ParticleSystem>();
        gameObject.GetComponentsInChildren<ParticleSystem>(true, particleSystems);
        foreach(ParticleSystem ps in particleSystems)
        {
            _destroyDelay = _destroyDelay < (ps.main.duration + ps.main.startLifetime.constantMax) ? (ps.main.duration + ps.main.startLifetime.constantMax) : _destroyDelay;
        }

        _myRenderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Missile")
        {
            if(_explosionParticleSystem != null) _explosionParticleSystem.Play();
            StartCoroutine(HideRenderAfter(0.3f));
            Destroy(this.gameObject, _destroyDelay);
            Destroy(collision.gameObject);
            
        }
    }

    IEnumerator HideRenderAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if(_myRenderer != null) { _myRenderer.enabled = false; }
    }
}
