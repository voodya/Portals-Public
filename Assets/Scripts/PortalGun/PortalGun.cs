using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using RenderPipeline = UnityEngine.Rendering.RenderPipelineManager;

public class PortalGun : ABasePortalHolder
{
    [SerializeField] private List<DefaultPortal> _portals;
    [SerializeField] private PortalPlacement _placement;
    [SerializeField] private int _iter;

    private PlayerTraveler Traveler;

    public override void Configure(PlayerTraveler traveler)
    {
        Traveler = traveler;
        _texture1 = new RenderTexture(Screen.width, Screen.height, 24);
        _texture2 = new RenderTexture(Screen.width, Screen.height, 24);

        Material mat1 = _portals[0].MeshRenderer.material;
        Material mat2 = _portals[1].MeshRenderer.material;

        mat1.SetTexture("_MainTex", _texture1);
        mat2.SetTexture("_MainTex", _texture2);

        _portals[0].MeshRenderer.material = mat1;
        _portals[1].MeshRenderer.material = mat2;
        _portals[0].Configure();
        _portals[1].Configure();
        _placement.Configure(traveler);
        RenderPipeline.beginCameraRendering += AllCamerasUpdate;
    }

    private void OnDisable()
    {
        RenderPipeline.beginCameraRendering -= AllCamerasUpdate;
    }

    private void AllCamerasUpdate(ScriptableRenderContext context, Camera camera)
    {

        if (!_portals[0].IsPlaced || !_portals[1].IsPlaced) return;

        if (_portals[0].MeshRenderer.isVisible)
        {
            _virtualRenderCamera.targetTexture = _texture1;
            _virtualRenderTransform.SetParent(_portals[1].transform);
            for (int i = _iter - 1; i >= 0; i--)
            {
                OneCameraUpdate(_portals[0], _portals[1], _virtualRenderCamera, _virtualRenderTransform, context, i);
            }
        }

        if (_portals[1].MeshRenderer.isVisible)
        {
            _virtualRenderCamera.targetTexture = _texture2;
            _virtualRenderTransform.SetParent(_portals[0].transform);
            for (int i = _iter - 1; i >= 0; i--)
            {
                OneCameraUpdate(_portals[1], _portals[0], _virtualRenderCamera, _virtualRenderTransform, context, i);
            }
        }
    }

    private void OneCameraUpdate(ABasePortal portalIn, ABasePortal portalOut, Camera virtualRenderCamera, Transform virtualRenderTransform, ScriptableRenderContext context, int IterId)
    {
        Transform inTransform = portalIn.transform;
        Transform outTransform = portalOut.transform;

        virtualRenderTransform.position = Traveler.TravelerCamera.position;
        virtualRenderTransform.rotation = Traveler.TravelerCamera.rotation;

        for (int i = 0; i <= IterId; ++i)
        {
            Vector3 relativePos = inTransform.InverseTransformPoint(virtualRenderTransform.position);
            Quaternion relativeRot = Quaternion.Inverse(inTransform.rotation) * virtualRenderTransform.rotation;
            relativePos = Quaternion.Euler(0f, 180f, 0f) * relativePos;
            relativeRot = Quaternion.Euler(0f, 180f, 0f) * relativeRot;

            virtualRenderTransform.position = outTransform.TransformPoint(relativePos);
            virtualRenderTransform.rotation = outTransform.rotation * relativeRot;
        }

        virtualRenderCamera.nearClipPlane = (virtualRenderTransform.position - outTransform.position).magnitude;

        UniversalRenderPipeline.RenderSingleCamera(context, virtualRenderCamera);
    }
}
