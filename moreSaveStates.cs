using System;
using System.Collections;
using System.IO;
using System.Reflection;
using GlobalEnums;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Patches
{
	[Serializable]
	public class Keybinds
	{
		public Keybinds()
		{
			this.LoadStateButton = "f1";
			this.SaveStateButton = "f2";
		}
		public string LoadStateButton;
    
		public string SaveStateButton;
	}
  
	[Serializable]
	public struct SavedState
	{
		public string saveScene;

		public PlayerData savedPlayerData;

		public SceneData savedSceneData;

		public Vector3 savePos;
	}
  
	public static class SaveStateManager
	{
		public static void LoadKeybinds()
		{
			try
			{
				SaveStateManager.Keybinds = JsonUtility.FromJson<Keybinds>(File.ReadAllText(Application.persistentDataPath + "/minisavestates.json"));
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}

		private static IEnumerator LoadStateCoro(SavedState savedState)
		{
			PlayerData savedPd = savedState.savedPlayerData;
			SceneData savedSd = savedState.savedSceneData;
			string saveScene = savedState.saveScene;
			Vector3 savePos = savedState.savePos;
			if (savedPd == null || string.IsNullOrEmpty(saveScene))
			{
				yield break;
			}
			GameManager.instance.entryGateName = "dreamGate";
			GameManager.instance.startedOnThisScene = true;
			UnityEngine.SceneManagement.SceneManager.LoadScene("Room_Sly_Storeroom");
			yield return new WaitUntil(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Room_Sly_Storeroom");
			GameManager.instance.sceneData = (SceneData.instance = JsonUtility.FromJson<SceneData>(JsonUtility.ToJson(savedSd)));
			GameManager.instance.ResetSemiPersistentItems();
			yield return null;
			PlayerData.instance = (GameManager.instance.playerData = (HeroController.instance.playerData = JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(savedPd))));
			GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
			{
				SceneName = saveScene,
				HeroLeaveDirection = new GatePosition?(GatePosition.unknown),
				EntryGateName = "dreamGate",
				EntryDelay = 0f,
				WaitForSceneTransitionCameraFade = false,
				Visualization = GameManager.SceneLoadVisualizations.Default,
				AlwaysUnloadUnusedAssets = true
			});
			SaveStateManager.cameraGameplayScene.SetValue(GameManager.instance.cameraCtrl, true);
			GameManager.instance.cameraCtrl.PositionToHero(false);
			if (SaveStateManager.lockArea != null)
			{
				GameManager.instance.cameraCtrl.LockToArea(SaveStateManager.lockArea as CameraLockArea);
			}
			yield return new WaitUntil(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == saveScene);
			GameManager.instance.cameraCtrl.FadeSceneIn();
			HeroController.instance.TakeMP(1);
			HeroController.instance.AddMPChargeSpa(1);
			HeroController.instance.TakeHealth(1);
			HeroController.instance.AddHealth(1);
			HeroController.instance.geoCounter.geoTextMesh.text = savedPd.geo.ToString();
			GameCameras.instance.hudCanvas.gameObject.SetActive(true);
			SaveStateManager.cameraGameplayScene.SetValue(GameManager.instance.cameraCtrl, true);
			yield return null;
			HeroController.instance.gameObject.transform.position = savePos;
			HeroController.instance.transitionState = HeroTransitionState.WAITING_TO_TRANSITION;
			MethodInfo method = typeof(HeroController).GetMethod("FinishedEnteringScene", BindingFlags.Instance | BindingFlags.NonPublic);
			if (method != null)
			{
				method.Invoke(HeroController.instance, new object[]
				{
					true,
					false
				});
			}
			yield break;
		}

		public static void SaveStateSlots(int slots)
		{
			SavedState savedState = new SavedState
			{
				saveScene = GameManager.instance.GetSceneNameString(),
				savedPlayerData = PlayerData.instance,
				savedSceneData = SceneData.instance,
				savePos = HeroController.instance.gameObject.transform.position
			};
			try
			{
				switch (slots)
				{
				case 1:
					File.WriteAllText(Application.persistentDataPath + "/minisavestates-saved1.json", JsonUtility.ToJson(savedState));
					break;
				case 2:
					File.WriteAllText(Application.persistentDataPath + "/minisavestates-saved2.json", JsonUtility.ToJson(savedState));
					break;
				case 3:
					File.WriteAllText(Application.persistentDataPath + "/minisavestates-saved3.json", JsonUtility.ToJson(savedState));
					break;
				case 4:
					File.WriteAllText(Application.persistentDataPath + "/minisavestates-saved4.json", JsonUtility.ToJson(savedState));
					break;
				case 5:
					File.WriteAllText(Application.persistentDataPath + "/minisavestates-saved5.json", JsonUtility.ToJson(savedState));
					break;
				}
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
			SaveStateManager.lockArea = SaveStateManager.cameraLockArea.GetValue(GameManager.instance.cameraCtrl);
		}

		public static void LoadStateSlots(int slots)
		{
			SavedState savedState = default(SavedState);
			try
			{
				switch (slots)
				{
				case 1:
					savedState = JsonUtility.FromJson<SavedState>(File.ReadAllText(Application.persistentDataPath + "/minisavestates-saved1.json"));
					break;
				case 2:
					savedState = JsonUtility.FromJson<SavedState>(File.ReadAllText(Application.persistentDataPath + "/minisavestates-saved2.json"));
					break;
				case 3:
					savedState = JsonUtility.FromJson<SavedState>(File.ReadAllText(Application.persistentDataPath + "/minisavestates-saved3.json"));
					break;
				case 4:
					savedState = JsonUtility.FromJson<SavedState>(File.ReadAllText(Application.persistentDataPath + "/minisavestates-saved4.json"));
					break;
				case 5:
					savedState = JsonUtility.FromJson<SavedState>(File.ReadAllText(Application.persistentDataPath + "/minisavestates-saved5.json"));
					break;
				}
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
			GameManager.instance.StartCoroutine(SaveStateManager.LoadStateCoro(savedState));
		}

		private static object lockArea;

		private static readonly FieldInfo cameraGameplayScene = typeof(CameraController).GetField("isGameplayScene", BindingFlags.Instance | BindingFlags.NonPublic);

		private static FieldInfo cameraLockArea = typeof(CameraController).GetField("currentLockArea", BindingFlags.Instance | BindingFlags.NonPublic);

		public static Keybinds Keybinds = new Keybinds();

		public static int slots = 1;
	}
  
}

//changes in the GameManager file:
protected void Update()
	{
		this.orig_Update();
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			SaveStateManager.slots = 1;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			SaveStateManager.slots = 2;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			SaveStateManager.slots = 3;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			SaveStateManager.slots = 4;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			SaveStateManager.slots = 5;
		}
		if (Input.GetKeyDown(SaveStateManager.Keybinds.SaveStateButton))
		{
			SaveStateManager.SaveStateSlots(SaveStateManager.slots);
			return;
		}
		if (Input.GetKeyDown(SaveStateManager.Keybinds.LoadStateButton))
		{
			SaveStateManager.LoadStateSlots(SaveStateManager.slots);
		}
	}
