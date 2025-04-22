using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankShaders : MonoBehaviour
{
    [SerializeField] private Material _tankDamage;

    private readonly Dictionary<Renderer, Material[]> _originalMaterials = new();
    private readonly List<Renderer> _renderers = new();

    private const string TANK_COLOR = "_TankColor";
    private const string TANK_SMOOTHNESS = "_TankSmoothness";
    private const string TANK_METALLIC = "_TankMetallic";

    private void Awake()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();

        foreach (var renderer in renderers)
        {
            _renderers.Add(renderer);
        }
    }

    public void ShowDamage()
    {
        foreach (var renderer in _renderers)
        {
            if (!_originalMaterials.ContainsKey(renderer))
                _originalMaterials[renderer] = renderer.sharedMaterials;

            var newMats = new Material[renderer.sharedMaterials.Length];

            for (int i = 0; i < newMats.Length; i++)
            {
                newMats[i] = new Material(_tankDamage);
                newMats[i].SetColor(TANK_COLOR, renderer.sharedMaterials[i].color);
                newMats[i].SetFloat(TANK_SMOOTHNESS, renderer.sharedMaterials[i].GetFloat("_Smoothness"));
                newMats[i].SetFloat(TANK_METALLIC, renderer.sharedMaterials[i].GetFloat("_Metallic"));
            }

            renderer.sharedMaterials = newMats;
        }

        StartCoroutine(ResetDamage());
    }

    private IEnumerator ResetDamage()
    {
        yield return new WaitForSeconds(0.1f);

        foreach (var item in _originalMaterials)
        {
            item.Key.sharedMaterials = item.Value;
        }
    }
}