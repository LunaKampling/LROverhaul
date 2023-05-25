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

using System;
using System.Drawing;
using Gwen;
using Gwen.Controls;
using linerider.Utils;
using linerider.Tools;
using System.Windows.Forms;

namespace linerider.UI
{
    public class SwatchColorButton : Gwen.Controls.Button
    {
        public Color color = Color.Black;
        private Texture m_texture;
        private readonly LineType _linetype;
        protected override Color CurrentColor
        {
            get => Settings.NightMode ? Color.White : Color.Black;
        }
        private bool Selected
        {
            get => CurrentTools.SelectedTool.Swatch.Selected == _linetype;
        }

        public SwatchColorButton(ControlBase canvas, LineType linetype) : base(canvas)
        {
            var size = (Screen.PrimaryScreen.Bounds.Width / 1600 < Screen.PrimaryScreen.Bounds.Height / 1080) ? (Screen.PrimaryScreen.Bounds.Width / 1600) : (Screen.PrimaryScreen.Bounds.Height / 1080);
            if (size < 1) { size = 1; };
            AutoSizeToContents = false;
            SetSize(size*32, size*16);
            MinimumSize = Size;
            MaximumSize = Size;
            _linetype = linetype;
            ShouldDrawBackground = false;
            IsTabable = false;
            Setup();
        }
        private void Setup()
        {
            TextRequest = (o, e) =>
             {
                 if (!Selected ||
                    CurrentTools.SelectedTool == CurrentTools.EraserTool ||
                    CurrentTools.SelectedTool == CurrentTools.SelectTool ||
                    CurrentTools.SelectedTool == CurrentTools.MoveTool)
                     return "";
                 switch (_linetype)
                 {
                     case LineType.Blue:
                         return "";
                     case LineType.Red:
                         {
                             var sw = CurrentTools.SelectedTool.Swatch;
                             return sw.RedMultiplier.ToString(Program.Culture) + "x";
                         }
                     case LineType.Scenery:
                         {
                             var sw = CurrentTools.SelectedTool.Swatch;
                             return sw.GreenMultiplier.ToString(Program.Culture) + "x";
                         }
                     default:
                         return "";
                 }
             };

            Clicked += (o, e) =>
            {
                CurrentTools.SelectedTool.Swatch.Selected = _linetype;
                Invalidate();
            };

            RightClicked += (o, e) =>
            {
                IncrementMultiplier();
            };
        }
        public void IncrementMultiplier()
        {
            if (Selected &&
            CurrentTools.SelectedTool != CurrentTools.EraserTool &&
            CurrentTools.SelectedTool != CurrentTools.EraserTool &&
            CurrentTools.SelectedTool != CurrentTools.SelectTool &&
            CurrentTools.SelectedTool.ShowSwatch)
            {
                CurrentTools.SelectedTool.Swatch.IncrementSelectedMultiplier();
                Invalidate();
            }
        }
        public void SetImage(Texture tex)
        {
            m_texture = tex;
        }
        protected override void Render(Gwen.Skin.SkinBase skin)
        {
            skin.Renderer.DrawColor = Color.FromArgb(IsDepressed || Selected ? 64 : (IsHovered ? 128 : 255), color);
            var rect = RenderBounds;
            if (m_texture != null)
            {
                skin.Renderer.DrawTexturedRect(m_texture, rect);
            }
            else
            {
                Skin.Renderer.DrawFilledRect(rect);
            }
            base.Render(skin);
        }
    }
}