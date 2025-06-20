using KSP.Localization;
using System;
using System.Collections.Generic;
using UnityEngine;
using ClickThroughFix;
using static TooManyOrbits.UI.RegisterToolbar;

namespace TooManyOrbits.UI
{
	internal class ConfigurationWindow : IDisposable
	{
		const int WidthMaximized = 300;
		const int HeightMaximized = 250;
		const int WidthMinimized = 80;
		const int HeightMinimized = 35;

		private static readonly IList<KeyCode> AllowedKeyCodes = GetAllowedKeyCodes(); 

		private readonly int m_windowId = Guid.NewGuid().GetHashCode();
		private readonly string m_title;
		private readonly Configuration m_configuration;
		private readonly IVisibilityController m_visibilityController;
		private readonly Texture m_pencilTexture;
		private readonly Texture m_toolbarTexture;
		private readonly Texture m_greenToolbarTexture;
		private readonly Texture m_moveTexture;
		private readonly Texture m_expandTexture;
		private readonly Texture m_retractTexture;

		private GUIStyle m_textfieldStyle;
		private bool m_visible = false;
		private bool m_setKeyMode = false;
		private Rect m_position;
		private bool m_minimized;

		private int Width => m_minimized ? WidthMinimized : WidthMaximized;
		private int Height => m_minimized ? HeightMinimized : HeightMaximized;
		private Texture ToolbarIcon => m_visibilityController.IsVisible? m_toolbarTexture : m_greenToolbarTexture;
        

		public ConfigurationWindow(string title, Configuration configuration, IVisibilityController visibilityController, ResourceProvider resources)
		{
			m_title = title;
			m_configuration = configuration;
			m_visibilityController = visibilityController;
			m_pencilTexture = resources.PencilIcon;
			m_toolbarTexture = resources.ToolbarIcon;
			m_greenToolbarTexture = resources.GreenToolbarIcon;
			m_moveTexture = resources.MoveIcon;
			m_expandTexture = resources.ExpandIcon;
			m_retractTexture = resources.RetractIcon;
			RestoreConfiguration();
		}

		public void Dispose()
		{
			SaveConfiguration();
		}

		public void Show()
		{
            Log.Info("ConfigurationWindow.Show");
			m_visible = true;
		}

		public void Hide()
		{
            Log.Info("ConfigurationWindow.Hide");
            m_visible = false;
		}

		public void Minimize()
		{
			m_minimized = true;
			RecalculateWindowPosition();
		}

		public void Maximize()
		{
			m_minimized = false;
			RecalculateWindowPosition();
		}

		public void Draw()
		{
            //Log.Info("Draw, m_minimized: " + m_minimized);
			if (m_visible)
			{
				DrawWindow();
			}
		}

		private void DrawWindow()
		{
            //Log.Info("DrawWindow, m_minimized: " + m_minimized);
			if (m_minimized)
			{
				m_position = ClickThruBlocker.GUILayoutWindow(m_windowId, m_position, DrawMinimizedWindow, string.Empty);
			}
			else
			{
				m_position = ClickThruBlocker.GUILayoutWindow(m_windowId, m_position, DrawMaximizedWindow, m_title);
			}
		}

		private void DrawMaximizedWindow(int windowId)
		{
			GUILayout.BeginVertical();
			DrawWindowButtons();

			bool enabled = !m_visibilityController.IsVisible;
			bool shouldEnable = GUILayout.Toggle(enabled, Localizer.Format("#LOC_TMO_33"));
			if (enabled != shouldEnable)
			{
				m_visibilityController.Toggle();
			}

			GUILayout.Space(20);
			m_configuration.HideVesselIcons = GUILayout.Toggle(m_configuration.HideVesselIcons, Localizer.Format("#LOC_TMO_34"));
			m_configuration.HideVesselOrbits = GUILayout.Toggle(m_configuration.HideVesselOrbits, Localizer.Format("#LOC_TMO_35"));
			m_configuration.HideCelestialBodyIcons = GUILayout.Toggle(m_configuration.HideCelestialBodyIcons, Localizer.Format("#LOC_TMO_36"));
			m_configuration.HideCelestialBodyOrbits = GUILayout.Toggle(m_configuration.HideCelestialBodyOrbits, Localizer.Format("#LOC_TMO_37"));
			GUILayout.Space(20);
			DrawKeyBinding();
			GUILayout.EndVertical();

			GUI.DragWindow();
		}

