using System.Collections;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NonLethalCompany_Mod;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]

public class Main : BaseUnityPlugin
{
    #region GUI Properties

    private bool _showMenu;
    private Vector2 _menuScrollPosition = Vector2.zero;
    private Vector2 _scrapListScrollPosition = Vector2.zero;
    private Vector2 _enemyListScrollPosition = Vector2.zero;
    private Vector2 _playerListScrollPosition = Vector2.zero;

    private float _movementSpeed = 4.6f;

    private float _noClipSpeed = 5.0f;
    private bool _setNoClip;
    // private bool _hasDisabledCollider = false;

    public static bool SetNoFallDamage;

    public static bool NoInvisible;

    private bool _setUnlimitedSprint;

    private bool _setNoWeight;

    public static bool SetGodMode;

    private float _grabDistance = 5.0f;

    private string _credits = "0";
    
    private bool _setUnlimitedBatteries;

    private bool _showRebind;
    private Vector2 _inputPosition = Vector2.zero;

    private bool _setESP;
    private bool _setESPColor = true;
    private bool _drawLine = true;
    private bool _drawDistance;
    private bool _drawName = true;
    private bool _setESPEnemy = true;
    private bool _setESPScrap = true;

    #endregion

    private void Awake()
    {
        Logger.LogMessage("\u001b[31mMOD LOADED WOW!!!!!!!!!!!!!!\u001b[0m");
        Harmony.CreateAndPatchAll(typeof(PlayerControllerBPatch));
        Harmony.CreateAndPatchAll(typeof(EnemyAIPatch));
    }

    private void Start()
    {
        StartCoroutine(SkipOptions());
    }

    private void Update()
    {
        UpdateInput();

        HandleNoClip();
        HandleUnlimitedSprint();
        HandleNoWeight();
        HandleGodMode();
        HandleGrabDistance();
        HandleUnlimitedBatteries();
    }

