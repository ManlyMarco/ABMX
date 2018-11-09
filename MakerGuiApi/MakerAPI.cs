﻿using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using Harmony;
using UnityEngine;
using Logger = BepInEx.Logger;

namespace MakerAPI
{
    [BepInPlugin(GUID, "Character Maker API", "1.0")]
    public partial class MakerAPI : BaseUnityPlugin
    {
        public const string GUID = "com.bepis.makerapi";

        private readonly List<MakerCategory> _categories = new List<MakerCategory>();
        private readonly List<MakerGuiEntryBase> _guiEntries = new List<MakerGuiEntryBase>();

        public MakerAPI()
        {
            Instance = this;
        }

        private void Start()
        {
            var harmony = HarmonyInstance.Create(GUID);
            harmony.PatchAll(typeof(Hooks));
        }

        public static MakerAPI Instance { get; private set; }

        private void CreateCustomControls()
        {
            foreach (Transform categoryTransfrom in GameObject.Find("CvsMenuTree").transform)
            {
                CreateCustomControlsInCategory(categoryTransfrom);

                /*foreach (var tr in categoryTransfrom.transform
                    .GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>(true))
                {
                    tr.CalculateLayoutInputHorizontal();
                    tr.CalculateLayoutInputVertical();
                    tr.SetLayoutHorizontal();
                    tr.SetLayoutVertical();
                }
                foreach (var tr in categoryTransfrom.transform.GetComponentsInChildren<RectTransform>(true))
                {
                    LayoutRebuilder.MarkLayoutForRebuild(tr);
                }*/
            }
            //Canvas.ForceUpdateCanvases();
        }

        private void CreateCustomControlsInCategory(Transform categoryTransfrom)
        {
            /*foreach (var guiEntry in _guiEntries)
            {
                if(guiEntry.Category.CategoryName != categoryTransfrom.name)
                    continue;


            }*/

            foreach (var subCategoryGroup in _guiEntries
                .Where(x => x.Category.CategoryName == categoryTransfrom.name)
                .GroupBy(x => x.Category.SubCategoryName))
            {
                var categorySubTransform = categoryTransfrom.Find(subCategoryGroup.Key);

                if (categorySubTransform != null)
                {
                    var contentParent = FindSubcategoryContentParent(categorySubTransform);

                    foreach (var customControl in subCategoryGroup)
                        customControl.CreateControl(contentParent);

                    Logger.Log(LogLevel.Debug, $"[MakerAPI] Added {subCategoryGroup.Count()} custom controls " +
                                               $"to {categoryTransfrom.name}/{subCategoryGroup.Key}");
                }
                else
                {
                    Logger.Log(LogLevel.Error, $"[MakerAPI] Subcategory {categoryTransfrom.name} / {subCategoryGroup.Key} " +
                                               $"used by {subCategoryGroup.Count()} custom controls was not registered by " +
                                               $"AddSubCategory and will be ignored.");
                }
            }
        }

        private static Transform FindSubcategoryContentParent(Transform categorySubTransform)
        {
            var scrollViewContent = categorySubTransform.Find("Scroll View/Viewport/Content");
            return scrollViewContent ?? categorySubTransform.Cast<Transform>().First(x => x.name != "imgOff");
        }

        /// <summary>
        ///     Needs to run before UI_ToggleGroupCtrl.Start of the category runs, or it won't get added properly
        /// </summary>
        private void AddMissingSubCategories(UI_ToggleGroupCtrl mainCategory)
        {
            var categoryTransfrom = mainCategory.transform;
            foreach (var category in _categories)
            {
                if (categoryTransfrom.name != category.CategoryName) continue;

                var categorySubTransform = categoryTransfrom.Find(category.SubCategoryName);
                if (categorySubTransform == null)
                {
                    SubCategoryCreator.AddNewSubCategory(mainCategory, category.SubCategoryName);
                }
            }
        }

        /// <summary>
        ///     Add custom controls. If you want to use custom sub categories, register them by calling AddSubCategory.
        /// </summary>
        public T AddControl<T>(T control) where T : MakerGuiEntryBase
        {
            _guiEntries.Add(control);
            return control;
        }

        /// <summary>
        ///     Add custom sub categories. They need to be added before maker starts loading,
        ///     or in the RegisterCustomSubCategories event.
        /// </summary>
        /// <param name="category">Subcategory to add</param>
        public void AddSubCategory(MakerCategory category)
        {
            if (!_categories.Contains(category))
            {
                _categories.Add(category);
            }
            else
            {
                Logger.Log(LogLevel.Warning, $"[MakerAPI] Duplicate custom subcategory was added: " +
                                             $"{category} The duplicate will be ignored.");
            }
        }

        /// <summary>
        ///     Called once during first maker load. This is the last chance to add custom sub categories.
        ///     Use AddSubCategory to add custom subcategories.
        /// </summary>
        public event EventHandler RegisterCustomSubCategories;

        /// <summary>
        ///     Early in the process of maker loading. Most game components are initialized and had their Start methods ran.
        ///     Warning: Some components and objects might not be loaded or initialized yet, especially if they are mods.
        /// </summary>
        public event EventHandler MakerStartedLoading;

        /// <summary>
        ///     Maker is fully loaded. Use to load mods that rely on something that is loaded late, else use MakerStartedLoading.
        ///     This is the last chance to add custom controls, but be careful to only add them once because this event runs on
        ///     every maker start!
        /// </summary>
        public event EventHandler MakerBaseLoaded;

        /// <summary>
        ///     Maker is fully loaded and the user has control.
        ///     WARNING: Avoid loading mods or doing anything heavy in this event, use EarlyMakerFinishedLoading instead.
        /// </summary>
        public event EventHandler MakerFinishedLoading;

        public event EventHandler MakerExiting;

        protected virtual void OnRegisterCustomSubCategories()
        {
            Logger.Log(LogLevel.Debug, "OnRegisterCustomSubCategories");
            RegisterCustomSubCategories?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnMakerStartedLoading()
        {
            Logger.Log(LogLevel.Debug, "Character Maker Started Loading");
            MakerStartedLoading?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnMakerFinishedLoading()
        {
            Logger.Log(LogLevel.Debug, "Character Maker Finished Loading");
            MakerFinishedLoading?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnMakerBaseLoaded()
        {
            Logger.Log(LogLevel.Debug, "Character Maker Base Loaded");
            MakerBaseLoaded?.Invoke(this, EventArgs.Empty);

            CreateCustomControls();
        }

        private void OnMakerExiting()
        {
            Logger.Log(LogLevel.Debug, "Character Maker is exiting");
            MakerExiting?.Invoke(this, EventArgs.Empty);
        }
    }
}