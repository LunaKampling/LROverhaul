//  Author:
//       Noah Ablaseau <nablaseau@hotmail.com>
//
//  Copyright (c) 2017 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using Gwen.Controls;
using linerider.UI.Components;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SkiaSharp;
using System;

namespace linerider.UI.Widgets
{
    public class LoadingSprite : Sprite
    {
        private readonly Color Color = Settings.Colors.StandardLine;

        public LoadingSprite(ControlBase canvas) : base(canvas)
        {
            IsTabable = false;
            KeyboardInputEnabled = false;
            MouseInputEnabled = false;

            // Double resolution for less janky sprite quality
            SKBitmap bitmap = SkiaUtils.LoadSVG(GameResources.ux_loading.Raw, 2);
            SetImage(bitmap);
        }
        protected override void Render(Gwen.Skin.SkinBase skin)
        {
            ((Gwen.Renderer.OpenTK)skin.Renderer).Flush();
            float rotation = Environment.TickCount % 1000 / 1000f;
            Vector3d trans = new(X + Width / 2, Y + Height / 2, 0);
            GL.Translate(Width / -4, Height / -4, 0);
            GL.PushMatrix();
            GL.Translate(trans);
            GL.Rotate(360 * rotation, Vector3d.UnitZ);
            GL.Scale(0.5, 0.5, 0);
            GL.Translate(-trans);
            skin.Renderer.DrawColor = Color.FromArgb(Alpha, Color);
            skin.Renderer.DrawTexturedRect(m_texture, RenderBounds);
            ((Gwen.Renderer.OpenTK)skin.Renderer).Flush();
            GL.PopMatrix();
        }
    }
}