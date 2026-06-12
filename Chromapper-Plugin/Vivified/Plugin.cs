using System;
using System.Globalization;
using System.Linq;
using Beatmap.Base;
using Beatmap.Helper;
using SimpleJSON;
using TMPro;
using UnityEngine;

namespace Vivified
{
    [Plugin("Vivified")]
    public class Plugin
    {
        private ExtensionButton previewButton;
        private ExtensionButton editButton;

        private DialogBox previewDialog;
        private TextComponent previewStatus;

        private DialogBox dataDialog;
        private TextBoxComponent jsonEditor;
        private TextComponent dataStatus;
        private BaseObject selectedObject;

        [Init]
        private void Init()
        {
            previewButton = ExtensionButtons.AddButton(null, "Vivified — Vivify bundle preview", OnPreviewButton);
            editButton = ExtensionButtons.AddButton(null, "Vivified — edit selected object's custom data", OnEditButton);
            SelectionController.SelectionChangedEvent += OnSelectionChanged;
            LoadInitialMap.LevelLoadedEvent += OnLevelLoaded;
        }

        [Exit]
        private void Exit()
        {
            SelectionController.SelectionChangedEvent -= OnSelectionChanged;
            LoadInitialMap.LevelLoadedEvent -= OnLevelLoaded;
            if (previewButton != null) ExtensionButtons.RemoveButton(previewButton);
            if (editButton != null) ExtensionButtons.RemoveButton(editButton);
        }

        private void OnLevelLoaded()
        {
            // the preview controller lives in the editor scene and cleans
            // itself (and the loaded bundle) up when the scene unloads
            VivifyPreview.EnsureCreated();
        }

        // --- preview dialog ------------------------------------------------------

        private void OnPreviewButton()
        {
            if (PersistentUI.Instance == null) return;
            if (previewDialog == null) CreatePreviewDialog();
            RefreshPreviewStatus();
            previewDialog.Open();
        }

        private void CreatePreviewDialog()
        {
            previewDialog = PersistentUI.Instance.CreateNewDialogBox().WithTitle("Vivified");
            previewDialog.DontDestroyOnClose();

            previewStatus = previewDialog.AddComponent<TextComponent>()
                .WithInitialValue("Vivify preview");

            previewDialog.AddComponent<ToggleComponent>()
                .WithLabel("Show Vivify prefabs / materials")
                .WithInitialValue(VivifiedSettings.PreviewEnabled)
                .OnChanged((bool v) => { VivifiedSettings.PreviewEnabled = v; });

            previewDialog.AddComponent<ToggleComponent>()
                .WithLabel("Hide default environment")
                .WithInitialValue(VivifiedSettings.HideEnvironment)
                .OnChanged((bool v) => { VivifiedSettings.HideEnvironment = v; });

            AddOffsetField(previewDialog, "Sync offset (beats)", VivifiedSettings.BeatOffset,
                v => VivifiedSettings.BeatOffset = v);
            AddOffsetField(previewDialog, "World offset X", VivifiedSettings.WorldOffset.x,
                v => VivifiedSettings.WorldOffset.x = v);
            AddOffsetField(previewDialog, "World offset Y", VivifiedSettings.WorldOffset.y,
                v => VivifiedSettings.WorldOffset.y = v);
            AddOffsetField(previewDialog, "World offset Z", VivifiedSettings.WorldOffset.z,
                v => VivifiedSettings.WorldOffset.z = v);

            previewDialog.AddFooterButton(() =>
            {
                if (VivifyPreview.Instance != null) VivifyPreview.Instance.ReloadBundle();
                RefreshPreviewStatus();
            }, "Reload Bundle");
            previewDialog.AddFooterButton(() =>
            {
                if (VivifyPreview.Instance != null) VivifyPreview.Instance.QueueRebuild();
                RefreshPreviewStatus();
            }, "Rebuild Events");
            previewDialog.AddFooterButton(null, "Close");
        }

