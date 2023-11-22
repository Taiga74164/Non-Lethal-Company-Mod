using BepInEx;
using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NonLethalCompany_Mod;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Main : BaseUnityPlugin
{
    #region GUI Properties

    private bool _showMenu;
    private Vector2 _scrollPosition = Vector2.zero;

    private float _movementSpeed = 4.6f;

    private float _noClipSpeed = 5.0f;
    private bool _setNoClip;
    // private bool _hasDisabledCollider = false;

    public static bool SetNoFallDamage;

    private bool _setUnlimitedSprint;

    private bool _setNoWeight;

    public static bool SetGodMode;

    private float _grabDistance = 5.0f;

    private string _credits = "0";

    #endregion

    private void Awake()
    {
        Logger.LogMessage("\u001b[31mMOD LOADED WOW!!!!!!!!!!!!!!\u001b[0m");
        Harmony.CreateAndPatchAll(typeof(PlayerControllerBPatch));
    }

    private void Update()
    {
        if (_showMenu)
            Focused = false;

        UpdateInput();

        HandleNoClip();
        HandleUnlimitedSprint();
        HandleNoWeight();
        HandleGodMode();
        HandleGrabDistance();
    }

    private void OnGUI()
    {
        if (!_showMenu)
            return;

        GUI.Box(new Rect(0, 0, 400, 700), "Non-Lethal Company Mod Menu");

        GUILayout.BeginArea(new Rect(10, 40, 380, 700));

        #region Movement Speed

        GUILayout.Label("Movement Speed: " + _movementSpeed);
        _movementSpeed = GUILayout.HorizontalSlider(_movementSpeed, 4.6f, 100.0f);
        if (GUILayout.Button("Set Movement Speed"))
            HandleMovementSpeed();

        #endregion

        #region No Clip

        _setNoClip = GUILayout.Toggle(_setNoClip, "No Clip");
        if (_setNoClip)
        {
            GUILayout.Label("No Clip Speed: " + _noClipSpeed);
            _noClipSpeed = GUILayout.HorizontalSlider(_noClipSpeed, 1, 50);
        }

        #endregion

        #region No Fall Damage

        SetNoFallDamage = GUILayout.Toggle(SetNoFallDamage, "No Fall Damage");

        #endregion

        #region Unlimited Sprint

        _setUnlimitedSprint = GUILayout.Toggle(_setUnlimitedSprint, "Unlimited Sprint");

        #endregion

        #region No Weight

        _setNoWeight = GUILayout.Toggle(_setNoWeight, "No Weight");

        #endregion

        #region God Mode

        SetGodMode = GUILayout.Toggle(SetGodMode, "God Mode");

        #endregion

        #region Grab Distance

        GUILayout.Label("Grab Distance: " + _grabDistance);
        _grabDistance = GUILayout.HorizontalSlider(_grabDistance, 5, 100);

        #endregion

        #region Credits

        GUILayout.Label("Current Credits: " + Credits);
        _credits = GUILayout.TextField(_credits);
        if (GUILayout.Button("Set Credits"))
            HandleCredits();

        #endregion

        GUILayout.Space(10.0f);
        GUILayout.Label("Scrap Lists: (T: Teleport), (+/-: Change Value)");

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(300));
        GUILayout.BeginVertical();
        DrawTable();
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.Space(10.0f);

        GUILayout.EndArea();
    }

    private void DrawTable()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Name", GUILayout.MinWidth(60));
        GUILayout.Label("Distance(m)", GUILayout.MinWidth(20));
        GUILayout.Label("Value", GUILayout.MinWidth(30));
        GUILayout.EndHorizontal();

        // GUILayout.BeginHorizontal("Options");
        // _showInShip = GUILayout.Toggle(_showInShip, "Show In Ship");
        // _showHeld = GUILayout.Toggle(_showHeld, "Show Held");
        // GUILayout.EndHorizontal();

        var propList = GameObject.FindGameObjectsWithTag("PhysicsProp");
        if (propList == null || propList.Length == 0)
            return;

        foreach (var prop in propList)
        {
            if (prop == null)
                continue;

            var player = GameNetworkManager.Instance.localPlayerController;
            if (player == null)
                continue;

            var physicsPropComp = prop.GetComponent<PhysicsProp>();
            if (physicsPropComp == null)
                continue;

            var scanNodeComp = prop.GetComponentInChildren<ScanNodeProperties>();
            if (scanNodeComp == null)
                continue;

            var grabbableObj = physicsPropComp as GrabbableObject;
            if (grabbableObj == null)
                continue;

            var actualName = scanNodeComp.headerText;
            var distance = Vector3.Distance(prop.transform.position, player.transform.position);
            var scrapValue = grabbableObj.scrapValue;
            if (scrapValue == 1 || !grabbableObj.grabbable || grabbableObj.isHeld)
                continue;

            GUILayout.BeginHorizontal();
            GUILayout.Label(actualName, GUILayout.MinWidth(120));
            GUILayout.Label(distance.ToString("F2"), GUILayout.MinWidth(50));
            GUILayout.Label(scrapValue.ToString(), GUILayout.MinWidth(30));
            if (GUILayout.Button("T"))
                TeleportPlayer(prop.transform.position);
            if (GUILayout.Button("+"))
                grabbableObj.SetScrapValue(scrapValue + 1);
            if (GUILayout.Button("-"))
                grabbableObj.SetScrapValue(scrapValue - 1);

            GUILayout.EndHorizontal();
        }

        // Seems like collider must be disabled and player must go through the door for items to be dropped.
        if (GUILayout.Button("Teleport to Ship"))
            TeleportPlayer(StartOfRound.Instance.shipDoorNode.transform.position);// StartCoroutine(TeleportToShipCoroutine());
    }

    private void HandleMovementSpeed()
    {
        if (!IsInGameScene())
            return;

        var player = GameNetworkManager.Instance.localPlayerController;
        if (player == null)
            return;

        player.movementSpeed = _movementSpeed;
    }

    private void HandleNoClip()
    {
        if (!IsInGameScene())
            return;

        var player = GameNetworkManager.Instance.localPlayerController;
        if (player == null)
            return;

        var camera = player.gameplayCamera.transform;
        if (camera == null)
            return;

        var collider = player.GetComponent<CharacterController>() as Collider;
        if (collider == null)
            return;

        if (_setNoClip)
        {
            // switch (_hasDisabledCollider)
            // {
            //     case false:
            //         collider.enabled = false;
            //         _hasDisabledCollider = true;
            //         break;
            //     case true when !collider.enabled:
            //         collider.enabled = false;
            //         break;
            // }
            collider.enabled = false;
            var dir = new Vector3();

            // Horizontal
            if (UnityInput.Current.GetKey(KeyCode.W))
                dir += camera.forward;
            if (UnityInput.Current.GetKey(KeyCode.S))
                dir += camera.forward * -1;
            if (UnityInput.Current.GetKey(KeyCode.D))
                dir += camera.right;
            if (UnityInput.Current.GetKey(KeyCode.A))
                dir += camera.right * -1;

            // Vertical
            if (UnityInput.Current.GetKey(KeyCode.Space))
                dir.y += camera.up.y;
            if (UnityInput.Current.GetKey(KeyCode.LeftControl))
                dir.y += camera.up.y * -1;

            var prevPos = player.transform.localPosition;
            if (prevPos.Equals(Vector3.zero))
                return;

            var newPos = prevPos + dir * (_noClipSpeed * Time.deltaTime);
            player.transform.localPosition = newPos;
        }
        else
        {
            // if (!_hasDisabledCollider)
            //     return;

            collider.enabled = true;
            // _hasDisabledCollider = false;

        }
    }

    private void HandleUnlimitedSprint()
    {
        if (!IsInGameScene())
            return;

        if (!_setUnlimitedSprint)
            return;

        var player = GameNetworkManager.Instance.localPlayerController;
        if (player == null)
            return;

        player.sprintMeter = 100.0f;
        player.isExhausted = false;
    }

    private void HandleNoWeight()
    {
        if (!IsInGameScene())
            return;

        if (!_setNoWeight)
            return;

        var player = GameNetworkManager.Instance.localPlayerController;
        if (player == null)
            return;

        player.carryWeight = 1.0f;
    }

    private void HandleGodMode()
    {
        if (!IsInGameScene())
            return;

        if (!SetGodMode)
            return;

        var player = GameNetworkManager.Instance.localPlayerController;
        if (player == null)
            return;

        player.isPlayerDead = false;
    }

    private void HandleGrabDistance()
    {
        if (!IsInGameScene())
            return;

        var player = GameNetworkManager.Instance.localPlayerController;
        if (player == null)
            return;

        player.grabDistance = _grabDistance;
    }

    private void HandleCredits()
    {
        if (!IsInGameScene())
            return;

        if (!int.TryParse(_credits, out var credits))
            return;

        Credits = credits;
    }

    private void UpdateInput()
    {
        if (UnityInput.Current.GetKeyUp(KeyCode.Insert))
            _showMenu = !_showMenu;
    }

    private Terminal? GetTerminal()
    {
        if (!IsInGameScene())
            return null;

        var obj = GameObject.FindObjectOfType<Terminal>();
        return obj == null ? null : obj.GetComponent<Terminal>();
    }

    private int Credits
    {
        get
        {
            var terminal = GetTerminal();
            if (terminal == null)
                return 0;

            return terminal.groupCredits;
        }
        set
        {
            var terminal = GetTerminal();
            if (terminal == null)
                return;

            terminal.groupCredits = value;
        }
    }

    private static bool IsInGameScene() => SceneManager.GetActiveScene().name == "SampleSceneRelay";

    private static void TeleportPlayer(Vector3 position)
    {
        var player = GameNetworkManager.Instance.localPlayerController;
        if (player == null)
            return;

        player.transform.position = position;
    }

    private static bool Focused
    {
        // get => Cursor.lockState == CursorLockMode.Locked;
        set
        {
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = value == false;
        }
    }
}

