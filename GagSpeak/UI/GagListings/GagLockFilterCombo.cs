using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;
using OtterGui;
using OtterGui.Raii;
﻿using Dalamud.Game.Text;
using Dalamud.Plugin;
using System.Diagnostics;
using Num = System.Numerics;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using OtterGui.Widgets;
using Dalamud.Interface;
using Dalamud.Interface.Utility;

using GagSpeak.Services;
using GagSpeak.UI.Helpers;
using GagSpeak.Chat;

namespace GagSpeak.UI.GagListings;

public sealed class GagLockFilterCombo
{
    private GagSpeakConfig _config;
    private string _comboSearchText;
    private GagPadlocks _selectedGagPadlocks;

    public GagLockFilterCombo(GagSpeakConfig config) {
        _comboSearchText = string.Empty;
        _config = config;
    } 

    /// <summary>
    /// This function draws ImGui's Combo list, but with a search filter. (and for GagPadlocks Enum)
    /// <list type="bullet">
    /// <item><c>ID</c><param name="ID"> - The list of items to display in the combo box</param></item>
    /// <item><c>label</c><param name="label"> - The label to display outside the combo box</param></item>
    /// <item><c>layerindex</c><param name="layerIndex"> - a list where the stored selection from the list is saved</param></item>
    /// </list>
    /// </summary>
    public void Draw(int ID, ref string label, List<GagPadlocks> listing, int layerIndex, int width) { // for whitelist gag selects
        try
        {
            ImGui.SetNextItemWidth(width);
            using( var gagLockCombo = ImRaii.Combo($"##{ID}_Enum",  label.ToString(), 
                                      ImGuiComboFlags.PopupAlignLeft | ImGuiComboFlags.HeightLargest)) 
            {
                if( gagLockCombo ) { // Assign it an ID if combo is sucessful.
                    // add the popup state
                    using var id = ImRaii.PushId($"##{ID}_Enum"); // Push an ID for the combo box (based on label / name)
                    ImGui.SetNextItemWidth(width); // Set filter length to full

                    foreach (var item in Enum.GetValues(typeof(GagPadlocks)).Cast<GagPadlocks>()) {
                        if (ImGui.Selectable(item.ToString(), listing[layerIndex] == item)) {
                            label = item.ToString(); // update label
                            GagSpeak.Log.Debug($"GagSpeak: GagPadlock changed to {item}");

                            _comboSearchText = string.Empty;
                            ImGui.CloseCurrentPopup();
                            _config.Save();
                            return;
                        }
                    }
                }
            }
        }
        catch (Exception e) {
            GagSpeak.Log.Debug(e.ToString());
        }
    }

    public void Draw(int ID, List<GagPadlocks> listing, int layerIndex, int width) { // for player gag equips
        try
        {
            ImGui.SetNextItemWidth(width);
            using( var gagLockCombo = ImRaii.Combo($"##{ID}_Enum",  listing[layerIndex].ToString(), 
                                      ImGuiComboFlags.PopupAlignLeft | ImGuiComboFlags.HeightLargest)) 
            {
                if( gagLockCombo ) { // Assign it an ID if combo is sucessful.
                    // add the popup state
                    using var id = ImRaii.PushId($"##{ID}_Enum"); // Push an ID for the combo box (based on label / name)
                    ImGui.SetNextItemWidth(width); // Set filter length to full

                    foreach (var item in Enum.GetValues(typeof(GagPadlocks)).Cast<GagPadlocks>()) {
                        if (ImGui.Selectable(item.ToString(), listing[layerIndex] == item)) {
                            listing[layerIndex] = item;
                            GagSpeak.Log.Debug($"GagSpeak: GagPadlock changed to {item}");

                            _comboSearchText = string.Empty;
                            ImGui.CloseCurrentPopup();
                            _config.Save();
                            return;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            GagSpeak.Log.Debug(e.ToString());
        }
    }
}