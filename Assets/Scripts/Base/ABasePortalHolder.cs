using UnityEngine;


public abstract class ABasePortalHolder : MonoBehaviour
{
    [SerializeField] protected Camera _virtualRenderCamera;
    [SerializeField] protected Transform _virtualRenderTransform;

    protected RenderTexture _texture1;
    protected RenderTexture _texture2;

    public abstract void Configure(PlayerTraveler traveler);
}
