﻿/* MIT License

Copyright (c) 2016 Edward Rowe, RedBlueGames

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace RedBlueGames.BulkRename
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// RenameOperation that enumerates characters onto the end of the rename string.
    /// </summary>
    public class EnumerateOperation : BaseRenameOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedBlueGames.BulkRename.EnumerateOperation"/> class.
        /// </summary>
        public EnumerateOperation()
        {
            this.Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedBlueGames.BulkRename.EnumerateOperation"/> class.
        /// This is a clone constructor, copying the values from one to another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public EnumerateOperation(EnumerateOperation operationToCopy)
        {
            this.StartingCount = operationToCopy.StartingCount;
            this.CountFormat = operationToCopy.CountFormat;

            this.Initialize();
        }

        /// <summary>
        /// Gets the path that's displayed when this rename op is used in the Add Op menu.
        /// </summary>
        /// <value>The display path.</value>
        public override string MenuDisplayPath
        {
            get
            {
                return "Add/Enumerate";
            }
        }

        /// <summary>
        /// Gets the order in which this rename op is displayed in the Add Op menu (lower is higher in the list.)
        /// </summary>
        /// <value>The menu order.</value>
        public override int MenuOrder
        {
            get
            {
                return 4;
            }
        }

        /// <summary>
        /// Gets or sets the starting count.
        /// </summary>
        /// <value>The starting count.</value>
        public int StartingCount { get; set; }

        /// <summary>
        /// Gets or sets the format for the count, appended to the end of the string.
        /// </summary>
        /// <value>The count format.</value>
        public string CountFormat { get; set; }

        /// <summary>
        /// Gets the heading label for the Rename Operation.
        /// </summary>
        /// <value>The heading label.</value>
        protected override string HeadingLabel
        {
            get
            {
                return "Enumerate";
            }
        }

        private List<EnumeratePresetGUI> GUIPresets { get; set; }

        private int SelectedPresetIndex { get; set; }

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public override BaseRenameOperation Clone()
        {
            var clone = new EnumerateOperation(this);
            return clone;
        }

        /// <summary>
        /// Rename the specified input, using the relativeCount.
        /// </summary>
        /// <param name="input">Input String to rename.</param>
        /// <param name="relativeCount">Relative count. This can be used for enumeration.</param>
        /// <returns>A new string renamed according to the rename operation's rules.</returns>
        public override string Rename(string input, int relativeCount)
        {
            var modifiedName = input;
            var currentCount = this.StartingCount + relativeCount;

            if (!string.IsNullOrEmpty(this.CountFormat))
            {
                try
                {
                    var currentCountAsString = currentCount.ToString(this.CountFormat);
                    modifiedName = string.Concat(modifiedName, currentCountAsString);
                }
                catch (System.FormatException)
                {
                    // Can't append anything if format is bad.
                }
            }

            return modifiedName;
        }

        /// <summary>
        /// Draws the contents of the Rename Op using EditorGUILayout.
        /// </summary>
        protected override void DrawContents()
        {   
            var presetsContent = new GUIContent("Format", "Select a preset format or specify your own format.");
            var names = new List<GUIContent>(this.GUIPresets.Count);
            foreach (var preset in this.GUIPresets)
            {
                names.Add(new GUIContent(preset.DisplayName));
            }

            this.SelectedPresetIndex = EditorGUILayout.Popup(presetsContent, this.SelectedPresetIndex, names.ToArray());
            var selectedPreset = this.GUIPresets[this.SelectedPresetIndex];

            EditorGUI.BeginDisabledGroup(selectedPreset.ReadOnly);
            var countFormatContent = new GUIContent("Count Format", "The string format to use when adding the Count to the name.");
            this.CountFormat = EditorGUILayout.TextField(countFormatContent, selectedPreset.Format);
            EditorGUI.EndDisabledGroup();

            try
            {
                this.StartingCount.ToString(this.CountFormat);
            }
            catch (System.FormatException)
            {
                var helpBoxMessage = "Invalid Count Format. Typical formats are D1 for one digit with no " +
                                     "leading zeros, D2, for two, etc." +
                                     "\nLookup String.Format method for more info." +
                                     " for more formatting options.";
                EditorGUILayout.HelpBox(helpBoxMessage, MessageType.Warning);
            }

            this.StartingCount = EditorGUILayout.IntField("Count From", this.StartingCount);
            selectedPreset.Format = this.CountFormat;
        }

        private void Initialize()
        {
            var singleDigitPreset = new EnumeratePresetGUI()
            {
                DisplayName = "0, 1, 2...",
                Format = "0",
                ReadOnly = true
            };
            
            var leadingZeroPreset = new EnumeratePresetGUI()
            {
                DisplayName = "00, 01, 02...",
                Format = "00",
                ReadOnly = true
            };

            var underscorePreset = new EnumeratePresetGUI()
            {
                DisplayName = "_00, _01, _02...",
                Format = "_00",
                ReadOnly = true
            };

            var customPreset = new EnumeratePresetGUI()
            {
                DisplayName = "Custom",
                Format = string.Empty,
                ReadOnly = false
            };

            this.GUIPresets = new List<EnumeratePresetGUI>
            {
                singleDigitPreset,
                leadingZeroPreset,
                underscorePreset,
                customPreset
            };

            this.SelectedPresetIndex = 0;
        }

        private class EnumeratePresetGUI
        {
            public string DisplayName { get; set; }

            public string Format { get; set; }

            public bool ReadOnly { get; set; }
        }
    }
}