        private static void AddOffsetField(DialogBox box, string label, float initial, Action<float> setter)
        {
            box.AddComponent<TextBoxComponent>()
                .WithLabel(label)
                .WithInitialValue(initial.ToString(CultureInfo.InvariantCulture))
                .OnEndEdit(s =>
                {
                    float v;
                    if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                        setter(v);
                });
        }

        private void RefreshPreviewStatus()
        {
            if (previewStatus == null) return;
            previewStatus.Value = VivifyPreview.Instance != null
                ? VivifyPreview.Instance.Status
                : "Open a map in the editor to load its .vivify bundle.";
        }

        // --- custom data editor ----------------------------------------------------

        private void OnSelectionChanged() => UpdateDataDialogSelection();

        private void OnEditButton()
        {
            if (PersistentUI.Instance == null) return;
            if (dataDialog == null) CreateDataDialog();
            UpdateDataDialogSelection();
            dataDialog.Open();
        }

        private void CreateDataDialog()
        {
            dataDialog = PersistentUI.Instance.CreateNewDialogBox().WithTitle("Vivified Custom Data");
            dataDialog.DontDestroyOnClose();

            dataStatus = dataDialog.AddComponent<TextComponent>()
                .WithInitialValue("Select a single beatmap object to edit its custom data.");
            jsonEditor = dataDialog.AddComponent<TextBoxComponent>()
                .WithLabel("Custom JSON")
                .WithInitialValue(string.Empty)
                .WithLineType(TMP_InputField.LineType.MultiLineNewline)
                .WithMaximumLength(32767);

            dataDialog.AddFooterButton(ApplyCurrentJson, "Apply");
            dataDialog.AddFooterButton(null, "Close");
            dataDialog.OnQuickSubmit(ApplyCurrentJson, false);
        }

        private void UpdateDataDialogSelection()
        {
            if (dataDialog == null) return;

            if (SelectionController.SelectedObjects.Count == 1)
            {
                selectedObject = SelectionController.SelectedObjects.First();
                if (jsonEditor != null)
                    jsonEditor.Value = selectedObject.CustomData != null
                        ? selectedObject.CustomData.ToString()
                        : new JSONObject().ToString();
                if (dataStatus != null)
                    dataStatus.Value = string.Format("Selected: {0} @ {1:0.###}",
                        selectedObject.ObjectType, selectedObject.JsonTime);
            }
            else
            {
                selectedObject = null;
                if (jsonEditor != null) jsonEditor.Value = string.Empty;
                if (dataStatus != null)
                    dataStatus.Value = SelectionController.SelectedObjects.Count == 0
                        ? "No object selected. Select one object to edit its custom data."
                        : "Multiple objects selected. Select exactly one object to edit.";
            }
        }

        private void ApplyCurrentJson()
        {
            if (selectedObject == null)
            {
                Message("Select exactly one object before applying.");
                return;
            }
            string rawJson = jsonEditor != null ? jsonEditor.Value : string.Empty;
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                Message("Custom JSON cannot be empty.");
                return;
            }

            JSONNode newCustomData;
            try
            {
                newCustomData = JSON.Parse(rawJson);
                if (newCustomData == null) newCustomData = new JSONObject();
            }
            catch (Exception e)
            {
                Message("Invalid JSON: " + e.Message);
                return;
            }

            BaseObject originalData = BeatmapFactory.Clone(selectedObject);
            selectedObject.CustomData = newCustomData;
            BeatmapActionContainer.AddAction(
                new BeatmapObjectModifiedAction(selectedObject, selectedObject, originalData,
                    "Edited Vivify custom JSON"));
            if (VivifyPreview.Instance != null) VivifyPreview.Instance.QueueRebuild();
            Message("Vivify custom data applied.");
        }

        private static void Message(string text)
        {
            if (PersistentUI.Instance != null)
                PersistentUI.Instance.DisplayMessage(text, PersistentUI.DisplayMessageType.Bottom);
        }
    }
}
