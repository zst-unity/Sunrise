using System.Collections;
using MonoWaves.QoL;
using UnityEngine;

public class PlayerGroundSlam : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField, Min(0)] private float _airTime = 0.5f;
    [SerializeField, Min(0)] private float _dashSpeed;
    [SerializeField, Min(0)] private float _slamDamage;
    [SerializeField, Min(0)] private float _blazeConsume;
    [SerializeField] private BoxChecker _slamChecker;

    [field: Header("Info")]
    [field: SerializeField, ReadOnly] public bool IsDashing { get; private set;}

    private Rigidbody2D _rb => PlayerBase.Singleton.Rigidbody;

    private void Update() 
    {
        bool isInAir = !PlayerBase.Singleton.IsTouchingGround && !PlayerBase.Singleton.IsTouchingRightWall && !PlayerBase.Singleton.IsTouchingLeftWall;

        if (PlayerBase.Singleton.WantToSlam && isInAir && !IsDashing)
            StartCoroutine(GroundSlam());
    }

    private IEnumerator GroundSlam()
    {
        if (!PlayerBlaze.Singleton.CanConsume(_blazeConsume)) yield break;

        IsDashing = true;
        PlayerBase.Singleton.BlockAllInputs = true;
        PlayerBase.Singleton.BlockGravity = true;
        PlayerHealth.Singleton.StartInvincible();

        PlayerBase.Singleton.IsGroundSlamPrepare = true;

        _rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(_airTime);

        PlayerBlaze.Singleton.Consume(_blazeConsume);

        PlayerBase.Singleton.IsGroundSlamPrepare = false;
        PlayerBase.Singleton.IsGroundSlamDash = true;

        while (!PlayerBase.Singleton.IsTouchingGround)
        {
            _rb.velocity = _dashSpeed * Vector2.down;
            yield return null;
        }

        PlayerBase.Singleton.IsGroundSlamDash = false;
        PlayerBase.Singleton.IsGroundStandUp = true;

        Vector2 position = transform.position;
        Collider2D[] colliders = Physics2D.OverlapBoxAll(position + _slamChecker.Offset, _slamChecker.Size, 0f, _slamChecker.Mask); 

        foreach (var other in colliders)
        {
            if (other.TryGetComponent(out EnemyBase enemy))
            {
                enemy.Hit(_slamDamage, transform.position, Vector2.one * 2);
            }

            if (other.TryGetComponent(out Destructable destructable))
            {
                if (destructable.DestructMode == DestructMode.GroundSlam) destructable.Destruct();
            }
        }

        PlayerBase.Singleton.SlamEffect.Spawn(transform.position + Vector3.down, Quaternion.identity);
        PlayerCamera.Singleton.Shake(1.75f);
        PlayerBase.Singleton.GroundSlamSound.Play(AudioOptions.HalfVolumeWithVariation);

        IsDashing = false;
        PlayerBase.Singleton.BlockGravity = false;

        yield return new WaitForSeconds(0.3f);

        PlayerBase.Singleton.BlockAllInputs = false;
        PlayerBase.Singleton.IsGroundStandUp = false;

        yield return new WaitForSeconds(0.5f);
        PlayerHealth.Singleton.StopInvincible();
    }

    private void OnDrawGizmos() 
    {
        Vector2 position = transform.position;

        Gizmos.color = _slamChecker.GizmosColor;
        Gizmos.DrawWireCube(position + _slamChecker.Offset, _slamChecker.Size);
    }
}
