using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public abstract class ABasePortal : MonoBehaviour
{
    [SerializeField] private Collider _trigger;
    [SerializeField] private MeshRenderer _meshRenderer;

    protected List<IPortalTraveler> Travelers = new List<IPortalTraveler>();

    public MeshRenderer MeshRenderer => _meshRenderer;
    public Collider Trigger => _trigger;


    public abstract void Configure();
    public abstract void UpdateClonePositions(long obj);
    public abstract void OnExit(Collider trigger);
    public abstract void OnEnter(Collider trigger);

    
}
