using UnityEngine;

public class PlayerTraveler : ABaseTraveler
{
    [field: SerializeField] public Transform TravelerCamera { get; set; }
    [field: SerializeField] public bool InAnotherWorld { get; set; }
    [field: SerializeField] public Camera ClippingCamera { get; set; }
    [SerializeField] private GameObject _clonePFB;

    public override void Init()
    {
        cloneObject = Instantiate(_clonePFB);
        cloneObject.SetActive(false);
        _rb = GetComponent<Rigidbody>();
    }

}
