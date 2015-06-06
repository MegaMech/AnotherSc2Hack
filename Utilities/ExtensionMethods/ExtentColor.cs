﻿using System;
using System.Drawing;

namespace Utilities.ExtensionMethods
{
    public static class ExtentColor
    {
        /// <summary>
        /// Generates a random color
        /// </summary>
        /// <param name="cl">The color instance we apply this from</param>
        /// <returns>A completely random color!</returns>
        public static Color GetRandomColor(this Color cl)
        {
            var rnd = new Random();

            var myColor = Color.FromArgb(rnd.Next(0, 256), rnd.Next(0, 256), rnd.Next(0, 256));

            return myColor;
        }
    }
}
