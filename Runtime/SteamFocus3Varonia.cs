using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using Valve.VR;
using VaroniaBackOffice;



public enum HelmetState
{
    Ok = 0,
    NoGameFocusOrMicroLag = 2,
    NoStreamOrPowerOff = 3,
}


public enum TrackingState
{
    Ok = 0,
    Strange = 1,
    Lost = 2,
    BigLost = 3,
    NO = 4,
}





public class SteamFocus3Varonia : MonoBehaviour
{

    [BoxGroup("UI")] public Text Latency;
    [BoxGroup("UI")] public Text Fps;
    [BoxGroup("UI")] public Text LatencyV2;
    [BoxGroup("UI")] public Text LatencyV3;
    [BoxGroup("UI")] public Text TotalLag_UI;
    [BoxGroup("UI")] public GameObject Canvas;

    [HideInInspector]
    public List<string> Ping_ = new List<string>();

    [HideInInspector]
    public List<string> Fps_ = new List<string>();



    [BoxGroup("Infos")] public HelmetState HelmetState;
    [BoxGroup("Infos")] public TrackingState TrackingState;


    [BoxGroup("Infos")] public TrackingState WristState;
    [BoxGroup("Infos")] public Wrist Wrist_l;
    [BoxGroup("Infos")] public Wrist Wrist_r;
    [BoxGroup("Infos")] public int FrameJitter;
    [BoxGroup("Infos")] public float HMD_Battery;
    [BoxGroup("Infos")] public bool Right_Wrist_Ready;
    [BoxGroup("Infos")] public Vector3 Right_Wrist_Pos;
    [BoxGroup("Infos")] public bool Left_Wrist_Ready;
    [BoxGroup("Infos")] public Vector3 Left_Wrist_Pos;
    [BoxGroup("Infos")] public bool Left_Hand_Ready;
    [BoxGroup("Infos")] public bool Right_Hand_Ready;
    [BoxGroup("Infos")] public bool HMD_Ready;
    [BoxGroup("Infos")] public bool HMD_HasActivity;
    [BoxGroup("Infos")] public bool StrangeTracking;
    [BoxGroup("Infos")] public static int TotalLag;
    [BoxGroup("Infos")] public static int LagCount;
    [BoxGroup("Infos")] public bool NoGyro;






    [BoxGroup("Others")]
    public static SteamFocus3Varonia Instance;


    private DateTime BeginLag;
    private bool Lagging;

    private string Info;


    public string VBS_PC_Version;
    public string VBS_APK_Version;


    List<int> Flux_TotalLatency = new List<int>();
    List<int> Flux_NetworkLatency = new List<int>();
    List<int> Flux_ServerState = new List<int>();
    List<int> Flux_ULBandwidth = new List<int>();
    List<int> Flux_DLBandwidth = new List<int>();

    int ReadUp = 0;
    int ReadLastLines = 0;

    private int index_Flux_TotalLatency;
    private int index_Flux_NetworkLatency;
    private int index_Flux_ServerState;
    private int index_Flux_ULBandwidth;
    private int index_Flux_DLBandwidth;


    List<int> FPS_Pose = new List<int>();
    List<int> FPS_Driver = new List<int>();
    List<int> FPS_Encode = new List<int>();
    List<int> FPS_Network = new List<int>();
    List<int> FPS_Decode = new List<int>();
    List<int> FPS_Render = new List<int>();


    private int index_Flux_FPS_Pose;
    private int index_Flux_FPS_Driver;
    private int index_Flux_FPS_Encode;
    private int index_Flux_FPS_Network;
    private int index_Flux_FPS_Decode;
    private int index_Flux_FPS_Render;

