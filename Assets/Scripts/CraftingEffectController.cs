using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles effects when recipes are matched in CraftingArea.
/// Disables all previous audio/animation/skin effects and applies the selected one.
/// In RecipeTrigger, call ApplyEffect(label) with the effect's label (e.g. "Pref6") or ApplyEffect(index) with its index.
/// </summary>
public class CraftingEffectController : MonoBehaviour
{
    [Serializable]
    public class EffectEntry
    {
        [Tooltip("Label for this effect (e.g. Pref6, Pref7) - for your reference in the inspector.")]
        public string label = "Effect";

        [Tooltip("Exact name of the audio player GameObject to enable (e.g. HipHopPlayer, SadTrumpetPlayer). That GameObject (or its children) must have an AudioSource.")]
        public string audioPlayerNameToEnable = "";

        [Tooltip("Name of the animation state in the Animator Controller (played via Animator.Play).")]
        public string animationStateName = "";

        [Tooltip("The model to show (has its own Animator and Avatar). Siblings under the same parent will be disabled. Animation is played on this model's Animator.")]
        public Transform modelToShow;

        [Header("Model transform (optional)")]
        [Tooltip("When enabled, the model's local position, rotation and scale are set to the values below (e.g. to correct wrong rotation from animations).")]
        public bool overrideModelTransform;

        [Tooltip("Local position to apply when Override Model Transform is enabled.")]
        public Vector3 modelLocalPosition = Vector3.zero;

        [Tooltip("Local rotation (Euler angles in degrees) to apply when Override Model Transform is enabled.")]
        public Vector3 modelLocalRotation = Vector3.zero;

        [Tooltip("Local scale to apply when Override Model Transform is enabled.")]
        public Vector3 modelLocalScale = Vector3.one;

        [Tooltip("Particle system or root to enable (optional).")]
        public GameObject particleToEnable;
    }

    [Header("References")]
    [Tooltip("Parent that contains all *Player GameObjects (e.g. HipHopPlayer, SadTrumpetPlayer). If null, the script searches the whole scene for them.")]
    public Transform audioPlayersRoot;

    [Header("Effects (one per recipe)")]
    [Tooltip("Add one entry per recipe. In each recipe's On Recipe Matched, call ApplyEffect with the effect's label (e.g. Pref6).")]
    public List<EffectEntry> effects = new List<EffectEntry>();

    private Transform _effectiveAudioRoot;

    /// <summary>
    /// Cached default local transform per model (instance ID), so we can restore when override is disabled.
    /// </summary>
    private readonly Dictionary<int, (Vector3 position, Quaternion rotation, Vector3 scale)> _defaultModelTransforms = new Dictionary<int, (Vector3, Quaternion, Vector3)>();

    private void Awake()
    {
        _effectiveAudioRoot = audioPlayersRoot; // null = search whole scene for *Player objects
    }

