using UnityEngine;

namespace CriminalStory
{
    /// <summary>
    /// A person built from basic shapes (body, head, arms, legs) with a
    /// floating nameplate, so the player can ALWAYS tell who somebody is
    /// (quality checklist B2 / F1). This is the placeholder body until
    /// free character assets are imported (see unity/README.md).
    ///
    /// An NPC either stands still (optionally turning slowly) or walks a
    /// simple waypoint loop on flat ground.
    /// </summary>
    public class Npc : MonoBehaviour
    {
        private TextMesh _nameplate;
        private float _bobPhase;
        private Vector3[] _waypoints;
        private int _waypointIndex;
        private float _walkSpeed = 1.1f;
        private bool _walking;

        /// <summary>Builds the visible body. Call once, right after AddComponent.</summary>
        public void Build(string displayName, Color clothes, Color pants,
            Color skin, float scale = 1.0f, bool nameplateVisible = true)
        {
            Build(displayName, clothes, pants, skin,
                new Color(0.16f, 0.12f, 0.10f), scale, nameplateVisible);
        }

        /// <summary>
        /// Full variant with an explicit hair colour, so each person is
        /// distinguishable by clothes + hair + nameplate (checklist F1).
        /// Bodies are placeholder primitives until real character models
        /// are imported (see unity/README.md) — but they cast real shadows
        /// and are never anonymous.
        /// </summary>
        public void Build(string displayName, Color clothes, Color pants,
            Color skin, Color hair, float scale = 1.0f, bool nameplateVisible = true)
        {
            transform.localScale = Vector3.one * scale;

            // Legs
            BuildUtil.Box("LegL", transform, new Vector3(-0.11f, 0.40f, 0f),
                new Vector3(0.16f, 0.80f, 0.18f), pants);
            BuildUtil.Box("LegR", transform, new Vector3(0.11f, 0.40f, 0f),
                new Vector3(0.16f, 0.80f, 0.18f), pants);
            // Torso (clothes)
            BuildUtil.Box("Torso", transform, new Vector3(0f, 1.12f, 0f),
                new Vector3(0.52f, 0.66f, 0.26f), clothes);
            // Arms
            BuildUtil.Box("ArmL", transform, new Vector3(-0.34f, 1.10f, 0f),
                new Vector3(0.13f, 0.62f, 0.15f), clothes);
            BuildUtil.Box("ArmR", transform, new Vector3(0.34f, 1.10f, 0f),
                new Vector3(0.13f, 0.62f, 0.15f), clothes);
            // Head + hair cap
            BuildUtil.Sphere("Head", transform, new Vector3(0f, 1.66f, 0f),
                new Vector3(0.34f, 0.38f, 0.34f), skin);
            BuildUtil.Sphere("Hair", transform, new Vector3(0f, 1.76f, -0.02f),
                new Vector3(0.36f, 0.26f, 0.36f), hair);

            RemoveBodyColliders();

            // Nameplate
            GameObject plateGo = new GameObject("Nameplate");
            plateGo.transform.SetParent(transform, false);
            plateGo.transform.localPosition = new Vector3(0f, 2.05f, 0f);
            _nameplate = plateGo.AddComponent<TextMesh>();
            _nameplate.text = displayName;
            _nameplate.fontSize = 48;
            _nameplate.characterSize = 0.09f;
            _nameplate.anchor = TextAnchor.MiddleCenter;
            _nameplate.alignment = TextAlignment.Center;
            _nameplate.color = Color.white;
            Font font = BuildUtil.DefaultFont();
            if (font != null)
            {
                _nameplate.font = font;
                _nameplate.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            }
            plateGo.SetActive(nameplateVisible);

            _bobPhase = Random.Range(0f, 6.28f);
        }

        /// <summary>Variant for a person lying in a bed (the mother in Chapter 0).</summary>
        public void BuildLying(string displayName, Color clothes, Color skin)
        {
            // Body under a blanket, head on a pillow: kept gentle and simple.
            BuildUtil.Box("BodyLying", transform, new Vector3(0f, 0.10f, 0.15f),
                new Vector3(0.52f, 0.20f, 1.15f), clothes);
            BuildUtil.Sphere("Head", transform, new Vector3(0f, 0.16f, -0.72f),
                new Vector3(0.34f, 0.30f, 0.34f), skin);
            BuildUtil.Sphere("Hair", transform, new Vector3(0f, 0.24f, -0.76f),
                new Vector3(0.36f, 0.20f, 0.30f), new Color(0.30f, 0.20f, 0.12f));
            RemoveBodyColliders();

            GameObject plateGo = new GameObject("Nameplate");
            plateGo.transform.SetParent(transform, false);
            plateGo.transform.localPosition = new Vector3(0f, 0.85f, -0.6f);
            _nameplate = plateGo.AddComponent<TextMesh>();
            _nameplate.text = displayName;
            _nameplate.fontSize = 48;
            _nameplate.characterSize = 0.09f;
            _nameplate.anchor = TextAnchor.MiddleCenter;
            _nameplate.alignment = TextAlignment.Center;
            _nameplate.color = Color.white;
            Font font = BuildUtil.DefaultFont();
            if (font != null)
            {
                _nameplate.font = font;
                _nameplate.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            }
            _bobPhase = Random.Range(0f, 6.28f);
        }

        public void SetWaypoints(Vector3[] worldPoints, float speed)
        {
            _waypoints = worldPoints;
            _walkSpeed = speed;
            _walking = worldPoints != null && worldPoints.Length > 1;
            if (_walking)
            {
                transform.position = worldPoints[0];
                _waypointIndex = 1;
            }
        }

        private void RemoveBodyColliders()
        {
            // Placeholder bodies must not block the player's path.
            Collider[] colliders = GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                Destroy(colliders[i]);
            }
        }

        private void Update()
        {
            // Face the nameplate to the camera.
            if (_nameplate != null && Camera.main != null)
            {
                Vector3 toCam = Camera.main.transform.position - _nameplate.transform.position;
                toCam.y = 0f;
                if (toCam.sqrMagnitude > 0.001f)
                {
                    _nameplate.transform.rotation = Quaternion.LookRotation(toCam);
                }
            }

            if (_walking && _waypoints != null)
            {
                Vector3 target = _waypoints[_waypointIndex];
                Vector3 pos = transform.position;
                Vector3 to = target - pos;
                to.y = 0f;
                float dist = to.magnitude;
                if (dist < 0.4f)
                {
                    _waypointIndex = (_waypointIndex + 1) % _waypoints.Length;
                }
                else
                {
                    Vector3 dir = to / dist;
                    transform.position = pos + dir * _walkSpeed * Time.deltaTime;
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                        Quaternion.LookRotation(dir), Time.deltaTime * 6f);
                    // Gentle walking bob.
                    Vector3 p = transform.position;
                    p.y = Mathf.Abs(Mathf.Sin(Time.time * 7f + _bobPhase)) * 0.04f;
                    transform.position = p;
                }
            }
        }
    }

    /// <summary>Slowly bobbing / rotating golden marker above the objective person.</summary>
    public class MarkerBob : MonoBehaviour
    {
        private Vector3 _basePos;
        private float _phase;

        private void Start()
        {
            _basePos = transform.localPosition;
            _phase = Random.Range(0f, 6.28f);
        }

        private void Update()
        {
            Vector3 p = _basePos;
            p.y += Mathf.Sin(Time.time * 2f + _phase) * 0.15f;
            transform.localPosition = p;
            transform.Rotate(Vector3.up, 60f * Time.deltaTime, Space.World);
        }
    }
}
