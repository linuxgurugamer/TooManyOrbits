using KSP.Localization;
using KSP.UI.Screens;
using System.ComponentModel;
using System.IO;
using System.Collections;
using ToolbarControl_NS;
using TooManyOrbits.UI;
using UnityEngine;
using static TooManyOrbits.UI.RegisterToolbar;


namespace TooManyOrbits
{

	public class TooManyOrbitsModule : TooManyOrbitsCoreModule
	{ }


	[KSPAddon(KSPAddon.Startup.TrackingStation, false)]
	public class TooManyOrbitsTrackingStation:MonoBehaviour
	{
		void Start()
		{
			//TooManyOrbitsCoreModule.Instance.OnEnterMapView();
			StartCoroutine(WaitASec());
		}
		IEnumerator WaitASec()
		{
			yield return new WaitForSeconds(0.05f);
			TooManyOrbitsCoreModule.Instance.OnEnterMapView();
		}
		private void OnDestroy()
		{
			TooManyOrbitsCoreModule.Instance.OnExitMapView();
		}
	}

#if false
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class TooManyOrbitsFlight : TooManyOrbitsCoreModule
    {

    }

#endif

	[KSPAddon(KSPAddon.Startup.AllGameScenes, true)]

	public abstract class TooManyOrbitsCoreModule : MonoBehaviour
	{
		static internal TooManyOrbitsCoreModule Instance;

		public const string ModName = "TooManyOrbits";
		public const string ResourcePath = ModName + "/";

		//private string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
		private string ConfigurationFile => $"GameData/{ModName}/PluginData/{ModName}.cfg";

		private KeyCode m_toggleButton;
		internal static ToolbarControl toolbarControl;

		internal ConfigurationWindow m_window;
		private Configuration m_configuration;
		private IVisibilityController m_visibilityController;
		private bool m_lastVisibilityState = true;
		private bool m_skipUpdate = false;
		ResourceProvider resourceProvider;

		internal void Start()
		{
			Log.Info("TooManyOrbitsCoreModule.Start");
			Instance = this;

			resourceProvider = new ResourceProvider(ModName);
				m_configuration = ConfigurationParser.LoadFromFile(ConfigurationFile);
			m_configuration.PropertyChanged += OnConfigurationChanged;
			m_toggleButton = m_configuration.ToggleKey;

			m_visibilityController = new OrbitVisibilityController(m_configuration);
			m_visibilityController.OnVisibilityChanged += OnOrbitVisibilityChanged;

			// setup window
			Log.Debug(Localizer.Format("#LOC_TMO_24"));
			m_window = new ConfigurationWindow(ModName, m_configuration, m_visibilityController, resourceProvider);

			// setup toolbar button
			Log.Debug(Localizer.Format("#LOC_TMO_25"));
			if (toolbarControl == null)
			{
				BuildButton();
			}

#if false
			if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
                OnEnterMapView();
            else
#endif
			{
				// get notifcations when player changes to map view
				MapView.OnEnterMapView += OnEnterMapView;
				MapView.OnExitMapView += OnExitMapView;
			}

			GameEvents.onGameSceneSwitchRequested.Add(onGameSceneSwitchRequested);
			// disable script until woken up by entering map view
			//enabled = false;
			DontDestroyOnLoad(this);
		}

		internal const string MODID = "toomanyorbits_NS";
		internal const string MODNAME = "Too Many Orbits";
		private void BuildButton()
		{
			if (toolbarControl == null)
			{
				toolbarControl = gameObject.AddComponent<ToolbarControl>();
				toolbarControl.AddToAllToolbars(OnEnable2, OnDisabl2e,
					ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.TRACKSTATION,
					MODID,
					Localizer.Format("#LOC_TMO_26"),
					//rp.BuildPath("ToolbarIcon-Green-38", false),
					resourceProvider.BuildPath(Localizer.Format("#LOC_TMO_18"), false),
					//rp.BuildPath("ToolbarIcon-Green-24", false),
					resourceProvider.BuildPath(Localizer.Format("#LOC_TMO_27"), false),
					MODNAME
				);
			}
		}

