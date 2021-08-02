using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;
using UnityEngine.UI;


public class Stasis : MonoBehaviour, IState
{
    [Header("Stasis Attributes")]
    public float stasisSeconds;
    public Material stasisMaterial;
    public float zoomInAmount = 1.3f;
    public float zoomDuration = 1.0f;

    [Header("Fill-ins")]
    public Transform playerHead;
    public GameObject stasisReticle;
    public List<ParticleSystem> stasisStartParticles;
    public List<ParticleSystem> stasisEndParticles;
    public CinemachineFreeLook cinemachineCam;

    Camera MainCamera;
    Animator animator;
    StateMachine SM;
    CinemachineCameraOffset cinemachineCameraOffset;
    bool isOnCooldown = false;

    RaycastHit hit;
    IEnumerator hightlightObjectCoroutine;
    [SerializeField] [Tooltip("Layer mask for what stasis should interact with. (Checked items are stasis-able)")]
    LayerMask layerMask;

    
    void Start() {
        SM = GetComponent<PlayerController>().SM;
        MainCamera = Camera.main;
        cinemachineCameraOffset = cinemachineCam.GetComponent<CinemachineCameraOffset>();
    }

    public void OnActivate() {
        if (isOnCooldown) {
            stasisReticle.GetComponent<Image>().color = Color.red;
		}
        stasisReticle.SetActive(true); // bring up stasis reticle ui
        animator = GetComponent<Animator>();
        animator.SetFloat("Speed", 0.0f);
        tweenCamAim(zoomInAmount, zoomDuration);
    }

    bool shouldCheckStasisRay = true;
    public void OnUpdate()
    {
        if (!isOnCooldown) {
            if (hightlightObjectCoroutine == null && shouldCheckStasisRay && Physics.Raycast(MainCamera.transform.position, MainCamera.transform.forward, out hit, Mathf.Infinity, layerMask)) {
                hightlightObjectCoroutine = highlightObject(hit.collider.gameObject);
            }

            if (hightlightObjectCoroutine != null) {
                shouldCheckStasisRay = false;
                StartCoroutine(hightlightObjectCoroutine);
                hightlightObjectCoroutine = null;
            }
        }

        if (!Input.GetMouseButton(1)) {
            StasisActivate();
            SM.SetState<Movement>();
        }

    }

    IEnumerator highlightObject(GameObject obj) {
        if (obj != null) {
            Material mat = obj.GetComponent<Renderer>().material;
            if (mat.HasColor("_Color")) { // if the material has a "Main Color"
                Color prev_color = mat.color;
                mat.color = Color.yellow;
                while (Physics.Raycast(MainCamera.transform.position, MainCamera.transform.forward, Mathf.Infinity, layerMask) && SM.getCurrState() is Stasis) {
                    yield return null;
                }
                shouldCheckStasisRay = true; // once you aim off of the object, use the raycast from OnUpdate
                mat.color = prev_color;
            }
        }
    }

    public void StasisActivate() {
        if (hit.collider != null) {
            StartCoroutine(ApplyStasis(hit.collider.gameObject));
        }
    }

    IEnumerator ApplyStasis(GameObject obj) {
        Renderer obj_rend = obj.GetComponent<Renderer>();
        if (obj_rend.sharedMaterial.shader.name == stasisMaterial.shader.name) yield break;
        isOnCooldown = true;

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
        if (stasisEndParticles.Count != 0) {
            foreach(ParticleSystem PS in stasisEndParticles)
            {
                PS.gameObject.transform.position = obj.transform.position;
                PS.Play();
            }
        }
        
    }

    // tween cinemachine camera zoom to simulate aiming over shoulder
    void tweenCamAim(float zAmount, float tweenDuration) {
        Vector3 camOffset = cinemachineCameraOffset.m_Offset;
        DOTween.To(() => cinemachineCameraOffset.m_Offset, x => cinemachineCameraOffset.m_Offset = x, new Vector3(camOffset.x, camOffset.y, zAmount), tweenDuration);
    }

    public void OnEnd() {
        stasisReticle.SetActive(false);
        tweenCamAim(0.0f, zoomDuration);
        if (isOnCooldown)
            StartCoroutine(startCooldown());
    }

    IEnumerator startCooldown() {
        yield return new WaitForSeconds(stasisSeconds);
        isOnCooldown = false;
        stasisReticle.GetComponent<Image>().color = Color.white;
    }
}