    IEnumerator UpdateFluxUI()
    {
        Flux_TotalLatency.Add(-1);
        Flux_NetworkLatency.Add(-1);
        Flux_ServerState.Add(-1);
        Flux_ULBandwidth.Add(-1);
        Flux_DLBandwidth.Add(-1);


        FPS_Pose.Add(-1);
        FPS_Driver.Add(-1);
        FPS_Encode.Add(-1);
        FPS_Network.Add(-1);
        FPS_Decode.Add(-1);
        FPS_Render.Add(-1);

        while (true)
        {
            LatencyV2.text = "";// ReadUp.ToString() + " " + ReadLastLines + "\n";

            LatencyV2.text += "VBS Server Version : " + VBS_PC_Version + "\n";
            // LatencyV2.text += "VBS Client Version : " + VBS_APK_Version + "\n";


            if (Flux_TotalLatency[index_Flux_TotalLatency] != 0)
                LatencyV2.text += "TotalLatency per frame : " + Flux_TotalLatency[index_Flux_TotalLatency] + "\n";
            else
                LatencyV2.text += "TotalLatency per frame : LOST" + "\n";

            if (Flux_TotalLatency[index_Flux_NetworkLatency] != 0)
                LatencyV2.text += "Network per frame : " + Flux_NetworkLatency[index_Flux_NetworkLatency] + "\n";
            else
                LatencyV2.text += "Network per frame : LOST" + "\n";


            LatencyV2.text += "Server State : " + Flux_ServerState[index_Flux_ServerState] + "\n";

            LatencyV2.text += "UL Bandwidth : " + Flux_ULBandwidth[index_Flux_ULBandwidth] + " Kbps\n";

            LatencyV2.text += "DL Bandwidth : " + Flux_DLBandwidth[index_Flux_DLBandwidth] + " Kbps\n";


            LatencyV3.text = "";
            LatencyV3.text += "FPS Pose : " + FPS_Pose[index_Flux_FPS_Pose] + "\n";
            LatencyV3.text += "FPS Driver : " + FPS_Driver[index_Flux_FPS_Driver] + "\n";
            LatencyV3.text += "FPS Encode : " + FPS_Encode[index_Flux_FPS_Encode] + "\n";
            LatencyV3.text += "FPS Network : " + FPS_Network[index_Flux_FPS_Network] + "\n";
            LatencyV3.text += "FPS Decode : " + FPS_Decode[index_Flux_FPS_Decode] + "\n";
            LatencyV3.text += "FPS Render : " + FPS_Render[index_Flux_FPS_Render] + "\n";

            DebugVaronia.Instance.Latency.text = LatencyV2.text;

            yield return new WaitForSeconds(0.4f);

            if (Flux_TotalLatency.Count - 1 > index_Flux_TotalLatency)
                index_Flux_TotalLatency++;

            if (Flux_NetworkLatency.Count - 1 > index_Flux_NetworkLatency)
                index_Flux_NetworkLatency++;


            if (Flux_ServerState.Count - 1 > index_Flux_ServerState)
                index_Flux_ServerState++;

            if (Flux_ULBandwidth.Count - 1 > index_Flux_ULBandwidth)
                index_Flux_ULBandwidth++;

            if (Flux_DLBandwidth.Count - 1 > index_Flux_DLBandwidth)
                index_Flux_DLBandwidth++;


            if (FPS_Pose.Count - 1 > index_Flux_FPS_Pose)
                index_Flux_FPS_Pose++;
            if (FPS_Driver.Count - 1 > index_Flux_FPS_Driver)
                index_Flux_FPS_Driver++;
            if (FPS_Encode.Count - 1 > index_Flux_FPS_Encode)
                index_Flux_FPS_Encode++;
            if (FPS_Network.Count - 1 > index_Flux_FPS_Network)
                index_Flux_FPS_Network++;
            if (FPS_Decode.Count - 1 > index_Flux_FPS_Decode)
                index_Flux_FPS_Decode++;
            if (FPS_Render.Count - 1 > index_Flux_FPS_Render)
                index_Flux_FPS_Render++;


        }
    }