		void OnEnable2()
		{
			Log.Info("OnEnable2");

			m_window.Show();
		}
		void OnDisabl2e()
		{
			Log.Info("OnDisabl2e");
			m_window.Hide();
		}

		// This should now only get called when the game is exiting
		private void OnDestroy()
		{
			Log.Info("Shutting down");
#if falsae
			if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
                OnExitMapView();
            else
#endif
			{
				MapView.OnEnterMapView -= OnEnterMapView;
				MapView.OnExitMapView -= OnExitMapView;
			}

			Log.Debug(Localizer.Format("#LOC_TMO_28"));
			m_window.Dispose();

			Log.Debug(Localizer.Format("#LOC_TMO_29"));
			m_visibilityController.OnVisibilityChanged -= OnOrbitVisibilityChanged;
			m_visibilityController.Dispose();

			Log.Debug(Localizer.Format("#LOC_TMO_30"));
			string configurationDirectory = Path.GetDirectoryName(ConfigurationFile);
			if (configurationDirectory != null && !Directory.Exists(configurationDirectory))
				Directory.CreateDirectory(configurationDirectory);
			//ConfigurationParser.SaveToFile(ConfigurationFile, m_configuration);
		}

		void onGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> fta)
        {
            string configurationDirectory = Path.GetDirectoryName(ConfigurationFile);
            if (configurationDirectory != null && !Directory.Exists(configurationDirectory))
                Directory.CreateDirectory(configurationDirectory);
            ConfigurationParser.SaveToFile(ConfigurationFile, m_configuration);
		}

		internal void Update()
		{
			if (m_skipUpdate)
			{
				m_skipUpdate = false;
				return;
			}

			if (Input.GetKeyDown(m_toggleButton))
			{
#if false
				if (m_visibilityController == null)
				{
					m_visibilityController = new OrbitVisibilityController(m_configuration);
					m_visibilityController.OnVisibilityChanged += OnOrbitVisibilityChanged;
				}
#endif
				m_visibilityController.Toggle();
			}
		}

        #region NO_LOCALIZATION
        internal void OnEnterMapView()
		{
			Log.Debug("OnEnterMapView Enabling...m_lastVisibilityState: " + m_lastVisibilityState);
			enabled = true;
			if (m_visibilityController == null)
				Log.Debug("OnEnterMapView, m_visibilityController is null");
			if (!m_lastVisibilityState && m_visibilityController != null)
			{
				m_visibilityController.Hide();
			}
		}

		internal void OnExitMapView()
		{
			Log.Debug("OnExitMapView Disabling...");
			enabled = false;

			m_lastVisibilityState = m_visibilityController.IsVisible;
			m_visibilityController.Show();
			//m_toolbarButton.Hide();
		}
        #endregion
        private void OnOrbitVisibilityChanged(bool orbitsVisible)
		{
			const float duration = 1.5f;
			if (enabled)
			{
				var message = orbitsVisible ? Localizer.Format("#LOC_TMO_31") : Localizer.Format("#LOC_TMO_32");
				ScreenMessages.PostScreenMessage(message, duration);
			}
		}

		internal void OnGUI()
		{
			if (m_window != null)
			{
				var originalSkin = GUI.skin;
				GUI.skin = HighLogic.Skin;

				m_window.Draw();

				GUI.skin = originalSkin;
			}
			else
				Log.Info("m_window is null");
		}

		private void OnConfigurationChanged(object sender, PropertyChangedEventArgs args)
		{
			if (args?.PropertyName == nameof(Configuration.ToggleKey))
			{
				m_toggleButton = m_configuration.ToggleKey;
				m_skipUpdate = true;
				Log.Info($"Changed toggle key to '{m_configuration.ToggleKey}'");
			}
		}
	}
}
