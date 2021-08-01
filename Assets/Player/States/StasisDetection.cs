using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StasisDetection : MonoBehaviour, IState
{
    [Header("Stasis Attributes")]
    public float stasisSeconds;
    public Material stasisMaterial;

    [Header("Fill-ins")]
    public Transform playerHead;
    public GameObject stasisReticle;
    public List<ParticleSystem> stasisStartParticles;
    public List<ParticleSystem> stasisEndParticles;

    Camera MainCamera;
    Animator animator;
    StateMachine SM;

    RaycastHit hit;
    [SerializeField] [Tooltip("Layer mask for what stasis should interact with. (Checked items are stasis-able)")]
    LayerMask layerMask;


    void Start() {
        SM = GetComponent<PlayerController>().SM;
        MainCamera = Camera.main;
    }

    public void OnActivate() {
        stasisReticle.SetActive(true); // bring up stasis reticle ui
        animator = GetComponent<Animator>();
        if (animator != null) {
            animator.SetTrigger("Rune"); // set "Rune" trigger which starts stasis animation
        }
    }
    
    public void OnUpdate()
    {
        if (Physics.Raycast(MainCamera.transform.position, MainCamera.transform.forward, out hit, Mathf.Infinity, layerMask)) {
            StartCoroutine(highlightObject());
        } 
        Debug.DrawLine(MainCamera.transform.position, MainCamera.transform.forward * 50.0f, Color.blue);
    }

    IEnumerator highlightObject() {
        if (hit.collider != null) {
            Material mat = hit.collider.gameObject.GetComponent<Renderer>().material;
            Color prev_color = mat.color;
            while (Physics.Raycast(MainCamera.transform.position, MainCamera.transform.forward, Mathf.Infinity, layerMask) && SM.getCurrState() is StasisDetection) {
                mat.color = Color.yellow;
                yield return null;
            }
            mat.color = prev_color;
        }
    }

    // called on an animation event
    public void StasisActivate() {

        if (hit.collider != null) {
            Debug.Log("Ray hit: " + hit.collider.gameObject.name);
            StartCoroutine(EngageStasis(hit.collider.gameObject));
        }
        else {
            Debug.Log("Didn't hit raycast");
        }

        SM.SetState<Movement>();
    }

    IEnumerator EngageStasis(GameObject obj) {
        Renderer obj_rend = obj.GetComponent<Renderer>();
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        Material prev = obj_rend.material;

        if (stasisStartParticles.Count != 0) { // if there are any particle systems in the list
            foreach (ParticleSystem PS in stasisStartParticles) {
                PS.gameObject.transform.position = obj.transform.position;
                PS.Play();
            }
        }

        // Checking these individually looks icky, but if an object has *either* an rb or a renderer, then it'll handle only that part properly

        if (obj_rend != null) 
            obj_rend.material = stasisMaterial;
        if (rb != null)
            rb.isKinematic = true;

        yield return new WaitForSeconds(stasisSeconds);

        if (rb != null)
            rb.isKinematic = false;
        if (obj_rend != null)
            obj_rend.material = prev;
        
        if (stasisStartParticles.Count != 0) {
            foreach (ParticleSystem PS in stasisStartParticles) {
                PS.Stop();
            }
        }
        if (stasisEndParticles.Count != 0)
        {
            foreach(ParticleSystem PS in stasisEndParticles)
            {
                PS.gameObject.transform.position = obj.transform.position;
                PS.Play();
            }
        }
        
    }



    public void OnEnd() {
        animator.ResetTrigger("Rune");
        stasisReticle.SetActive(false);
    }

}
