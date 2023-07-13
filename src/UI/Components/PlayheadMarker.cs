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
using System;
using System.Diagnostics;
using System.Drawing;

namespace linerider.UI.Components
{
    public class PlayheadMarker : Gwen.ControlInternal.Dragger
    {
        private Texture m_texture;
        private readonly byte Alpha = 255;
        public int Frame
        {
            get
            {
                int left = X - Margin.Left;
                double w = (double)_owner.Width - (Margin.Width + Width);
                double perc = left / w;
                Debug.Assert(perc >= 0, "Playhead marker frame cannot be < 0%");
                Debug.Assert(perc <= 1, "Playhead marker frame cannot be > 100%");
                int frame = (int)Math.Round(_owner.Max * perc);
                return frame;
            }
        }
        private readonly Playhead _owner;
        public PlayheadMarker(Playhead owner)
            : base(owner)
        {
            _owner = owner;
            m_Target = this;
            RestrictToParent = true;
            IsTabable = false;
            KeyboardInputEnabled = false;
        }
        public void SetImage(Bitmap bmp)
        {
            m_texture?.Dispose();

            Texture tx = new Texture(Skin.Renderer);
            Gwen.Renderer.OpenTK.LoadTextureInternal(tx, bmp);
            m_texture = tx;
            Size = bmp.Size;
        }
        protected override void Render(Gwen.Skin.SkinBase skin)
        {
            skin.Renderer.DrawColor = Color.FromArgb(Alpha, 255, 255, 255);
            Rectangle bounds = RenderBounds;
            bounds.Y = bounds.Y + Height / 2 - m_texture.Height / 2;
            bounds.X = bounds.X + Width / 2 - m_texture.Width / 2;
            bounds.Height = m_texture.Height;
            bounds.Width = m_texture.Width;
            skin.Renderer.DrawTexturedRect(m_texture, bounds);
        }
        public override void Dispose()
        {
            m_texture?.Dispose();
            base.Dispose();
        }
    }
}