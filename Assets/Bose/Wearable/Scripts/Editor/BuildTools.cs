using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif

namespace Bose.Wearable.Editor
{
	public static class BuildTools
	{
		// Shared
		private const string AppVersion = "1.0";

		// Wearable Demo
		private const string WearableDemoProductName = "Wearable Demo";
		private const string WearableDemoAppIdentifier = "com.bose.demo.wearableunity";
		private const string WearableDemoIconGuid = "e06b243adbc49564b8ba586a2a0ed2d0";

		private const string RootSceneGuid = "b100476eb79061246a7d53542a204e54";
		private const string MainMenuSceneGuid = "814d265ed5a714b2f8a496b0e00010e1";
		private const string BasicDemoSceneGuid = "e822a72393d35429f941bfee942e76f4";
		private const string AdvancedDemoSceneGuid = "422b6c809820b4a78b2c60a058c8a7b4";

		private static readonly string[] WearableDemoSceneGuids =
		{
			RootSceneGuid,
			MainMenuSceneGuid,
			BasicDemoSceneGuid,
			AdvancedDemoSceneGuid
		};

		// Proxy Server
		private const string ProxyServerProductName = "Proxy Server";
		private const string ProxyServerAppIdentifier = "com.bose.demo.wearableproxyserver";
		private const string ProxyAppIconGuid = "f8f31fc86e17d1b489c1e409610b715a";

		private const string ProxyServerSceneGuid = "be6d1a5a9c2994033954c8265229c4e8";

		// Build
		private const string SwitchingPlatformMessage = "Switching to the {0} platform, please try again in a moment.";
		private const string CannotBuildErrorMessage = "[Bose Wearable] Cannot build the {0} for {1} as component " +
		                                               "support for that platform is not installed. Please " +
		                                               "install this component to continue, stopping build...";
		private const string CannotBuildMissingSceneErrorMessage = "[Bose Wearable] Could not find a scene for " +
		                                                           "the {0}, stopping build";
		private const string CannotFindAppIcon = "[Bose Wearable] Could not find the application icon for the Bose Wearable " +
		                                         "example content.";
		private const string BuildSucceededMessage = "[Bose Wearable] {0} Build Succeeded!";
		private const string BuildFailedMessage = "[Bose Wearable] {0} Build Failed! {1}";

		/// <summary>
		/// An editor-pref key for where the user last selected the build location.
		/// </summary>
		private const string BuildLocationPreferenceKey = "bose_wearable_pref_key";

		// Folder Picker
		private const string FolderPickerTitle = "Build Location";

		// Menu Items
		private const string BuildProxyServerMenuItem = "Tools/Bose Wearable/Build Proxy Server";
		private const string BuildWearableDemoMenuItem = "Tools/Bose Wearable/Build Wearable Demo";

		[MenuItem(BuildProxyServerMenuItem)]
		private static void BuildProxyServer()
		{
			BuildProxyServer(EditorUserBuildSettings.activeBuildTarget);
		}

		[MenuItem(BuildProxyServerMenuItem, validate = true)]
		private static bool IsSupportedPlatformForProxyServer()
		{
			return EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;
		}

		[MenuItem(BuildWearableDemoMenuItem)]
		private static void BuildWearableDemo()
		{
			BuildWearableDemo(EditorUserBuildSettings.activeBuildTarget);
		}

		[MenuItem(BuildWearableDemoMenuItem, validate = true)]
		private static bool IsSupportedPlatformForWearableDemo()
		{
			return EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;
		}

		/// <summary>
		/// Builds the proxy server for the specified <see cref="BuildTarget"/> <paramref name="buildTarget"/>
		/// </summary>
		/// <param name="buildTarget"></param>
		private static void BuildProxyServer(BuildTarget buildTarget)
		{
			// Check for player support
			if (!CanBuildTarget(buildTarget))
			{
				Debug.LogErrorFormat(CannotBuildErrorMessage, ProxyServerProductName, buildTarget);
				return;
			}

			var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
			if (EditorUserBuildSettings.activeBuildTarget != buildTarget)
			{
				Debug.LogFormat(SwitchingPlatformMessage, buildTarget);

				EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);
				return;
			}

