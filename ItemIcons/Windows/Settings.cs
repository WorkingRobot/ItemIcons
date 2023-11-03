using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ItemIcons.IconProviders;
using Dalamud.Interface.Utility.Raii;
using ItemIcons.IconTypes;
using Dalamud.Utility;
using ItemIcons.Config;
using ItemIcons.Utils;

namespace ItemIcons.Windows;

public class Settings : Window
{
    private static Configuration Config => Service.Configuration;
    private ConfigDrawState<Configuration> DrawState { get; } = new("None");

    private const bool ShowArmoryJobIcons = false;

    public Settings() : base("Item Icons Settings")
    {
        Service.WindowSystem.AddWindow(this);

        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new Vector2(400, 400),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        Size = SizeConstraints.Value.MinimumSize;
        SizeCondition = ImGuiCond.Appearing;
    }

    public override unsafe void Draw()
    {
        var config = Config;
        if (DrawState.Draw(ref config, new()))
        {
            Log.Debug("Saved!");
            config.Save();
        }

        if (ShowArmoryJobIcons)
        {
            var a = new ArmoryJob();
            DrawIconSet("Gold", a.JobSetGold);
            DrawIconSet("Framed", a.JobSetFramed);
            DrawIconSet("Glowing", a.JobSetGlowing);
            DrawIconSet("Gray", a.JobSetGray);
            DrawIconSet("Black", a.JobSetBlack);
            DrawIconSet("Yellow", a.JobSetYellow);
            DrawIconSet("Orange", a.JobSetOrange);
            DrawIconSet("Red", a.JobSetRed);
            DrawIconSet("Purple", a.JobSetPurple);
            DrawIconSet("Blue", a.JobSetBlue);
            DrawIconSet("Green", a.JobSetGreen);
            ImGui.Separator();
            DrawIconSet("Icons", a.RoleSetIcons);
            DrawIconSet("Square", a.RoleSetSquare);
            DrawIconSet("Rounded", a.RoleSetRounded);
            DrawIconSet("Mini", a.RoleSetMini);
            DrawIconSet("Roles", a.RoleSet);
            ImGui.Separator();
            DrawAllIcons(a);
        }
    }
    
    private static void DrawIconSet(string name, ArmoryJob.IconSet set)
    {
        if (ImGui.CollapsingHeader($"{name}##jobSet"))
        {
            using var t = ImRaii.Table("jobTable", 10);
            if (t)
            {
                for (var i = 0; i < set.Count; ++i)
                {
                    ImGui.TableNextColumn();
                    DrawIcon(set.TryGet(i));
                }
            }
        }
    }

    private static void DrawAllIcons(ArmoryJob job)
    {
        if (ImGui.CollapsingHeader($"categoryIcons##jobSet"))
        {
            using var t = ImRaii.Table("jobTable", 6);
            if (t)
            {
                foreach(var row in LuminaSheets.ClassJobCategorySheet)
                {
                    if (job.CategoryIcons.TryGetValue(row.RowId, out var icon))
                    {
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(row.Name.ToDalamudString().ToString());
                        ImGui.TableNextColumn();
                        DrawIcon(icon);
                    }
                }
            }
        }
    }

    private static void DrawIcon(BaseIcon? icon)
    {
        if (icon is TextureIcon { } texture)
        {
            var tex = Service.IconManager.GetTexture(texture.Texture);
            Vector2 uv0 = new(), uv1 = new(1);
            if (texture.Rect is { } rect)
            {
                uv0 = new(rect.U, rect.V);
                uv1 = new(rect.Width, rect.Height);
                uv1 += uv0;
                uv0 /= tex.Size;
                uv1 /= tex.Size;
            }
            ImGui.Image(tex.ImGuiHandle, new(64), uv0, uv1);
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip($"Texture: {texture.Texture}\nRect: {texture.Rect}\nScale: {texture.Scale}");
        }
        else
        {
            ImGui.TextUnformatted($"Unknown\n{icon}");
        }
    }
}
