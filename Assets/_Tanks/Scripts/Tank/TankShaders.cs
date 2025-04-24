using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankShaders : MonoBehaviour
{
    [SerializeField] private Material _tankEnable;
    [SerializeField] private Material _tankDamage;

    [SerializeField] private int _actionDuration;

    private readonly Dictionary<Renderer, Material[]> _originalMaterials = new();
    private readonly List<Renderer> _renderers = new();

    private const string TANK_COLOR = "_TankColor";
    private const string TANK_SMOOTHNESS = "_TankSmoothness";
    private const string TANK_ANIMATION_PROGRESS = "_TankAnimationProgress";
    private const string TANK_METALLIC = "_TankMetallic";

    private void Awake()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();

        foreach (var renderer in renderers)
        {
            _renderers.Add(renderer);
        }
    }
    public void ShowSpawn(Action action = null)
    {
        Show(_tankEnable);
        StartCoroutine(SetAnimationProgress(false, action));
    }

    public void ShowDamage()
    {
        Show(_tankDamage);
        StartCoroutine(ResetToDefaultMaterial());
    }

    public void ShowDeath(Action action = null)
    {
        Show(_tankEnable);
        StartCoroutine(SetAnimationProgress(true, action));
    }

    private void Show(Material matRef)
    {
        foreach (var renderer in _renderers)
        {
            if (!_originalMaterials.ContainsKey(renderer))
                _originalMaterials[renderer] = renderer.sharedMaterials;

            var newMats = new Material[renderer.sharedMaterials.Length];

            for (int i = 0; i < newMats.Length; i++)
            {
                newMats[i] = new Material(matRef);
                newMats[i].SetColor(TANK_COLOR, renderer.sharedMaterials[i].color);
                newMats[i].SetFloat(TANK_SMOOTHNESS, renderer.sharedMaterials[i].GetFloat("_Smoothness"));
                newMats[i].SetFloat(TANK_METALLIC, renderer.sharedMaterials[i].GetFloat("_Metallic"));
            }

            renderer.sharedMaterials = newMats;
        }
    }

    private IEnumerator SetAnimationProgress(bool invertProgress = false, Action action = null)
    {
        float currentTime = 0f;
        while (currentTime < _actionDuration)
        {
            float progress = currentTime / _actionDuration;

            if (invertProgress)
            {
                progress = 1 - progress;
            }

            foreach (var renderer in _renderers)
            {
                foreach (var sharedMaterial in renderer.sharedMaterials)
                {
                    sharedMaterial.SetFloat(TANK_ANIMATION_PROGRESS, progress);
                }
            }

            currentTime += Time.deltaTime;
            yield return null;
        }

        yield return StartCoroutine(ResetToDefaultMaterial());

        action?.Invoke();
    }

    private IEnumerator ResetToDefaultMaterial()
    {
        yield return new WaitForSeconds(0.1f);

        foreach (var item in _originalMaterials)
        {
            item.Key.sharedMaterials = item.Value;
        }
    }
}