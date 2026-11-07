using UnityEngine;

/// <summary>Recorded mechanical motion, stopped by actual contact or settling.</summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(110)]
public sealed class CastleBlockAudio : MonoBehaviour
{
    Rigidbody2D body;
    AudioSource movement;
    AudioSource activation;
    AudioSource impacts;
    Vector2 commandDirection;
    Vector2 velocityBeforePhysics;
    bool commanded;
    bool hasMoved;
    float commandedAt;
    float stillTime;
    float lastImpact = -10f;
    int previousImpact = -1;

    // Useful for the editor's isolated physics checks, without modifying gameplay.
    public int ImpactCueCount { get; private set; }
    public int ArrivalCueCount { get; private set; }
    public int StartCueCount { get; private set; }
    public int MovementLoopStarts { get; private set; }
    public bool MovementLoopPlaying { get { return movement != null && movement.isPlaying; } }

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        movement = CreateSource(96);
        activation = CreateSource(64);
        impacts = CreateSource(48);
        movement.loop = true;
    }

    AudioSource CreateSource(int priority)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.priority = priority;
        return source;
    }

    void FixedUpdate()
    {
        if (body != null) velocityBeforePhysics = body.velocity;
    }

    void LoadMotion(CastleAudioRuntime audio)
    {
        AudioClip expected = audio.LoadClip("A/mechanism_loop");
        if (movement.clip == expected) return;
        bool wasPlaying = movement.isPlaying;
        movement.Stop();
        movement.clip = expected;
        if (wasPlaying && commanded && expected != null) movement.Play();
    }

    public void BeginMove(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.5f) return;
        commandDirection = direction.normalized;
        commanded = true;
        hasMoved = false;
        stillTime = 0f;
        commandedAt = Time.time;
        CastleAudioRuntime audio = CastleAudioRuntime.Instance;
        if (audio == null) return;
        LoadMotion(audio);
        AudioClip clip = audio.LoadClip("A/mechanism_start");
        if (clip == null) return;
        activation.volume = audio.Settings.mechanismStartVolume;
        activation.pitch = 1f;
        activation.PlayOneShot(clip);
        StartCueCount++;
    }

    void Update()
    {
        CastleAudioRuntime audio = CastleAudioRuntime.Instance;
        if (audio == null || body == null) return;
        LoadMotion(audio);
        float speed = Mathf.Abs(Vector2.Dot(body.velocity, commandDirection));
        bool travelling = commanded && speed > 0.25f;
        if (travelling)
        {
            hasMoved = true;
            stillTime = 0f;
            if (!movement.isPlaying && movement.clip != null)
            {
                movement.volume = 0f;
                movement.Play();
                MovementLoopStarts++;
            }
        }
        else if (commanded)
        {
            stillTime += Time.deltaTime;
            if (hasMoved && stillTime >= audio.Settings.arrivalStillTime)
                NotifyArrived();
            else if (!hasMoved && Time.time - commandedAt > 0.4f)
                FinishMotion(false); // An already blocked object should not repeatedly thump.
        }

        float target = travelling ? audio.Settings.mechanismVolume * Mathf.Lerp(0.35f, 1f, Mathf.InverseLerp(.25f, 12f, speed)) : 0f;
        movement.volume = Mathf.MoveTowards(movement.volume, target, Time.deltaTime * 4f);
        movement.pitch = Mathf.Lerp(.96f, 1.025f, Mathf.InverseLerp(.25f, 12f, speed));
        if (!travelling && movement.volume <= .001f && movement.isPlaying) movement.Stop();
    }

    /// <summary>Also available to a future endpoint/rail controller.</summary>
    public void NotifyArrived()
    {
        if (!commanded) return;
        bool needsArrivalSound = hasMoved && Time.time - lastImpact > .2f;
        FinishMotion(needsArrivalSound);
    }

    void FinishMotion(bool playArrival)
    {
        commanded = false;
        hasMoved = false;
        stillTime = 0f;
        if (!playArrival) return;
        CastleAudioRuntime audio = CastleAudioRuntime.Instance;
        if (audio == null) return;
        AudioClip clip = audio.LoadClip("A/mechanism_stop");
        if (clip == null) return;
        impacts.volume = audio.Settings.objectImpactVolume;
        impacts.pitch = 1f;
        impacts.PlayOneShot(clip, .6f);
        ArrivalCueCount++;
        lastImpact = Time.time;
    }

    void OnCollisionEnter2D(Collision2D collision) { HandleCollision(collision); }
    void OnCollisionStay2D(Collision2D collision) { HandleCollision(collision); }

    void HandleCollision(Collision2D collision)
    {
        CastleAudioRuntime audio = CastleAudioRuntime.Instance;
        if (audio == null || body == null || collision.collider == null || collision.collider.isTrigger) return;
        // A passenger running or landing on top is handled by the player's own footwear audio.
        if (collision.collider.GetComponentInParent<Player1>() != null) return;
        float speed = 0f;
        bool blockedInCommandDirection = false;
        Rigidbody2D other = collision.rigidbody;
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            Vector2 otherVelocity = other != null ? other.GetPointVelocity(contact.point) : Vector2.zero;
            float closingSpeed = Mathf.Abs(Vector2.Dot(collision.relativeVelocity, contact.normal));
            closingSpeed = Mathf.Max(closingSpeed, Mathf.Max(0f, -Vector2.Dot(velocityBeforePhysics - otherVelocity, contact.normal)));
            speed = Mathf.Max(speed, closingSpeed);
            if (Vector2.Dot(contact.normal, commandDirection) < -.5f) blockedInCommandDirection = true;
        }
        if (speed < audio.Settings.minimumObjectImpactSpeed || Time.time - lastImpact < audio.Settings.objectImpactCooldown) return;
        int index = Random.Range(0, 3);
        if (index >= previousImpact) index++;
        previousImpact = index;
        AudioClip clip = audio.LoadClip("A/object_impact_" + (index + 1).ToString("00"));
        if (clip == null) return;
        impacts.volume = audio.Settings.objectImpactVolume;
        impacts.pitch = Random.Range(.985f, 1.015f);
        impacts.PlayOneShot(clip, Mathf.Lerp(.25f, 1f, Mathf.InverseLerp(audio.Settings.minimumObjectImpactSpeed, 14f, speed)));
        ImpactCueCount++;
        lastImpact = Time.time;
        if (commanded && blockedInCommandDirection) FinishMotion(false);
    }

    void OnDisable()
    {
        commanded = false;
        if (movement != null) movement.Stop();
    }
}
