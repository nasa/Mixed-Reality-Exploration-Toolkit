// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

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