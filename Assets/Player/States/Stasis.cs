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
            if (!isOnCooldown)
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
                while (Physics.Raycast(MainCamera.transform.position, MainCamera.transform.forward, out hit, Mathf.Infinity, layerMask) && SM.getCurrState() is Stasis) {
                    if (hit.collider != null && hit.collider.gameObject != obj) { // if we've hovered over a different gameobject while looking at the current one
                        mat.color = prev_color;
                        shouldCheckStasisRay = true;
                        yield break;
                    }
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

    List<Material> tmp_material_list = new List<Material>(); // use list so we only allocate this once. Material is a class (reference type) so doing array stuff here is pretty fast

    IEnumerator ApplyStasis(GameObject obj) {
        isOnCooldown = true;

        Renderer obj_rend = obj.GetComponent<Renderer>();
        Rigidbody rb = obj.GetComponent<Rigidbody>();

        if (obj_rend == null || rb == null) {
            Debug.Log("Object is not suitable for stasis!");
            yield break;
        }

        obj_rend.GetMaterials(tmp_material_list); // populate list with materials

        handleParticleSystems(stasisStartParticles, obj, true);

        // -------- Checking these individually looks icky, but if an object has *either* an rb or a renderer, then it'll handle only that part properly

        tmp_material_list.Add(stasisMaterial);
        obj_rend.materials = tmp_material_list.ToArray();
        rb.isKinematic = true;

        yield return new WaitForSeconds(stasisSeconds);

        rb.isKinematic = false;
        tmp_material_list.RemoveAt(tmp_material_list.Count -1); // pop from back
        obj_rend.materials = tmp_material_list.ToArray();

        // --------------------------------------------------
        
        handleParticleSystems(stasisStartParticles, obj, false);
        handleParticleSystems(stasisEndParticles, obj, true);
        tmp_material_list.Clear();
        
    }

    void handleParticleSystems(List<ParticleSystem> particleSystems, GameObject source, bool on_off) {
        if (particleSystems.Count != 0) {
            foreach(ParticleSystem PS in particleSystems)
            {
                if (on_off) {
                    PS.gameObject.transform.position = source.transform.position;
                    PS.Play();
                }
                else {
                    PS.Stop();
                }
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