    private void OnGUI()
    {
        if (_setESP) HandleESP();

        if (!_showMenu)
            return;

        GUI.Box(new Rect(5, 5, 400, 700), "Non-Lethal Company Mod Menu");

        GUILayout.BeginArea(new Rect(10, 40, 380, 700));
        _menuScrollPosition = GUILayout.BeginScrollView(_menuScrollPosition);
        
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

        #region Unlimited Batteries
        
        _setUnlimitedBatteries = GUILayout.Toggle(_setUnlimitedBatteries, "Unlimited Batteries");

        #endregion

        #region No Invisible Enemy

        NoInvisible = GUILayout.Toggle(NoInvisible, "No Invisible Enemies");

        #endregion

        #region ESP

        _setESP = GUILayout.Toggle(_setESP, "ESP");
        if (_setESP)
        {
            _setESPEnemy = GUILayout.Toggle(_setESPEnemy, "ESP Enemy");
            _setESPScrap = GUILayout.Toggle(_setESPScrap, "ESP Scrap");
            _setESPColor = GUILayout.Toggle(_setESPColor, "Different ESP Color");
            _drawLine = GUILayout.Toggle(_drawLine, "Draw Line");
            _drawName = GUILayout.Toggle(_drawName, "Draw Name");
            _drawDistance = GUILayout.Toggle(_drawDistance, "Draw Distance");
        }

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
        
        #region Key Rebinding

        var rebindText = _showRebind ? "Hide" : "Show";
        if (GUILayout.Button($"{rebindText} Key Rebinding"))
            _showRebind = !_showRebind;

        if (_showRebind)
        {
            _inputPosition = GUILayout.BeginScrollView(
                _inputPosition, GUILayout.Height(300));
            InputUtils.ShowBindings();
            GUILayout.EndScrollView();
        }

        #endregion

        #region Scrap List

        GUILayout.Space(10.0f);
        GUILayout.Label("Scrap List: (T: Teleport), (+/-: Change Value)");

        _scrapListScrollPosition = GUILayout.BeginScrollView(_scrapListScrollPosition, GUILayout.Height(100));
        GUILayout.BeginVertical();
        DrawScrapTable();
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.Space(10.0f);

        #endregion

        #region Enemy List

        GUILayout.Space(10.0f);
        GUILayout.Label("Enemy List: (T: Teleport), (K: Kill)");

        _enemyListScrollPosition = GUILayout.BeginScrollView(_enemyListScrollPosition, GUILayout.Height(100));
        GUILayout.BeginVertical();
        DrawEnemyTable();
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.Space(10.0f);

        #endregion
        
        #region Player List

        GUILayout.Space(10.0f);
        GUILayout.Label("Player List: (T: Teleport)");

        _playerListScrollPosition = GUILayout.BeginScrollView(_playerListScrollPosition, GUILayout.Height(100));
        GUILayout.BeginVertical();
        DrawPlayerTable();
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.Space(10.0f);

        #endregion

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    private bool IsOnScreen(Vector3 position)
    {
        if (position.x < 0f || position.y < 0f || position.z < 0f) return false;
        return true;
    }

    private void HandleESP()
    {
        var player = GameNetworkManager.Instance.localPlayerController;
        if (player == null) return;

        var camera = player.gameplayCamera;
        if (camera == null)
            return;

        var propList = GameObject.FindGameObjectsWithTag("PhysicsProp");
        if (propList == null || propList.Length == 0)
            return;

        var ScreenScale = Screen.width / 864f;

        foreach (var prop in propList)
        {
            if (!_setESPScrap) break;

            if (prop == null)
                continue;

            var physicsPropComp = prop.GetComponent<PhysicsProp>();
            if (physicsPropComp == null)
                continue;

            var grabbableObj = physicsPropComp as GrabbableObject;
            if (grabbableObj == null)
                continue;

            var actualName = grabbableObj.itemProperties.itemName;
            var distance = Vector3.Distance(player.transform.position, prop.transform.position);

            Vector3 pos = prop.transform.position;
            Vector3 screenPos = camera.WorldToScreenPoint(pos);
            if (!IsOnScreen(screenPos))
                continue;

            Vector2 vec2Pos = new Vector2(screenPos.x * ScreenScale, (float)Screen.height - (screenPos.y * ScreenScale));
            Color color = _setESPColor ? Color.blue : Color.white;
            
            string renderTxt = "";
            if (_drawName)
                renderTxt += actualName;

            if (_drawDistance)
                renderTxt += " | " + Math.Round(distance) + "m";

            if (renderTxt != "")
                Render.DrawString(new Vector2(vec2Pos.x, vec2Pos.y - 20f), renderTxt, true);
            if (_drawLine)
                Render.DrawLine(new Vector2((float)(Screen.width / 2), (float)(Screen.height - 1)), vec2Pos, color, 2f);
        }

        var enemyAIs = GameObject.FindObjectsOfType<EnemyAI>();
        if (enemyAIs == null)
            return;

        foreach (var enemy in enemyAIs)
        {
            if (!_setESPEnemy) break;

            if (enemy == null || enemy.isEnemyDead)
                continue;

            var actualName = enemy.enemyType.enemyName;
            var distance = Vector3.Distance(player.transform.position, enemy.transform.position);

            Vector3 pos = enemy.transform.position;
            Vector3 screenPos = camera.WorldToScreenPoint(pos);
            if (!IsOnScreen(screenPos))
                continue;

            Vector2 vec2Pos = new Vector2(screenPos.x * ScreenScale, (float)Screen.height - (screenPos.y * ScreenScale));
            Color color = _setESPColor ? Color.red : Color.white;
            string renderTxt = "";
            if (_drawName)
                renderTxt += actualName;

            if (_drawDistance)
                renderTxt += " | " + Math.Round(distance) + "m";

            if (renderTxt != "")
                Render.DrawString(new Vector2(vec2Pos.x, vec2Pos.y - 20f), renderTxt, true);
            if (_drawLine)
                Render.DrawLine(new Vector2((float)(Screen.width / 2), (float)(Screen.height-1)), vec2Pos, color, 2f);
        }
    }

    private void DrawScrapTable()
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
            if (scrapValue == 1 || !grabbableObj.grabbable || grabbableObj.isHeld || grabbableObj is { isInShipRoom: true, isInElevator: true })
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

    private void DrawEnemyTable()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Name", GUILayout.MinWidth(60));
        GUILayout.Label("Distance(m)", GUILayout.MinWidth(20));
        GUILayout.Label("Command", GUILayout.MinWidth(30));
        GUILayout.EndHorizontal();

        var enemyAIs = GameObject.FindObjectsOfType<EnemyAI>();
        if (enemyAIs == null)
            return;

        foreach (var enemy in enemyAIs)
        {
            if (enemy == null || enemy.isEnemyDead)
                continue;
            
            var player = GameNetworkManager.Instance.localPlayerController;
            if (player == null)
                continue;
            
            var actualName = enemy.enemyType.enemyName;
            var distance = Vector3.Distance(enemy.transform.position, player.transform.position);

            GUILayout.BeginHorizontal();
            GUILayout.Label(actualName, GUILayout.MinWidth(120));
            GUILayout.Label(distance.ToString("F2"), GUILayout.MinWidth(50));
            if (GUILayout.Button("T"))
                TeleportPlayer(enemy.transform.position);
            if (GUILayout.Button("K"))
                enemy.KillEnemy(true);

            GUILayout.EndHorizontal();
        }
    }

    private void DrawPlayerTable()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Name", GUILayout.MinWidth(60));
        GUILayout.Label("Distance(m)", GUILayout.MinWidth(20));
        GUILayout.Label("Command", GUILayout.MinWidth(30));
        GUILayout.EndHorizontal();

        var allPlayerObjs = StartOfRound.Instance.allPlayerObjects;
        if (allPlayerObjs == null)
            return;
        
