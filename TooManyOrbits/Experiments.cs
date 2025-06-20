#if false
using KSP.Localization;
using System.Reflection;
using UnityEngine;

namespace TooManyOrbits
{
	// just attempts to get things to work in this class
	internal static class Experiments
	{
		public static void LogTargetInfo(ITargetable target)
		{
			var method = typeof(OrbitRenderer).GetMethod(Localizer.Format("#LOC_TMO_2"), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			Color orbitColor2 = (Color)method.Invoke(target.GetOrbitDriver().Renderer, new object[0]);

			Log.Debug(Localizer.Format("#LOC_TMO_3") + target.GetName());
			Log.Debug(Localizer.Format("#LOC_TMO_4") + target.GetOrbitDriver().Renderer.orbitColor);
			Log.Debug(Localizer.Format("#LOC_TMO_5") + orbitColor2);
			Log.Debug(Localizer.Format("#LOC_TMO_6") + FlightGlobals.Vessels.Contains(target.GetVessel()));
			Log.Debug(Localizer.Format("#LOC_TMO_7") + (target.GetOrbitDriver().Renderer == target.GetVessel().orbitRenderer));
			Log.Debug(Localizer.Format("#LOC_TMO_8") + target.GetOrbitDriver().Renderer.drawMode);
			Log.Debug(Localizer.Format("#LOC_TMO_9") + target.GetOrbitDriver().Renderer.isActive);
			Log.Debug(Localizer.Format("#LOC_TMO_10") + target.GetOrbitDriver().Renderer.isFocused);
			Log.Debug(Localizer.Format("#LOC_TMO_11") + target.GetOrbitDriver().drawOrbit);
			Log.Debug(Localizer.Format("#LOC_TMO_12") + target.GetOrbitDriver().updateMode);
			Log.Debug(Localizer.Format("#LOC_TMO_13") + target.GetOrbitDriver().lastMode);
			Log.Debug(Localizer.Format("#LOC_TMO_14") + target.GetOrbitDriver().orbitColor);
			Log.Debug(Localizer.Format("#LOC_TMO_15") + (target.GetVessel().patchedConicRenderer != null));

		}

		#region Test 1: attempt to hide vessel target
		public static void HideTargetVessel_v1()
		{
			var target = FlightGlobals.ActiveVessel.targetObject;
			if (target != null)
			{
				var conicRenderer = target.GetVessel().patchedConicRenderer;
				if (conicRenderer != null)
				{
					conicRenderer.patchRenders.ForEach(r => r.Terminate());
					conicRenderer.flightPlanRenders.ForEach(r => r.Terminate());
					conicRenderer.enabled = false;
					//UnityEngine.Object.Destroy(conicRenderer);
				}

				var orbitTargeter = target.GetVessel().orbitTargeter;
				if (orbitTargeter != null)
				{
					UnityEngine.Object.Destroy(orbitTargeter);
				}

				UnityEngine.Object.Destroy(FlightGlobals.ActiveVessel.orbitTargeter);
			}
		}

		public static void ShowTargetVessel_v1()
		{
			var target = FlightGlobals.ActiveVessel.targetObject;
			if (target != null)
			{
				var conicRenderer = target.GetVessel().patchedConicRenderer;
				if (conicRenderer != null)
				{
					conicRenderer.enabled = true;
				}

				target.GetVessel().orbitTargeter = target.GetVessel().gameObject.AddComponent<OrbitTargeter>();
				FlightGlobals.ActiveVessel.orbitTargeter = FlightGlobals.ActiveVessel.gameObject.AddComponent<OrbitTargeter>();
			}
		}
		#endregion

		#region Test 2: attempt to hide vessel target
		public static void HideTargetVessel_v2()
		{
			var target = FlightGlobals.ActiveVessel.targetObject;
			if (target != null)
			{
				target.GetVessel().DetachPatchedConicsSolver();
			}
		}

		public static void ShowTargetVessel_v2()
		{
			var target = FlightGlobals.ActiveVessel.targetObject;
			if (target != null)
			{
				target.GetVessel().AttachPatchedConicsSolver();
			}
		}
		#endregion
	}
}
#endif
