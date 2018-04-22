﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using Oxide.Game.Rust.Cui;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("PlayerAdministration", "ThibmoRozier", "1.1.0", ResourceId = 0)]
    [Description("Allows server admins to moderate users using a GUI from within the game.")]
    public class PlayerAdministration : RustPlugin
    {
        #region Custom GUI Classes
        /// <summary>
        /// UI Color object
        /// </summary>
        public class CuiColor
        {
            public byte Red { get; set; } = 0;
            public byte Green { get; set; } = 0;
            public byte Blue { get; set; } = 0;
            public float Alpha { get; set; } = 0f;

            /// <summary>
            /// Constructor
            /// </summary>
            public CuiColor() { }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="red">Red color value</param>
            /// <param name="green">Green color value</param>
            /// <param name="blue">Blue color value</param>
            /// <param name="alpha">Opacity</param>
            public CuiColor(byte red, byte green, byte blue, float alpha)
            {
                Red = red;
                Green = green;
                Blue = blue;
                Alpha = alpha;
            }

            /// <summary>
            /// Transform the values to a CuiColor string
            /// </summary>
            /// <returns></returns>
            public override string ToString() => $"{(double)Red / 255} {(double)Green / 255} {(double)Blue / 255} {Alpha}";
        }

        /// <summary>
        /// Element position object
        /// </summary>
        public class CuiRect
        {
            public float Top { get; set; } = 0f;
            public float Bottom { get; set; } = 0f;
            public float Left { get; set; } = 0f;
            public float Right { get; set; } = 0f;

            /// <summary>
            /// Constructor
            /// </summary>
            public CuiRect() { }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="top">Relative top position</param>
            /// <param name="bottom">Relative bottom position</param>
            /// <param name="left">Relative left position</param>
            /// <param name="right">Relative right position</param>
            public CuiRect(float top, float bottom, float left, float right)
            {
                Top = top;
                Bottom = bottom;
                Left = left;
                Right = right;
            }

            /// <summary>
            /// Return Left-Bottom as a string
            /// </summary>
            /// <returns></returns>
            public string GetPosMin() => $"{Left} {Bottom}";
            /// <summary>
            /// Return Right-Top as a string
            /// </summary>
            /// <returns></returns>
            public string GetPosMax() => $"{Right} {Top}";
        }

        /// <summary>
        /// Predefined default color set
        /// </summary>
        public class CuiDefaultColors
        {
            /// <summary>
            /// Default background color
            /// </summary>
            public static CuiColor Background { get; } = new CuiColor() { Red = 240, Green = 240, Blue = 240, Alpha = 0.3f };
            /// <summary>
            /// Medium-dark background color
            /// </summary>
            public static CuiColor BackgroundMedium { get; } = new CuiColor() { Red = 76, Green = 74, Blue = 72, Alpha = 0.83f };
            /// <summary>
            /// Dark background color
            /// </summary>
            public static CuiColor BackgroundDark { get; } = new CuiColor() { Red = 42, Green = 42, Blue = 42, Alpha = 0.93f };
            /// <summary>
            /// Default button color
            /// </summary>
            public static CuiColor Button { get; } = new CuiColor() { Red = 42, Green = 42, Blue = 42, Alpha = 0.9f };
            /// <summary>
            /// Inactive button color
            /// </summary>
            public static CuiColor ButtonInactive { get; } = new CuiColor() { Red = 168, Green = 168, Blue = 168, Alpha = 0.9f };
            /// <summary>
            /// Decline/Cancel button color
            /// </summary>
            public static CuiColor ButtonDecline { get; } = new CuiColor() { Red = 192, Green = 0, Blue = 0, Alpha = 0.9f };
            /// <summary>
            /// Default text color ( Black )
            /// </summary>
            public static CuiColor Text { get; } = new CuiColor() { Red = 0, Green = 0, Blue = 0, Alpha = 1f };
            /// <summary>
            /// Alternate default text color ( White )
            /// </summary>
            public static CuiColor TextAlt { get; } = new CuiColor() { Red = 255, Green = 255, Blue = 255, Alpha = 1f };
            /// <summary>
            /// Title text color ( Red-brown )
            /// </summary>
            public static CuiColor TextTitle { get; } = new CuiColor() { Red = 206, Green = 66, Blue = 43, Alpha = 1f };

            /// <summary>
            /// Fully opaque color
            /// </summary>
            public static CuiColor None { get; } = new CuiColor() { Red = 0, Green = 0, Blue = 0, Alpha = 0f };
        }

        /// <summary>
        /// Input field object
        /// </summary>
        public class CuiInputField
        {
            public CuiInputFieldComponent InputField { get; } = new CuiInputFieldComponent();
            public CuiRectTransformComponent RectTransform { get; } = new CuiRectTransformComponent();
            public float FadeOut { get; set; }
        }

        /// <summary>
        /// Custom version of the CuiElementContainer to add InputFields
        /// </summary>
        public class CustomCuiElementContainer : CuiElementContainer
        {
            public string Add(CuiInputField inputField, string parent = "Hud", string name = null)
            {
                if (string.IsNullOrEmpty(name)) name = CuiHelper.GetGuid();
                Add(new CuiElement {
                    Name = name,
                    Parent = parent,
                    FadeOut = inputField.FadeOut,
                    Components = {
                        inputField.InputField,
                        inputField.RectTransform
                    }
                });
                return name;
            }
        }

        /// <summary>
        /// Rust UI object
        /// </summary>
        public class Cui
        {
            /// <summary>
            /// The default Hud parent name
            /// </summary>
            public const string PARENT_HUD = "Hud";
            /// <summary>
            /// The default Overlay parent name
            /// </summary>
            public const string PARENT_OVERLAY = "Overlay";

            /// <summary>
            /// The main panel name
            /// </summary>
            public string MainPanelName { get; set; }

            private BasePlayer player;
            private CustomCuiElementContainer container = new CustomCuiElementContainer();

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="player">The player this object is meant for</param>
            public Cui(BasePlayer player)
            {
                this.player = player;
            }

            /// <summary>
            /// Add a new panel
            /// </summary>
            /// <param name="parent">The parent object name</param>
            /// <param name="anchor">The object's relative position</param>
            /// <param name="cursorEnabled">The panel requires the cursor</param>
            /// <param name="color">Image color</param>
            /// <param name="name">The object's name</param>
            /// <param name="png">Image PNG file path</param>
            /// <returns>New object name</returns>
            public string AddPanel(string parent,
                                   CuiRect anchor,
                                   bool cursorEnabled,
                                   CuiColor color = null,
                                   string name = null,
                                   string png = null) => AddPanel(parent, anchor, new CuiRect(), cursorEnabled, color, name, png);

            /// <summary>
            /// Add a new panel
            /// </summary>
            /// <param name="parent">The parent object name</param>
            /// <param name="anchor">The object's relative position</param>
            /// <param name="offset">The object's relative offset</param>
            /// <param name="cursorEnabled">The panel requires the cursor</param>
            /// <param name="color">Image color</param>
            /// <param name="name">The object's name</param>
            /// <param name="png">Image PNG file path</param>
            /// <returns>New object name</returns>
            public string AddPanel(string parent,
                                   CuiRect anchor,
                                   CuiRect offset,
                                   bool cursorEnabled,
                                   CuiColor color = null,
                                   string name = null,
                                   string png = null)
            {
                CuiPanel panel = new CuiPanel() {
                    RectTransform = {
                        AnchorMin = anchor.GetPosMin(),
                        AnchorMax = anchor.GetPosMax(),
                        OffsetMin = offset.GetPosMin(),
                        OffsetMax = offset.GetPosMax()
                    },
                    CursorEnabled = cursorEnabled
                };

                if (!string.IsNullOrEmpty(png) || (color != null)) {
                    panel.Image = new CuiImageComponent() {
                        Color = color.ToString(),
                        Png = png
                    };
                }

                return container.Add(panel, parent, name);
            }

            /// <summary>
            /// Add a new label
            /// </summary>
            /// <param name="parent">The parent object name</param>
            /// <param name="anchor">The object's relative position</param>
            /// <param name="color">Text color</param>
            /// <param name="text">Text to show</param>
            /// <param name="name">The object's name</param>
            /// <param name="fontSize">Font size</param>
            /// <param name="align">Text alignment</param>
            /// <returns>New object name</returns>
            public string AddLabel(string parent,
                                   CuiRect anchor,
                                   CuiColor color,
                                   string text,
                                   string name = null,
                                   int fontSize = 14,
                                   TextAnchor align = TextAnchor.UpperLeft) => AddLabel(parent, anchor, new CuiRect(), color, text, name, fontSize, align);

            /// <summary>
            /// Add a new label
            /// </summary>
            /// <param name="parent">The parent object name</param>
            /// <param name="anchor">The object's relative position</param>
            /// <param name="offset">The object's relative offset</param>
            /// <param name="color">Text color</param>
            /// <param name="text">Text to show</param>
            /// <param name="name">The object's name</param>
            /// <param name="fontSize">Font size</param>
            /// <param name="align">Text alignment</param>
            /// <returns>New object name</returns>
            public string AddLabel(string parent,
                                   CuiRect anchor,
                                   CuiRect offset,
                                   CuiColor color,
                                   string text,
                                   string name = null,
                                   int fontSize = 14,
                                   TextAnchor align = TextAnchor.UpperLeft)
            {
                return container.Add(new CuiLabel() {
                    Text = {
                        Text = text ?? "",
                        FontSize = fontSize,
                        Align = align,
                        Color = color.ToString()
                    },
                    RectTransform = {
                        AnchorMin = anchor.GetPosMin(),
                        AnchorMax = anchor.GetPosMax(),
                        OffsetMin = offset.GetPosMin(),
                        OffsetMax = offset.GetPosMax()
                    }
                }, parent, name);
            }

            /// <summary>
            /// Add a new button
            /// </summary>
            /// <param name="parent">The parent object name</param>
            /// <param name="anchor">The object's relative position</param>
            /// <param name="buttonColor">Button background color</param>
            /// <param name="textColor">Text color</param>
            /// <param name="text">Text to show</param>
            /// <param name="command">OnClick event callback command</param>
            /// <param name="close">Panel to close</param>
            /// <param name="name">The object's name</param>
            /// <param name="fontSize">Font size</param>
            /// <param name="align">Text alignment</param>
            /// <returns>New object name</returns>
            public string AddButton(string parent,
                                    CuiRect anchor,
                                    CuiColor buttonColor,
                                    CuiColor textColor,
                                    string text,
                                    string command = "",
                                    string close = "",
                                    string name = null,
                                    int fontSize = 14,
                                    TextAnchor align = TextAnchor.MiddleCenter) => AddButton(parent, anchor, new CuiRect(), buttonColor, textColor, text, command, close, name, fontSize, align);

            /// <summary>
            /// Add a new button
            /// </summary>
            /// <param name="parent">The parent object name</param>
            /// <param name="anchor">The object's relative position</param>
            /// <param name="offset">The object's relative offset</param>
            /// <param name="buttonColor">Button background color</param>
            /// <param name="textColor">Text color</param>
            /// <param name="text">Text to show</param>
            /// <param name="command">OnClick event callback command</param>
            /// <param name="close">Panel to close</param>
            /// <param name="name">The object's name</param>
            /// <param name="fontSize">Font size</param>
            /// <param name="align">Text alignment</param>
            /// <returns>New object name</returns>
            public string AddButton(string parent,
                                    CuiRect anchor,
                                    CuiRect offset,
                                    CuiColor buttonColor,
                                    CuiColor textColor,
                                    string text,
                                    string command = "",
                                    string close = "",
                                    string name = null,
                                    int fontSize = 14,
                                    TextAnchor align = TextAnchor.MiddleCenter)
            {
                return container.Add(new CuiButton() {
                    Button = {
                        Command = command ?? "",
                        Close = close ?? "",
                        Color = buttonColor.ToString()
                    },
                    RectTransform = {
                        AnchorMin = anchor.GetPosMin(),
                        AnchorMax = anchor.GetPosMax(),
                        OffsetMin = offset.GetPosMin(),
                        OffsetMax = offset.GetPosMax()
                    },
                    Text = {
                        Text = text ?? "",
                        FontSize = fontSize,
                        Align = align,
                        Color = textColor.ToString()
                    }
                }, parent, name);
            }

            /// <summary>
            /// Add a new input field
            /// </summary>
            /// <param name="parent">The parent object name</param>
            /// <param name="anchor">The object's relative position</param>
            /// <param name="color">Text color</param>
            /// <param name="text">Text to show</param>
            /// <param name="charsLimit">Max character count</param>
            /// <param name="command">OnChanged event callback command</param>
            /// <param name="isPassword">Indicates that this input should show password chars</param>
            /// <param name="name">The object's name</param>
            /// <param name="fontSize">Font size</param>
            /// <param name="align">Text alignment</param>
            /// <returns>New object name</returns>
            public string AddInputField(string parent,
                                        CuiRect anchor,
                                        CuiColor color,
                                        string text = "",
                                        int charsLimit = 100,
                                        string command = "",
                                        bool isPassword = false,
                                        string name = null,
                                        int fontSize = 14,
                                        TextAnchor align = TextAnchor.MiddleLeft) => AddInputField(parent, anchor, new CuiRect(), color, text, charsLimit, command, isPassword, name, fontSize, align);

            /// <summary>
            /// Add a new input field
            /// </summary>
            /// <param name="parent">The parent object name</param>
            /// <param name="anchor">The object's relative position</param>
            /// <param name="offset">The object's relative offset</param>
            /// <param name="fadeOut">Fade-out time</param>
            /// <param name="color">Text color</param>
            /// <param name="text">Text to show</param>
            /// <param name="charsLimit">Max character count</param>
            /// <param name="command">OnChanged event callback command</param>
            /// <param name="isPassword">Indicates that this input should show password chars</param>
            /// <param name="name">The object's name</param>
            /// <param name="fontSize">Font size</param>
            /// <returns>New object name</returns>
            public string AddInputField(string parent,
                                        CuiRect anchor,
                                        CuiRect offset,
                                        CuiColor color,
                                        string text = "",
                                        int charsLimit = 100,
                                        string command = "",
                                        bool isPassword = false,
                                        string name = null,
                                        int fontSize = 14,
                                        TextAnchor align = TextAnchor.MiddleLeft)
            {
                return container.Add(new CuiInputField() {
                    InputField = {
                        Text = text ?? "",
                        FontSize = fontSize,
                        Align = align,
                        Color = color.ToString(),
                        CharsLimit = charsLimit,
                        Command = command ?? "",
                        IsPassword = isPassword
                    },
                    RectTransform = {
                        AnchorMin = anchor.GetPosMin(),
                        AnchorMax = anchor.GetPosMax(),
                        OffsetMin = offset.GetPosMin(),
                        OffsetMax = offset.GetPosMax()
                    }
                }, parent, name);
            }

            /// <summary>
            /// Draw the UI to the player's client
            /// </summary>
            /// <returns>Success</returns>
            public bool Draw()
            {
                if (!string.IsNullOrEmpty(MainPanelName))
                    return CuiHelper.AddUi(player, container);

                return false;
            }

            /// <summary>
            /// Retrieve the userId of the player this GUI is intended for
            /// </summary>
            /// <returns>Player ID</returns>
            public string GetPlayerId() => player.UserIDString;
        }
        #endregion Custom GUI Classes

        #region GUI Helpers
        /// <summary>
        /// UI pages to make the switching more humanly readable
        /// </summary>
        private enum UiPage
        {
            Main = 0,
            Players,
            PlayersBanned,
            PlayerPage,
            PlayerPageBanned
        }

        /// <summary>
        /// Get a "page" of entities from a specified list
        /// </summary>
        /// <param name="aList">List of entities</param>
        /// <param name="aPage">Page number (Starting from 0)</param>
        /// <param name="aPageSize">Page size</param>
        /// <returns>List of entities</returns>
        List<T> GetPage<T>(IList<T> aList, int aPage, int aPageSize) => aList.Skip(aPage * aPageSize).Take(aPageSize).ToList();

        /// <summary>
        /// Add a button to the tab menu
        /// </summary>
        /// <param name="aUIObj">Cui object</param>
        /// <param name="aParent">Name of the parent object</param>
        /// <param name="aCaption">Text to show</param>
        /// <param name="aCommand">Button to execute</param>
        /// <param name="aPos">Bounds of the button</param>
        /// <param name="aIndActive">To indicate whether or not the button is active</param>
        private void AddTabMenuBtn(ref Cui aUIObj, string aParent, string aCaption, string aCommand, int aPos, bool aIndActive)
        {
            Vector2 dimensions = new Vector2(0.096f, 0.75f);
            Vector2 offset = new Vector2(0.005f, 0.1f);
            CuiColor btnColor = (aIndActive ? CuiDefaultColors.ButtonInactive : CuiDefaultColors.Button);
            float leftPos = ((dimensions.x + offset.x) * aPos) + offset.x;

            CuiRect anchor = new CuiRect() {
                Left = leftPos,
                Bottom = offset.y,
                Right = leftPos + dimensions.x,
                Top = offset.y + dimensions.y
            };

            aUIObj.AddButton(aParent, anchor, btnColor, CuiDefaultColors.TextAlt, aCaption, (aIndActive ? "" : aCommand));
        }

        /// <summary>
        /// Add a set of user buttons to the parent object
        /// </summary>
        /// <param name="aUIObj">Cui object</param>
        /// <param name="aParent">Name of the parent object</param>
        /// <param name="aUserList">List of BasePlayer entities</param>
        /// <param name="aCommandFmt">Base format of the command to execute (Will be completed with the user ID</param>
        /// <param name="aPage">User list page</param>
        private void AddPlayerButtons(ref Cui aUIObj, string aParent, ref List<BasePlayer> aUserList, string aCommandFmt, int aPage)
        {
            List<BasePlayer> userRange = GetPage(aUserList, aPage, MAX_PLAYER_BUTTONS);
            Vector2 dimensions = new Vector2(0.194f, 0.09f);
            Vector2 offset = new Vector2(0.005f, 0.01f);
            int col = -1;
            int row = 0;
            float margin = 0.12f;

            for (int i = 0; i < userRange.Count; i++) {
                if (++col >= MAX_PLAYER_COLS) {
                    row++;
                    col = 0;
                };

                BasePlayer user = userRange[i];
                float calcTop = (1f - margin) - (((dimensions.y + offset.y) * row) + offset.y);
                float calcLeft = ((dimensions.x + offset.x) * col) + offset.x;
                CuiRect anchor = new CuiRect() {
                    Left = calcLeft,
                    Bottom = calcTop - dimensions.y,
                    Right = calcLeft + dimensions.x,
                    Top = calcTop
                };
                string btnText = user.displayName;

                aUIObj.AddButton(aParent, anchor, CuiDefaultColors.Button, CuiDefaultColors.TextAlt, btnText, string.Format(aCommandFmt, user.UserIDString), "", "", 16);
            };
        }

        /// <summary>
        /// Add a set of user buttons to the parent object
        /// </summary>
        /// <param name="aUIObj">Cui object</param>
        /// <param name="aParent">Name of the parent object</param>
        /// <param name="aUserList">List of ServerUsers.User entities</param>
        /// <param name="aCommandFmt">Base format of the command to execute (Will be completed with the user ID</param>
        /// <param name="aPage">User list page</param>
        private void AddPlayerButtons(ref Cui aUIObj, string aParent, ref List<ServerUsers.User> aUserList, string aCommandFmt, int aPage)
        {
            List<ServerUsers.User> userRange = GetPage(aUserList, aPage, MAX_PLAYER_BUTTONS);
            Vector2 dimensions = new Vector2(0.194f, 0.09f);
            Vector2 offset = new Vector2(0.005f, 0.01f);
            int col = -1;
            int row = 0;
            float margin = 0.12f;

            for (int i = 0; i < userRange.Count; i++) {
                if (++col >= MAX_PLAYER_COLS) {
                    row++;
                    col = 0;
                };

                ServerUsers.User user = userRange[i];
                float calcTop = (1f - margin) - (((dimensions.y + offset.y) * row) + offset.y);
                float calcLeft = ((dimensions.x + offset.x) * col) + offset.x;
                CuiRect anchor = new CuiRect() {
                    Left = calcLeft,
                    Bottom = calcTop - dimensions.y,
                    Right = calcLeft + dimensions.x,
                    Top = calcTop
                };
                string btnText = ((string.IsNullOrEmpty(user.username) || UNKNOWN_NAME_LIST.Contains(user.username)) ? $"{user.steamid}" : user.username);

                aUIObj.AddButton(aParent, anchor, CuiDefaultColors.Button, CuiDefaultColors.TextAlt, btnText, string.Format(aCommandFmt, user.steamid), "", "", 16);
            };
        }
        #endregion GUI Helpers

        #region GUI Build Methods
        /// <summary>
        /// Build the tab nav-bar
        /// </summary>
        /// <param name="aUIObj">Cui object</param>
        /// <param name="aPageType">The active page type</param>
        private void BuildTabMenu(ref Cui aUIObj, UiPage aPageType)
        {
            string uiUserId = aUIObj.GetPlayerId();
            // Add the panels and title label
            string headerPanel = aUIObj.AddPanel(aUIObj.MainPanelName, TabMenuHeaderContainerAnchor, false, CuiDefaultColors.None);
            string tabBtnPanel = aUIObj.AddPanel(aUIObj.MainPanelName, TabMenuTabBtnContainerAnchor, false, CuiDefaultColors.Background);
            aUIObj.AddLabel(headerPanel, TabMenuHeaderLblAnchor, CuiDefaultColors.TextTitle,
                            "Player Administration by ThibmoRozier", "", 22, TextAnchor.MiddleCenter);

            // Add the tab menu buttons
            aUIObj.AddButton(headerPanel, TabMenuCloseBtnAnchor, CuiDefaultColors.ButtonDecline,
                             CuiDefaultColors.TextAlt, "X", "padm_closeui", "", "", 22);
            AddTabMenuBtn(ref aUIObj, tabBtnPanel, _("Main Tab Text", uiUserId),
                          "padm_switchui Main", 0, (aPageType == UiPage.Main ? true : false));
            AddTabMenuBtn(ref aUIObj, tabBtnPanel, _("Player Tab Text", uiUserId),
                          "padm_switchui Players 0", 1, (aPageType == UiPage.Players ? true : false));
            AddTabMenuBtn(ref aUIObj, tabBtnPanel, _("Banned Player Tab Text", uiUserId),
                          "padm_switchui PlayersBanned 0", 2, (aPageType == UiPage.PlayersBanned ? true : false));
        }

        /// <summary>
        /// Build the main-menu
        /// </summary>
        /// <param name="aUIObj">Cui object</param>
        private void BuildMainPage(ref Cui aUIObj)
        {
            string uiUserId = aUIObj.GetPlayerId();
            // Add the panels and title
            string panel = aUIObj.AddPanel(aUIObj.MainPanelName, MainPagePanelAnchor, false, CuiDefaultColors.Background);
            aUIObj.AddLabel(panel, MainPageLblTitleAnchor, CuiDefaultColors.TextAlt, "Main", "", 18, TextAnchor.MiddleLeft);

            // Add the ban by ID group
            aUIObj.AddLabel(panel, MainPageLblBanByIdTitleAnchor, CuiDefaultColors.TextTitle,
                            _("Ban By ID Title Text", uiUserId), null, 16, TextAnchor.MiddleLeft);
            aUIObj.AddLabel(panel, MainPageLblBanByIdAnchor, CuiDefaultColors.TextAlt,
                            _("Ban By ID Label Text", uiUserId), null, 14, TextAnchor.MiddleLeft);
            string panelBanByIdGroup = aUIObj.AddPanel(panel, MainPagePanelBanByIdAnchor, false, CuiDefaultColors.BackgroundDark);
                aUIObj.AddInputField(panelBanByIdGroup, MainPageEdtBanByIdAnchor, CuiDefaultColors.TextAlt, null, 24, "padm_mainpagebanidinputtext");

            if (configData.EnableBan) {
                aUIObj.AddButton(panel, MainPageBtnBanByIdAnchor, CuiDefaultColors.Button, CuiDefaultColors.TextAlt, "Ban", "padm_mainpagebanbyid");
            } else {
                aUIObj.AddButton(panel, MainPageBtnBanByIdAnchor, CuiDefaultColors.ButtonInactive, CuiDefaultColors.TextAlt, "Ban");
            }
        }

        /// <summary>
        /// Build a page of user buttons
        /// </summary>
        /// <param name="aUIObj">Cui object</param>
        /// <param name="aPageType">The active page type</param>
        /// <param name="aPage">User list page</param>
        private void BuildUserBtnPage(ref Cui aUIObj, UiPage aPageType, int aPage)
        {
            string pageLabel = _("User Button Page Title Text", aUIObj.GetPlayerId());
            string npBtnCommandFmt;
            int userCount;
            string panel = aUIObj.AddPanel(aUIObj.MainPanelName, UserBtnPanelAnchor, false, CuiDefaultColors.Background);
            aUIObj.AddLabel(panel, UserBtnLblAnchor, CuiDefaultColors.TextAlt, pageLabel, "", 18, TextAnchor.MiddleLeft);

            if (aPageType == UiPage.PlayersBanned) {
                BuildBannedUserButtons(ref aUIObj, panel, ref aPage, out npBtnCommandFmt, out userCount);
            } else {
                BuildUserButtons(ref aUIObj, panel, ref aPage, out npBtnCommandFmt, out userCount);
            }

            // Decide whether or not to activate the "previous" button
            if (aPage == 0) {
                aUIObj.AddButton(panel, UserBtnPreviousBtnAnchor, CuiDefaultColors.ButtonInactive, CuiDefaultColors.TextAlt, "<<", "", "", "", 18);
            } else {
                aUIObj.AddButton(panel, UserBtnPreviousBtnAnchor, CuiDefaultColors.Button, CuiDefaultColors.TextAlt,
                                 "<<", string.Format(npBtnCommandFmt, aPage - 1), "", "", 18);
            };

            // Decide whether or not to activate the "next" button
            if (userCount > MAX_PLAYER_BUTTONS * (aPage + 1)) {
                aUIObj.AddButton(panel, UserBtnNextBtnAnchor, CuiDefaultColors.Button, CuiDefaultColors.TextAlt,
                                 ">>", string.Format(npBtnCommandFmt, aPage + 1), "", "", 18);
            } else {
                aUIObj.AddButton(panel, UserBtnNextBtnAnchor, CuiDefaultColors.ButtonInactive, CuiDefaultColors.TextAlt, ">>", "", "", "", 18);
            };
        }

        /// <summary>
        /// Build the current user buttons
        /// </summary>
        /// <param name="aUIObj">Cui object</param>
        /// <param name="aParent">The active page type</param>
        /// <param name="aPage">User list page</param>
        /// <param name="aBtnCommandFmt">Command format for the buttons</param>
        /// <param name="aUserCount">Total user count</param>
        private void BuildUserButtons(ref Cui aUIObj, string aParent, ref int aPage, out string aBtnCommandFmt, out int aUserCount)
        {
            string commandFmt = "padm_switchui PlayerPage {0}";
            List<BasePlayer> userList = GetServerUserList();
            aBtnCommandFmt = "padm_switchui Players {0}";
            aUserCount = userList.Count;

            if ((aPage != 0) && (userList.Count <= MAX_PLAYER_BUTTONS))
                aPage = 0; // Reset page to 0 if user count is lower or equal to max button count

            AddPlayerButtons(ref aUIObj, aParent, ref userList, commandFmt, aPage);
        }

        /// <summary>
        /// Build the banned user buttons
        /// </summary>
        /// <param name="aUIObj">Cui object</param>
        /// <param name="aParent">The active page type</param>
        /// <param name="aPage">User list page</param>
        /// <param name="aBtnCommandFmt">Command format for the buttons</param>
        /// <param name="aUserCount">Total user count</param>
        private void BuildBannedUserButtons(ref Cui aUIObj, string aParent, ref int aPage, out string aBtnCommandFmt, out int aUserCount)
        {
            string commandFmt = "padm_switchui PlayerPageBanned {0}";
            List<ServerUsers.User> userList = GetBannedUserList();
            aBtnCommandFmt = "padm_switchui PlayersBanned {0}";
            aUserCount = userList.Count;

            if ((aPage != 0) && (userList.Count <= MAX_PLAYER_BUTTONS))
                aPage = 0; // Reset page to 0 if user count is lower or equal to max button count

            AddPlayerButtons(ref aUIObj, aParent, ref userList, commandFmt, aPage);
        }

        /// <summary>
        /// Build the user information and administration page
        /// </summary>
        /// <param name="aUIObj">Cui object</param>
        /// <param name="aPageType">The active page type</param>
        /// <param name="aPlayerId">Player ID (SteamId64)</param>
        private void BuildUserPage(ref Cui aUIObj, UiPage aPageType, ulong aPlayerId)
        {
            string uiUserId = aUIObj.GetPlayerId();

            // Add panels
            string panel = aUIObj.AddPanel(aUIObj.MainPanelName, UserPagePanelAnchor, false, CuiDefaultColors.Background);
            string infoPanel = aUIObj.AddPanel(panel, UserPageInfoPanelAnchor, false, CuiDefaultColors.BackgroundMedium);
            string actionPanel = aUIObj.AddPanel(panel, UserPageActionPanelAnchor, false, CuiDefaultColors.BackgroundMedium);

            // Add title labels
            aUIObj.AddLabel(infoPanel, UserPageLblinfoTitleAnchor, CuiDefaultColors.TextTitle,
                            _("Player Info Label Text", uiUserId), "", 14, TextAnchor.MiddleLeft);
            aUIObj.AddLabel(actionPanel, UserPageLblActionTitleAnchor, CuiDefaultColors.TextTitle,
                            _("Player Actions Label Text", uiUserId), "", 14, TextAnchor.MiddleLeft);

            if (aPageType == UiPage.PlayerPage) {
                BasePlayer player = BasePlayer.FindByID(aPlayerId) ?? BasePlayer.FindSleeping(aPlayerId);
                bool playerConnected = player?.IsConnected ?? false;
                string lastCheatStr = _("Never Label Text", uiUserId);

                // Pre-calc last admin cheat
                if (player.lastAdminCheatTime != 0f) {
                    TimeSpan lastCheatSinceStart = new TimeSpan(0, 0, (int)(Time.realtimeSinceStartup - player.lastAdminCheatTime));
                    DateTime lastCheat = DateTime.UtcNow.Subtract(lastCheatSinceStart);
                    lastCheatStr = $"{lastCheat.ToString(@"yyyy\/MM\/dd HH:mm:ss")} UTC";
                };

                aUIObj.AddLabel(panel, UserPageLblAnchor, CuiDefaultColors.TextAlt,
                                _("User Page Title Format", uiUserId, player.displayName, ""),
                                "", 18, TextAnchor.MiddleLeft);
                // Add user info labels
                aUIObj.AddLabel(infoPanel, UserPageLblIdAnchor, CuiDefaultColors.TextAlt,
                                _("Id Label Format", uiUserId, aPlayerId, (player.IsDeveloper ? _("Dev Label Text", uiUserId) : "")),
                                "", 14, TextAnchor.MiddleLeft);
                aUIObj.AddLabel(infoPanel, UserPageLblAuthAnchor, CuiDefaultColors.TextAlt,
                                _("Auth Level Label Format", uiUserId, player.net.connection.authLevel),
                                "", 14, TextAnchor.MiddleLeft);
                aUIObj.AddLabel(infoPanel, UserPageLblConnectAnchor, CuiDefaultColors.TextAlt,
                                _("Connection Label Format", uiUserId, (
                                    playerConnected ? _("Connected Label Text", uiUserId)
                                                    : _("Disconnected Label Text", uiUserId))
                                ),
                                "", 14, TextAnchor.MiddleLeft);
                aUIObj.AddLabel(infoPanel, UserPageLblSleepAnchor, CuiDefaultColors.TextAlt,
                                _("Status Label Format", uiUserId, (
                                    player.IsSleeping() ? _("Sleeping Label Text", uiUserId)
                                                        : _("Awake Label Text", uiUserId)
                                ), (
                                    player.IsAlive() ? _("Alive Label Text", uiUserId)
                                                     : _("Dead Label Text", uiUserId))
                                ),
                                "", 14, TextAnchor.MiddleLeft);
                aUIObj.AddLabel(infoPanel, UserPageLblFlagAnchor, CuiDefaultColors.TextAlt,
                                _("Flags Label Format", uiUserId, (player.IsFlying ? _("Flying Label Text", uiUserId) : ""),
                                                                  (player.isMounted ? _("Mounted Label Text", uiUserId) : "")),
                                "", 14, TextAnchor.MiddleLeft);
                aUIObj.AddLabel(infoPanel, UserPageLblPosAnchor, CuiDefaultColors.TextAlt,
                                _("Position Label Format", uiUserId, player.ServerPosition),
                                "", 14, TextAnchor.MiddleLeft);
                aUIObj.AddLabel(infoPanel, UserPageLblRotAnchor, CuiDefaultColors.TextAlt,
                                _("Rotation Label Format", uiUserId, player.GetNetworkRotation()),
                                "", 14, TextAnchor.MiddleLeft);
                aUIObj.AddLabel(infoPanel, UserPageLblAdminCheatAnchor, CuiDefaultColors.TextAlt,
                                _("Last Admin Cheat Label Format", uiUserId, lastCheatStr),
                                "", 14, TextAnchor.MiddleLeft);
                aUIObj.AddLabel(infoPanel, UserPageLblIdleAnchor, CuiDefaultColors.TextAlt,
                                _("Idle Time Label Format", uiUserId, Convert.ToInt32(player.IdleTime)),
                                "", 14, TextAnchor.MiddleLeft);
                aUIObj.AddLabel(infoPanel, UserPageLblHealthAnchor, CuiDefaultColors.TextAlt,
                                _("Health Label Format", uiUserId, player.health),
                                "", 14, TextAnchor.MiddleLeft);
                aUIObj.AddLabel(infoPanel, UserPageLblCalAnchor, CuiDefaultColors.TextAlt,
                                _("Calories Label Format", uiUserId, player.metabolism.calories.value),
                                "", 14, TextAnchor.MiddleLeft);
                aUIObj.AddLabel(infoPanel, UserPageLblHydraAnchor, CuiDefaultColors.TextAlt,
                                _("Hydration Label Format", uiUserId, player.metabolism.hydration.value),
                                "", 14, TextAnchor.MiddleLeft);
                aUIObj.AddLabel(infoPanel, UserPageLblTempAnchor, CuiDefaultColors.TextAlt,
                                _("Temp Label Format", uiUserId, player.metabolism.temperature.value),
                                "", 14, TextAnchor.MiddleLeft);
                aUIObj.AddLabel(infoPanel, UserPageLblWetAnchor, CuiDefaultColors.TextAlt,
                                _("Wetness Label Format", uiUserId, player.metabolism.wetness.value),
                                "", 14, TextAnchor.MiddleLeft);
                aUIObj.AddLabel(infoPanel, UserPageLblComfortAnchor, CuiDefaultColors.TextAlt,
                                _("Comfort Label Format", uiUserId, player.metabolism.comfort.value),
                                "", 14, TextAnchor.MiddleLeft);
                aUIObj.AddLabel(infoPanel, UserPageLblBleedAnchor, CuiDefaultColors.TextAlt,
                                _("Bleeding Label Format", uiUserId, player.metabolism.bleeding.value),
                                "", 14, TextAnchor.MiddleLeft);
                aUIObj.AddLabel(infoPanel, UserPageLblRads1Anchor, CuiDefaultColors.TextAlt,
                                _("Radiation Label Format", uiUserId, player.metabolism.radiation_poison.value),
                                "", 14, TextAnchor.MiddleLeft);
                aUIObj.AddLabel(infoPanel, UserPageLblRads2Anchor, CuiDefaultColors.TextAlt,
                                _("Radiation Protection Label Format", uiUserId, player.RadiationProtection()),
                                "", 14, TextAnchor.MiddleLeft);

                /* Build player action panel */
                if (configData.EnableBan) {
                    aUIObj.AddButton(actionPanel, UserPageBtnBanAnchor, CuiDefaultColors.Button, CuiDefaultColors.TextAlt,
                                     _("Ban Button Text", uiUserId), $"padm_banuser {aPlayerId}");
                } else {
                    aUIObj.AddButton(actionPanel, UserPageBtnBanAnchor, CuiDefaultColors.ButtonInactive, CuiDefaultColors.TextAlt,
                                     _("Ban Button Text", uiUserId));
                }
                if (configData.EnableKill) {
                    aUIObj.AddButton(actionPanel, UserPageBtnKillAnchor, CuiDefaultColors.Button, CuiDefaultColors.TextAlt,
                                     _("Kill Button Text", uiUserId), $"padm_killuser {aPlayerId}");
                } else {
                    aUIObj.AddButton(actionPanel, UserPageBtnKillAnchor, CuiDefaultColors.ButtonInactive, CuiDefaultColors.TextAlt,
                                     _("Kill Button Text", uiUserId));
                }
                if (configData.EnableKick && playerConnected) {
                    aUIObj.AddButton(actionPanel, UserPageBtnKickAnchor, CuiDefaultColors.Button, CuiDefaultColors.TextAlt,
                                     _("Kick Button Text", uiUserId), $"padm_kickuser {aPlayerId}");
                } else {
                    aUIObj.AddButton(actionPanel, UserPageBtnKickAnchor, CuiDefaultColors.ButtonInactive, CuiDefaultColors.Text,
                                     _("Kick Button Text", uiUserId));
                };

                // Add reset buttons
                if (configData.EnableClearInv) {
                    aUIObj.AddButton(actionPanel, UserPageBtnClearInventoryAnchor, CuiDefaultColors.Button, CuiDefaultColors.TextAlt,
                                     _("Clear Inventory Button Text", uiUserId), $"padm_clearuserinventory {aPlayerId}");
                } else {
                    aUIObj.AddButton(actionPanel, UserPageBtnClearInventoryAnchor, CuiDefaultColors.ButtonInactive, CuiDefaultColors.TextAlt,
                                     _("Clear Inventory Button Text", uiUserId));
                }
                if (configData.EnableResetBP) {
                    aUIObj.AddButton(actionPanel, UserPageBtnResetBPAnchor, CuiDefaultColors.Button, CuiDefaultColors.TextAlt,
                                     _("Reset Blueprints Button Text", uiUserId), $"padm_resetuserblueprints {aPlayerId}");
                } else {
                    aUIObj.AddButton(actionPanel, UserPageBtnResetBPAnchor, CuiDefaultColors.ButtonInactive, CuiDefaultColors.TextAlt,
                                     _("Reset Blueprints Button Text", uiUserId));
                }
                if (configData.EnableResetMetabolism) {
                    aUIObj.AddButton(actionPanel, UserPageBtnResetMetabolismAnchor, CuiDefaultColors.Button, CuiDefaultColors.TextAlt,
                                     _("Reset Metabolism Button Text", uiUserId), $"padm_resetusermetabolism {aPlayerId}");
                } else {
                    aUIObj.AddButton(actionPanel, UserPageBtnResetMetabolismAnchor, CuiDefaultColors.ButtonInactive, CuiDefaultColors.TextAlt,
                                     _("Reset Metabolism Button Text", uiUserId));
                }

                // Add hurt buttons
                if (configData.EnableHurt) {
                    aUIObj.AddButton(actionPanel, UserPageBtnHurt25Anchor, CuiDefaultColors.Button, CuiDefaultColors.TextAlt,
                                     _("Hurt 25 Button Text", uiUserId), $"padm_hurtuser {aPlayerId} 25");
                    aUIObj.AddButton(actionPanel, UserPageBtnHurt50Anchor, CuiDefaultColors.Button, CuiDefaultColors.TextAlt,
                                     _("Hurt 50 Button Text", uiUserId), $"padm_hurtuser {aPlayerId} 50");
                    aUIObj.AddButton(actionPanel, UserPageBtnHurt75Anchor, CuiDefaultColors.Button, CuiDefaultColors.TextAlt,
                                     _("Hurt 75 Button Text", uiUserId), $"padm_hurtuser {aPlayerId} 75");
                    aUIObj.AddButton(actionPanel, UserPageBtnHurt100Anchor, CuiDefaultColors.Button, CuiDefaultColors.TextAlt,
                                     _("Hurt 100 Button Text", uiUserId), $"padm_hurtuser {aPlayerId} 100");
                } else {
                    aUIObj.AddButton(actionPanel, UserPageBtnHurt25Anchor, CuiDefaultColors.ButtonInactive, CuiDefaultColors.TextAlt,
                                     _("Hurt 25 Button Text", uiUserId));
                    aUIObj.AddButton(actionPanel, UserPageBtnHurt50Anchor, CuiDefaultColors.ButtonInactive, CuiDefaultColors.TextAlt,
                                     _("Hurt 50 Button Text", uiUserId));
                    aUIObj.AddButton(actionPanel, UserPageBtnHurt75Anchor, CuiDefaultColors.ButtonInactive, CuiDefaultColors.TextAlt,
                                     _("Hurt 75 Button Text", uiUserId));
                    aUIObj.AddButton(actionPanel, UserPageBtnHurt100Anchor, CuiDefaultColors.ButtonInactive, CuiDefaultColors.TextAlt,
                                     _("Hurt 100 Button Text", uiUserId));
                }

                // Add heal buttons
                if (configData.EnableHeal) {
                    aUIObj.AddButton(actionPanel, UserPageBtnHeal25Anchor, CuiDefaultColors.Button, CuiDefaultColors.TextAlt,
                                     _("Heal 25 Button Text", uiUserId), $"padm_healuser {aPlayerId} 25");
                    aUIObj.AddButton(actionPanel, UserPageBtnHeal50Anchor, CuiDefaultColors.Button, CuiDefaultColors.TextAlt,
                                     _("Heal 50 Button Text", uiUserId), $"padm_healuser {aPlayerId} 50");
                    aUIObj.AddButton(actionPanel, UserPageBtnHeal75Anchor, CuiDefaultColors.Button, CuiDefaultColors.TextAlt,
                                     _("Heal 75 Button Text", uiUserId), $"padm_healuser {aPlayerId} 75");
                    aUIObj.AddButton(actionPanel, UserPageBtnHeal100Anchor, CuiDefaultColors.Button, CuiDefaultColors.TextAlt,
                                     _("Heal 100 Button Text", uiUserId), $"padm_healuser {aPlayerId} 100");
                } else {
                    aUIObj.AddButton(actionPanel, UserPageBtnHeal25Anchor, CuiDefaultColors.ButtonInactive, CuiDefaultColors.TextAlt,
                                     _("Heal 25 Button Text", uiUserId));
                    aUIObj.AddButton(actionPanel, UserPageBtnHeal50Anchor, CuiDefaultColors.ButtonInactive, CuiDefaultColors.TextAlt,
                                     _("Heal 50 Button Text", uiUserId));
                    aUIObj.AddButton(actionPanel, UserPageBtnHeal75Anchor, CuiDefaultColors.ButtonInactive, CuiDefaultColors.TextAlt,
                                     _("Heal 75 Button Text", uiUserId));
                    aUIObj.AddButton(actionPanel, UserPageBtnHeal100Anchor, CuiDefaultColors.ButtonInactive, CuiDefaultColors.TextAlt,
                                     _("Heal 100 Button Text", uiUserId));
                }
            } else {
                ServerUsers.User serverUser = ServerUsers.Get(aPlayerId);

                aUIObj.AddLabel(panel, UserPageLblAnchor, CuiDefaultColors.TextAlt,
                                _("User Page Title Format", uiUserId, serverUser.username, _("Banned Label Text", uiUserId)),
                                "", 18, TextAnchor.MiddleLeft);
                // Add user info labels
                aUIObj.AddLabel(infoPanel, UserPageLblIdAnchor, CuiDefaultColors.TextAlt,
                                _("Id Label Format", uiUserId, aPlayerId, ""), "", 14, TextAnchor.MiddleLeft);

                /* Build player action panel */
                if (configData.EnableUnban) {
                    aUIObj.AddButton(actionPanel, UserPageBtnBanAnchor, CuiDefaultColors.Button, CuiDefaultColors.TextAlt,
                                     _("Unban Button Text", uiUserId), $"padm_unbanuser {aPlayerId}");
                } else {
                    aUIObj.AddButton(actionPanel, UserPageBtnBanAnchor, CuiDefaultColors.ButtonInactive, CuiDefaultColors.TextAlt,
                                     _("Unban Button Text", uiUserId));
                }
            }
        }

        /// <summary>
        /// Initiate the building of the UI page to show
        /// </summary>
        /// <param name="aPlayer"></param>
        /// <param name="aPageType"></param>
        /// <param name="aArg"></param>
        private void BuildUI(BasePlayer aPlayer, UiPage aPageType, string aArg = "")
        {
            // Initiate the new UI and panel
            Cui newUiLib = new Cui(aPlayer);
            newUiLib.MainPanelName = newUiLib.AddPanel(Cui.PARENT_OVERLAY, MainAnchor, true, CuiDefaultColors.BackgroundDark, MAIN_PANEL_NAME);

            BuildTabMenu(ref newUiLib, aPageType);

            switch (aPageType) {
                case UiPage.Main: {
                    BuildMainPage(ref newUiLib);
                    break;
                }
                case UiPage.Players:
                case UiPage.PlayersBanned: {
                    int page = 0;

                    if (aArg != "")
                        if (!int.TryParse(aArg, out page))
                            page = 0; // Just to be sure

                    BuildUserBtnPage(ref newUiLib, aPageType, page);
                    break;
                }
                case UiPage.PlayerPage:
                case UiPage.PlayerPageBanned: {
                    ulong playerId = aPlayer.userID;

                    if (aArg != "")
                        if (!ulong.TryParse(aArg, out playerId))
                            playerId = aPlayer.userID; // Just to be sure

                    BuildUserPage(ref newUiLib, aPageType, playerId);
                    break;
                }
            };

            // Cleanup any old/active UI and draw the new one
            CuiHelper.DestroyUi(aPlayer, MAIN_PANEL_NAME);
            newUiLib.Draw();
        }
        #endregion GUI Build Methods

        #region Config
        private class ConfigData
        {
            [DefaultValue("true")]
            [JsonProperty("Enable kick action")]
            public bool EnableKick { get; set; }
            [DefaultValue("true")]
            [JsonProperty("Enable ban action")]
            public bool EnableBan { get; set; }
            [DefaultValue("true")]
            [JsonProperty("Enable unban action")]
            public bool EnableUnban { get; set; }
            [DefaultValue("true")]
            [JsonProperty("Enable kill action")]
            public bool EnableKill { get; set; }
            [DefaultValue("true")]
            [JsonProperty("Enable inventory clear action")]
            public bool EnableClearInv { get; set; }
            [DefaultValue("true")]
            [JsonProperty("Enable blueprint resetaction")]
            public bool EnableResetBP { get; set; }
            [DefaultValue("true")]
            [JsonProperty("Enable metabolism reset action")]
            public bool EnableResetMetabolism { get; set; }
            [DefaultValue("true")]
            [JsonProperty("Enable hurt action")]
            public bool EnableHurt { get; set; }
            [DefaultValue("true")]
            [JsonProperty("Enable heal action")]
            public bool EnableHeal { get; set; }
        }
        #endregion

        #region Constants
        /// <summary>
        /// Max columns that fit in the menu
        /// </summary>
        private const int MAX_PLAYER_COLS = 5;
        /// <summary>
        /// Max rows that fit in the menu
        /// </summary>
        private const int MAX_PLAYER_ROWS = 8;
        /// <summary>
        /// Max total buttons that fit in the menu
        /// </summary>
        private const int MAX_PLAYER_BUTTONS = MAX_PLAYER_COLS * MAX_PLAYER_ROWS;
        /// <summary>
        /// The main menu panel name
        /// </summary>
        private const string MAIN_PANEL_NAME = "PAdm_MainPanel";
        /// <summary>
        /// Aliasses for unknown player names
        /// </summary>
        private readonly List<string> UNKNOWN_NAME_LIST = new List<string> { "unnamed", "Unknown" };
        /* Define layout */
        // Main panel bounds
        private readonly CuiRect MainAnchor = new CuiRect() { Left = 0.03f, Bottom = 0.15f, Right = 0.97f, Top = 0.97f };
        // Tab menu bounds
        private readonly CuiRect TabMenuHeaderContainerAnchor = new CuiRect() { Left = 0.005f, Bottom = 0.9085f, Right = 0.995f, Top = 1f };
        private readonly CuiRect TabMenuTabBtnContainerAnchor = new CuiRect() { Left = 0.005f, Bottom = 0.83f, Right = 0.995f, Top = 0.90849f };
        private readonly CuiRect TabMenuHeaderLblAnchor = new CuiRect() { Left = 0f, Bottom = 0f, Right = 1f, Top = 1f };
        private readonly CuiRect TabMenuCloseBtnAnchor = new CuiRect() { Left = 0.965f, Bottom = 0.1f, Right = 0.997f, Top = 0.9f };
        // Main page bounds
        private readonly CuiRect MainPagePanelAnchor = new CuiRect() { Left = 0.005f, Bottom = 0.01f, Right = 0.995f, Top = 0.817f };
        private readonly CuiRect MainPageLblTitleAnchor = new CuiRect() { Left = 0.005f, Bottom = 0.88f, Right = 0.995f, Top = 0.99f };
        private readonly CuiRect MainPageLblBanByIdTitleAnchor = new CuiRect() { Left = 0.005f, Bottom = 0.82f, Right = 0.995f, Top = 0.87f };
        private readonly CuiRect MainPageLblBanByIdAnchor = new CuiRect() { Left = 0.005f, Bottom = 0.76f, Right = 0.05f, Top = 0.81f };
        private readonly CuiRect MainPagePanelBanByIdAnchor = new CuiRect() { Left = 0.055f, Bottom = 0.76f, Right = 0.305f, Top = 0.81f };
        private readonly CuiRect MainPageEdtBanByIdAnchor = new CuiRect() { Left = 0.005f, Bottom = 0f, Right = 0.995f, Top = 1f };
        private readonly CuiRect MainPageBtnBanByIdAnchor = new CuiRect() { Left = 0.315f, Bottom = 0.76f, Right = 0.365f, Top = 0.81f };
        // User button page bounds 
        private readonly CuiRect UserBtnPanelAnchor = new CuiRect() { Left = 0.005f, Bottom = 0.01f, Right = 0.995f, Top = 0.817f };
        private readonly CuiRect UserBtnLblAnchor = new CuiRect() { Left = 0.005f, Bottom = 0.88f, Right = 0.995f, Top = 0.99f };
        private readonly CuiRect UserBtnPreviousBtnAnchor = new CuiRect() { Left = 0.005f, Bottom = 0.01f, Right = 0.035f, Top = 0.061875f };
        private readonly CuiRect UserBtnNextBtnAnchor = new CuiRect() { Left = 0.96f, Bottom = 0.01f, Right = 0.995f, Top = 0.061875f };
        // User page panel bounds
        private readonly CuiRect UserPagePanelAnchor = new CuiRect() { Left = 0.005f, Bottom = 0.01f, Right = 0.995f, Top = 0.817f };
        private readonly CuiRect UserPageInfoPanelAnchor = new CuiRect() { Left = 0.005f, Bottom = 0.01f, Right = 0.28f, Top = 0.87f };
        private readonly CuiRect UserPageActionPanelAnchor = new CuiRect() { Left = 0.285f, Bottom = 0.01f, Right = 0.995f, Top = 0.87f };
        // User page title label bounds
        private readonly CuiRect UserPageLblAnchor = new CuiRect() { Left = 0.005f, Bottom = 0.88f, Right = 0.995f, Top = 0.99f };
        private readonly CuiRect UserPageLblinfoTitleAnchor = new CuiRect() { Left = 0.025f, Bottom = 0.94f, Right = 0.975f, Top = 0.99f };
        private readonly CuiRect UserPageLblActionTitleAnchor = new CuiRect() { Left = 0.01f, Bottom = 0.94f, Right = 0.99f, Top = 0.99f };
        // User page info label bounds
        private readonly CuiRect UserPageLblIdAnchor = new CuiRect() { Left = 0.025f, Bottom = 0.87f, Right = 0.975f, Top = 0.92f };
        private readonly CuiRect UserPageLblAuthAnchor = new CuiRect() { Left = 0.025f, Bottom = 0.81f, Right = 0.975f, Top = 0.86f };
        private readonly CuiRect UserPageLblConnectAnchor = new CuiRect() { Left = 0.025f, Bottom = 0.75f, Right = 0.975f, Top = 0.80f };
        private readonly CuiRect UserPageLblSleepAnchor = new CuiRect() { Left = 0.025f, Bottom = 0.69f, Right = 0.975f, Top = 0.74f };
        private readonly CuiRect UserPageLblFlagAnchor = new CuiRect() { Left = 0.025f, Bottom = 0.63f, Right = 0.975f, Top = 0.68f };
        private readonly CuiRect UserPageLblPosAnchor = new CuiRect() { Left = 0.025f, Bottom = 0.57f, Right = 0.975f, Top = 0.62f };
        private readonly CuiRect UserPageLblRotAnchor = new CuiRect() { Left = 0.025f, Bottom = 0.51f, Right = 0.975f, Top = 0.56f };
        private readonly CuiRect UserPageLblAdminCheatAnchor = new CuiRect() { Left = 0.025f, Bottom = 0.45f, Right = 0.975f, Top = 0.50f };
        private readonly CuiRect UserPageLblIdleAnchor = new CuiRect() { Left = 0.025f, Bottom = 0.39f, Right = 0.975f, Top = 0.44f };
        private readonly CuiRect UserPageLblHealthAnchor = new CuiRect() { Left = 0.025f, Bottom = 0.25f, Right = 0.975f, Top = 0.30f };
        private readonly CuiRect UserPageLblCalAnchor = new CuiRect() { Left = 0.025f, Bottom = 0.19f, Right = 0.5f, Top = 0.24f };
        private readonly CuiRect UserPageLblHydraAnchor = new CuiRect() { Left = 0.5f, Bottom = 0.19f, Right = 0.975f, Top = 0.24f };
        private readonly CuiRect UserPageLblTempAnchor = new CuiRect() { Left = 0.025f, Bottom = 0.13f, Right = 0.5f, Top = 0.18f };
        private readonly CuiRect UserPageLblWetAnchor = new CuiRect() { Left = 0.5f, Bottom = 0.13f, Right = 0.975f, Top = 0.18f };
        private readonly CuiRect UserPageLblComfortAnchor = new CuiRect() { Left = 0.025f, Bottom = 0.07f, Right = 0.5f, Top = 0.12f };
        private readonly CuiRect UserPageLblBleedAnchor = new CuiRect() { Left = 0.5f, Bottom = 0.07f, Right = 0.975f, Top = 0.12f };
        private readonly CuiRect UserPageLblRads1Anchor = new CuiRect() { Left = 0.025f, Bottom = 0.01f, Right = 0.5f, Top = 0.06f };
        private readonly CuiRect UserPageLblRads2Anchor = new CuiRect() { Left = 0.5f, Bottom = 0.01f, Right = 0.975f, Top = 0.06f };
        // User page button bounds
        private readonly CuiRect UserPageBtnBanAnchor = new CuiRect() { Left = 0.01f, Bottom = 0.85f, Right = 0.16f, Top = 0.92f };
        private readonly CuiRect UserPageBtnKillAnchor = new CuiRect() { Left = 0.01f, Bottom = 0.76f, Right = 0.16f, Top = 0.83f };
        private readonly CuiRect UserPageBtnKickAnchor = new CuiRect() { Left = 0.17f, Bottom = 0.76f, Right = 0.32f, Top = 0.83f };
        private readonly CuiRect UserPageBtnClearInventoryAnchor = new CuiRect() { Left = 0.01f, Bottom = 0.67f, Right = 0.16f, Top = 0.74f };
        private readonly CuiRect UserPageBtnResetBPAnchor = new CuiRect() { Left = 0.17f, Bottom = 0.67f, Right = 0.32f, Top = 0.74f };
        private readonly CuiRect UserPageBtnResetMetabolismAnchor = new CuiRect() { Left = 0.33f, Bottom = 0.67f, Right = 0.48f, Top = 0.74f };
        private readonly CuiRect UserPageBtnHurt25Anchor = new CuiRect() { Left = 0.01f, Bottom = 0.49f, Right = 0.16f, Top = 0.56f };
        private readonly CuiRect UserPageBtnHurt50Anchor = new CuiRect() { Left = 0.17f, Bottom = 0.49f, Right = 0.32f, Top = 0.56f };
        private readonly CuiRect UserPageBtnHurt75Anchor = new CuiRect() { Left = 0.33f, Bottom = 0.49f, Right = 0.48f, Top = 0.56f };
        private readonly CuiRect UserPageBtnHurt100Anchor = new CuiRect() { Left = 0.49f, Bottom = 0.49f, Right = 0.64f, Top = 0.56f };
        private readonly CuiRect UserPageBtnHeal25Anchor = new CuiRect() { Left = 0.01f, Bottom = 0.40f, Right = 0.16f, Top = 0.47f };
        private readonly CuiRect UserPageBtnHeal50Anchor = new CuiRect() { Left = 0.17f, Bottom = 0.40f, Right = 0.32f, Top = 0.47f };
        private readonly CuiRect UserPageBtnHeal75Anchor = new CuiRect() { Left = 0.33f, Bottom = 0.40f, Right = 0.48f, Top = 0.47f };
        private readonly CuiRect UserPageBtnHeal100Anchor = new CuiRect() { Left = 0.49f, Bottom = 0.40f, Right = 0.64f, Top = 0.47f };
        #endregion Constants

        #region Variables
        /// <summary>
        /// Plugin configuration
        /// </summary>
        private ConfigData configData;
        /// <summary>
        /// List of user ID input texts
        /// </summary>
        // Format: <userId, text>
        private Dictionary<ulong, string> mainPageBanIdInputText = new Dictionary<ulong, string>();
        #endregion Variables

        #region Utils
        /// <summary>
        /// Get translated message for the specified key
        /// </summary>
        /// <param name="aKey">Message key</param>
        /// <param name="aPlayerId">Player ID</param>
        /// <param name="args">Optional args</param>
        /// <returns></returns>
        private string _(string aKey, string aPlayerId, params object[] args) => string.Format(lang.GetMessage(aKey, this, aPlayerId), args);

        /// <summary>
        /// Log an error message to the logfile
        /// </summary>
        /// <param name="aMessage"></param>
        private void LogError(string aMessage) => LogToFile("", $"[{DateTime.Now.ToString("hh:mm:ss")}] ERROR > {aMessage}", this);

        /// <summary>
        /// Log an informational message to the logfile
        /// </summary>
        /// <param name="aMessage"></param>
        private void LogInfo(string aMessage) => LogToFile("", $"[{DateTime.Now.ToString("hh:mm:ss")}] INFO > {aMessage}", this);

        /// <summary>
        /// Send a message to a specific player
        /// </summary>
        /// <param name="aPlayer">The player to send the message to</param>
        /// <param name="aMessage">The message to send</param>
        private void SendMessage(ref BasePlayer aPlayer, string aMessage) => rust.SendChatMessage(aPlayer, "", aMessage);

        /// <summary>
        /// Verify if a user has the specified permission
        /// </summary>
        /// <param name="aPlayer">The player</param>
        /// <param name="aPermission"></param>
        /// <returns></returns>
        private bool VerifyPermission(ref BasePlayer aPlayer, string aPermission)
        {
            if (permission.UserHasPermission(aPlayer.UserIDString, aPermission)) // User MUST have the required permission
                return true;

            SendMessage(ref aPlayer, _("Permission Error Text", aPlayer.UserIDString));
            LogError(_("Permission Error Log Text", aPlayer.UserIDString, aPlayer.displayName, aPermission));
            return false;
        }

        /// <summary>
        /// Retrieve server users
        /// </summary>
        /// <returns></returns>
        private List<BasePlayer> GetServerUserList()
        {
            List<BasePlayer> result = new List<BasePlayer>();
            Player.Players.ForEach(user => {
                ServerUsers.User servUser = ServerUsers.Get(user.userID);
                if (servUser == null || servUser?.group != ServerUsers.UserGroup.Banned)
                    result.Add(user);
            });
            Player.Sleepers.ForEach(user => {
                ServerUsers.User servUser = ServerUsers.Get(user.userID);
                if (servUser == null || servUser?.group != ServerUsers.UserGroup.Banned)
                    result.Add(user);
            });
            return result;
        }

        /// <summary>
        /// Retrieve server users
        /// </summary>
        /// <returns></returns>
        private List<ServerUsers.User> GetBannedUserList() => ServerUsers.GetAll(ServerUsers.UserGroup.Banned).ToList();

        /// <summary>
        /// Retrieve the target player ID from the arguments and report success
        /// </summary>
        /// <param name="aArg">Argument object</param>
        /// <param name="aTarget">Player ID</param>
        /// <returns></returns>
        private bool GetTargetFromArg(ref ConsoleSystem.Arg aArg, out ulong aTarget)
        {
            aTarget = 0;

            if (!aArg.HasArgs() || !ulong.TryParse(aArg.Args[0], out aTarget))
                return false;

            return true;
        }

        /// <summary>
        /// Retrieve the target player ID and amount from the arguments and report success
        /// </summary>
        /// <param name="aArg">Argument object</param>
        /// <param name="aTarget">Player ID</param>
        /// <param name="aAmount">Amount</param>
        /// <returns></returns>
        private bool GetTargetAmountFromArg(ref ConsoleSystem.Arg aArg, out ulong aTarget, out float aAmount)
        {
            aTarget = 0;
            aAmount = 0;

            if (!aArg.HasArgs(2) || !ulong.TryParse(aArg.Args[0], out aTarget) || !float.TryParse(aArg.Args[1], out aAmount))
                return false;

            return true;
        }
        #endregion Utils

        #region Hooks
        void Loaded()
        {
            configData = Config.ReadObject<ConfigData>();
            permission.RegisterPermission("playeradministration.show", this);
        }

        void Unload()
        {
            foreach (BasePlayer player in Player.Players)
                CuiHelper.DestroyUi(player, MAIN_PANEL_NAME);
        }

        protected override void LoadDefaultConfig()
        {
            ConfigData config = new ConfigData {
                EnableKick = true,
                EnableBan = true,
                EnableUnban = true,
                EnableKill = true,
                EnableClearInv = true,
                EnableResetBP = true,
                EnableResetMetabolism = true,
                EnableHurt = true,
                EnableHeal = true
            };
            Config.WriteObject(config, true);
        }

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string> {
                { "Permission Error Text", "You do not have the required permissions to use this command." },
                { "Permission Error Log Text", "{0}: Tried to execute a command requiring the '{1}' permission" },
                { "Kick Reason Message Text", "Administrative decision" },
                { "Ban Reason Message Text", "Administrative decision" },

                { "Never Label Text", "Never" },
                { "Banned Label Text", " (Banned)" },
                { "Dev Label Text", " (Developer)" },
                { "Connected Label Text", "Connected" },
                { "Disconnected Label Text", "Disconnected" },
                { "Sleeping Label Text", "Sleeping" },
                { "Awake Label Text", "Awake" },
                { "Alive Label Text", "Alive" },
                { "Dead Label Text", "Dead" },
                { "Flying Label Text", " Flying" },
                { "Mounted Label Text", " Mounted" },

                { "User Button Page Title Text", "Click a username to go to the player's control page" },
                { "User Page Title Format", "Control page for player '{0}'{1}" },

                { "Ban By ID Title Text", "Ban a user by ID" },
                { "Ban By ID Label Text", "User ID:" },
                { "Player Info Label Text", "Player information:" },
                { "Player Actions Label Text", "Player actions:" },

                { "Id Label Format", "ID: {0}{1}" },
                { "Auth Level Label Format", "Auth level: {0}" },
                { "Connection Label Format", "Connection: {0}" },
                { "Status Label Format", "Status: {0} and {1}" },
                { "Flags Label Format", "Flags:{0}{1}" },
                { "Position Label Format", "Position: {0}" },
                { "Rotation Label Format", "Rotation: {0}" },
                { "Last Admin Cheat Label Format", "Last admin cheat: {0}" },
                { "Idle Time Label Format", "Idle time: {0} seconds" },
                { "Health Label Format", "Health: {0}" },
                { "Calories Label Format", "Calories: {0}" },
                { "Hydration Label Format", "Hydration: {0}" },
                { "Temp Label Format", "Temperature: {0}" },
                { "Wetness Label Format", "Wetness: {0}" },
                { "Comfort Label Format", "Comfort: {0}" },
                { "Bleeding Label Format", "Bleeding: {0}" },
                { "Radiation Label Format", "Radiation: {0}" },
                { "Radiation Protection Label Format", "Protection: {0}" },

                { "Main Tab Text", "Main" },
                { "Player Tab Text", "Players" },
                { "Banned Player Tab Text", "Banned Players" },

                { "Clear Inventory Button Text", "Clear Inventory" },
                { "Reset Blueprints Button Text", "Reset Blueprints" },
                { "Reset Metabolism Button Text", "Reset Metabolism" },

                { "Hurt 25 Button Text", "Hurt 25" },
                { "Hurt 50 Button Text", "Hurt 50" },
                { "Hurt 75 Button Text", "Hurt 75" },
                { "Hurt 100 Button Text", "Hurt 100" },

                { "Heal 25 Button Text", "Heal 25" },
                { "Heal 50 Button Text", "Heal 50" },
                { "Heal 75 Button Text", "Heal 75" },
                { "Heal 100 Button Text", "Heal 100" },

                { "Ban Button Text", "Ban" },
                { "Kick Button Text", "Kick" },
                { "Kill Button Text", "Kill" },
                { "Unban Button Text", "Unban" }
            }, this, "en");
        }
        #endregion Hooks

        #region Command Callbacks
        [ChatCommand("padmin")]
        void PlayerManagerUICallback(BasePlayer aPlayer, string aCommand, string[] aArgs)
        {
            if (!VerifyPermission(ref aPlayer, "playeradministration.show"))
                return;

            LogInfo($"{aPlayer.displayName}: Opened the menu");
            BuildUI(aPlayer, UiPage.Main);
        }

        [ConsoleCommand("padm_closeui")]
        void PlayerManagerCloseUICallback(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            CuiHelper.DestroyUi(arg.Player(), MAIN_PANEL_NAME);

            if (mainPageBanIdInputText.ContainsKey(player.userID))
                mainPageBanIdInputText.Remove(player.userID);
        }

        [ConsoleCommand("padm_switchui")]
        void PlayerManagerSwitchUICallback(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();

            if (!VerifyPermission(ref player, "playeradministration.show") || !arg.HasArgs())
                return;

            switch (arg.Args[0].ToLower()) {
                case "players": {
                    BuildUI(player, UiPage.Players, (arg.HasArgs(2) ? arg.Args[1] : ""));
                    break;
                }
                case "playersbanned": {
                    BuildUI(player, UiPage.PlayersBanned, (arg.HasArgs(2) ? arg.Args[1] : ""));
                    break;
                }
                case "playerpage": {
                    BuildUI(player, UiPage.PlayerPage, (arg.HasArgs(2) ? arg.Args[1] : ""));
                    break;
                }
                case "playerpagebanned": {
                    BuildUI(player, UiPage.PlayerPageBanned, (arg.HasArgs(2) ? arg.Args[1] : ""));
                    break;
                }
                default: { // Main is the default for everything
                    BuildUI(player, UiPage.Main);
                    break;
                }
            };
        }

        [ConsoleCommand("padm_kickuser")]
        void PlayerManagerKickUserCallback(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            ulong targetId;
            
            if (!VerifyPermission(ref player, "playeradministration.show") ||
                !GetTargetFromArg(ref arg, out targetId) ||
                !configData.EnableKick)
                return;

            BasePlayer.FindByID(targetId)?.Kick(_("Kick Reason Message Text", targetId.ToString()));
            LogInfo($"{player.displayName}: Kicked user ID {targetId}");
            BuildUI(player, UiPage.PlayerPage, targetId.ToString());
        }

        [ConsoleCommand("padm_banuser")]
        void PlayerManagerBanUserCallback(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            ulong targetId;

            if (!VerifyPermission(ref player, "playeradministration.show") ||
                !GetTargetFromArg(ref arg, out targetId) ||
                !configData.EnableBan)
                return;

            Player.Ban(targetId, _("Ban Reason Message Text", targetId.ToString()));
            LogInfo($"{player.displayName}: Banned user ID {targetId}");
            BuildUI(player, UiPage.PlayerPage, targetId.ToString());
        }

        [ConsoleCommand("padm_mainpagebanbyid")]
        void PlayerManagerMainPageBanByIdCallback(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            ulong targetId;

            if (!VerifyPermission(ref player, "playeradministration.show") ||
                !mainPageBanIdInputText.ContainsKey(player.userID) ||
                !ulong.TryParse(mainPageBanIdInputText[player.userID], out targetId) ||
                !configData.EnableBan)
                return;

            Player.Ban(targetId, _("Ban Reason Message Text", targetId.ToString()));
            LogInfo($"{player.displayName}: Banned user ID {targetId}");
            BuildUI(player, UiPage.Main);
        }

        [ConsoleCommand("padm_unbanuser")]
        void PlayerManagerUnbanUserCallback(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            ulong targetId;

            if (!VerifyPermission(ref player, "playeradministration.show") ||
                !GetTargetFromArg(ref arg, out targetId) ||
                !configData.EnableUnban)
                return;

            Player.Unban(targetId);
            LogInfo($"{player.displayName}: Unbanned user ID {targetId}");
            BuildUI(player, UiPage.Main);
        }

        [ConsoleCommand("padm_killuser")]
        void PlayerManagerKillUserCallback(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            ulong targetId;

            if (!VerifyPermission(ref player, "playeradministration.show") ||
                !GetTargetFromArg(ref arg, out targetId) ||
                !configData.EnableKill)
                return;

            (BasePlayer.FindByID(targetId) ?? BasePlayer.FindSleeping(targetId))?.Die();
            LogInfo($"{player.displayName}: Killed user ID {targetId}");
            BuildUI(player, UiPage.PlayerPage, targetId.ToString());
        }

        [ConsoleCommand("padm_clearuserinventory")]
        void PlayerManagerClearUserInventoryCallback(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            ulong targetId;

            if (!VerifyPermission(ref player, "playeradministration.show") ||
                !GetTargetFromArg(ref arg, out targetId) ||
                !configData.EnableClearInv)
                return;

            (BasePlayer.FindByID(targetId) ?? BasePlayer.FindSleeping(targetId))?.inventory.Strip();
            LogInfo($"{player.displayName}: Cleared the inventory of user ID {targetId}");
            BuildUI(player, UiPage.PlayerPage, targetId.ToString());
        }

        [ConsoleCommand("padm_resetuserblueprints")]
        void PlayerManagerResetUserBlueprintsCallback(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            ulong targetId;

            if (!VerifyPermission(ref player, "playeradministration.show") ||
                !GetTargetFromArg(ref arg, out targetId) ||
                !configData.EnableResetBP)
                return;

            (BasePlayer.FindByID(targetId) ?? BasePlayer.FindSleeping(targetId))?.blueprints.Reset();
            LogInfo($"{player.displayName}: Reset the blueprints of user ID {targetId}");
            BuildUI(player, UiPage.PlayerPage, targetId.ToString());
        }

        [ConsoleCommand("padm_resetusermetabolism")]
        void PlayerManagerResetUserMetabolismCallback(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            ulong targetId;

            if (!VerifyPermission(ref player, "playeradministration.show") ||
                !GetTargetFromArg(ref arg, out targetId) ||
                !configData.EnableResetMetabolism)
                return;

            (BasePlayer.FindByID(targetId) ?? BasePlayer.FindSleeping(targetId))?.metabolism.Reset();
            LogInfo($"{player.displayName}: Reset the metabolism of user ID {targetId}");
            BuildUI(player, UiPage.PlayerPage, targetId.ToString());
        }

        [ConsoleCommand("padm_hurtuser")]
        void PlayerManagerHurtUserCallback(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            ulong targetId;
            float amount;

            if (!VerifyPermission(ref player, "playeradministration.show") ||
                !GetTargetAmountFromArg(ref arg, out targetId, out amount) ||
                !configData.EnableHurt)
                return;

            (BasePlayer.FindByID(targetId) ?? BasePlayer.FindSleeping(targetId))?.Hurt(amount);
            LogInfo($"{player.displayName}: Hurt user ID {targetId} for {amount} points");
            BuildUI(player, UiPage.PlayerPage, targetId.ToString());
        }

        [ConsoleCommand("padm_healuser")]
        void PlayerManagerHealUserCallback(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            ulong targetId;
            float amount;

            if (!VerifyPermission(ref player, "playeradministration.show") ||
                !GetTargetAmountFromArg(ref arg, out targetId, out amount) ||
                !configData.EnableHeal)
                return;

            (BasePlayer.FindByID(targetId) ?? BasePlayer.FindSleeping(targetId))?.Heal(amount);
            LogInfo($"{player.displayName}: Healed user ID {targetId} for {amount} points");
            BuildUI(player, UiPage.PlayerPage, targetId.ToString());
        }
        #endregion Command Callbacks

        #region Text Update Callbacks
        [ConsoleCommand("padm_mainpagebanidinputtext")]
        void PlayerManagerMainPageBanIdInputTextCallback(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();

            if (!VerifyPermission(ref player, "playeradministration.show") || !arg.HasArgs()) {
                if (mainPageBanIdInputText.ContainsKey(player.userID))
                    mainPageBanIdInputText.Remove(player.userID);

                return;
            };

            if (mainPageBanIdInputText.ContainsKey(player.userID)) {
                mainPageBanIdInputText[player.userID] = arg.Args[0];
            } else {
                mainPageBanIdInputText.Add(player.userID, arg.Args[0]);
            };
        }
        #endregion Text Update Callbacks
    }
}