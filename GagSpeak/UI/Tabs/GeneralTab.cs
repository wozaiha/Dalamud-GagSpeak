﻿using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;
using OtterGui;
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
using GagSpeak.UI.GagListings;
using Dalamud.Interface.Utility.Raii;
﻿using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
﻿using System;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Microsoft.VisualBasic;

namespace GagSpeak.UI.Tabs.GeneralTab;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class GeneralTab : ITab
{
    // Begin by appending the readonlys and privates
    private readonly IDalamudTextureWrap _dalamudTextureWrap; // for image display
    private readonly UiBuilder _uiBuilder; // for image display
    private readonly GagSpeakConfig _config;
    private readonly GagListingsDrawer _gagListingsDrawer;
    private GagTypeFilterCombo[] _gagTypeFilterCombo; // create an array of item combos
    private GagLockFilterCombo[] _gagLockFilterCombo; // create an array of item combos
    private string? _tempSafeword; // for initializing a temporary safeword for the text input field
    // style variables
    private bool _isLocked;
    private bool? _inDomMode;
    
    public GeneralTab(GagListingsDrawer gagListingsDrawer, GagSpeakConfig config,
            UiBuilder uiBuilder, DalamudPluginInterface pluginInterface)
    {
        _isLocked = false;
        _config = config;
        _gagListingsDrawer = gagListingsDrawer;
        _uiBuilder = uiBuilder;

        var imagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "icon.png");
        var IconImage = _uiBuilder.LoadImage(imagePath);
        _dalamudTextureWrap = IconImage;


        _gagTypeFilterCombo = new GagTypeFilterCombo[] {
            new GagTypeFilterCombo(_config.GagTypes, _config),
            new GagTypeFilterCombo(_config.GagTypes, _config),
            new GagTypeFilterCombo(_config.GagTypes, _config)
        };
        // draw out our gagpadlock filter combo listings
        _gagLockFilterCombo = new GagLockFilterCombo[] {
            new GagLockFilterCombo(_config),
            new GagLockFilterCombo(_config),
            new GagLockFilterCombo(_config)
        };


    }

    // store our current safeword
    public string _currentSafeword = string.Empty;
    // Apply our lable for the tab
    public ReadOnlySpan<byte> Label
        => "General"u8;

    /// <summary>
    /// This Function draws the content for the window of the General Tab
    /// </summary>
    public void DrawContent() {
        // Definitely need to refine the ImGui code here, but this is a good start.
        // Create a child for the Main Window (not sure if we need this without a left selection panel)
        using var child = ImRaii.Child("MainWindowChild");
        if (!child)
            return;

        
        // Draw the child grouping for the ConfigSettings Tab
        using (var child2 = ImRaii.Child("GeneralTabChild"))
        {
            DrawHeader();
            DrawGeneral();
        }
    }

    // Draw the header stuff
    private void DrawHeader()
        => WindowHeader.Draw("Gag Selections / Inspector", 0, ImGui.GetColorU32(ImGuiCol.FrameBg));

    // Draw the actual general tab contents
    private void DrawGeneral() {
        // let's start by drawing the outline for the container
        using var child = ImRaii.Child("GeneralTabPanel", -Vector2.One, true);
        // Let's create a table in this panel
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ButtonTextAlign, new Vector2(0, 0.5f));
        using (var table = ImRaii.Table("Main Declarations", 2)) {
            if(!table) { return; } // make sure our table was made
            // Identify our columns.
            ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Declare Safewordm").X);
            ImGui.TableSetupColumn("Data", ImGuiTableColumnFlags.WidthStretch);

            // draw our our first row
            ImGuiUtil.DrawFrameColumn("Declare Safeword");
            ImGui.TableNextColumn();
            // add variables for the safeword stuff
            var width = new Vector2(ImGui.GetContentRegionAvail().X,0);
            var safeword  = _tempSafeword ?? _config.Safeword; // temp storage to hold until we de-select the text input
            ImGui.SetNextItemWidth(width.X);
            if (ImGui.InputText("##Safeword", ref safeword, 128, ImGuiInputTextFlags.None))
                _tempSafeword = safeword;
            if (ImGui.IsItemDeactivatedAfterEdit()) { // will only update our safeword once we click away from the safeword bar
                _config.Safeword = safeword;
                _tempSafeword = null;
            }

            // draw our our second row
            var mode = _inDomMode ?? _config.InDomMode;
            ImGuiUtil.DrawFrameColumn("Mode Selector");
            ImGui.TableNextColumn();
            // draw out our two buttons to set the modes. When the button labeled sub is pressed, it will switch isDomMode to false, and lock the interactability of the sub button.
            // when the button labeled dom is pressed, it will switch isDomMode to true, and lock the interactability of the dom button.
            if (mode == true) {
                // User is in Dom mode
                ImGui.BeginDisabled();
                if (ImGui.Button("Dominant")) {
                    // Dom mode is already active, do nothing or display a message
                }
                ImGui.EndDisabled();
                ImGui.SameLine();
                if (ImGui.Button("Submissive")) {
                    _inDomMode = false; // Switch to Sub mode
                    _config.InDomMode = false;
                    _config.Save();
                }
            } else {
                // User is in Sub mode
                if (ImGui.Button("Dominant")) {
                    _inDomMode = true; // Switch to Dom mode
                    _config.InDomMode = true;
                    _config.Save();
                }
                ImGui.BeginDisabled();
                ImGui.SameLine();
                if (ImGui.Button("Submissive")) {
                    // do nothing
                }
                ImGui.EndDisabled();
            }
        } // end our table
        ImGui.NewLine();

        // Now let's draw our 3 gag appliers
        _gagListingsDrawer.PrepareGagListDrawing(); // prepare our listings

        int DDwidth = (int)(ImGui.GetContentRegionAvail().X / 2);
        // draw our 3 gag listings
        foreach(var slot in Enumerable.Range(0, 3)) {
            _gagListingsDrawer.DrawGagAndLockListing(slot, _config, _gagTypeFilterCombo[slot],
                    _gagLockFilterCombo[slot], slot, $"Gag Slot {slot + 1}", _isLocked, DDwidth);
            ImGui.NewLine();
        }
    }
}

#pragma warning restore IDE1006