    /// <summary>
    /// Call this from a recipe's OnRecipeMatched event. Finds the effect with the given label and applies it.
    /// Label comparison is case-insensitive. Does nothing if no effect has that label.
    /// </summary>
    public void ApplyEffect(string label)
    {
        if (effects == null || string.IsNullOrEmpty(label)) return;

        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i] != null && effects[i].label.Equals(label, StringComparison.OrdinalIgnoreCase))
            {
                DisableAllAudioPlayers();
                ApplyEffectEntry(effects[i]);
                return;
            }
        }
    }

    /// <summary>
    /// Applies the effect at the given index (0, 1, 2, ...). Use ApplyEffect(label) to apply by label instead.
    /// </summary>
    public void ApplyEffect(int index)
    {
        if (effects == null || index < 0 || index >= effects.Count) return;

        DisableAllAudioPlayers();
        ApplyEffectEntry(effects[index]);
    }

    /// <summary>
    /// Disable all GameObjects whose name ends with "Player" (and mute their AudioSources).
    /// If audioPlayersRoot is set, searches under it; otherwise searches the whole scene.
    /// </summary>
    public void DisableAllAudioPlayers()
    {
        if (_effectiveAudioRoot != null)
            DisablePlayersRecursive(_effectiveAudioRoot);
        else
            DisablePlayersInScene();
    }

    private void DisablePlayersInScene()
    {
        var scene = gameObject.scene;
        if (!scene.IsValid()) return;
        foreach (GameObject go in scene.GetRootGameObjects())
        {
            if (go != null)
                DisablePlayersRecursive(go.transform);
        }
    }

    private void DisablePlayersRecursive(Transform root)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child == null) continue;

            if (child.name.EndsWith("Player", System.StringComparison.OrdinalIgnoreCase))
            {
                var source = child.GetComponentInChildren<AudioSource>();
                if (source != null)
                    source.mute = true;
                child.gameObject.SetActive(false);
            }
            else
                DisablePlayersRecursive(child);
        }
    }

    private void ApplyEffectEntry(EffectEntry entry)
    {
        if (entry == null) return;

        DisableAllEffectParticles();
        ApplyAudio(entry.audioPlayerNameToEnable);
        ApplySkin(entry.modelToShow);
        ApplyModelTransform(entry);
        ApplyAnimation(entry.animationStateName, entry.modelToShow);  // Play on this model's Animator
        ApplyParticle(entry.particleToEnable);
    }

    /// <summary>
    /// Disable all particle systems that are assigned in any effect, so only the current effect's particle is visible.
    /// </summary>
    private void DisableAllEffectParticles()
    {
        if (effects == null) return;
        foreach (var e in effects)
        {
            if (e != null && e.particleToEnable != null)
                e.particleToEnable.SetActive(false);
        }
    }

    private void ApplyAudio(string audioPlayerNameToEnable)
    {
        if (string.IsNullOrEmpty(audioPlayerNameToEnable)) return;

        Transform target = _effectiveAudioRoot != null
            ? FindRecursive(_effectiveAudioRoot, audioPlayerNameToEnable)
            : FindInScene(audioPlayerNameToEnable);

        if (target != null)
        {
            var source = target.GetComponentInChildren<AudioSource>();
            if (source != null)
            {
                source.mute = false;
                if (!source.isPlaying && source.clip != null)
                    source.Play();
            }
            target.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Find first transform in the active scene whose name matches (case-insensitive).
    /// </summary>
    private static Transform FindInScene(string name)
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid()) return null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root == null) continue;
            var found = FindRecursive(root.transform, name);
            if (found != null) return found;
        }
        return null;
    }

    /// <summary>
    /// Find first transform under root whose name matches (direct or nested).
    /// </summary>
    private static Transform FindRecursive(Transform root, string name)
    {
        if (root == null || string.IsNullOrEmpty(name)) return null;
        if (root.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            return root;
        for (int i = 0; i < root.childCount; i++)
        {
            var found = FindRecursive(root.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }

    /// <summary>
    /// Play animation by state name on the given model's Animator (each model has its own Animator).
    /// </summary>
    private void ApplyAnimation(string stateName, Transform model)
    {
        if (model == null || string.IsNullOrEmpty(stateName)) return;

        var animator = model.GetComponent<Animator>();
        if (animator != null)
            animator.Play(stateName);
    }

    /// <summary>
    /// Apply transform override, or restore the model's default local transform when override is disabled.
    /// </summary>
    private void ApplyModelTransform(EffectEntry entry)
    {
        if (entry == null || entry.modelToShow == null) return;

        Transform model = entry.modelToShow;
        int id = model.GetInstanceID();

        if (entry.overrideModelTransform)
        {
            if (!_defaultModelTransforms.ContainsKey(id))
                _defaultModelTransforms[id] = (model.localPosition, model.localRotation, model.localScale);

            model.localPosition = entry.modelLocalPosition;
            model.localRotation = Quaternion.Euler(entry.modelLocalRotation);
            model.localScale = entry.modelLocalScale;
        }
        else if (_defaultModelTransforms.TryGetValue(id, out var defaultTransform))
        {
            model.localPosition = defaultTransform.position;
            model.localRotation = defaultTransform.rotation;
            model.localScale = defaultTransform.scale;
        }
    }

    /// <summary>
    /// Enable the selected model, disable its siblings (other models under the same parent).
    /// </summary>
    private void ApplySkin(Transform modelToShow)
    {
        if (modelToShow == null) return;

        Transform parent = modelToShow.parent;
        if (parent != null)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child != null)
                    child.gameObject.SetActive(child == modelToShow);
            }
        }

        modelToShow.gameObject.SetActive(true);
    }

    private void ApplyParticle(GameObject particleToEnable)
    {
        if (particleToEnable != null)
            particleToEnable.SetActive(true);
    }
}
