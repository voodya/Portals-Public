using DG.Tweening;
using UniRx;
using UnityEngine;

public class Player : PlayerTraveler
{
    [Space]
    [Header("Portals types")]
    [SerializeField] private PortalGun _gun;
    [SerializeField] private AnotherWorld _world;
    [SerializeField] private PortalType _portalType;

    [Space]
    [Header("Input")]
    [SerializeField] private float _speed = 10;
    [SerializeField] private float _speedRotate = 10;

    public Quaternion TargetRotation { private set; get; }
    public Quaternion TargetRotationCamera { private set; get; }

    private Vector3 moveVector = Vector3.zero;
    private Tween Tween;

    public override void Init()
    {
        Cursor.lockState = CursorLockMode.Locked;
        TargetRotation = Traveler.rotation;
        TargetRotationCamera = TravelerCamera.localRotation;
        Observable.EveryFixedUpdate().Subscribe(PhysicsMovementTemp).AddTo(this.gameObject);
        Observable.EveryUpdate().Subscribe(PhysicsMovementInput).AddTo(this.gameObject);
        base.Init();
        if (_portalType == PortalType.Gun)
            _gun.Configure(this);
        else
            _world.Configure(this);
    }

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    public void SetType(PortalType type)
    {
        _portalType = type;
        Init();
    }

    public override void Warp()
    {
        base.Warp();
        if(_portalType == PortalType.Worlds)
        InAnotherWorld = !InAnotherWorld;
        ResetTargetRotation();
    }

    private void PhysicsMovementInput(long _)
    {
        var rotationCam = new Vector2(-Input.GetAxis("Mouse Y"), 0);
        var targetEulerCam = TargetRotationCamera.eulerAngles + (Vector3)rotationCam * _speedRotate;
        if (targetEulerCam.x > 180.0f)
        {
            targetEulerCam.x -= 360.0f;
        }
        targetEulerCam.x = Mathf.Clamp(targetEulerCam.x, -75.0f, 75.0f);
        targetEulerCam.y = 0f; targetEulerCam.z = 0f;
        TargetRotationCamera = Quaternion.Euler(targetEulerCam);

        var rotation = new Vector2(0, Input.GetAxis("Mouse X"));
        var targetEuler = TargetRotation.eulerAngles + (Vector3)rotation * _speedRotate;
        TargetRotation = Quaternion.Euler(targetEuler);



        var LR = Input.GetAxis("Horizontal") * Traveler.right;
        var FB = Input.GetAxis("Vertical") * Traveler.forward;

        Vector3 vert = Vector3.zero;
        if (Input.GetKey(KeyCode.Q))
            vert = Vector3.down;
        else if (Input.GetKey(KeyCode.E))
            vert = Vector3.up;
        moveVector = LR + FB + vert;
    }

    private void PhysicsMovementTemp(long _)
    {
        TravelerCamera.localRotation = Quaternion.Slerp(TravelerCamera.localRotation, TargetRotationCamera,
            Time.deltaTime * 15.0f);
        Traveler.rotation = Quaternion.Slerp(Traveler.rotation, TargetRotation,
            Time.deltaTime * 15.0f);

        float speed = _speed;
        if (Input.GetKey(KeyCode.LeftShift))
            speed *= 2;

        _rb.velocity = moveVector * speed * Time.fixedDeltaTime;
    }

    public void ResetTargetRotation()
    {
        Tween?.Kill(false);
        TargetRotation = Quaternion.LookRotation(Traveler.forward, Traveler.up);
        Tween = Traveler.DORotateQuaternion(new(0, TargetRotation.y, 0, TargetRotation.w), 0.1f).OnComplete(() =>
        {
            TargetRotation = Traveler.rotation;
        });
    }
}

public enum PortalType
{
    Gun,
    Worlds,
}

