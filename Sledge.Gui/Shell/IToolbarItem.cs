﻿using System;
using System.Drawing;

namespace Sledge.Gui.Shell
{
    public interface IToolbarItem : IDisposable
    {
        string Identifier { get; set; }
        string Text { get; set; }
        Bitmap Icon { set; }
        event EventHandler Clicked;
    }
}