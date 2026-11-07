using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Uses a separate 2D physics scene; never simulates the user's level.</summary>
[InitializeOnLoad]
public static class CastleAudioPhysicsCheck
{
    static Scene testScene;
    static GameObject actor;
    static GameObject floor;
    static CastleBlockAudio audio;
    static Rigidbody2D body;
    static int phase;
    static int collisionCount;
    static double checkAfter;

    static CastleAudioPhysicsCheck() { EditorApplication.update += Tick; }

    [MenuItem("Tools/城堡音频/测试机关启动、到位和碰撞")]
    static void Request()
    {
        if (!EditorApplication.isPlaying) { Debug.Log("请先进入 Play 模式。"); return; }
        File.WriteAllText("Library/CastleAudioPhysicsCheck.request", "check");
    }

    static void Tick()
    {
        try
        {
            if (phase == 0)
            {
                const string request = "Library/CastleAudioPhysicsCheck.request";
                if (!File.Exists(request) || !EditorApplication.isPlaying || Time.timeSinceLevelLoad < 1f) return;
                File.Delete(request);
                RunStartArrivalPhase();
            }
            else if (EditorApplication.timeSinceStartup >= checkAfter)
            {
                if (!EditorApplication.isPlaying) { Cleanup(); return; }
                if (phase == 1) { RunImpactPhase(); return; }
                PhysicsScene2D physics = testScene.GetPhysicsScene2D();
                for (int i = 0; i < 20; i++)
                {
                    audio.SendMessage("FixedUpdate");
                    physics.Simulate(.02f);
                }
                Require(audio.ImpactCueCount == collisionCount, "Resting contact retriggered an impact");
                string report = "{\"status\":\"passed\",\"isolated_physics_scene\":true,\"same_direction_deduplicated\":true,\"movement_loop_started\":true,\"arrival_once\":true,\"collision_thud_count\":" + collisionCount + ",\"resting_contact_quiet\":true}";
                File.WriteAllText("Library/CastleAudioPhysicsCheck.json", report);
                Debug.Log("[CastleAudio] Physics checks passed: " + report);
                Cleanup();
            }
        }
        catch (Exception e)
        {
            File.WriteAllText("Library/CastleAudioPhysicsCheck.json", JsonUtility.ToJson(new ErrorReport { error = e.Message }));
            Debug.LogException(e);
            Cleanup();
        }
    }

    static void RunStartArrivalPhase()
    {
        Require(CastleAudioRuntime.Instance != null, "Audio runtime not ready");
        testScene = SceneManager.CreateScene("Castle Audio Isolated Physics Check", new CreateSceneParameters(LocalPhysicsMode.Physics2D));
        floor = new GameObject("Audio test floor");
        SceneManager.MoveGameObjectToScene(floor, testScene);
        floor.transform.position = new Vector3(10000f, 10000f, 0f);
        floor.AddComponent<BoxCollider2D>().size = new Vector2(6f, .4f);

        actor = new GameObject("Audio test block");
        SceneManager.MoveGameObjectToScene(actor, testScene);
        actor.transform.position = new Vector3(10000f, 10002f, 0f);
        actor.AddComponent<SpriteRenderer>().enabled = false;
        actor.AddComponent<BoxCollider2D>().size = new Vector2(.5f, .5f);
        body = actor.AddComponent<Rigidbody2D>();
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        GravityBlock block = actor.AddComponent<GravityBlock>();
        block.enabled = false; // Do not accumulate global FixedUpdate forces between local simulations.
        audio = actor.GetComponent<CastleBlockAudio>();
        foreach (AudioSource source in actor.GetComponents<AudioSource>()) source.mute = true;

        block.SetDirection(Vector2.right);
        Require(audio.StartCueCount == 1, "Direction change did not start the mechanism");
        block.SetDirection(Vector2.right);
        Require(audio.StartCueCount == 1, "Repeated direction retriggered the start");
        body.velocity = new Vector2(4f, 0f);
        audio.SendMessage("Update");
        Require(audio.MovementLoopPlaying && audio.MovementLoopStarts == 1, "Moving block did not play its mechanism loop");
        audio.NotifyArrived();
        audio.NotifyArrived();
        Require(audio.ArrivalCueCount == 1, "Arrival must sound exactly once");

        // Local PhysicsScene.Simulate does not advance Time.time. Wait in real Play time
        // so the intentionally short arrival/impact debounce has elapsed.
        checkAfter = EditorApplication.timeSinceStartup + .35;
        phase = 1;
    }

    static void RunImpactPhase()
    {
        // A second operation hits a real collider. The callback must produce one dull impact.
        // Use an independent fixture, so the arrival fixture's audio cooldown is unrelated.
        UnityEngine.Object.DestroyImmediate(actor);
        actor = new GameObject("Audio collision fixture");
        SceneManager.MoveGameObjectToScene(actor, testScene);
        actor.transform.position = new Vector3(10000f, 10002f, 0f);
        actor.AddComponent<SpriteRenderer>().enabled = false;
        actor.AddComponent<BoxCollider2D>().size = new Vector2(.5f, .5f);
        body = actor.AddComponent<Rigidbody2D>();
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        GravityBlock block = actor.AddComponent<GravityBlock>();
        block.enabled = false;
        audio = actor.GetComponent<CastleBlockAudio>();
        foreach (AudioSource source in actor.GetComponents<AudioSource>()) source.mute = true;
        body.velocity = new Vector2(0f, -6f);
        Physics2D.SyncTransforms();
        PhysicsScene2D physics = testScene.GetPhysicsScene2D();
        for (int i = 0; i < 40; i++)
        {
            audio.SendMessage("FixedUpdate");
            physics.Simulate(.02f);
        }
        collisionCount = audio.ImpactCueCount;
        Require(collisionCount == 1, "Expected one collision thud, got " + collisionCount + "; position=" + body.position + "; velocity=" + body.velocity);
        checkAfter = EditorApplication.timeSinceStartup + .4;
        phase = 2;
    }

    static void Require(bool value, string message) { if (!value) throw new Exception(message); }
    static void Cleanup()
    {
        if (actor != null) UnityEngine.Object.DestroyImmediate(actor);
        if (floor != null) UnityEngine.Object.DestroyImmediate(floor);
        if (testScene.IsValid() && testScene.isLoaded) SceneManager.UnloadSceneAsync(testScene);
        actor = null; floor = null; audio = null; body = null; phase = 0;
    }
    [Serializable] class ErrorReport { public string error; }
}
