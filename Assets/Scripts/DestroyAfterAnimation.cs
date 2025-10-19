using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}