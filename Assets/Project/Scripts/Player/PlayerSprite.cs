using System;
using InMotion.Tools.RuntimeScripts;
using UnityEngine;

public class PlayerSprite : MonoBehaviour
{
    public static PlayerSprite Singleton { get; private set;}
    [SerializeField] private MotionExecutor _motionExecutor;

    private void Awake() => _motionExecutor.OnMotionFrame = FrameUpdate;

    private void FrameUpdate()
    {
        if (PlayerBase.Singleton.IsWallSliding) 
            _motionExecutor.Target.flipX = PlayerBase.Singleton.IsTouchingRightWall;
        else
            _motionExecutor.Target.flipX = PlayerBase.Singleton.Facing == PlayerFacing.Left;

        _motionExecutor.SetParameter("isRunning", PlayerBase.Singleton.IsRunning);
        _motionExecutor.SetParameter("isJumping", PlayerBase.Singleton.IsJumping);
        _motionExecutor.SetParameter("isFalling", PlayerBase.Singleton.IsFalling);
        _motionExecutor.SetParameter("isWallSliding", PlayerBase.Singleton.IsWallSliding);
        _motionExecutor.SetParameter("isWallJumping", PlayerBase.Singleton.IsWallJumping);
    }
}
