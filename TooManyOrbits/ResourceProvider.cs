using KSP.Localization;
using UnityEngine;
using ToolbarControl_NS;
using static TooManyOrbits.UI.RegisterToolbar;

namespace TooManyOrbits
{
    internal class ResourceProvider
    {
        private readonly string m_resourcePath;
        private readonly string m_resourcePathNoGameData;


        public Texture ToolbarIcon => LoadTextureResource(Localizer.Format("#LOC_TMO_18"));
        public Texture GreenToolbarIcon => LoadTextureResource(Localizer.Format("#LOC_TMO_19"));
        public Texture PencilIcon => LoadTextureResource(Localizer.Format("#LOC_TMO_20"));
        public Texture ExpandIcon => LoadTextureResource(Localizer.Format("#LOC_TMO_21"));
        public Texture RetractIcon => LoadTextureResource(Localizer.Format("#LOC_TMO_22"));
        public Texture MoveIcon => LoadTextureResource(Localizer.Format("#LOC_TMO_23"));


        public ResourceProvider(string resourcePath)
        {
            m_resourcePath = resourcePath;
            if (!m_resourcePath.EndsWith("/"))
            {
                m_resourcePath += '/';
            }
            m_resourcePathNoGameData = m_resourcePath;
            m_resourcePath = KSPUtil.ApplicationRootPath + "GameData/" + m_resourcePath;


        }

        private Texture LoadTextureResource(string resourceName)
        {
            string path = BuildPath(resourceName);

            Texture2D texture = new Texture2D(2, 2);

            if (!ToolbarControl.LoadImageFromFile(ref texture, path))
            {
                Log.Error("Failed to load texture " + resourceName);
            }
            return texture;
        }

        #region NO_LOCALIZATION
        internal string BuildPath(string resourceName, bool gamedata = true)
        {
            if (gamedata)
                return m_resourcePath + "Images/" + resourceName;
            else
                return m_resourcePathNoGameData + "Images/" + resourceName;
        }
        #endregion
    }
}
