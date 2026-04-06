using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    [Header("현재 적용 중인 플레이어 사운드")]
    [SerializeField] private SoundManager.SFXType footstepSFX = SoundManager.SFXType.None;
    [SerializeField] private SoundManager.SFXType jumpSFX = SoundManager.SFXType.Jump;
    [SerializeField] private SoundManager.SFXType landSFX = SoundManager.SFXType.Land;

    [Header("발걸음 설정")]
    [SerializeField] private bool footstepAs3D = true;
    [SerializeField][Range(0f, 1f)] private float footstepVolume = 0.7f;

    [Header("점프 / 착지 볼륨")]
    [SerializeField][Range(0f, 1f)] private float jumpVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float landVolume = 1f;

    public void ApplySceneSoundProfile(SoundProfile profile)
    {
        if (profile == null) return;

        footstepSFX = profile.playerFootstep;
        jumpSFX = profile.playerJump;
        landSFX = profile.playerLand;

        footstepAs3D = profile.playerFootstepAs3D;
        footstepVolume = profile.playerFootstepVolume;
        jumpVolume = profile.playerJumpVolume;
        landVolume = profile.playerLandVolume;
    }

    public void PlayFootstep()
    {
        if (SoundManager.Instance == null) return;
        if (footstepSFX == SoundManager.SFXType.None) return;

        if (footstepAs3D)
            SoundManager.Instance.PlaySFX3D(footstepSFX, transform.position, footstepVolume);
        else
            SoundManager.Instance.PlaySFX(footstepSFX, footstepVolume);
    }

    public void PlayJump()
    {
        if (SoundManager.Instance == null) return;
        if (jumpSFX == SoundManager.SFXType.None) return;

        SoundManager.Instance.PlaySFX(jumpSFX, jumpVolume);
    }

    public void PlayLand()
    {
        if (SoundManager.Instance == null) return;
        if (landSFX == SoundManager.SFXType.None) return;

        SoundManager.Instance.PlaySFX(landSFX, landVolume);
    }
}