﻿using System;
using System.Collections.Generic;

namespace Gwen.Anim
{
    public class Animation
    {
        protected Controls.ControlBase m_Control;

        //private static List<Animation> g_AnimationsListed = new List<Animation>(); // Unused
        private static readonly Dictionary<Controls.ControlBase, List<Animation>> m_Animations = new Dictionary<Controls.ControlBase, List<Animation>>();

        protected virtual void Think()
        {

        }

        public virtual bool Finished => throw new InvalidOperationException("Pure virtual function call");

        public static void Add(Controls.ControlBase control, Animation animation)
        {
            animation.m_Control = control;
            if (!m_Animations.ContainsKey(control))
                m_Animations[control] = new List<Animation>();
            m_Animations[control].Add(animation);
        }

        public static void Cancel(Controls.ControlBase control)
        {
            if (m_Animations.ContainsKey(control))
            {
                m_Animations[control].Clear();
                _ = m_Animations.Remove(control);
            }
        }

        internal static void GlobalThink()
        {
            foreach (KeyValuePair<Controls.ControlBase, List<Animation>> pair in m_Animations)
            {
                List<Animation> valCopy = pair.Value.FindAll(x => true); // List copy so foreach won't break when we remove elements
                foreach (Animation animation in valCopy)
                {
                    animation.Think();
                    if (animation.Finished)
                    {
                        _ = pair.Value.Remove(animation);
                    }
                }
            }
        }
    }
}
