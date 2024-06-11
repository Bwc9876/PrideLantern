using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using UnityEngine;

namespace PrideLantern;

public class PrideLantern : ModBehaviour
{
	
	private static Color[] GetPrideArray() => new Color[]
	{
		new(1f, 0f, 0f),
		new(1f, 0.5f, 0f),
		new(1f, 1f, 0f),
		new(0f, 1f, 0f),
		new(0f, 0f, 1f),
		new(0.29f, 0f, 0.51f),
		new(0.93f, 0.51f, 0.93f),
		new(0.36f, 0.2f, 0.09f),
	};
	
	private static Color[] _colors = GetPrideArray();
	private static int _colorLength = _colors.Length;
	
	private static readonly int LanternFlameColorID = Shader.PropertyToID("_Color");
	private static readonly int MainLanternFlameTexID = Shader.PropertyToID("_MainTex");

	private static float _time;
	private static bool _active;
	private static Color _currentColor;
	private static List<OWRenderer> _renderers = new();

	private static Texture2D _grayscaleFlame;
	
	private const float STEP = 0.01f;

	private void Start() {
		_grayscaleFlame = ModHelper.Storage.Load<Texture2D>("assets/Greyscale-Lantern-Flame.png");

		// Example of accessing game code.
		LoadManager.OnCompleteSceneLoad += (scene, loadScene) => {
			if (loadScene != OWScene.SolarSystem) return;
			OnSolarSystemLoaded();
			_active = true;
		};

		LoadManager.OnStartSceneLoad += (scene, loadScene) =>
		{
			_active = false;
			_renderers = new();
			_time = 0f;
		};
	}

	private void OnSolarSystemLoaded()
	{
		ModHelper.Console.WriteLine("Loaded into solar system", MessageType.Info);
		
		//if (_grayscaleFlame == null) return;
		
		foreach (var lanternController in FindObjectsOfType<DreamLanternController>()) {
			var lantern = lanternController.gameObject;
			var realFlame = lantern.transform.Find("Props_IP_Artifact/Flame")?.GetComponent<OWRenderer>();
			var viewModelFlame = lantern.transform.Find("Props_IP_Artifact_ViewModel/Flame_ViewModel")
				?.GetComponent<OWRenderer>();
			
			if (realFlame != null)
			{
				_renderers.Add(realFlame);
			}
			if (viewModelFlame != null)
			{
				_renderers.Add(viewModelFlame);	
			}
		}
		
		ModHelper.Console.WriteLine($"Found {_renderers.Count} lantern flames", MessageType.Info);

		foreach (var renderer in _renderers)
		{
			renderer._renderer.material.SetTexture(MainLanternFlameTexID, _grayscaleFlame);
		}
	}

	private void Update()
	{
		if (!_active) return;
		
		_time += STEP;

		var colorA = _colors[Mathf.FloorToInt(_time) % _colorLength];
		var colorB = _colors[Mathf.CeilToInt(_time) % _colorLength];
		var lerpValue = _time % 1f;
		
		_currentColor = Color.Lerp(colorA, colorB, lerpValue);
		
		foreach (var renderer in _renderers)
		{
			renderer._renderer.sharedMaterial.SetColor(LanternFlameColorID, _currentColor);
		}
	}
	
}

