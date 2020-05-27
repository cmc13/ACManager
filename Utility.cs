﻿using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;
using Microsoft.Win32;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace ACManager
{
    public static class Utility
    {
        // Installation location from registry entry
        private static readonly string PluginFolder = Registry.GetValue(
            @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Decal\Plugins\{A56AFA67-44C9-4DB9-871E-4A450FA5FBAC}",
            "Path",
            Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Asheron's Call").ToString();
        private static readonly string SettingsFile = PluginFolder + @"\settings.xml";
        private static readonly string ErrorFile = PluginFolder + @"\errors.txt";
        private static readonly string CrashLog = PluginFolder + @"\crashlog.txt";

        public static void SaveSetting(string module, string characterName, string setting, string value)
        {
            if (characterName.Contains(" "))
            {
                characterName = characterName.Replace(" ", "_");
            }
            try
            {
                if (File.Exists(SettingsFile))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(SettingsFile);

                    XmlNode node = doc.SelectSingleNode(String.Format(@"/Settings/{0}", CoreManager.Current.CharacterFilter.Server));
                    {
                        if (node != null)
                        // this server exists
                        {
                            node = doc.SelectSingleNode(String.Format(@"/Settings/{0}/{1}", CoreManager.Current.CharacterFilter.Server, CoreManager.Current.CharacterFilter.AccountName));
                            if (node != null)
                            {
                                // account exists
                                node = doc.SelectSingleNode(String.Format(@"/Settings/{0}/{1}/{2}", CoreManager.Current.CharacterFilter.Server, CoreManager.Current.CharacterFilter.AccountName, module));
                                if (node != null)
                                {
                                    // module exists
                                    node = doc.SelectSingleNode(String.Format(@"/Settings/{0}/{1}/{2}/Characters/{3}", CoreManager.Current.CharacterFilter.Server, CoreManager.Current.CharacterFilter.AccountName, module, characterName));
                                    if (node != null)
                                    {
                                        // character exists
                                        node = doc.SelectSingleNode(String.Format(@"/Settings/{0}/{1}/{2}/Characters/{3}/{4}", CoreManager.Current.CharacterFilter.Server, CoreManager.Current.CharacterFilter.AccountName, module, characterName, setting));
                                        if (node != null)
                                        {
                                            // setting exists
                                            node.InnerText = value;
                                        }
                                        else
                                        {
                                            // setting does not exist
                                            node = doc.SelectSingleNode(String.Format(@"/Settings/{0}/{1}/{2}/Characters/{3}", CoreManager.Current.CharacterFilter.Server, CoreManager.Current.CharacterFilter.AccountName, module, characterName));
                                            XmlNode newSetting = doc.CreateNode(XmlNodeType.Element, setting, string.Empty);
                                            newSetting.InnerText = value;
                                            node.AppendChild(newSetting);
                                        }
                                    }
                                    else
                                    {
                                        // character does not exist
                                        node = doc.SelectSingleNode(String.Format(@"/Settings/{0}/{1}/{2}/Characters", CoreManager.Current.CharacterFilter.Server, CoreManager.Current.CharacterFilter.AccountName, module));
                                        XmlNode newCharacterNode = doc.CreateNode(XmlNodeType.Element, characterName, string.Empty);
                                        XmlNode newSetting = doc.CreateNode(XmlNodeType.Element, setting, string.Empty);
                                        newSetting.InnerText = value;
                                        newCharacterNode.AppendChild(newSetting);
                                        node.AppendChild(newCharacterNode);
                                    }
                                }
                                else
                                {
                                    // module does not exist
                                    node = doc.SelectSingleNode(String.Format(@"/Settings/{0}/{1}", CoreManager.Current.CharacterFilter.Server, CoreManager.Current.CharacterFilter.AccountName));
                                    XmlNode newModule = doc.CreateNode(XmlNodeType.Element, module, string.Empty);
                                    XmlNode newCharacters = doc.CreateNode(XmlNodeType.Element, "Characters", string.Empty);
                                    XmlNode newCharacterNode = doc.CreateNode(XmlNodeType.Element, characterName, string.Empty);
                                    XmlNode newSetting = doc.CreateNode(XmlNodeType.Element, setting, string.Empty);
                                    newSetting.InnerText = value;
                                    newCharacterNode.AppendChild(newSetting);
                                    newCharacters.AppendChild(newCharacterNode);
                                    newModule.AppendChild(newCharacters);
                                    node.AppendChild(newModule);
                                }
                            }
                            else
                            {
                                // account doesn't exist
                                node = doc.SelectSingleNode(String.Format(@"/Settings/{0}", CoreManager.Current.CharacterFilter.Server));
                                XmlNode newAccount = doc.CreateNode(XmlNodeType.Element, CoreManager.Current.CharacterFilter.AccountName, string.Empty);
                                XmlNode newModule = doc.CreateNode(XmlNodeType.Element, module, string.Empty);
                                XmlNode newCharacters = doc.CreateNode(XmlNodeType.Element, "Characters", string.Empty);
                                XmlNode newCharacterNode = doc.CreateNode(XmlNodeType.Element, characterName, string.Empty);
                                XmlNode newSetting = doc.CreateNode(XmlNodeType.Element, setting, string.Empty);
                                newSetting.InnerText = value;
                                newCharacterNode.AppendChild(newSetting);
                                newCharacters.AppendChild(newCharacterNode);
                                newModule.AppendChild(newCharacters);
                                newAccount.AppendChild(newModule);
                                node.AppendChild(newAccount);
                            }
                        }
                        else
                        {
                            // server doesn't exists
                            node = doc.SelectSingleNode(@"/Settings");
                            XmlNode newServer = doc.CreateNode(XmlNodeType.Element, CoreManager.Current.CharacterFilter.Server, string.Empty);
                            XmlNode newAccount = doc.CreateNode(XmlNodeType.Element, CoreManager.Current.CharacterFilter.AccountName, string.Empty);
                            XmlNode newModule = doc.CreateNode(XmlNodeType.Element, module, string.Empty);
                            XmlNode newCharacters = doc.CreateNode(XmlNodeType.Element, "Characters", string.Empty);
                            XmlNode newCharacterNode = doc.CreateNode(XmlNodeType.Element, characterName, string.Empty);
                            XmlNode newSetting = doc.CreateNode(XmlNodeType.Element, setting, string.Empty);
                            newSetting.InnerText = value;
                            newCharacterNode.AppendChild(newSetting);
                            newCharacters.AppendChild(newCharacterNode);
                            newModule.AppendChild(newCharacters);
                            newAccount.AppendChild(newModule);
                            newServer.AppendChild(newAccount);
                            node.AppendChild(newServer);
                        }
                    }

                    doc.Save(SettingsFile);
                }
                else
                {
                    // file does not exist
                    using(XmlWriter writer = XmlWriter.Create(SettingsFile, SetupXmlWriter()))
                    {

                        writer.WriteStartDocument();
                        writer.WriteStartElement("Settings");

                        writer.WriteStartElement(CoreManager.Current.CharacterFilter.Server);
                        writer.WriteStartElement(CoreManager.Current.CharacterFilter.AccountName);
                        writer.WriteStartElement(module);
                        writer.WriteStartElement("Characters");
                        writer.WriteStartElement(characterName);
                        writer.WriteStartElement(setting);
                        writer.WriteString(value);

                        writer.WriteEndDocument();
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToChat(ex.Message);
            }
        }

        public static List<string> GetAdvertisements()
        {
            try
            {
                List<string> advertisements = new List<string>();
                if (File.Exists(SettingsFile))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(SettingsFile);
                    XmlNode node = doc.SelectSingleNode(string.Format(@"/Settings/{0}/{1}/{2}/Characters/{3}", CoreManager.Current.CharacterFilter.Server, CoreManager.Current.CharacterFilter.AccountName, "PortalBot", "BotGlobal"));
                    if (node != null)
                    {
                        XmlNodeList ads = node.ChildNodes;
                        foreach (XmlNode ad in ads)
                        {
                            advertisements.Add(ad.InnerText);
                        }
                    }
                }
                return advertisements;
            }
            catch
            {
                return null;
            }
        }

        public static XmlNode LoadCharacterSettings(string module, bool portal=false, string characterName="")
        {
            if (characterName.Contains(" "))
            {
                characterName = characterName.Replace(" ", "_");
            }

            XmlNode node = null;
            if (File.Exists(SettingsFile))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(SettingsFile);
                if (portal == false)
                {
                    node = doc.SelectSingleNode(String.Format(@"/Settings/{0}/{1}/{2}/Characters/{3}", CoreManager.Current.CharacterFilter.Server, CoreManager.Current.CharacterFilter.AccountName,  module, characterName));
                } else
                {
                    node = doc.SelectSingleNode(String.Format(@"/Settings/{0}/{1}/{2}/Characters", CoreManager.Current.CharacterFilter.Server, CoreManager.Current.CharacterFilter.AccountName, module));
                }
            }
            return node;
        }

        public static void DeleteSetting(string module, string characterName, string value)
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(SettingsFile);

                    XmlNode node = doc.SelectSingleNode(String.Format(@"/Settings/{0}", CoreManager.Current.CharacterFilter.Server));
                    {
                        if (node != null)
                        // this server exists
                        {
                            node = doc.SelectSingleNode(String.Format(@"/Settings/{0}/{1}", CoreManager.Current.CharacterFilter.Server, CoreManager.Current.CharacterFilter.AccountName));
                            if (node != null)
                            {
                                // account exists
                                node = doc.SelectSingleNode(String.Format(@"/Settings/{0}/{1}/{2}", CoreManager.Current.CharacterFilter.Server, CoreManager.Current.CharacterFilter.AccountName, module));
                                if (node != null)
                                {
                                    // module exists
                                    node = doc.SelectSingleNode(String.Format(@"/Settings/{0}/{1}/{2}/Characters/{3}", CoreManager.Current.CharacterFilter.Server, CoreManager.Current.CharacterFilter.AccountName, module, characterName));
                                    if (node != null)
                                    {
                                        XmlNodeList ads = node.ChildNodes;
                                        foreach (XmlNode ad in ads)
                                        {
                                            if (ad.InnerText.Equals(value))
                                            {
                                                ad.ParentNode.RemoveChild(ad);
                                                doc.Save(SettingsFile);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private static XmlWriterSettings SetupXmlWriter()
        {
            return new XmlWriterSettings
            {
                Indent = true
            };
        }

        public static void WriteToChat(string message)
        {
            try
            {
                CoreManager.Current.Actions.AddChatText(" <{ " + PluginCore.PluginName + " }>: " + message, 5);
            }
            catch (Exception ex) { LogError(ex); }
        }

        public static void LogError(Exception ex)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(ErrorFile, true))
                {
                    writer.WriteLine("============================================================================");
                    writer.WriteLine(DateTime.Now.ToString());
                    writer.WriteLine("Error: " + ex.Message);
                    writer.WriteLine("Source: " + ex.Source);
                    writer.WriteLine("Stack: " + ex.StackTrace);
                    if (ex.InnerException != null)
                    {
                        writer.WriteLine("Inner: " + ex.InnerException.Message);
                        writer.WriteLine("Inner Stack: " + ex.InnerException.StackTrace);
                    }
                    writer.WriteLine("============================================================================");
                    writer.WriteLine("");
                }
            } catch {}
        }

        public static void LogCrash(string characterName, string duration, string xp, string reasonIfKnown="Crash")
        {
            try
            {
                if (!characterName.Equals(""))
                {
                    using (StreamWriter writer = new StreamWriter(CrashLog, true))
                    {
                        writer.WriteLine(DateTime.Now.ToString() + " -" +
                            " Character=" + characterName + 
                            " Duration=" + duration + 
                            " XP=" + xp + 
                            " Reason=" + reasonIfKnown);
                    }
                }
            }
            catch (Exception ex) { LogError(ex); }
        }

        public static string GetVersion()
        {
            return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
        }
    }
}