		private void DrawMinimizedWindow(int windowId)
		{
			const int windowPadding = 5;
			const int buttonMargin = 5;

			const int toggleButtonSize = 50;
			const int controlButtonSize = 20;

			const int contentWidth = 2 * windowPadding + toggleButtonSize - 20;
			const int contentHeight = 2 * windowPadding + toggleButtonSize - 47;

			// reserve space
			GUILayoutUtility.GetRect(contentWidth, contentHeight);

			var toggleButtonRect = new Rect(0, 0, toggleButtonSize, toggleButtonSize);
			if (GUI.Button(toggleButtonRect, ToolbarIcon, GUIStyle.none))
			{
				m_visibilityController.Toggle();
			}

			var moveIconRect = new Rect(toggleButtonRect.xMax, toggleButtonRect.y + buttonMargin, controlButtonSize, controlButtonSize);
			GUI.DrawTexture(moveIconRect, m_moveTexture);

			const int maximizeButtonPadding = 5;
			const int maximizeButtonSize = controlButtonSize - 2 * maximizeButtonPadding;
			var maximizeButtonRect = new Rect(moveIconRect.x + maximizeButtonPadding, moveIconRect.yMax + buttonMargin + maximizeButtonPadding, maximizeButtonSize, maximizeButtonSize);
			if (GUI.Button(maximizeButtonRect, m_expandTexture, GUIStyle.none))
			{
				Maximize();
			}

			GUI.DragWindow();
		}

		private void DrawKeyBinding()
		{
			const int buttonWidth = 22;

			if (m_textfieldStyle == null) // must reside in a function called by OnGui
			{
                #region NO_LOCALIZATION
                m_textfieldStyle = GUI.skin.GetStyle("TextField");
                #endregion
            }

            GUILayout.BeginHorizontal();
			GUILayout.Label(Localizer.Format("#LOC_TMO_38"));
			GUILayout.Space(10);

			if (m_setKeyMode)
			{
				GUILayout.Label(Localizer.Format("#LOC_TMO_39"), m_textfieldStyle);
				KeyCode? newKeyCode = GetNextPressedKey();

				if (newKeyCode.HasValue)
				{
					m_configuration.ToggleKey = newKeyCode.Value;
					m_setKeyMode = false;
				}

				if (GUILayout.Button("X", GUILayout.Width(buttonWidth), GUILayout.Height(buttonWidth)))
				{
					m_setKeyMode = false;
				}
			}
			else
			{
				GUILayout.Label(m_configuration.ToggleKey.ToString(), m_textfieldStyle);
				m_setKeyMode = GUILayout.Button(m_pencilTexture, GUILayout.Width(buttonWidth), GUILayout.Height(buttonWidth));
			}
			GUILayout.EndHorizontal();
		}

		private void DrawWindowButtons()
		{
			const int offset = 5;
			const int size = 20;

			var position = new Rect(m_position.width - size - offset, offset, size, size);
			if (GUI.Button(position, "X"))
			{
				Hide();
			}

			position.x -= size + offset;
			if (GUI.Button(position, m_retractTexture))
			{
				Minimize();
			}
		}

		private Rect CalculateCenterPosition()
		{
			return new Rect((Screen.width - Width) / 2F, (Screen.height - Height) / 2F, Width, Height);
		}

		private KeyCode? GetNextPressedKey()
		{
			for (int i = 0; i < AllowedKeyCodes.Count; i++)
			{
				if (Input.GetKeyDown(AllowedKeyCodes[i]))
				{
					return AllowedKeyCodes[i];
				}
			}
			return null;
		}

		private void SaveConfiguration()
		{
			m_configuration.WindowPositionX = Mathf.FloorToInt(m_position.xMin);
			m_configuration.WindowPositionY = Mathf.FloorToInt(m_position.yMin);
			m_configuration.WindowMinimized = m_minimized;
		}

		private void RestoreConfiguration()
		{
			m_minimized = m_configuration.WindowMinimized;

			int x = m_configuration.WindowPositionX;
			int y = m_configuration.WindowPositionY;
			SetWindowPosition(x, y);
		}

		private void RecalculateWindowPosition()
		{
			SetWindowPosition((int)m_position.x, (int)m_position.y);
		}

		private void SetWindowPosition(int x, int y)
		{
			if (x < 0 || y < 0) // position invalid or not set
			{
				m_position = CalculateCenterPosition();
			}
			else
			{
				m_position = new Rect(x, y, Width, Height);

				if (m_position.xMax > Screen.width)
				{
					m_position.x = Screen.width - Width;
				}
				if (m_position.yMax > Screen.height)
				{
					m_position.y = Screen.height - Height;
				}
			}
		}

		private static IList<KeyCode> GetAllowedKeyCodes()
		{
			var keys = new List<KeyCode>(128);

			foreach (var value in Enum.GetValues(typeof (KeyCode)))
			{
				var keyCode = (KeyCode)value;
                #region NO_LOCALIZATION
                if (!keyCode.ToString().StartsWith("Mouse")
				    && keyCode != KeyCode.Escape)
				{
					keys.Add(keyCode);
				}
                #endregion
            }

            return keys;
		}
	}
}
