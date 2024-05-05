using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ABaseTraveler : MonoBehaviour, IPortalTraveler
{
    public Transform Traveler => transform;
    public Transform Clone => cloneObject.transform;
    public Transform PortalIn { get; set; }
    public Transform PortalOut { get; set; }

    protected static readonly Quaternion halfTurn = Quaternion.Euler(0.0f, 180.0f, 0.0f);
    protected Rigidbody _rb;
    protected GameObject cloneObject;
    private bool WarpDelay;

    private void Start()
    {
        if(this is not Player && this is not PlayerTraveler)
            Init();
    }

    public virtual void Init()
    {
        cloneObject = new GameObject();
        cloneObject.SetActive(false);
        var meshFilter = cloneObject.AddComponent<MeshFilter>();
        var meshRenderer = cloneObject.AddComponent<MeshRenderer>();

        meshFilter.mesh = GetComponent<MeshFilter>().mesh;
        meshRenderer.materials = GetComponent<MeshRenderer>().materials;
        cloneObject.transform.localScale = transform.localScale;

        _rb = GetComponent<Rigidbody>();
    }

    public virtual void UpdateClonePosition()
    {
        Vector3 relativePos = PortalIn.InverseTransformPoint(Traveler.position);
        relativePos = halfTurn * relativePos;
        Clone.position = PortalOut.TransformPoint(relativePos);

        Quaternion relativeRot = Quaternion.Inverse(PortalIn.rotation) * Traveler.rotation;
        relativeRot = halfTurn * relativeRot;
        Clone.rotation = PortalOut.rotation * relativeRot;
    }

    public virtual void Warp()
    {
        if (WarpDelay) return;
        SetDelay();
        var inTransform = PortalIn.transform;
        var outTransform = PortalOut.transform; 

        Vector3 relativePos = inTransform.InverseTransformPoint(Traveler.position);
        relativePos = halfTurn * relativePos;
        Traveler.position = outTransform.TransformPoint(relativePos);

        Quaternion relativeRot = Quaternion.Inverse(inTransform.rotation) * Traveler.rotation;
        relativeRot = halfTurn * relativeRot;
        Traveler.rotation = outTransform.rotation * relativeRot;

        Vector3 relativeVel = inTransform.InverseTransformDirection(_rb.velocity);
        relativeVel = halfTurn * relativeVel;
        _rb.velocity = outTransform.TransformDirection(relativeVel);

        var tmp = PortalIn;
        PortalIn = PortalOut;
        PortalOut = tmp; 
    }

    public async void SetDelay()
    {
        WarpDelay = true;
        await UniTask.Delay(500);
        WarpDelay = false;
    }
}
