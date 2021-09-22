using System;
using System.Collections.Generic;
using EntityComponent;
using JKMP.Plugin.Multiplayer.Game.Entities;
using JKMP.Plugin.Multiplayer.Matchmaking;
using JumpKing.PauseMenu;
using JumpKing.PauseMenu.BT;
using IDrawable = JumpKing.Util.IDrawable;

namespace JKMP.Plugin.Multiplayer.Game.UI
{
    public static class MenuManager
    {
        public static void CreateOptionsMenu(MenuFactory menuFactory, GuiFormat guiFormat, MenuSelector menuSelector, List<IDrawable> drawables)
        {
            var mpMenu = new MenuSelector(guiFormat);
            var passwordField = new TextInputField("Password", MatchmakingManager.Password ?? string.Empty)
            {
                TrimWhitespace = true,
                Visibility = TextInputField.TextVisibility.HiddenWhenUnfocused,
                Readonly = () => MatchmakingManager.Instance.IsConnected // Disable ability to change matchmaking password while connected
            };
            mpMenu.AddChild(passwordField);
            mpMenu.Initialize();
            drawables.Add(mpMenu);

            menuSelector.AddChild(new TextButton("Multiplayer", mpMenu));

            passwordField.OnValueChanged += OnPasswordChanged;
        }

        private static void OnPasswordChanged(string newPassword)
        {
            MatchmakingManager.Password = newPassword.Length > 0 ? newPassword : null;
            var gameEntity = EntityManager.instance.Find<GameEntity>();

            if (gameEntity != null)
            {
                gameEntity.P2P.DisconnectAll();
            }
        }
    }
}