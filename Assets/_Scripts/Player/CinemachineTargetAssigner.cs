using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineCamera))]
public class CinemachineTargetAssigner : MonoBehaviour
{

    public string playerTag = "Player";

    private CinemachineCamera virtualCamera;
    private bool targetAssigned = false;

    void Awake()
    {

        virtualCamera = GetComponent<CinemachineCamera>();
    }

    void LateUpdate()
    {

        if (targetAssigned) return;

        if (virtualCamera.Follow == null)
        {

            GameObject player = GameObject.FindGameObjectWithTag(playerTag);

            if (player != null)
            {

                Debug.Log("Cinemachine: Target Player ditemukan, lock in kamera.");
                virtualCamera.Follow = player.transform;
                virtualCamera.OnTargetObjectWarped(player.transform, player.transform.position - virtualCamera.transform.position);
                targetAssigned = true;
            }
        }
    }
}