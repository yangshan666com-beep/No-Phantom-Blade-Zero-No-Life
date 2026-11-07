using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(100)]
public sealed class CastlePlayerAudio : MonoBehaviour
{
    Player1 player;
    AudioSource steps;
    AudioSource landings;
    Vector2 lastPosition;
    Vector2 lastGravity;
    bool wasGrounded;
    bool firstFrame = true;
    float distance;
    float nextStepTime;
    float airborneTime;
    float maximumFallSpeed;
    int previousStep = -1;

    void Awake()
    {
        player = GetComponent<Player1>();
        steps = gameObject.AddComponent<AudioSource>();
        landings = gameObject.AddComponent<AudioSource>();
        foreach (AudioSource source in new[] { steps, landings })
        {
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.priority = 64;
        }
        if (player != null) lastGravity = player.gravityDirection;
        lastPosition = transform.position;
    }

    void LateUpdate()
    {
        CastleAudioRuntime audio = CastleAudioRuntime.Instance;
        if (player == null || player.rb == null || audio == null) return;
        Vector2 position = player.rb.position;
        Vector2 gravity = player.gravityDirection;
        bool grounded = player.IsGrounded;
        if (firstFrame)
        {
            firstFrame = false;
            lastPosition = position;
            lastGravity = gravity;
            wasGrounded = grounded;
            distance = audio.Settings.strideDistance;
            return;
        }

        bool flipped = Vector2.Dot(gravity, lastGravity) < 0.99f;
        if (flipped)
        {
            distance = 0f;
            maximumFallSpeed = 0f;
            airborneTime = 0f;
            nextStepTime = Time.time + 0.12f;
        }
        bool wood = IsWood(player.GroundCollider);
        if (!grounded)
        {
            airborneTime += Time.deltaTime;
            maximumFallSpeed = Mathf.Max(maximumFallSpeed, Vector2.Dot(player.rb.velocity, gravity));
            distance = audio.Settings.strideDistance;
        }
        else
        {
            if (!wasGrounded && !flipped && airborneTime > 0.12f && maximumFallSpeed > 2.5f)
            {
                AudioClip clip = audio.LoadClip(wood ? "A/land_wood" : "A/land_stone");
                if (clip != null)
                {
                    landings.pitch = Random.Range(0.99f, 1.01f);
                    landings.volume = audio.Settings.landingVolume;
                    landings.PlayOneShot(clip, Mathf.Clamp(maximumFallSpeed / 12f, 0.45f, 1f));
                }
                nextStepTime = Time.time + 0.13f;
                distance = 0f;
            }
            airborneTime = 0f;
            maximumFallSpeed = 0f;
            Vector2 tangent = new Vector2(-gravity.y, gravity.x);
            float travelled = Mathf.Abs(Vector2.Dot(position - lastPosition, tangent));
            if (!flipped && Mathf.Abs(player.AudioMoveInput) > 0.1f && travelled > 0.001f && travelled < 3f)
            {
                distance += travelled;
                if (distance >= audio.Settings.strideDistance && Time.time >= nextStepTime)
                {
                    PlayStep(audio, wood);
                    distance = 0f;
                    nextStepTime = Time.time + audio.Settings.minimumStepInterval;
                }
            }
            else if (Mathf.Abs(player.AudioMoveInput) <= 0.1f)
                distance = audio.Settings.strideDistance;
        }
        wasGrounded = grounded;
        lastPosition = position;
        lastGravity = gravity;
    }

    void PlayStep(CastleAudioRuntime audio, bool wood)
    {
        // Six takes, with no immediate repetition.
        int index = Random.Range(0, 5);
        if (index >= previousStep) index++;
        previousStep = index;
        AudioClip clip = audio.LoadClip((wood ? "A/wood_" : "A/stone_") + (index + 1).ToString("00"));
        if (clip == null) return;
        steps.volume = audio.Settings.footstepVolume;
        steps.pitch = Random.Range(0.99f, 1.01f);
        steps.PlayOneShot(clip);
    }

    public static bool IsWood(Collider2D collider)
    {
        if (collider == null) return false;
        if (collider.GetComponentInParent<GravityBlock>() != null) return true;
        string objectName = collider.name.ToLowerInvariant();
        return objectName.Contains("wood") || objectName.Contains("crate") || objectName.Contains("木");
    }
}
