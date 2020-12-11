using UnityEngine;
using UnityEngine.UI;

public class DropdownHelper : MonoBehaviour
{
    public Dropdown dropdown;

    public void OnEnable()
    {
        dropdown.Hide();
        dropdown.interactable = true;
    }
}