using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;
using UnityEngine.UI;

 /*  todo:
    
- Arrow
- Tween arrow position/rotation
- Color
- Flashing

*/

public struct StasisMomentum {

    public StasisMomentum(Vector3 f, Vector3 p) {
        force = f;
        position = p; 
    }

    public Vector3 force;
    public Vector3 position;
};

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
    public GameObject stasisArrow;

    [Tooltip("Layer mask for what stasis should interact with. (Checked items are stasis-able)")]
    public LayerMask layerMask;

    PlayerController playerController;
    Camera MainCamera;
    CinemachineCameraOffset cinemachineCameraOffset;
    bool isOnCooldown = false;

    RaycastHit hit;
    IEnumerator hightlightObjectCoroutine;
    StasisMomentum momentum;

    
    void Awake() {
        playerController = GetComponent<PlayerController>();
        MainCamera = playerController.MainCamera;
        cinemachineCameraOffset = cinemachineCam.GetComponent<CinemachineCameraOffset>();
    }

    public void OnActivate() {
        if (isOnCooldown) {
            stasisReticle.GetComponent<Image>().color = Color.red;
		}
        stasisReticle.SetActive(true); // bring up stasis reticle ui
        playerController.animator.SetFloat("Speed", 0.0f);
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
            playerController.SM.SetState<Movement>();
        }

    }

    IEnumerator highlightObject(GameObject obj) {
        if (obj != null) {
            Material mat = obj.GetComponent<Renderer>().material;
            if (mat.HasColor("_Color")) { // if the material has a "Main Color"
                Color prev_color = mat.color;
                mat.color = Color.yellow;
                while (Physics.Raycast(MainCamera.transform.position, MainCamera.transform.forward, out hit, Mathf.Infinity, layerMask) && playerController.SM.getCurrState() is Stasis) {
                    if (hit.collider.gameObject != obj) { // if we've hovered over a different gameobject while looking at the current one
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
        momentum = new StasisMomentum(Vector3.zero, Vector3.zero);
        isOnCooldown = true;

        Renderer obj_rend = obj.GetComponent<Renderer>();
        Rigidbody rb = obj.GetComponent<Rigidbody>();

        if (obj_rend == null || rb == null) {
            Debug.Log("Object is not suitable for stasis!");
            yield break;
        }

        //stasisArrow.transform.parent = obj.transform;
        stasisArrow.transform.position = obj.transform.position;

        obj_rend.GetMaterials(tmp_material_list); // populate list with materials

        handleParticleSystems(stasisStartParticles, obj, true);

        tmp_material_list.Add(stasisMaterial);
        obj_rend.materials = tmp_material_list.ToArray();
        rb.isKinematic = true;

        // ---
        yield return new WaitForSeconds(stasisSeconds);
        // ---

        rb.isKinematic = false;
        tmp_material_list.RemoveAt(tmp_material_list.Count -1); // pop from back
        obj_rend.materials = tmp_material_list.ToArray();

        
        handleParticleSystems(stasisStartParticles, obj, false);
        handleParticleSystems(stasisEndParticles, obj, true);
        tmp_material_list.Clear();

        rb.AddForceAtPosition(momentum.force, momentum.position, ForceMode.Impulse); // launch stasis'd object
        stasisArrow.SetActive(false);
        //stasisArrow.transform.parent = null;
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

    public void AddStasisForce(StasisMomentum addedMomentum) {
        if (hit.collider != null) {
            momentum.force += addedMomentum.force;
            momentum.position = addedMomentum.position;

            if (!stasisArrow.activeSelf) {
                stasisArrow.SetActive(true);
            }

            stasisArrow.transform.LookAt(MainCamera.transform, Vector3.up);
            stasisArrow.transform.Rotate(new Vector3(90, 0, 0));
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
