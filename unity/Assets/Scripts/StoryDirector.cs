using UnityEngine;
using UnityEngine.SceneManagement;

namespace CriminalStory
{
    /// <summary>
    /// The story state machine. Story-driven by design: the player only
    /// ever has ONE objective, most scenes play themselves, and finishing
    /// an objective automatically starts the next scene.
    ///
    /// Flow in this foundation build:
    ///   Chapter 0 (birth, hospital)  — watch; look a little; lines advance
    ///   -> fade -> Chapter 1 (town)  — walk to the grandmother, press E
    ///   -> dialogue                  — advance lines
    ///   -> end panel                 — free-roam the town, or restart
    /// </summary>
    public class StoryDirector : MonoBehaviour
    {
        private enum Phase
        {
            Birth,
            BirthFadeOut,
            TownWalk,
            Dialogue,
            Done,
        }

        private PlayerController _player;
        private SubtitleUI _ui;
        private WorldRefs _world;
        private PostProcessSetup _post;

        private Phase _phase = Phase.Birth;
        private int _lineIndex = -1;
        private float _lineTimer;
        private float _birthAge;
        private float _baseYaw;
        private float _basePitch;
        private float _lookYaw;
        private float _lookPitch;
        private bool _transitionStarted;

        public void Init(PlayerController player, SubtitleUI ui,
            WorldRefs world, PostProcessSetup post)
        {
            _player = player;
            _ui = ui;
            _world = world;
            _post = post;
            StartBirth();
        }

        // ---------------------------------------------------------------
        // Chapter 0 — birth
        // ---------------------------------------------------------------
        private void StartBirth()
        {
            _phase = Phase.Birth;
            _birthAge = 0f;
            _lineIndex = -1;
            _transitionStarted = false;

            _player.SetEyeHeight(0.3f, 0.0f);
            _player.Teleport(_world.BabyCameraPos, 0f);
            _player.ControlEnabled = false;

            // Aim the rig at the mother's bed / doctor, slightly upward
            // like a newborn looking up from the cot.
            Vector3 toTarget = _world.BabyLookTarget - _world.BabyCameraPos;
            _baseYaw = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
            float horizontal = Mathf.Sqrt(toTarget.x * toTarget.x + toTarget.z * toTarget.z);
            _basePitch = -Mathf.Atan2(toTarget.y, horizontal) * Mathf.Rad2Deg;
            _lookYaw = 0f;
            _lookPitch = 0f;
            ApplyBirthView();

            _player.LockCursor(true);
            _ui.SetChapter(StoryData.Chapter0Title);
            _ui.SetObjective("");
            _ui.SetControlsHint("マウス／矢印キーで少しだけ見回せます　｜　クリック or スペース：セリフを進める");
            _ui.SetHaze(1f);
            if (_post != null)
            {
                _post.SetBirthBlur(1f);
            }
            NextBirthLine();
        }

        private void ApplyBirthView()
        {
            _player.transform.rotation = Quaternion.Euler(0f, _baseYaw + _lookYaw, 0f);
            if (_player.Cam != null)
            {
                _player.Cam.transform.localRotation =
                    Quaternion.Euler(_basePitch + _lookPitch, 0f, 0f);
            }
        }

        private void NextBirthLine()
        {
            _lineIndex++;
            if (_lineIndex >= StoryData.BirthLines.Length)
            {
                BeginFadeToTown();
                return;
            }
            StoryLine line = StoryData.BirthLines[_lineIndex];
            _ui.ShowLine(line.Speaker, line.Text);
            _lineTimer = line.Seconds;
        }

        private void BeginFadeToTown()
        {
            if (_transitionStarted)
            {
                return;
            }
            _transitionStarted = true;
            _phase = Phase.BirthFadeOut;
            _ui.HideDialogue();
            _ui.FadeTo(1f, 1.6f, () =>
            {
                MoveToTown();
                _ui.FadeTo(0f, 1.4f, null);
            });
        }

        // ---------------------------------------------------------------
        // Chapter 1 — town
        // ---------------------------------------------------------------
        private void MoveToTown()
        {
            if (_post != null)
            {
                _post.SetTownMode();
            }
            _ui.SetHaze(0f);
            _player.SetEyeHeight(1.1f, 1.0f); // a small child (Chapter 1)
            _player.Teleport(_world.TownPlayerStart, _world.TownPlayerYaw);
            _player.ControlEnabled = true;
            _player.LockCursor(true);
            _phase = Phase.TownWalk;
            _ui.SetChapter(StoryData.Chapter1Title);
            _ui.SetControlsHint("WASD：歩く　マウス：見回す　E：話しかける　Esc：マウスを離す");
            UpdateTownObjective();
        }

