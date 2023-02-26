// Decompiled with JetBrains decompiler
// Type: BugsnagWrapper
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 7A049970-C314-4BB9-8469-3CD036A16C96
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\King of the Hat\KingOfTheHat_Data\Managed\Assembly-CSharp.dll

using BugsnagUnity;
using BugsnagUnity.Payload;
using System;
using System.Collections.Generic;
using UnityEngine;
using VioletUI;

public static class BugsnagWrapper
{
    public static bool isStub;

    private static UIState State => StateMonobehaviour<UIState>.Singleton.State;

    public static void SetStage(string stage) => Bugsnag.Configuration.ReleaseStage = stage;

    public static void SetAppVersion(string appVersion) => Bugsnag.Configuration.AppVersion = appVersion;

    public static void BeforeNotify(Middleware middleware) {
        // Used to have a Bugsnag BeforeNotify function
    }

    public static void Notify(System.Exception exception) { 
        // This used to have Bugsnag.Notify(exception);
    }

    public static void LeaveBreadcrumb(string message, IDictionary<string, string> metadata) {
        // lmao no code.
    }

    public static void TagErrors(System.Collections.Generic.Dictionary<string, object> filePaths, bool notifyInEditor) {
        // Lmao no code.
    }
}
