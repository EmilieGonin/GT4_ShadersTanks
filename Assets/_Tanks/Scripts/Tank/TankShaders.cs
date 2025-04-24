using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankShaders : MonoBehaviour
{
    [SerializeField] private GameObject _rendererParent;

    [Header("Damage Shader")]
    [SerializeField] private Material _tankDamage;

    [Header("Damage Shader")]
    [SerializeField] private GameObject m_hitTank;

    [Header("Fire Ball Shader")]
    [SerializeField] private GameObject m_fireBallShader;
    [SerializeField] private float m_ShootScale = 0.1f;
    [SerializeField] private float m_offset = 0.5f;
    [SerializeField] private GameObject m_explosionVFX;

    [Header("Spawn / Removed Shader")]
    [SerializeField] private int _actionEnableTankDuration;
    [SerializeField] private Material _tankEnable;
    [SerializeField] private GameObject _teleporter;

    private readonly Dictionary<Renderer, Material[]> _originalMaterials = new();
    private readonly List<Renderer> _renderers = new();

    private const string TANK_COLOR = "_TankColor";
    private const string TANK_SMOOTHNESS = "_TankSmoothness";
    private const string TANK_ANIMATION_PROGRESS = "_TankAnimationProgress";
    private const string TANK_METALLIC = "_TankMetallic";

    private float _maxHeight;
    private float m_currentLoadingTime;

    private void Awake()
    {
        MeshRenderer[] renderers = _rendererParent.GetComponentsInChildren<MeshRenderer>();

        foreach (var renderer in renderers)
        {
            _renderers.Add(renderer);
        }

        _maxHeight = GetComponent<BoxCollider>().size.y + 0.25f;
    }
    public void ShowSpawn(Action action = null)
    {
        MoveTeleporterY(0f);

        Show(_tankEnable);
        StartCoroutine(SetAnimationEnableProgress(false, action));
    }

    public void ShowDamage()
    {
        Show(_tankDamage);
        StartCoroutine(ResetToDefaultMaterial());
    }

    public void ShowDeath(Action action = null)
    {
        MoveTeleporterY(_maxHeight);

        Show(_tankEnable);
        StartCoroutine(SetAnimationEnableProgress(true, action));
    }

    private void MoveTeleporterY(float newY)
    {
        Vector3 newPos = _teleporter.transform.position;
        newPos.y = newY;
        _teleporter.transform.position = newPos;
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
                if (renderer.sharedMaterials[i].HasProperty("_Color"))
                {
                    newMats[i].SetColor(TANK_COLOR, renderer.sharedMaterials[i].color);
                }
                if (renderer.sharedMaterials[i].HasProperty("_Smoothness"))
                {
                    newMats[i].SetFloat(TANK_SMOOTHNESS, renderer.sharedMaterials[i].GetFloat("_Smoothness"));
                }
                if (renderer.sharedMaterials[i].HasProperty("_Metallic"))
                {
                    newMats[i].SetFloat(TANK_METALLIC, renderer.sharedMaterials[i].GetFloat("_Metallic"));
                }
            }

            renderer.sharedMaterials = newMats;
        }
    }

    public void UpdateFireBall(float MaxChargeTime)
    {
        if (m_currentLoadingTime == 0f)
        {
            m_explosionVFX.SetActive(false);
            m_fireBallShader.SetActive(true);
            m_fireBallShader.transform.localScale = Vector3.zero;
        }

        m_currentLoadingTime += Time.deltaTime;
        float progress = m_currentLoadingTime / MaxChargeTime;
        float scaleValue = Mathf.Lerp(0f, m_ShootScale, progress + m_offset);

        m_fireBallShader.transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
    }
    public void StopFireBall()
    {
        m_fireBallShader.SetActive(false);
        m_currentLoadingTime = 0f;
        m_explosionVFX.SetActive(true);
    }

    private IEnumerator SetAnimationEnableProgress(bool invertProgress = false, Action action = null)
    {
        _teleporter.SetActive(true);

        float currentTime = 0f;
        while (currentTime < _actionEnableTankDuration)
        {
            float progress = currentTime / _actionEnableTankDuration;

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

            float newY = Mathf.Lerp(0f, _maxHeight, progress);
            MoveTeleporterY(newY);

            currentTime += Time.deltaTime;
            yield return null;
        }

        _teleporter.SetActive(false);

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