			// Check for required scenes and other assets
			var scenePath = AssetDatabase.GUIDToAssetPath(ProxyServerSceneGuid);
			if (string.IsNullOrEmpty(scenePath))
			{
				Debug.LogErrorFormat(CannotBuildMissingSceneErrorMessage, ProxyServerProductName);
				return;
			}

			// Get folder path from the user for the build
			var folderPath = GetBuildLocation(ProxyServerProductName);
			if (string.IsNullOrEmpty(folderPath))
			{
				return;
			}

			// Cache values for the current Player Settings
			var originalProductName = PlayerSettings.productName;
			var bundleVersion = PlayerSettings.bundleVersion;
			var appId = PlayerSettings.GetApplicationIdentifier(buildTargetGroup);
			var iconGroup = PlayerSettings.GetIconsForTargetGroup(buildTargetGroup);

			// Override Player Settings for this build.
			PlayerSettings.productName = ProxyServerProductName;
			PlayerSettings.bundleVersion = AppVersion;
			PlayerSettings.SetApplicationIdentifier(buildTargetGroup, ProxyServerAppIdentifier);
			TrySetAppIcons(ProxyAppIconGuid, buildTargetGroup);
			AssetDatabase.SaveAssets();

			// Attempt to build the app
			var buildPlayerOptions = new BuildPlayerOptions
			{
				scenes = new[] { scenePath },
				locationPathName = folderPath,
				target = buildTarget
			};

			var buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);

			#if UNITY_2018_1_OR_NEWER
			if (buildReport.summary.result == BuildResult.Succeeded)
			#else
			if (string.IsNullOrEmpty(buildReport))
			#endif
			{
				Debug.LogFormat(BuildSucceededMessage, ProxyServerProductName);
			}
			else
			{
				Debug.LogFormat(BuildFailedMessage, ProxyServerProductName, buildReport);
			}

