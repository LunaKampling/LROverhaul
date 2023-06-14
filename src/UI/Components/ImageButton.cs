﻿//  Author:
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

using Gwen;
using Gwen.Controls;
using System.Drawing;

namespace linerider.UI.Components
{
    public class ImageButton : Button
    {
        public byte Alpha = 255;
        private Texture tx1;
        private Texture _overridetex = null;
        private bool _override = false;
        protected Texture m_texture;

        public ImageButton(ControlBase canvas) : base(canvas) { AutoSizeToContents = false; }

        public override void Dispose()
        {
            tx1?.Dispose();
            _overridetex?.Dispose();
            base.Dispose();
        }
        public void SetImage(Bitmap bmp)
        {
            m_texture?.Dispose();
            Texture tx = new Texture(Skin.Renderer);

            Gwen.Renderer.OpenTK.LoadTextureInternal(tx, bmp);
            m_texture = tx;
            tx1 = tx;
        }
        public void DisableImageOverride() => _override = false;
        public void EnableImageOverride() => _override = true;
        public void SetOverride(Bitmap bitmap)
        {
            _overridetex?.Dispose();
            Texture tx = new Texture(Skin.Renderer);

            Gwen.Renderer.OpenTK.LoadTextureInternal(tx, bitmap);
            _overridetex = tx;
        }
        public override void SetImage(string textureName, bool center = false)
        {
            m_texture?.Dispose();
            m_texture = new Texture(Skin.Renderer);
            m_texture.Load(textureName);
        }

        protected override void Render(Gwen.Skin.SkinBase skin)
        {
            skin.Renderer.DrawColor = Color.FromArgb(IsDepressed ? 64 : (IsHovered ? 128 : Alpha), 255, 255, 255);
            if (_override && _overridetex != null)
            {
                skin.Renderer.DrawTexturedRect(_overridetex, RenderBounds);
            }
            else
            {
                skin.Renderer.DrawTexturedRect(m_texture, RenderBounds);
            }
        }
    }
}