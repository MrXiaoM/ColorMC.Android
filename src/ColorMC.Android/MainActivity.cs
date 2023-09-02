﻿using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Util;
using AndroidX.Core.App;
using Avalonia.Android;
using ColorMC.Android.Resources;
using ColorMC.Core;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui;
using Esprima.Ast;
using Java.Lang;
using Net.Kdt.Pojavview;
using Net.Kdt.Pojavview.Multirt;
using Net.Kdt.Pojavview.Tasks;
using Net.Kdt.Pojavview.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Java.Lang.Thread;
using StringBuilder = System.Text.StringBuilder;
using Uri = Android.Net.Uri;

namespace ColorMC.Android;

[Activity(Label = "ColorMC",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode,
    ScreenOrientation = ScreenOrientation.FullSensor)]
public class MainActivity : AvaloniaMainActivity<App>, IUncaughtExceptionHandler
{
    protected override void AttachBaseContext(Context? context)
    {
        base.AttachBaseContext(LocaleUtils.SetLocale(context));
    }

    public void UncaughtException(Thread t, Throwable e)
    {
        string file = GetExternalFilesDir(null).AbsolutePath + "/" + "latestcrash.txt";
        try
        {
            var crashStream = new StringBuilder();
            crashStream.Append("PojavLauncher crash report\n");
            crashStream.Append(" - Time: ").Append(DateTime.Now.ToString()).Append("\n");
            crashStream.Append(" - Device: ").Append(Build.Product).Append(" ").Append(Build.Model).Append("\n");
            crashStream.Append(" - Android version: ").Append(Build.VERSION.Release).Append("\n");
            crashStream.Append(" - Crash stack trace:\n");
            //crashStream.append(" - Launcher version: " + BuildConfig.VERSION_NAME + "\n");
            crashStream.Append(Log.GetStackTraceString(e));
            File.WriteAllText(file, crashStream.ToString());
        }
        catch (Throwable throwable)
        {
            Log.Error("ColorMC_Crash", " - Exception attempt saving crash stack trace:", throwable);
            Log.Error("ColorMC_Crash", " - The crash stack trace was:", e);
        }
    }

    protected override void OnCreate(Bundle savedInstanceState)
    {
        DefaultUncaughtExceptionHandler = this;

        ColorMCCore.PhoneGameLaunch = Start;
        ColorMCCore.PhoneJvmIntasll = PhoneJvmIntasll;
        ColorMCCore.PhoneReadJvm = PhoneReadJvm;
        ColorMCGui.StartPhone(GetExternalFilesDir(null).AbsolutePath + "/");
        base.OnCreate(savedInstanceState);

        Tools.AppName = "ColorMC";
    }

    public JavaInfo? PhoneReadJvm(string path)
    {
        var info = MultiRTUtils.Read(path);
        if(info == null)
        {
            return null;
        }

        return new()
        {
            Path = path,
            MajorVersion = info.JavaVersion,
            Type = "openjdk",
            Version = info.VersionString!,
        };
    }

    public void PhoneJvmIntasll(string path, string name)
    {
        MultiRTUtils.InstallRuntimeNamed(path);
    }

    public void OpenUrl(string url)
    {
        Uri uri = Uri.Parse(url);
        StartActivity(new Intent(Intent.ActionView, uri));
    }

    public void Start(GameSettingObj obj, List<string> list)
    {
        var file = obj.GetOptionsFile();
        if (!File.Exists(file))
        {
            File.WriteAllText(file, Resource1.options);
        }
        int i = 0;
        var mainIntent = new Intent();
        mainIntent.SetAction("ColorMC.Minecraft");
        mainIntent.PutExtra("GAME_DIR", list[i++]);
        mainIntent.PutExtra("JAVA_DIR", list[i++]);
        mainIntent.PutExtra("GAME_VERSION", list[i++]);
        mainIntent.PutExtra("JVM_VERSION", list[i++]);
        mainIntent.PutExtra("GAME_TIME", list[i++]);
        mainIntent.PutExtra("GAME_V2", list[i++] == "true");
        var jvmarg = int.Parse(list[i++]);
        var list1 = new List<string>();
        for (int a = 0; a < jvmarg; a++)
        {
            list1.Add(list[i++]);
        }
        mainIntent.PutExtra("JVM_ARGS", list1.ToArray());
        mainIntent.PutExtra("CLASSPATH", list[i++]);
        mainIntent.PutExtra("MAINCLASS", list[i++]);
        var gamearg = int.Parse(list[i++]);
        var list2 = new List<string>();
        for (int a = 0; a < gamearg; a++)
        {
            list2.Add(list[i++]);
        }
        mainIntent.PutExtra("GAME_ARGS", list2.ToArray());

        mainIntent.AddFlags(ActivityFlags.SingleTop);
        mainIntent.AddFlags(ActivityFlags.NewTask);
        StartActivity(mainIntent);
        //Finish();
    }
}
