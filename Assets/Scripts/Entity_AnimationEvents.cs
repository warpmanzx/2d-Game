using UnityEngine;

public class Entity_AnimationEvents : MonoBehaviour
{
    private Entity entity;
    private Player player;

    private void Awake()
    {
        entity = GetComponentInParent<Entity>();
        player = GetComponentInParent<Player>();
    }

    public void DamageTargets() => entity.DamageTargets();
    public void DamageTargetsAbove() => entity.DamageTargetsAbove();
    public void DamageTargetsBelow() => entity.DamageTargetsBelow();

    private void DisableMovementAndJumpAndFlip() => entity.EnableMovementAndJumpAndFlip(false);

    private void DisableFlipAndDash() => player.EnableFlipAndDash(false);

    private void EnableFlipAndDash() => player.EnableFlipAndDash(true);

    private void SpawnSlash() => player.SpawnSlash();

    private void EnableMovementAndJumpAndFlip() => entity.EnableMovementAndJumpAndFlip(true);

}