    IEnumerator SearchJitter()
    {
        yield return new WaitForSeconds(1);

        StartCoroutine(UpdateFluxUI());

        if (!Directory.Exists(@"C:\ProgramData\HTC\ViveSoftware\ViveRR\Log"))
            yield break;


        bool First = true;

        while (true)
        {
            yield return new WaitForFixedUpdate();
            DirectoryInfo Dir = new DirectoryInfo(@"C:\ProgramData\HTC\ViveSoftware\ViveRR\Log");

            var F = Dir.GetFiles();
            F = F.Where(l => l.Name.Contains("RRConsole")).ToArray();
            F = F.OrderBy(l => l.CreationTime).ToArray();

            FileInfo FinalFile = F[F.Length - 1];

            FileStream stream = File.Open(FinalFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader reader = new StreamReader(stream);

            float LastUpdate = Time.time;
            ReadUp = 0;

            while (LastUpdate + 8 > Time.time)
            {

                while (!reader.EndOfStream)
                {
                    LastUpdate = Time.time;
                   // Debug.LogWarning("Up");
                    ReadUp++;
                    var XX = reader.ReadToEnd();

                    var Flux = XX.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                    ReadLastLines = Flux.Length;

                    foreach (var item in Flux)
                    {
                        switch (item)
                        {
                            case string a when a.Contains("VIVE Business Streaming version") && First:
                                VBS_PC_Version = item.Split(new string[] { " :" }, StringSplitOptions.None)[1];
                                break;
                            case string a when a.Contains("VBS client app version") && First:
                                VBS_APK_Version = item.Split(new string[] { " :" }, StringSplitOptions.None)[1];
                                break;
                            case string a when a.Contains("Total Latency") && !First:
                                Flux_TotalLatency.Add(int.Parse(item.Split(new string[] { "Value:" }, StringSplitOptions.None)[1]));
                                break;
                            case string a when a.Contains("Network Latency") && !First:
                                Flux_NetworkLatency.Add(int.Parse(item.Split(new string[] { "Value:" }, StringSplitOptions.None)[1]));
                                break;
                            case string a when a.Contains("ServerState") && !First:
                                Flux_ServerState.Add(int.Parse(item.Split(new string[] { "Value:" }, StringSplitOptions.None)[1]));
                                break;
                            case string a when a.Contains("UL Bandwidth") && !First:
                                Flux_ULBandwidth.Add(int.Parse(item.Split(new string[] { "Value:" }, StringSplitOptions.None)[1].Split('K')[0]));
                                break;
                            case string a when a.Contains("DL Bandwidth") && !First:
                                Flux_DLBandwidth.Add(int.Parse(item.Split(new string[] { "Value:" }, StringSplitOptions.None)[1].Split('K')[0]));
                                break;
                            case string a when a.Contains("Pose FPS") && !First:
                                FPS_Pose.Add(int.Parse(item.Split(new string[] { "Value:" }, StringSplitOptions.None)[1]));
                                break;
                            case string a when a.Contains("Driver FPS") && !First:
                                FPS_Driver.Add(int.Parse(item.Split(new string[] { "Value:" }, StringSplitOptions.None)[1]));
                                break;
                            case string a when a.Contains("Encode FPS") && !First:
                                FPS_Encode.Add(int.Parse(item.Split(new string[] { "Value:" }, StringSplitOptions.None)[1]));
                                break;
                            case string a when a.Contains("Network FPS") && !First:
                                FPS_Network.Add(int.Parse(item.Split(new string[] { "Value:" }, StringSplitOptions.None)[1]));
                                break;
                            case string a when a.Contains("Decode FPS") && !First:
                                FPS_Decode.Add(int.Parse(item.Split(new string[] { "Value:" }, StringSplitOptions.None)[1]));
                                break;
                            case string a when a.Contains("Render FPS") && !First:
                                FPS_Render.Add(int.Parse(item.Split(new string[] { "Value:" }, StringSplitOptions.None)[1]));
                                break;
                            default:
                                break;
                        }
                    }


                    First = false;


                }

                yield return new WaitForFixedUpdate();


            }


        }



    }


    private void Awake()
    {
        StartCoroutine(CheckFPS());

    }


    void Warning_UI()
    {
        if (VaroniaGlobal.VG == null)
            return;

        #if VBO_Input
        if (Config.VaroniaConfig.Controller == Controller.FOCUS3_VBS_VaroniaGun || Config.VaroniaConfig.Controller == Controller.FOCUS3_VBS_Striker)
        {
            if (!Right_Wrist_Ready && !Left_Wrist_Ready && HelmetState == HelmetState.Ok)
            {
                VaroniaInput.Instance.OnWeaponFail.Invoke();

            }
            else
            {
                VaroniaInput.Instance.OnWeaponOk.Invoke();
            }
        }
        #endif


        if ((TrackingState == TrackingState.Strange || TrackingState == TrackingState.Lost) && HelmetState != HelmetState.NoStreamOrPowerOff)
        {

            VaroniaGlobal.VG.OnStrangeTracking.Invoke();
        }
        else
        {

            VaroniaGlobal.VG.OnTrackingOk.Invoke();
        }


        TotalLag_UI.text = "Total L : " + (TotalLag * 100) + " ms"
                          + "\nTotal L : " + LagCount + " Count(s)";
    }
    void CheckAllSteamVRDevices()
    {


        for (int i = 0; i < SteamVR.connected.Length; ++i)
        {
            try
            {

                var A = "";
                float B = 0f;
                var C = "";
                EDeviceActivityLevel E = new EDeviceActivityLevel();



                if (OpenVR.System != null) A = SteamVR.instance.GetStringProperty(Valve.VR.ETrackedDeviceProperty.Prop_ModelNumber_String, (uint)i);
                if (OpenVR.System != null) B = SteamVR.instance.GetFloatProperty(Valve.VR.ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float, (uint)i);
                if (OpenVR.System != null) C = SteamVR.instance.GetStringProperty(Valve.VR.ETrackedDeviceProperty.Prop_RenderModelName_String, (uint)i);
                if (OpenVR.System != null) E = OpenVR.System.GetTrackedDeviceActivityLevel((uint)i);

                VRControllerState_t state1 = new VRControllerState_t();
                TrackedDevicePose_t pose1 = new TrackedDevicePose_t();

                var D = false;

                if (OpenVR.System != null)
                    D = OpenVR.System.GetControllerStateWithPose(ETrackingUniverseOrigin.TrackingUniverseRawAndUncalibrated, (uint)i, ref state1, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VRControllerState_t)), ref pose1);



                if (C.Contains("hmd"))
                {
                    HMD_Battery = B;

                    HMD_Ready = D;

                    string Activity = "";

                    if (E == EDeviceActivityLevel.k_EDeviceActivityLevel_UserInteraction)
                    {
                        Activity = "HasActive";
                        HMD_HasActivity = true;

                    }
                    else
                    {
                        Activity = "NoActive";
                        HMD_HasActivity = false;
                    }
                    Info += "<color=white>----- Vive Buisness Streaming -----</color>\n";

                    if (B > 0.25f)
                        Info += "Casque Vive Focus 3 = Battery: <color=green>" + Math.Round(B * 100) + "%</color> Active: " + D + " " + Activity + "\n";
                    else
                        Info += "Casque Vive Focus 3 = Battery: <color=red>" + Math.Round(B * 100) + "%</color> Active: " + D + " " + Activity + "\n";
                }
                else if (!A.Contains("unknown"))
                {
                    if (C.Contains("right_tracker"))
                    {
                        Right_Wrist_Ready = D;
                        Wrist_r.index = i;
                        Info += "Wrist Tracker RIGHT = Active: " + D + " Tracking = " + Wrist_r.Trakingstate + "\n";



                    }
                    else
                    if (C.Contains("left_tracker"))
                    {
                        Left_Wrist_Ready = D;
                        Wrist_l.index = i;
                        Info += "Wrist Tracker Left = Active: " + D + " Tracking = " + Wrist_l.Trakingstate + "\n";
                    }
                    else
                    if (C.Contains("controller_right"))
                    {
                        Right_Hand_Ready = D;

                        Info += "Right Hand = Active: " + D + " " + "\n";
                    }
                    else
                    if (C.Contains("controller_left"))
                    {
                        Left_Hand_Ready = D;

                        Info += "Left Hand = Active: " + D + " " + "\n";
                    }
                    else
                    if (C.Contains("Invalid"))
                    {

                    }
                    else
                    {
                        Info += A + " - " + B + " - " + C + " - " + D + " " + "\n";
                    }

                }

#if VBO_Input

#if VBO_VORTEX
                if (Config.VaroniaConfig.Controller != Controller.VORTEX_WEAPON_FOCUS)
                {
#endif

                    if (Wrist_r.Trakingstate == TrackingState.Ok || Wrist_l.Trakingstate == TrackingState.Ok)
                    {
                        if (WristState == TrackingState.Lost)
                        VaroniaInput.Instance.OnWeaponHasTracking.Invoke();

                        WristState = TrackingState.Ok;
                    }
                    else
                    {
                        if (WristState == TrackingState.Ok)
                        VaroniaInput.Instance.OnWeaponLostTracking.Invoke();

                        WristState = TrackingState.Lost;
                    }

#if VBO_VORTEX
                } 
#endif

#endif
            }
            catch (System.Exception)
            {
            }


        }
    }


    IEnumerator CheckFPS()
    {
        while (true)
        {
            Fps.text = "";
            switch (FPS.S_Fps)
            {
                case int n when n >= 70:
                    Fps_.Add("▄");
                    break;
                case int n when n >= 45:
                    Fps_.Add("<color=yellow>▄</color>");
                    break;
                case int n when n < 45:
                    Fps_.Add("<color=red>▄</color>");
                    break;
            }

            if (Fps_.Count > 100)
                Fps_.Remove(Fps_[0]);

            foreach (var item in Fps_)
            {
                Fps.text += item;
            }

            yield return new WaitForSeconds(0.1f);
        }

    }

    void CheckLatency()
    {
        Latency.text = "";
        if (HelmetState == HelmetState.Ok)
        {
            switch (TrackingState)
            {
                case TrackingState.Ok:
                    Ping_.Add("█");
                    break;
                case TrackingState.Strange:
                    Ping_.Add("<color=yellow>█</color>");
                    break;
                case TrackingState.Lost:
                    Ping_.Add("<color=yellow>█</color>");
                    break;
                case TrackingState.BigLost:
                    Ping_.Add("<color=orange>█</color>");
                    break;
                case TrackingState.NO:
                    Ping_.Add("<color=purple>█</color>");
                    break;
                default:
                    break;
            }

            if (HelmetState == HelmetState.NoGameFocusOrMicroLag)
                Info += "<color=red>HeadSet = " + HelmetState + "</color>" + "\n";
            else
                Info += "<color=green>HeadSet = " + HelmetState + "</color>" + "\n";

            //if (Lagging)
            //{
            //    LagCount++;
            //    MQTTVaronia.MQTTVaronia_.SetSoftLag(BeginLag, DateTime.UtcNow);
            //}

            Lagging = false;

        }
        else if (HelmetState == HelmetState.NoGameFocusOrMicroLag)
        {
            if (!Lagging)
                BeginLag = DateTime.UtcNow;


            Ping_.Add("<color=purple>█</color>");



            Info += "<color=purple>HeadSet = " + HelmetState + "</color>" + "\n";
            Lagging = true;
            TotalLag++;



            Debug.Log(DateTime.Now.ToString("[HH:mm]") + " ~~Stream Latency Alert !~~");


        }
        else if (HelmetState == HelmetState.NoStreamOrPowerOff)
        {
            if (!Lagging)
                BeginLag = DateTime.UtcNow;

            Lagging = true;
            TotalLag++;
            Info += "<color=red>HeadSet = " + HelmetState + "</color>" + "\n";
            Ping_.Add("<color=red>█</color>");

            Debug.Log(DateTime.Now.ToString("[HH:mm]") + " ~~Stream Latency Alert !~~");
        }







        if (Ping_.Count > 100)
            Ping_.Remove(Ping_[0]);

        foreach (var item in Ping_)
        {
            Latency.text += item;
        }


    }


    Vector3 Pos;
    void CheckTracking()
    {
        if (VaroniaGlobal.VG.MainCamera == null)
            return;


        if ((VaroniaGlobal.VG.MainCamera.transform.localPosition.y > 2.6f) || (VaroniaGlobal.VG.MainCamera.transform.localPosition.y < 0.45f))
            StrangeTracking = true;
        else
            StrangeTracking = false;


        if (HelmetState == HelmetState.NoStreamOrPowerOff)
            TrackingState = TrackingState.NO;
        else
        if (Pos == VaroniaGlobal.VG.MainCamera.transform.position)
            TrackingState = TrackingState.BigLost;
        else if (StrangeTracking)
            TrackingState = TrackingState.Strange;
        else
            TrackingState = TrackingState.Ok;

        Pos = VaroniaGlobal.VG.MainCamera.transform.position;






        if (TrackingState == TrackingState.Ok)
            Info += "<color=green>Tracking = OK</color>" + "\n";
        else
            Info += "<color=red>Tracking = " + TrackingState + "</color>" + "\n";
    }

    void CheckHelmetState()
    {
        if (VaroniaGlobal.VG.MainCamera != null)
        {
            if (HMD_HasActivity && (HMD_Ready || TrackingState == TrackingState.BigLost))
                HelmetState = HelmetState.Ok;
            else if (HMD_HasActivity && !HMD_Ready && TrackingState != TrackingState.BigLost)
                HelmetState = HelmetState.NoGameFocusOrMicroLag;
            else if (!HMD_HasActivity)
                HelmetState = HelmetState.NoStreamOrPowerOff;
        }

    }

    IEnumerator Start()
    {
        DontDestroyOnLoad(this);

        Instance = this;

        // Wait Config Load
        while (Config.VaroniaConfig == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1);





        // If Vive Buisness Streaming Don't Destroy else Destroy GameObject
        try
        {
            if (!SteamVR.instance.GetStringProperty(Valve.VR.ETrackedDeviceProperty.Prop_ModelNumber_String, 0).Contains("Vive VBStreaming Focus3"))
                Destroy(gameObject);
        }
        catch (System.Exception)
        {
            Destroy(gameObject);
        }

        if (Config.VaroniaConfig.DeviceMode != DeviceMode.Server_Spectator && Config.VaroniaConfig.DeviceMode != DeviceMode.Client_Spectator)
            Canvas.SetActive(true);

        yield return new WaitForSeconds(0.25f);


        StartCoroutine(SearchJitter());


        while (SteamVR.instance != null)
        {
            Info = "";


            CheckAllSteamVRDevices();
            CheckTracking();
            CheckHelmetState();
            CheckLatency();
            Warning_UI();
            yield return new WaitForSeconds(0.1f);
        }

    }

    private void LateUpdate()
    {
        DebugVaronia.Instance.TextDebugInfo.text += Info;
    }



}
