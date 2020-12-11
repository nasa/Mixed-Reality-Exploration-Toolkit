using UnityEngine;

public class LobbyOpenMenuManager : MonoBehaviour
{
    public void LoadLobby()
    {
        ModeNavigator modeNavigator = FindObjectOfType<ModeNavigator>();
        if (modeNavigator)
        {
            modeNavigator.LoadLobby();
            
        }

        WorldSpaceMenuManager worldSpaceMenuManager = GetComponentInParent<WorldSpaceMenuManager>();
        if (worldSpaceMenuManager)
        {
            worldSpaceMenuManager.DimMenu();
        }
    }
}