﻿using System;
using System.Reflection;
using System.Windows.Forms;
using Sledge.Common.Mediator;
using Sledge.UI;

namespace Sledge.Editor.UI
{
    public static class Hotkeys
    {
        public static bool HotkeyDown(Keys keyData)
        {
            var keyCombination = KeyboardState.KeysToString(keyData);
            var hotkeyImplementation = Sledge.Settings.Hotkeys.GetHotkeyFor(keyCombination);
            if (hotkeyImplementation != null)
            {
                var def = hotkeyImplementation.Definition;
                Mediator.Publish(def.Action, def.Parameter);
                return true;
            }
            return false;
        }
    }
}
