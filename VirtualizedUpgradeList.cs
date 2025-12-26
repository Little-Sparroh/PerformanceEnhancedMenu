using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

public static class VirtualizedUpgradeList
{
    private const int VISIBLE_BUFFER = 10;
    private const float ITEM_HEIGHT = 60f;

    private static List<UpgradeInstance> allUpgrades = new();
    private static Dictionary<int, GearUpgradeUI> activeUIElements = new();
    private static List<GearUpgradeUI> uiPool = new();
    private static RectTransform upgradeListParent;
    private static ScrollRect scrollRect;
    private static GearDetailsWindow currentWindow;
    private static bool isVirtualized = false;
    private static GearUpgradeUI uiPrefab;

    private static float viewportHeight = 0f;
    private static float currentScrollPosition = 0f;

    public static void Initialize(GearDetailsWindow window, RectTransform parent, ScrollRect scroll)
    {
        currentWindow = window;
        upgradeListParent = parent;
        scrollRect = scroll;

        viewportHeight = parent.rect.height;

        scrollRect.onValueChanged.AddListener(OnScrollChanged);

        isVirtualized = true;
    }

    public static void SetUpgradeData(List<UpgradeInstance> upgrades, GearUpgradeUI prefab)
    {
        if (!isVirtualized) return;

        allUpgrades = new List<UpgradeInstance>(upgrades);
        uiPrefab = prefab;

        ClearActiveUI();

        UpdateVisibleItems();
    }

    private static GearUpgradeUI GetPooledUI()
    {
        GearUpgradeUI ui;
        if (uiPool.Count > 0)
        {
            ui = uiPool[uiPool.Count - 1];
            uiPool.RemoveAt(uiPool.Count - 1);
            ui.gameObject.SetActive(true);
        }
        else
        {
            ui = UnityEngine.Object.Instantiate(uiPrefab, upgradeListParent);
        }
        return ui;
    }

    private static void ReturnToPool(GearUpgradeUI ui)
    {
        ui.gameObject.SetActive(false);
        uiPool.Add(ui);
    }

    public static void UpdateVisibleItems()
    {
        if (!isVirtualized || allUpgrades.Count == 0) return;

        int visibleStart = CalculateVisibleStartIndex();
        int visibleEnd = CalculateVisibleEndIndex();

        visibleStart = Math.Max(0, visibleStart - VISIBLE_BUFFER);
        visibleEnd = Math.Min(allUpgrades.Count - 1, visibleEnd + VISIBLE_BUFFER);

        var toRemove = new List<int>();
        foreach (var kvp in activeUIElements)
        {
            if (kvp.Key < visibleStart || kvp.Key > visibleEnd)
            {
                ReturnToPool(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (int index in toRemove)
        {
            activeUIElements.Remove(index);
        }

        for (int i = visibleStart; i <= visibleEnd; i++)
        {
            if (!activeUIElements.ContainsKey(i))
            {
                var ui = GetPooledUI();
                ui.SetUpgrade(allUpgrades[i]);
                var isGridViewField = AccessTools.Field(typeof(GearDetailsWindow), "isGridView");
                bool isGridView = (bool)isGridViewField.GetValue(null);
                ui.EnableGridView(isGridView);
                SetUpgradePosition(ui, i);
                activeUIElements[i] = ui;
            }
        }
    }

    private static void SetUpgradePosition(GearUpgradeUI ui, int index)
    {
        RectTransform transform = (RectTransform)ui.transform;
        var isGridViewField = AccessTools.Field(typeof(GearDetailsWindow), "isGridView");
        bool isGridView = (bool)isGridViewField.GetValue(null);

        if (isGridView)
        {
            double num1 = (double)upgradeListParent.rect.width - 6.0;
            UnityEngine.Rect rect = transform.rect;
            double num2 = (double)rect.width + 10.0;
            int num3 = Mathf.Max(Mathf.FloorToInt((float)(num1 / num2)), 1);
            double num4 = (double)(index % num3);
            rect = transform.rect;
            double num5 = (double)rect.width + 10.0;
            float x = (float)(3.0 + num4 * num5);
            double num6 = (double)(index / num3);
            rect = transform.rect;
            double num7 = (double)rect.height + 10.0;
            float y = (float)(-3.0 - num6 * num7);
            transform.anchoredPosition = new Vector2(x, y);
        }
        else
        {
            transform.offsetMin = new Vector2(0.0f, transform.offsetMin.y);
            transform.offsetMax = new Vector2(0.0f, transform.offsetMax.y);
            transform.anchoredPosition = new Vector2(0.0f, (float)(-3.0 - (transform.rect.height + 4.0) * index));
        }
    }

    private static int CalculateVisibleStartIndex()
    {
        var isGridViewField = AccessTools.Field(typeof(GearDetailsWindow), "isGridView");
        bool isGridView = (bool)isGridViewField.GetValue(null);

        if (isGridView)
        {
            return 0;
        }
        else
        {
            float scrollPos = 1f - currentScrollPosition;
            float totalHeight = allUpgrades.Count * (ITEM_HEIGHT + 4f);
            float visibleStartY = scrollPos * (totalHeight - viewportHeight);
            return Math.Max(0, (int)(visibleStartY / (ITEM_HEIGHT + 4f)));
        }
    }

    private static int CalculateVisibleEndIndex()
    {
        var isGridViewField = AccessTools.Field(typeof(GearDetailsWindow), "isGridView");
        bool isGridView = (bool)isGridViewField.GetValue(null);

        if (isGridView)
        {
            return Math.Min(allUpgrades.Count - 1, CalculateVisibleStartIndex() + 50);
        }
        else
        {
            int start = CalculateVisibleStartIndex();
            int visibleCount = (int)(viewportHeight / (ITEM_HEIGHT + 4f)) + 1;
            return Math.Min(allUpgrades.Count - 1, start + visibleCount);
        }
    }

    private static void OnScrollChanged(Vector2 scrollPos)
    {
        currentScrollPosition = scrollPos.y;
        UpdateVisibleItems();
    }

    public static void ClearActiveUI()
    {
        foreach (var ui in activeUIElements.Values)
        {
            ReturnToPool(ui);
        }
        activeUIElements.Clear();
    }

    public static void Shutdown()
    {
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
        }
        ClearActiveUI();
        allUpgrades.Clear();
        isVirtualized = false;
        currentWindow = null;
    }

    public static bool IsVirtualized => isVirtualized;
    public static GearUpgradeUI GetUIForUpgrade(UpgradeInstance upgrade)
    {
        foreach (var kvp in activeUIElements)
        {
            if (kvp.Value.Upgrade == upgrade)
                return kvp.Value;
        }
        return null;
    }
}
