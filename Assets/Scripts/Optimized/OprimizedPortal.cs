using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class OprimizedPortal : ABasePortal
{
    [SerializeField] private OprimizedPortal _otherPortal;
    public OprimizedPortal OtherPortal => _otherPortal;

    public override void Configure()
    {
        Trigger.OnTriggerEnterAsObservable().Subscribe(OnEnter).AddTo(this.gameObject);
        Trigger.OnTriggerExitAsObservable().Subscribe(OnExit).AddTo(this.gameObject);
        Observable.EveryLateUpdate().Subscribe(UpdateClonePositions).AddTo(this.gameObject);
    }

    public override void OnExit(Collider collider)
    {
        if (collider.TryGetComponent(out IPortalTraveler obj))
        {
            Physics.IgnoreCollision(collider, collider, false);
            obj.Clone.gameObject.SetActive(false);
            Travelers.Remove(obj);
        }
    }

    public override void OnEnter(Collider collider)
    {
        if (collider.TryGetComponent(out IPortalTraveler obj))
        {
            Physics.IgnoreCollision(collider, collider);
            obj.Clone.gameObject.SetActive(true);
            obj.PortalIn = transform;
            obj.PortalOut = OtherPortal.transform;
            Travelers.Add(obj);
        }
    }

    public override void UpdateClonePositions(long lon)
    {
        
        if (Travelers.Count == 0) return;

        for (int i = 0; i < Travelers.Count; i++)
        {
            //обновить позицию клона
            Travelers[i].UpdateClonePosition();

            Vector3 objPos = transform.InverseTransformPoint(Travelers[i].Traveler.position);
            //если клон выполз больше чем на половину
            if (objPos.x > 0.0f)
            {
                //меняем местами
                Travelers[i].Warp();
            }
        }
    }
}
