// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.Animation
{
    /// <summary>
    /// Animation easing curve for animations.
    /// </summary>
    public class EasingCurve
    {
        private string type;
        private Func<float, float> easing;

        public string Type { get => type; }

        public EasingCurve(string type, Func<float, float> easing)
        {
            this.type = type ?? throw new ArgumentNullException(nameof(type));
            this.easing = easing ?? throw new ArgumentNullException(nameof(easing));
        }

        public virtual float ValueForProgress(float progress)
        {
            return easing(progress);
        }

        public override bool Equals(object obj)
        {
            var curve = obj as EasingCurve;
            return curve != null &&
                   Type == curve.Type;
        }

        public override int GetHashCode()
        {
            return 2049151605 + EqualityComparer<string>.Default.GetHashCode(Type);
        }

        /// <summary>
        /// Simple linear tweening with no easing or acceleration.</summary>
        public static EasingCurve Linear = new EasingCurve("Linear", x => x);
        /// <summary>
        /// Quadratic easing in, accelerating from zero velocity.</summary>
        public static EasingCurve InQuad = new EasingCurve("InQuad", x => x * x);
        public static EasingCurve InCubic = new EasingCurve("InCubic", x => x * x * x);
        public static EasingCurve OutQuad = new EasingCurve("OutQuad", x => -(x * (x - 2)));
        public static EasingCurve InOutQuad = new EasingCurve("InOutQuad",
            x => 
            {
                x /= 0.5f;
                if (x < 1.0) return (0.5f * x * x);
                x--;
                return -0.5f * (x*(x-2) - 1);
            });

        //x => (x< 0.5) ? (x* x) : (x* 0.5)
        //    );
    }
}
