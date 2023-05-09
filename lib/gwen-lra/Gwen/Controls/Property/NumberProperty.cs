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
using Gwen;
using System.Globalization;
using System.Drawing;
namespace Gwen.Controls
{
    public class NumberProperty : PropertyBase
    {
        public double NumberValue
        {
            get
            {
                return m_Spinner.Value;
            }
            set
            {
                m_Spinner.Value = value;
            }
        }
        public double Min
        {
            get
            {
                return m_Spinner.Min;
            }
            set
            {
                m_Spinner.Min = value;
            }
        }
        public double Max
        {
            get
            {
                return m_Spinner.Max;
            }
            set
            {
                m_Spinner.Max = value;
            }
        }

        /// <summary>
        /// Indicates whether the property value is being edited.
        /// </summary>
        public override bool IsEditing
        {
            get { return m_Spinner.HasFocus; }
        }

        /// <summary>
        /// Indicates whether the control is hovered by mouse pointer.
        /// </summary>
        public override bool IsHovered
        {
            get { return base.IsHovered | m_Spinner.IsHovered; }
        }
        public bool OnlyWholeNumbers
        {
            get
            {
                return m_Spinner.OnlyWholeNumbers;
            }
            set
            {
                m_Spinner.OnlyWholeNumbers = value;
            }
        }
        protected readonly Spinner m_Spinner;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextProperty"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public NumberProperty(Gwen.Controls.ControlBase parent) : base(parent)
        {
            Padding = Padding.One;
            m_Spinner = new Spinner(this);
            m_Spinner.MinimumSize = m_Spinner.Size;
            this.Height = m_Spinner.Height;
            m_Spinner.Dock = Dock.Fill;
            m_Spinner.ShouldDrawBackground = false;
            m_Spinner.ValueChanged += OnValueChanged;
            AutoSizeToContents = true;
        }

        public override void Disable()
        {
            base.Disable();
            m_Spinner.Disable();
        }
        /// <summary>
        /// Property value.
        /// </summary>
        public override string Value
        {
            get { return m_Spinner.Value.ToString(); }
            set { base.Value = value; }
        }

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        /// <param name="fireEvents">Determines whether to fire "value changed" event.</param>
        public override void SetValue(string value, bool fireEvents = false)
        {
            m_Spinner.Value = double.Parse(value);
        }
    }
}