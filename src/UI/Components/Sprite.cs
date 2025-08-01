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

using Gwen;
using Gwen.Controls;
using SkiaSharp;

namespace linerider.UI.Components
{
    public class Sprite : ControlBase
    {
        public byte Alpha = 255;
        protected Texture m_texture;
        public Sprite(ControlBase canvas)
            : base(canvas)
        {
            IsTabable = false;
            KeyboardInputEnabled = false;
            MouseInputEnabled = false;
        }

        public void SetImage(SKBitmap bmp)
        {
            m_texture?.Dispose();

            Texture tx = new(Skin.Renderer);
            Gwen.Renderer.OpenTK.LoadTextureInternal(tx, bmp);
            m_texture = tx;
            Size = new Size(bmp.Width, bmp.Height);
        }
        protected override void Render(Gwen.Skin.SkinBase skin)
        {
            skin.Renderer.DrawColor = Color.FromArgb(Alpha, 255, 255, 255);
            skin.Renderer.DrawTexturedRect(m_texture, RenderBounds);
        }
        public override void Dispose()
        {
            m_texture?.Dispose();
            base.Dispose();
        }
    }
}