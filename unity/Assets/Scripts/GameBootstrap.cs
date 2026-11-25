using UnityEngine;
using UnityEngine.EventSystems;

namespace CriminalStory
{
    /// <summary>
    /// Entry point. The Main scene contains only an empty GameObject with
    /// this component; EVERYTHING else (world, player, UI, post-processing,
    /// story flow) is built in code at startup, so the project does not
    /// depend on hand-authored scene YAML.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;

            // EventSystem so the end-panel buttons can be clicked.
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            // World (hospital + town), with its own sun / sky / fog.
            WorldRefs world = WorldBuilder.BuildAll();

            // Player rig (first person).
            GameObject playerGo = new GameObject("Player");
            PlayerController player = playerGo.AddComponent<PlayerController>();
            player.BuildRig();

            // Post-processing (HDRP Volume; see PostProcessSetup).
            GameObject postGo = new GameObject("PostProcess");
            PostProcessSetup post = postGo.AddComponent<PostProcessSetup>();
            post.Build();

            // UI (subtitles, objective, prompts, fades, end panel).
            GameObject uiGo = new GameObject("GameUI");
            SubtitleUI ui = uiGo.AddComponent<SubtitleUI>();
            ui.Build();

            // Story flow.
            GameObject directorGo = new GameObject("StoryDirector");
            StoryDirector director = directorGo.AddComponent<StoryDirector>();
            director.Init(player, ui, world, post);
        }
    }
}
