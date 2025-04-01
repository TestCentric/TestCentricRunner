// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestCentric.Engine;

namespace TestCentric.Gui
{
    /// <summary>
    /// An ImageSet holds a group of images used as icons in various parts of the GUI.
    /// </summary>
    public class OutcomeImageSet
    {
        private static readonly string[] REQUIRED_FILES = new[] {
            "Inconclusive.png", "Success.png", "Failure.png", "Ignored.png", "Skipped.png" };

        private string _imageSetDir;

        public OutcomeImageSet(string imageSetDir)
        {
            Guard.ArgumentValid(IsValidImageSetDirectory(imageSetDir), $"Directory {imageSetDir} does not contain an image set.", nameof(imageSetDir));

            _imageSetDir = imageSetDir;
            
            Name = Path.GetFileName(imageSetDir);
        }

        public string Name { get; }

        public static bool IsValidImageSetDirectory(string dir)
        {
            foreach (string file in REQUIRED_FILES)
                if (!File.Exists(Path.Combine(dir, file)))
                    return false;

            return true;
        }

        // Counter used for testing
        public int LoadCount { get; private set; } = 0;

        public Image LoadImage(string imgName)
        {
            if (_images.ContainsKey(imgName))
                return _images[imgName];

            LoadCount++;
            
            return _images[imgName] = Image.FromFile(Path.Combine(_imageSetDir, imgName + ".png"));
        }

        private Dictionary<string, Image> _images = new Dictionary<string, Image>();
    }
}
