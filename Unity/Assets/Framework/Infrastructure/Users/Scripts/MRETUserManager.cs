using System.Collections.Generic;
using UnityEngine;

public class MRETUserManager : MonoBehaviour
{
    public List<MRETUser> mretUsers;

    public static MRETUserManager Get()
    {
        return FindObjectOfType<MRETUserManager>();
    }

	public void UpdateUserList()
    {
        mretUsers = new List<MRETUser>();
        foreach (MRETUser mretUser in FindObjectsOfType<MRETUser>())
        {
            mretUsers.Add(mretUser);
        }
    }
}