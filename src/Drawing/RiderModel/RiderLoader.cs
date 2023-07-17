using linerider.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace linerider.Drawing.RiderModel
{
    internal class RiderLoader
    {
        private static string currentBoshSkin = null;
        private static string currentScarf = null;

        // Update scarf or rider if needed
        public static void Validate()
        {
            bool needUpdateRider = currentBoshSkin != Settings.SelectedBoshSkin;
            bool needUpdateScarf = currentScarf != Settings.SelectedScarf;

            if (needUpdateRider)
                currentBoshSkin = Settings.SelectedBoshSkin;
            if (needUpdateScarf)
                currentScarf = Settings.SelectedScarf;

            if (needUpdateRider || needUpdateScarf)
                ReloadAll();
        }
        public static void ReloadAll()
        {
            ReloadScarf();
            ReloadModel();

            // Make segments between multiple scarves invisible
            for (int i = 1; i < Settings.multiScarfAmount; i++)
            {
                int index = i * Settings.multiScarfSegments + (i - 1) - (1 + i);
                ScarfColors.Insert(0x0000FF, 0x00, index);
            }
        }
        public static void ReloadScarf()
        {
            bool isDefaultScarf = Settings.SelectedScarf.Equals(Constants.InternalDefaultName);

            try
            {
                if (isDefaultScarf)
                {
                    ScarfColors.SetDefault();
                }
                else
                {
                    string scarfLocation = Path.Combine(Settings.Local.UserDirPath, Constants.ScarvesFolderName, Settings.SelectedScarf);
                    ScarfLoader loader = new ScarfLoader(scarfLocation);

                    ScarfColors.RemoveAll();

                    List<ScarfSegment> segments = loader.Load();
                    foreach (ScarfSegment segment in segments)
                        ScarfColors.Add(segment.Color, segment.Opacity);

                    ScarfColors.Normalize();
                }
            }
            catch
            {
                ScarfColors.SetDefault();
            }
        }
        public static void ReloadModel()
        {
            bool isDefaultSkin = Settings.SelectedBoshSkin == null || Settings.SelectedBoshSkin.Equals(Constants.InternalDefaultName);
            Resources riderRes = new ResourcesDefault();

            if (!isDefaultSkin)
                riderRes = new ResourcesCustom(Settings.SelectedBoshSkin);

            try
            {
                riderRes.Load();
            }
            catch (Exception e)
            {
                if (e is IOException || e is ArgumentException)
                {
                    Debug.WriteLine(e);
                    riderRes = new ResourcesDefault();
                    riderRes.Load();
                }
                else
                {
                    throw;
                }
            }

            ModelLoader loader = new ModelLoaderDynamic();

            if (riderRes.Legacy)
                loader = new ModelLoaderLegacy();

            loader.Load(riderRes);
        }
    }
}
