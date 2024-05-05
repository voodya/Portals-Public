using UnityEngine;

public interface IPortalTraveler
{
    public Transform Traveler { get; }
    public Transform Clone { get; }
    public Transform PortalIn { get; set; }
    public Transform PortalOut { get; set; }

    public void UpdateClonePosition();
    public void Warp();
}
