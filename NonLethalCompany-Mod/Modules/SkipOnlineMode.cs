using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NonLethalCompany_Mod.Modules;

public class SkipOnlineMode : Module
{
    public override void Load()
    {
        if (ModConfig.SkipOnlineMode != null && ModConfig.SkipOnlineMode.Value)
        {
            NonLethalCompany.Instance.StartCoroutine(SkipOptions());
        }
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
}
