// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Windows.Forms;

namespace TestCentric.Gui.Elements
{
    /// <summary>
    /// The IListBox interface is implemented by an element
    /// representing a ListBox containing string items or
    /// items that implement ToString() in a useful way.
    /// </summary>
    [Obsolete("No longer used", true)]
    public interface IListBox : IControlElement
    {
        ListBox.ObjectCollection Items { get; }
        ListBox.SelectedObjectCollection SelectedItems { get; }

        event CommandHandler DoubleClick;

        void Add(string item);
        void Remove(string item);
    }
}
