using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using RenderPipeline = UnityEngine.Rendering.RenderPipelineManager;

public class AnotherWorld : ABasePortalHolder
{
    [SerializeField] protected Material _material;
    [SerializeField] private OprimizedPortal _inPortal;
    [SerializeField] private OprimizedPortal _outPortal;
    private PlayerTraveler Traveler;

    public override void Configure(PlayerTraveler traveler)
    {
        Traveler = traveler;
        _texture1 = new RenderTexture(Screen.width, Screen.height, 24);
        _virtualRenderCamera.targetTexture = _texture1;
        _material.SetTexture("_MainTex", _texture1);
        _inPortal.MeshRenderer.material = _material;
        _outPortal.MeshRenderer.material = _material;
        _inPortal.Configure();
        _outPortal.Configure();
        RenderPipeline.beginCameraRendering += CamerasUpdate;
    }

    private void OnDisable()
    {
        RenderPipeline.beginCameraRendering -= CamerasUpdate;
    }

    private void CamerasUpdate(ScriptableRenderContext context, Camera camera)
    {   
        //������� ����� ��������
        Transform inTransform;
        Transform outTransform;

        // ������ � ����������� �� ���������� ������ � ����� �����������
        if (Traveler.InAnotherWorld)
        {
            inTransform = _inPortal.transform;
            outTransform = _outPortal.transform;
        }
        else
        {
            inTransform = _outPortal.transform;
            outTransform = _inPortal.transform;
        }
        //��� �������� ��� ������
        _virtualRenderTransform.SetParent(outTransform);

        //������� ������� � �������
        Vector3 loockerPosition = inTransform.worldToLocalMatrix.MultiplyPoint3x4(Traveler.TravelerCamera.position);
        Quaternion diffrence = outTransform.rotation * Quaternion.Inverse(inTransform.rotation * Quaternion.Euler(0, 180, 0));
        loockerPosition.y = -loockerPosition.y;

        //���������
        _virtualRenderTransform.localPosition = -loockerPosition;
        _virtualRenderTransform.rotation = diffrence * Traveler.TravelerCamera.rotation;

        //������ ������
        UniversalRenderPipeline.RenderSingleCamera(context, _virtualRenderCamera);
    }
}
