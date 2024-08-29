﻿using Content.Shared.CCVar;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Configuration;

namespace Content.Client.Backmen.JoinQueue;

[GenerateTypedNameReferences]
public sealed partial class QueueGui : Control
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    public event Action? QuitPressed;

    public QueueGui()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);
        LayoutContainer.SetAnchorPreset(this, LayoutContainer.LayoutPreset.Wide);

        QuitButton.OnPressed += (_) => QuitPressed?.Invoke();

        // Disable "priority join" button on Steam builds
        // since it violates Valve's rules about alternative storefronts.
        PriorityJoinButton.Visible = !_cfg.GetCVar(CCVars.BrandingSteam);

        PriorityJoinButton.OnPressed += (_) =>
        {
            var linkPatreon = _cfg.GetCVar(CCVars.InfoLinksPatreon);
            IoCManager.Resolve<IUriOpener>().OpenUri(linkPatreon);
        };
    }

    public void UpdateInfo(int total, int position)
    {
        QueueTotal.Text = total.ToString();
        QueuePosition.Text = position.ToString();
    }
}
