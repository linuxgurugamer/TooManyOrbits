﻿using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using TooManyOrbits.UI;
using UnityEngine;
using ToolbarControl_NS;
using KSP.UI.Screens;


namespace TooManyOrbits
{

    public class TooManyOrbitsModule : TooManyOrbitsCoreModule
    { }

    [KSPAddon( KSPAddon.Startup.TrackingStation, false)]
    public class TooManyOrbitsTrackingStation : TooManyOrbitsCoreModule
    {

    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class TooManyOrbitsFlight : TooManyOrbitsCoreModule
    {

    }



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
			Log.Debug("Creating window");
			m_window = new ConfigurationWindow(ModName, m_configuration, m_visibilityController, resourceProvider);

			// setup toolbar button
			Log.Debug("Creating toolbar button");
            if (toolbarControl == null)
            {
                BuildButton();
            }

            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
                OnEnterMapView();
            else
            {
                // get notifcations when player changes to map view
                MapView.OnEnterMapView += OnEnterMapView;
                MapView.OnExitMapView += OnExitMapView;
            }
     
			// disable script until woken up by entering map view
			//enabled = false;
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
                    "tooManyOrbitsButton",
                    //rp.BuildPath("ToolbarIcon-Green-38", false),
                    resourceProvider.BuildPath("ToolbarIcon-38", false),
                    //rp.BuildPath("ToolbarIcon-Green-24", false),
                    resourceProvider.BuildPath("ToolbarIcon-24", false)

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
        
		private void OnDestroy()
		{
			Log.Info("Shutting down");
            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
                OnExitMapView();
            else
            {
                MapView.OnEnterMapView -= OnEnterMapView;
                MapView.OnExitMapView -= OnExitMapView;
            }

			Log.Debug("Disposing window");
			m_window.Dispose();

			Log.Debug("Disposing OrbitVisibilityController");
			m_visibilityController.OnVisibilityChanged -= OnOrbitVisibilityChanged;
			m_visibilityController.Dispose();

			Log.Debug("Writing configuration file");
			string configurationDirectory = Path.GetDirectoryName(ConfigurationFile);
			if (configurationDirectory != null && !Directory.Exists(configurationDirectory))
			{
				Directory.CreateDirectory(configurationDirectory);
			}
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
                if (m_visibilityController == null)
                {
                    m_visibilityController = new OrbitVisibilityController(m_configuration);
                    m_visibilityController.OnVisibilityChanged += OnOrbitVisibilityChanged;
                }
                m_visibilityController.Toggle();
			}
		}

		private void OnEnterMapView()
		{
			Log.Debug("OnEnterMapView Enabling...");
			enabled = true;

			if (!m_lastVisibilityState && m_visibilityController != null)
			{
				m_visibilityController.Hide();
			}
			//m_toolbarButton.Show();
		}

		private void OnExitMapView()
		{
			Log.Debug("OnExitMapView Disabling...");
			enabled = false;

			m_lastVisibilityState = m_visibilityController.IsVisible;
			m_visibilityController.Show();
			//m_toolbarButton.Hide();
		}

		private void OnOrbitVisibilityChanged(bool orbitsVisible)
		{
			const float duration = 1.5f;
			if (enabled)
			{
				var message = orbitsVisible ? "Orbits shown" : "Orbits hidden";
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
                Log.Error("m_window is null");
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
