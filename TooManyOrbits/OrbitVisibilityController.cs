using KSP.Localization;
using System.ComponentModel;
using TooManyOrbits.Commands;
using TooManyOrbits.Filter;
using static TooManyOrbits.UI.RegisterToolbar;

namespace TooManyOrbits
{
	internal class OrbitVisibilityController : IVisibilityController
	{
		private readonly Configuration m_configuration;
		private readonly IFilter<Vessel> m_vesselFilter;
		private readonly IFilter<CelestialBody> m_celestialBodyFilter;

		private CommandSet m_hideOrbitCommands;

		public event Callback<bool> OnVisibilityChanged;

		public bool IsVisible { get; private set; }

		public OrbitVisibilityController(Configuration configuration)
		{
			m_configuration = configuration;
			m_configuration.PropertyChanged += OnConfigurationChanged;

			m_vesselFilter = new VesselFilter();
			m_celestialBodyFilter = new CelestialBodyFilter();
			m_hideOrbitCommands = new CommandSet();

			IsVisible = true;
		}

		public void Dispose()
		{
			m_configuration.PropertyChanged -= OnConfigurationChanged;
		}

		private void OnConfigurationChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
            Log.Info("OrbitVisibilityController.OnConvigurationChanged");
			if (!IsVisible)
			{
				Show();
				Hide();
			}
		}

		public void Show()
		{
            Log.Info("OrbitVisibilityController.Show");

            if (IsVisible)
			{
				return;
			}

			Log.Debug(Localizer.Format("#LOC_TMO_16"));
			m_hideOrbitCommands.Undo();
			m_hideOrbitCommands = new CommandSet();
			IsVisible = true;
			OnVisibilityChanged?.Invoke(true);
		}

		public void Hide()
		{
			Log.Info("Hide");
			if (!IsVisible)
			{
				//return;
			}

			Log.Debug(Localizer.Format("#LOC_TMO_17"));
			CreateHideVesselCommands(m_hideOrbitCommands);
			CreateHideBodiesCommands(m_hideOrbitCommands);
			m_hideOrbitCommands.Execute();
			IsVisible = false;
			OnVisibilityChanged?.Invoke(false);
		}

		public void Toggle()
		{
			if (IsVisible)
			{
				Hide();
			}
			else
			{
				Show();
			}
		}

		private void CreateHideVesselCommands(CommandSet commands)
		{
			var vessels = FlightGlobals.Vessels;
			for (int i = 0; i < vessels.Count; i++)
			{
				var vessel = vessels[i];
				var needCommand = m_configuration.HideVesselIcons | m_configuration.HideVesselOrbits;
				if (needCommand && m_vesselFilter.Accept(vessel))
				{
					var command = new HideOrbitCommand(vessel.orbitRenderer, m_configuration.HideVesselIcons, m_configuration.HideVesselOrbits);
					commands.Add(command);
				}
			}
		}

		private void CreateHideBodiesCommands(CommandSet commands)
		{
			var bodies = FlightGlobals.Bodies;
			for (int i = 0; i < bodies.Count; i++)
			{
				var body = bodies[i];
				var needCommand = m_configuration.HideCelestialBodyIcons | m_configuration.HideCelestialBodyOrbits;
				if (needCommand && m_celestialBodyFilter.Accept(body))
				{
					var command = new HideOrbitCommand(body.GetOrbitDriver().Renderer, m_configuration.HideCelestialBodyIcons, m_configuration.HideCelestialBodyOrbits);
					commands.Add(command);
				}
			}
		}

	}
}