        foreach (var allPlayerObj in allPlayerObjs)
        {
            var playerObj = allPlayerObj.GetComponent<PlayerControllerB>();
            if (playerObj == null || playerObj.isPlayerDead || playerObj.IsOwner)
                continue;

            var player = GameNetworkManager.Instance.localPlayerController;
            if (player == null)
                continue;

            var actualName = playerObj.playerUsername;
            var distance = Vector3.Distance(playerObj.transform.position, player.transform.position);

            if (actualName.Contains("Player #"))
                continue;
            
            GUILayout.BeginHorizontal();
            GUILayout.Label(actualName, GUILayout.MinWidth(120));
            GUILayout.Label(distance.ToString("F2"), GUILayout.MinWidth(50));
            if (GUILayout.Button("T"))
                TeleportPlayer(playerObj.transform.position);

            GUILayout.EndHorizontal();
        }
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
    
    private void HandleUnlimitedBatteries()
    {
        if (!IsInGameScene())
            return;

        if (!_setUnlimitedBatteries)
            return;

        var grabbableObjs = GameObject.FindObjectsOfType<GrabbableObject>();
        if (grabbableObjs == null || grabbableObjs.Length == 0)
            return;

        foreach (var grabbableObj in grabbableObjs)
        {
            if (grabbableObj == null)
                continue;
            
            var player = GameNetworkManager.Instance.localPlayerController;
            if (player == null)
                continue;

            if (!grabbableObj.itemProperties.requiresBattery || grabbableObj.playerHeldBy != player)
                continue;
            
            grabbableObj.insertedBattery.empty = false;
            grabbableObj.insertedBattery.charge = 1.0f;
        }
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
        {
            _showMenu = !_showMenu;
            Focused = !_showMenu;
        }
    }

    private Terminal? GetTerminal()
    {
        if (!IsInGameScene())
            return null;

        var obj = GameObject.FindObjectOfType<Terminal>();
        return obj == null ? null : obj.GetComponent<Terminal>();
    }

    /// <summary>
    /// Waits for the launch options screen to appear.
    /// Automatically selects 'Online' when it appears.
    /// </summary>
    private static IEnumerator SkipOptions()
    {
        // Wait for launch options to appear.
        while (!SceneUtils.InScene(Constants.LaunchOptionsScene))
        {
            yield return new WaitForSeconds(2);
        }

        // Fetch the 'Online' button.
        GameObject onlineButton;
        while ((onlineButton = GameObject.Find("/Canvas/GameObject/LANOrOnline/OnlineButton")) == null)
        {
            yield return new WaitForSeconds(2);
        }

        // Emulate clicking the 'Online' button.
        var button = onlineButton.GetComponent<Button>();
        button.onClick.Invoke();
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

        player.TeleportPlayer(position);
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


[HarmonyPatch(typeof(EnemyAI))]
public class EnemyAIPatch
{
    [HarmonyPatch(nameof(EnemyAI.EnableEnemyMesh))]
    [HarmonyPrefix]
    private static void EnableEnemyMeshPrefix(ref bool enable)
    {
        if (!Main.NoInvisible)
            return;

        enable = true;
    }
}

public static class Constants
{
    public const string LaunchOptionsScene = "InitSceneLaunchOptions";
}

public static class SceneUtils
{
    /// <summary>
    /// Method to check if the current scene is the specified scene.
    /// </summary>
    /// <param name="sceneName">The name of the scene.</param>
    /// <returns>True if the scene name matches the active scene.</returns>
    public static bool InScene(string sceneName)
    {
        return SceneManager.GetActiveScene().name == sceneName;
    }
}

public static class InputUtils
{
    private static readonly List<InputAction> _actions = new();

    /// <summary>
    /// Fetches the name of the binding for an input action.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <returns>The name of the binding.</returns>
    public static string GetBindingName(this InputAction action)
    {
        return action.GetBindingDisplayString(action.GetBindingIndex());
    }

    /// <summary>
    /// Resolves all re-bindable actions.
    /// </summary>
    public static void ResolveActions()
    {
        var playerActions = GameNetworkManager.Instance
            .localPlayerController.playerActions;

        foreach (var field in typeof(PlayerActions).GetFields(
                     BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (field.FieldType != typeof(InputAction))
                continue;

            if (field.GetValue(playerActions)
                is InputAction action) _actions.Add(action);
        }
    }

    /// <summary>
    /// Shows all re-bindable actions in a GUI.
    /// </summary>
    public static void ShowBindings()
    {
        // Check if actions are resolved.
        if (_actions.Count == 0)
            ResolveActions();

        foreach (var action in _actions.Where(action => GUILayout.Button(
                     $"Rebind {action.name} ({action.GetBindingName()})")))
        {
            RebindAction(action);
        }
    }

    /// <summary>
    /// Rebinds an input action.
    /// </summary>
    /// <param name="action">The action to rebind.</param>
    private static void RebindAction(InputAction action)
    {
        HUDManager.Instance.AddTextToChatOnServer("Press to rebind action.");

        action.Disable();
        var operation = action.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.1f)
            .Start();

        operation.OnComplete(rebind =>
        {
            action.Enable();
            rebind.Dispose();
        });

        operation.OnCancel(rebind =>
        {
            action.Enable();
            rebind.Dispose();
        });
    }
}