        private void UpdateTownObjective()
        {
            if (_world.Grandmother == null)
            {
                _ui.SetObjective(StoryData.TownObjective);
                return;
            }
            float dist = HorizontalDistance(_player.transform.position,
                _world.Grandmother.position);
            _ui.SetObjective(StoryData.TownObjective + "（あと約 " +
                Mathf.Max(0, Mathf.RoundToInt(dist)) + " m）");
            _ui.ShowPrompt(dist < 3.0f ? "E：祖母に話しかける" : "");
        }

        private void StartDialogue()
        {
            _phase = Phase.Dialogue;
            _lineIndex = -1;
            _player.ControlEnabled = false;
            _player.LockCursor(false);
            _ui.ShowPrompt("");
            NextDialogueLine();
        }

        private void NextDialogueLine()
        {
            _lineIndex++;
            if (_lineIndex >= StoryData.GrandmotherDialogue.Length)
            {
                FinishDialogue();
                return;
            }
            StoryLine line = StoryData.GrandmotherDialogue[_lineIndex];
            _ui.ShowLine(line.Speaker, line.Text);
            _lineTimer = line.Seconds;
        }

        private void FinishDialogue()
        {
            _phase = Phase.Done;
            _ui.HideDialogue();
            _ui.SetObjective(StoryData.TownObjectiveDone);
            _ui.SetControlsHint("WASD：歩く　マウス：見回す　Esc：マウスを離す");
            _player.LockCursor(false);
            _ui.ShowEndPanel(
                () =>
                {
                    _ui.HideEndPanel();
                    _player.ControlEnabled = true;
                    _player.LockCursor(true);
                },
                () =>
                {
                    Scene scene = SceneManager.GetActiveScene();
                    if (scene.buildIndex >= 0)
                    {
                        SceneManager.LoadScene(scene.buildIndex);
                    }
                    else
                    {
                        SceneManager.LoadScene(scene.name);
                    }
                });
        }

        // ---------------------------------------------------------------
        // Per-frame
        // ---------------------------------------------------------------
        private void Update()
        {
            bool advancePressed =
                Input.GetMouseButtonDown(0) ||
                Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Return);

            switch (_phase)
            {
                case Phase.Birth:
                    _birthAge += Time.deltaTime;
                    float clear = Mathf.Clamp01(_birthAge / 6f);
                    _ui.SetHaze(1f - clear);
                    if (_post != null)
                    {
                        _post.SetBirthBlur(1f - clear);
                    }
                    // Very limited look-around only (a newborn).
                    if (Cursor.lockState == CursorLockMode.Locked)
                    {
                        _lookYaw = Mathf.Clamp(_lookYaw + Input.GetAxis("Mouse X") * 0.35f, -28f, 28f);
                        _lookPitch = Mathf.Clamp(_lookPitch - Input.GetAxis("Mouse Y") * 0.35f, -14f, 14f);
                    }
                    if (Input.GetKey(KeyCode.LeftArrow))
                    {
                        _lookYaw = Mathf.Clamp(_lookYaw - 22f * Time.deltaTime, -28f, 28f);
                    }
                    if (Input.GetKey(KeyCode.RightArrow))
                    {
                        _lookYaw = Mathf.Clamp(_lookYaw + 22f * Time.deltaTime, -28f, 28f);
                    }
                    ApplyBirthView();

                    _lineTimer -= Time.deltaTime;
                    if (_lineTimer <= 0f || advancePressed)
                    {
                        NextBirthLine();
                    }
                    break;

                case Phase.TownWalk:
                    UpdateTownObjective();
                    if (Input.GetMouseButtonDown(0) &&
                        Cursor.lockState != CursorLockMode.Locked)
                    {
                        _player.LockCursor(true);
                    }
                    if (_world.Grandmother != null &&
                        HorizontalDistance(_player.transform.position,
                            _world.Grandmother.position) < 3.0f &&
                        Input.GetKeyDown(KeyCode.E))
                    {
                        StartDialogue();
                    }
                    break;

                case Phase.Dialogue:
                    _lineTimer -= Time.deltaTime;
                    if (_lineTimer <= 0f || advancePressed || Input.GetKeyDown(KeyCode.E))
                    {
                        NextDialogueLine();
                    }
                    break;
            }
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}
