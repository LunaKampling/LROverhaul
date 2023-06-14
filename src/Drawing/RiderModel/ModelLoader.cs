﻿using System;
using System.Collections.Generic;
using System.Drawing;

namespace linerider.Drawing.RiderModel
{
    internal abstract class ModelLoader
    {
        public Resources Model;

        public void Load(Resources model)
        {
            Model = model;

            ApplyScarfColorsToBody();
            ApplyRope();
            ApplyRects();
            ApplySprites();
        }

        protected virtual void ApplyScarfColorsToBody()
        {
            if (!ScarfColors.AreEmpty())
            {
                if (Model.HasRegions)
                    ApplyRegions();

                if (Model.HasPalette)
                    ApplyPalette();
            }
        }

        protected virtual void ApplyRope() => throw new NotImplementedException();
        protected virtual void ApplyRects() => throw new NotImplementedException();

        protected virtual void ApplySprites() => Models.SetSprites(Model.Body, Model.BodyDead, Model.Sled, Model.SledBroken, Model.Arm, Model.Leg);

        private void ApplyPalette()
        {
            Dictionary<Color, Color> colorsMap = new Dictionary<Color, Color>();

            for (int i = 0; i < Model.Palette.Width; i++)
            {
                Color paletteColor = Model.Palette.GetPixel(i, 0);
                paletteColor = Color.FromArgb(255, paletteColor);

                Color scarfColor = Color.FromArgb(ScarfColors.GetColorList()[i % ScarfColors.Count()]);
                scarfColor = Color.FromArgb(255, scarfColor);

                colorsMap[paletteColor] = scarfColor;
            }

            for (int x = 0; x < Model.Body.Width; x++)
            {
                for (int y = 0; y < Model.Body.Height; y++)
                {
                    Color bodyColor = Model.Body.GetPixel(x, y);
                    Color bodyDeadColor = Model.BodyDead.GetPixel(x, y);

                    if (colorsMap.ContainsKey(bodyColor))
                        Model.Body.SetPixel(x, y, colorsMap[bodyColor]);

                    if (colorsMap.ContainsKey(bodyDeadColor))
                        Model.BodyDead.SetPixel(x, y, colorsMap[bodyDeadColor]);
                }
            }

            ScarfColors.Shift(ScarfColors.Count() * Model.Palette.Width - Model.Palette.Width);

        }
        private void ApplyRegions()
        {
            for (int i = 0; i < Model.RegionsBody.Count; i++)
            {
                Rectangle region = Model.RegionsBody[i];
                Color color = Color.FromArgb(ScarfColors.GetColorList()[i % ScarfColors.Count()]);
                color = Color.FromArgb(255, color); // Add 255 alpha

                SolidBrush brush = new SolidBrush(color);
                using (Graphics g = Graphics.FromImage(Model.Body))
                    g.FillRectangle(brush, region);
            }
            for (int i = 0; i < Model.RegionsBodyDead.Count; i++)
            {
                Rectangle region = Model.RegionsBodyDead[i];
                Color color = Color.FromArgb(ScarfColors.GetColorList()[i % ScarfColors.Count()]);
                color = Color.FromArgb(255, color); // Add 255 alpha

                SolidBrush brush = new SolidBrush(color);
                using (Graphics g = Graphics.FromImage(Model.BodyDead))
                    g.FillRectangle(brush, region);
            }

            ScarfColors.Shift(ScarfColors.Count() * Model.RegionsBody.Count - Model.RegionsBody.Count);

        }
    }
}
