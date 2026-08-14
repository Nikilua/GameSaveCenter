using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.Views;

namespace GameSaveCenter.RenderHarness.UiAudit;

public static class UiPageRouteFactory
{
    public static List<UiRuntimeRoute> CreateRuntimeRoutes()
    {
        var routes = new List<UiRuntimeRoute>
        {
            new UiRuntimeRoute { RouteId = "overview", DisplayName = "首页", ViewType = typeof(OverviewView), IsKnown = true },
            new UiRuntimeRoute { RouteId = "save-center", DisplayName = "存档中心", ViewType = typeof(SaveCenterView), IsKnown = true },
            new UiRuntimeRoute { RouteId = "trainer-center", DisplayName = "修改器中心", ViewType = typeof(TrainerCenterView), IsKnown = true },
            new UiRuntimeRoute { RouteId = "media-center", DisplayName = "媒体中心", ViewType = typeof(MediaCenterView), IsKnown = true },
            new UiRuntimeRoute { RouteId = "task-center", DisplayName = "任务中心", ViewType = typeof(TaskCenterView), IsKnown = true },
            new UiRuntimeRoute { RouteId = "maintenance", DisplayName = "维护中心", ViewType = typeof(MaintenanceView), IsKnown = true },
            new UiRuntimeRoute { RouteId = "settings", DisplayName = "设置", ViewType = typeof(GameSaveCenterSettingsView), IsKnown = true, IsSettings = true }
        };

        // Future page compatibility: every parameterless UserControl under the production
        // Views namespace is discovered automatically and rendered with the same fake
        // binding surface used by the existing render harness.
        var knownTypes = new HashSet<Type>(routes.Select(route => route.ViewType));
        var assembly = typeof(OverviewView).Assembly;
        foreach (var type in assembly.GetTypes()
                     .Where(type =>
                         type.Namespace == "GameSaveCenter.Playnite.Views"
                         && !type.IsAbstract
                         && !knownTypes.Contains(type)
                         && type != typeof(DashboardView)
                         && typeof(UserControl).IsAssignableFrom(type)
                         && type.GetConstructor(Type.EmptyTypes) != null)
                     .OrderBy(type => type.Name, StringComparer.OrdinalIgnoreCase))
        {
            routes.Add(new UiRuntimeRoute
            {
                RouteId = Slugify(type.Name),
                DisplayName = type.Name,
                ViewType = type,
                IsKnown = false
            });
        }

        return routes;
    }

    private static string Slugify(string value)
    {
        var chars = value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray();
        return new string(chars);
    }
}
