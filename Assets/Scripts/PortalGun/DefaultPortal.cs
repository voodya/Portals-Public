using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class DefaultPortal : ABasePortal
{
    [SerializeField] private bool _isPlaced;
    [SerializeField] private Transform testTransform;
    [SerializeField] private LayerMask placementMask;
    [SerializeField] private DefaultPortal _otherPortal;
    [SerializeField] private Color _portalColor;
    [SerializeField] private Renderer _colorRenderer;

    public DefaultPortal OtherPortal => _otherPortal;
    public bool IsPlaced => _isPlaced;
    public Color PortalColor => _portalColor;

    private Collider WallCollider;

    public override void Configure()
    {
        Material mat = _colorRenderer.material;
        mat.color = _portalColor;
        _colorRenderer.material = mat;

        Trigger.OnTriggerEnterAsObservable().Subscribe(OnEnter).AddTo(this.gameObject);
        Trigger.OnTriggerExitAsObservable().Subscribe(OnExit).AddTo(this.gameObject);
        Observable.EveryLateUpdate().Subscribe(UpdateClonePositions).AddTo(this.gameObject);
    }

    public override void UpdateClonePositions(long obj)
    {
        MeshRenderer.enabled = OtherPortal.IsPlaced;

        for (int i = 0; i < Travelers.Count; ++i)
        {
            Travelers[i].UpdateClonePosition();

            Vector3 objPos = transform.InverseTransformPoint(Travelers[i].Traveler.position);

            if (objPos.x > 0.0f)
            {
                Travelers[i].Warp();
            }
        }
    }

    public override void OnExit(Collider trigger)
    {
        if (!IsPlaced || !OtherPortal.IsPlaced) return;
        if (trigger.TryGetComponent(out IPortalTraveler obj))
        {
            Physics.IgnoreCollision(trigger, WallCollider, false);
            obj.Clone.gameObject.SetActive(false);
            Travelers.Remove(obj);
        }
    }

    public override void OnEnter(Collider trigger)
    {
        if (!IsPlaced || !OtherPortal.IsPlaced) return;
        if (trigger.TryGetComponent(out IPortalTraveler obj))
        {
            Physics.IgnoreCollision(trigger, WallCollider);
            obj.Clone.gameObject.SetActive(true);
            obj.PortalIn = transform;
            obj.PortalOut = OtherPortal.transform;
            Travelers.Add(obj);
        }
    }

    public bool PlacePortal(Collider wallCollider, Vector3 pos, Quaternion rot)
    {
        testTransform.position = pos;
        testTransform.rotation = rot;

        testTransform.position -= testTransform.forward * 0.001f;

        FixOverhangs();
        FixIntersects();

        if (CheckOverlap())
        {
            this.WallCollider = wallCollider;
            transform.position = testTransform.position;
            testTransform.rotation *= Quaternion.Euler(0, -90, 0);
            transform.rotation = testTransform.rotation;

            gameObject.SetActive(true);
            _isPlaced = true;
            return true;
        }

        return false;
    }

    public void RemovePortal()
    {
        gameObject.SetActive(false);
        _isPlaced = false;
    }

    private bool CheckOverlap()
    {
        var checkExtents = new Vector3(0.9f, 1.9f, 0.05f);

        var checkPositions = new Vector3[]
        {
            testTransform.position + testTransform.TransformVector(new Vector3( 0.0f,  0.0f, -0.1f)),

            testTransform.position + testTransform.TransformVector(new Vector3(-1.0f, -2.0f, -0.1f)),
            testTransform.position + testTransform.TransformVector(new Vector3(-1.0f,  2.0f, -0.1f)),
            testTransform.position + testTransform.TransformVector(new Vector3( 1.0f, -2.0f, -0.1f)),
            testTransform.position + testTransform.TransformVector(new Vector3( 1.0f,  2.0f, -0.1f)),

            testTransform.TransformVector(new Vector3(0.0f, 0.0f, 0.2f))
        };

        // Ensure the portal does not intersect walls.
        var intersections = Physics.OverlapBox(checkPositions[0], checkExtents, testTransform.rotation, placementMask);

        if (intersections.Length > 1)
        {
            return false;
        }
        else if (intersections.Length == 1)
        {
            // We are allowed to intersect the old portal position.
            if (intersections[0] != Trigger)
            {
                return false;
            }
        }

        // Ensure the portal corners overlap a surface.
        bool isOverlapping = true;

        for (int i = 1; i < checkPositions.Length - 1; ++i)
        {
            isOverlapping &= Physics.Linecast(checkPositions[i],
                checkPositions[i] + checkPositions[checkPositions.Length - 1], placementMask);
        }

        return isOverlapping;
    }

    private void FixOverhangs()
    {
        var testPoints = new List<Vector3>
        {
            new Vector3(-1.1f,  0.0f, 0.1f),
            new Vector3( 1.1f,  0.0f, 0.1f),
            new Vector3( 0.0f, -2.1f, 0.1f),
            new Vector3( 0.0f,  2.1f, 0.1f)
        };

        var testDirs = new List<Vector3>
        {
             Vector3.right,
            -Vector3.right,
             Vector3.up,
            -Vector3.up
        };

        for (int i = 0; i < 4; ++i)
        {
            RaycastHit hit;
            Vector3 raycastPos = testTransform.TransformPoint(testPoints[i]);
            Vector3 raycastDir = testTransform.TransformDirection(testDirs[i]);

            if (Physics.CheckSphere(raycastPos, 0.05f, placementMask))
            {
                break;
            }
            else if (Physics.Raycast(raycastPos, raycastDir, out hit, 2.1f, placementMask))
            {
                var offset = hit.point - raycastPos;
                testTransform.Translate(offset, Space.World);
            }
        }
    }

    private void FixIntersects()
    {
        var testDirs = new List<Vector3>
        {
             Vector3.right,
            -Vector3.right,
             Vector3.up,
            -Vector3.up
        };

        var testDists = new List<float> { 1.1f, 1.1f, 2.1f, 2.1f };

        for (int i = 0; i < 4; ++i)
        {
            RaycastHit hit;
            Vector3 raycastPos = testTransform.TransformPoint(0.0f, 0.0f, -0.1f);
            Vector3 raycastDir = testTransform.TransformDirection(testDirs[i]);

            if (Physics.Raycast(raycastPos, raycastDir, out hit, testDists[i], placementMask))
            {
                var offset = (hit.point - raycastPos);
                var newOffset = -raycastDir * (testDists[i] - offset.magnitude);
                testTransform.Translate(newOffset, Space.World);
            }
        }
    }
}
