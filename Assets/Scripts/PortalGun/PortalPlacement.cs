using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class PortalPlacement : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private List<DefaultPortal> _portals;
    [SerializeField] private Crosshair crosshair;

    PlayerTraveler Traveler;

    public void Configure(PlayerTraveler traveler)
    {
        Traveler = traveler;
        Observable.EveryUpdate().Subscribe(HandleInput).AddTo(this.gameObject);
    }

    private void HandleInput(long obj)
    {
        if (Input.GetButtonDown("Fire1"))
        {
            FirePortal(0, Traveler.TravelerCamera.position, Traveler.TravelerCamera.forward, 250.0f);
        }
        else if (Input.GetButtonDown("Fire2"))
        {
            FirePortal(1, Traveler.TravelerCamera.position, Traveler.TravelerCamera.forward, 250.0f);
        }
    }

    private void SetPortal(int id, Vector3 pos, Vector3 dir, float distance)
    {
        RaycastHit hit;
        Physics.Raycast(pos, dir, out hit, distance, layerMask);

        if (hit.collider != null)
        {
            _portals[id].PlacePortal(hit.collider, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }



    private void FirePortal(int portalID, Vector3 pos, Vector3 dir, float distance)
    {
        RaycastHit hit;
        Physics.Raycast(pos, dir, out hit, distance, layerMask);

        if (hit.collider != null)
        {
            // If we shoot a portal, recursively fire through the portal.
            if (hit.collider.tag == "Portal")
            {
                var inPortal = hit.collider.GetComponent<DefaultPortal>();

                if (inPortal == null)
                {
                    return;
                }

                var outPortal = inPortal.OtherPortal;

                // Update position of raycast origin with small offset.
                Vector3 relativePos = inPortal.transform.InverseTransformPoint(hit.point + dir);
                relativePos = Quaternion.Euler(0.0f, 180.0f, 0.0f) * relativePos;
                pos = outPortal.transform.TransformPoint(relativePos);

                // Update direction of raycast.
                Vector3 relativeDir = inPortal.transform.InverseTransformDirection(dir);
                relativeDir = Quaternion.Euler(0.0f, 180.0f, 0.0f) * relativeDir;
                dir = outPortal.transform.TransformDirection(relativeDir);

                distance -= Vector3.Distance(pos, hit.point);

                FirePortal(portalID, pos, dir, distance);

                return;
            }

            // Orient the portal according to camera look direction and surface direction.
            var cameraRotation = Traveler.Traveler.rotation;
            var portalRight = cameraRotation * Vector3.right;

            if (Mathf.Abs(portalRight.x) >= Mathf.Abs(portalRight.z))
            {
                portalRight = (portalRight.x >= 0) ? Vector3.right : -Vector3.right;
            }
            else
            {
                portalRight = (portalRight.z >= 0) ? Vector3.forward : -Vector3.forward;
            }

            var portalForward = -hit.normal;
            var portalUp = -Vector3.Cross(portalRight, portalForward);

            var portalRotation = Quaternion.LookRotation(portalForward, portalUp);

            // Attempt to place the portal.
            bool wasPlaced = _portals[portalID].PlacePortal(hit.collider, hit.point, portalRotation);

            if (wasPlaced)
            {
                crosshair.SetPortalPlaced(portalID, true);
            }
        }
    }
}