			// Reset all PlayerSetting changes back to their original values.
			PlayerSettings.productName = originalProductName;
			PlayerSettings.bundleVersion = bundleVersion;
			PlayerSettings.SetApplicationIdentifier(buildTargetGroup, appId);
			PlayerSettings.SetIconsForTargetGroup(buildTargetGroup, iconGroup);
			AssetDatabase.SaveAssets();
		}

		/// <summary>
		/// Builds the Wearable Demo for the specified <see cref="BuildTarget"/> <paramref name="buildTarget"/>
		/// </summary>
		/// <param name="buildTarget"></param>
		private static void BuildWearableDemo(BuildTarget buildTarget)
		{
			// Check for player support
			if (!CanBuildTarget(buildTarget))
			{
				Debug.LogErrorFormat(CannotBuildErrorMessage, WearableDemoProductName, buildTarget);
				return;
			}

			var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
			if (EditorUserBuildSettings.activeBuildTarget != buildTarget)
			{
				Debug.LogFormat(SwitchingPlatformMessage, buildTarget);

				EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);
				return;
			}

			// Get folder path from the user for the build
			var folderPath = GetBuildLocation(WearableDemoProductName);
			if (string.IsNullOrEmpty(folderPath))
			{
				return;
			}

			// Check for required scenes and other assets
			var sceneAssetPaths = new string[WearableDemoSceneGuids.Length];
			for (var i = 0; i < WearableDemoSceneGuids.Length; i++)
			{
				sceneAssetPaths[i] = AssetDatabase.GUIDToAssetPath(WearableDemoSceneGuids[i]);
				if (string.IsNullOrEmpty(sceneAssetPaths[i]))
				{
					Debug.LogErrorFormat(CannotBuildMissingSceneErrorMessage, WearableDemoProductName);
					return;
				}
			}

			// Cache values for the current Player Settings
			var originalProductName = PlayerSettings.productName;
			var bundleVersion = PlayerSettings.bundleVersion;
			var appId = PlayerSettings.GetApplicationIdentifier(buildTargetGroup);
			var iconGroup = PlayerSettings.GetIconsForTargetGroup(buildTargetGroup);

			// Override Player Settings for this build.
			PlayerSettings.productName = WearableDemoProductName;
			PlayerSettings.bundleVersion = AppVersion;
			PlayerSettings.SetApplicationIdentifier(buildTargetGroup, WearableDemoAppIdentifier);
			TrySetAppIcons(WearableDemoIconGuid, buildTargetGroup);
			AssetDatabase.SaveAssets();

			// Attempt to build the app
			var buildPlayerOptions = new BuildPlayerOptions
			{
				scenes = sceneAssetPaths,
				locationPathName = folderPath,
				target = buildTarget
			};

			var buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
			#if UNITY_2018_1_OR_NEWER
			if (buildReport.summary.result == BuildResult.Succeeded)
			#else
			if (string.IsNullOrEmpty(buildReport))
			#endif
			{
				Debug.LogFormat(BuildSucceededMessage, WearableDemoProductName);
			}
			else
			{
				Debug.LogFormat(BuildFailedMessage, WearableDemoProductName, buildReport);
			}

			// Reset all PlayerSetting changes back to their original values.
			PlayerSettings.productName = originalProductName;
			PlayerSettings.bundleVersion = bundleVersion;
			PlayerSettings.SetApplicationIdentifier(buildTargetGroup, appId);
			PlayerSettings.SetIconsForTargetGroup(buildTargetGroup, iconGroup);
			AssetDatabase.SaveAssets();
		}

		/// <summary>
		/// Returns true or false depending on whether the local Unity Editor can build
		/// the desired BuildTarget based on whether or not the player support has been
		/// installed.
		/// </summary>
		/// <param name="buildTarget"></param>
		/// <returns></returns>
		private static bool CanBuildTarget(BuildTarget buildTarget)
		{
			var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);

			#if UNITY_2018_1_OR_NEWER

			return BuildPipeline.IsBuildTargetSupported(buildTargetGroup, buildTarget);

			#else

			try
			{
				// IsBuildTargetSupported is an internal method of BuildPipeline on 2017.4 so we must
				// use reflection in order to access it.
				var internalMethod = typeof(BuildPipeline).GetMethod(
					"IsBuildTargetSupported",
					BindingFlags.NonPublic | BindingFlags.Static);

				var result = internalMethod.Invoke(null, new object[] { buildTargetGroup, buildTarget });

				return (bool)result;
			}
			// Default to true if we cannot programmatically determine player support in the editor.
			catch (Exception e)
			{
				Debug.LogError(e);
				return true;
			}

			#endif
		}

		/// <summary>
		/// Get a build location from the user via a dialog box. If the path is valid, it will be saved in the
		/// user's preferences for use next time as a suggestion.
		/// </summary>
		/// <returns></returns>
		private static string GetBuildLocation(string productName)
		{
			// Get folder path from the user for the build
			var startFolder = string.Empty;
			if (EditorPrefs.HasKey(BuildLocationPreferenceKey))
			{
				startFolder = EditorPrefs.GetString(BuildLocationPreferenceKey);
			}

			var panelTitle = string.Format("{0} for {1}", FolderPickerTitle, productName);
			var folderPath = EditorUtility.SaveFolderPanel(panelTitle, startFolder, productName);
			if (!string.IsNullOrEmpty(folderPath))
			{
				var directory = new DirectoryInfo(folderPath);
				if (directory.Parent != null)
				{
					var parentDirectory = directory.Parent;
					EditorPrefs.SetString(BuildLocationPreferenceKey, parentDirectory.FullName);
				}
			}

			return folderPath;
		}

		/// <summary>
		/// Attempt to use an <see cref="Texture2D"/> identified by <see cref="string"/> <paramref name="iconGuid"/> to
		/// override the App Icon settings for <see cref="BuildTargetGroup"/> <paramref name="buildTargetGroup"/>.
		/// </summary>
		/// <param name="iconGuid"></param>
		/// <param name="buildTargetGroup"></param>
		private static void TrySetAppIcons(string iconGuid, BuildTargetGroup buildTargetGroup)
		{
			var iconPath = AssetDatabase.GUIDToAssetPath(iconGuid);
			if (string.IsNullOrEmpty(iconPath))
			{
				Debug.LogWarning(CannotFindAppIcon);
			}
			else
			{
				var iconSizes = PlayerSettings.GetIconSizesForTargetGroup(buildTargetGroup);
				var iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
				var newIconGroup = new Texture2D[iconSizes.Length];
				for (var i = 0; i < newIconGroup.Length; i++)
				{
					newIconGroup[i] = iconTexture;
				}

				PlayerSettings.SetIconsForTargetGroup(buildTargetGroup, newIconGroup);
			}
		}
	}
}