[HarmonyPatch(typeof(PlayerControllerB))]
public class PlayerControllerBPatch
{
    [HarmonyPatch(nameof(PlayerControllerB.DamagePlayer))]
    [HarmonyPrefix]
    private static void DamagePlayerPrefix(ref int damageNumber, ref CauseOfDeath causeOfDeath, ref bool fallDamage)
    {
        if (!Main.SetNoFallDamage)
            return;

        if (!fallDamage && causeOfDeath != CauseOfDeath.Gravity)
            return;

        damageNumber = 0;
    }

    [HarmonyPatch(nameof(PlayerControllerB.KillPlayer))]
    [HarmonyPrefix]
    private static void KillPlayerPrefix(ref CauseOfDeath causeOfDeath)
    {
        if (Main.SetGodMode && causeOfDeath is CauseOfDeath.Suffocation or CauseOfDeath.Drowning)
            return;
    }

    [HarmonyPatch("AllowPlayerDeath")]
    [HarmonyPrefix]
    private static bool AllowPlayerDeathPrefix(PlayerControllerB __instance)
    {
        if (Main.SetGodMode)
            return false;

        // var originalMethod = AccessTools.Method(typeof(PlayerControllerB), "AllowPlayerDeath");
        // if (originalMethod != null)
        //     return (bool)originalMethod.Invoke(__instance, null)!;

        return true;
    }
}
