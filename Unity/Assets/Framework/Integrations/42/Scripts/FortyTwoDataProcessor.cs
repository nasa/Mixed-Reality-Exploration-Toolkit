using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using GSFC.ARVR.UTILITIES;
using GSFC.ARVR.MRET.Common.Schemas;

public class FortyTwoDataProcessor : MonoBehaviour
{
    private class FortyTwoRawObjectInfo
    {
        public string mnemonicPrefix = "DEFAULT";
        public string ORBPOS_N_PREFIX;
        public Vector3d orbpos_n;   // <PRE>.ORBPOS_N.<X/Y/Z>.
        public Vector3d orbvel_n;   // <PRE>.ORBVEL_N.<X/Y/Z>.
        public Vector3d pos_r;      // <PRE>.POS_R.<X/Y/Z>.
        public Vector3d vel_r;      // <PRE>.VEL_R.<X/Y/Z>.
        public Vector3d angvel;     // <PRE>.ANGVEL.<X/Y/Z>.
        public Quaterniond qbn;     // <PRE>.QBN.<X/Y/Z/W>.
        public Vector3d sunvec;     // <PRE>.SUNVEC.<X/Y/Z>.
        public Vector3d magvec;     // <PRE>.MAGVEC.<X/Y/Z>.
        public Vector3d angmom;     // <PRE>.ANGMOM.<X/Y/Z>.

        public FortyTwoRawObjectInfo(string prefix)
        {
            mnemonicPrefix = prefix;
        }
    }

    public List<string> objectsToTrack;
    public double scaleFactor = 0.0001;
    public double sunDistance = 149.6;
    public int updateDivisor = 16;

    private List<FortyTwoRawObjectInfo> tracked42Objects = new List<FortyTwoRawObjectInfo>();
    private List<FortyTwoRawObjectInfo> lastTracked42Objects = new List<FortyTwoRawObjectInfo>();
    private DataManager dataManager;
    private bool reading = false;
    private int ticksSinceLastUpdate = 0;

    void Start()
    {
        GameObject loadedProjectObject = GameObject.Find("LoadedProject");
        if (loadedProjectObject)
        {
            UnityProject loadedProject = loadedProjectObject.GetComponent<UnityProject>();
            if (loadedProject)
            {
                dataManager = loadedProject.dataManager;
            }
        }

        foreach (string obj in objectsToTrack)
        {
            tracked42Objects.Add(new FortyTwoRawObjectInfo(obj));
        }
    }
	
	void Update()
    {
        if (ticksSinceLastUpdate++ >= updateDivisor)
        {
            StartCoroutine(Store42Data());
            Debug.Log("hello");
            Process42DataWithInterpolation();
            //Process42Data();
            ticksSinceLastUpdate = 0;
        }
        else
        {
            Process42DataWithInterpolation();
        }
	}

    private IEnumerator Store42Data()
    {
        if (!reading)
        {
            reading = true;
            lastTracked42Objects = new List<FortyTwoRawObjectInfo>();
            foreach (string obj in objectsToTrack)
            {
                lastTracked42Objects.Add(new FortyTwoRawObjectInfo(obj));
            }

            int listIndex = 0;
            foreach (FortyTwoRawObjectInfo objectInfo in tracked42Objects)
            {
                if (objectInfo.orbpos_n != null)
                {
                    lastTracked42Objects[listIndex].orbpos_n =
                        new Vector3d(objectInfo.orbpos_n.x, objectInfo.orbpos_n.y, objectInfo.orbpos_n.z);
                }
                objectInfo.orbpos_n = FindPointsAsVector3d(objectInfo.mnemonicPrefix + ".ORBPOS_N");
                yield return null;

                if (objectInfo.orbvel_n != null)
                {
                    lastTracked42Objects[listIndex].orbvel_n =
                        new Vector3d(objectInfo.orbvel_n.x, objectInfo.orbvel_n.y, objectInfo.orbvel_n.z);
                }
                objectInfo.orbvel_n = FindPointsAsVector3d(objectInfo.mnemonicPrefix + ".ORBVEL_N");
                yield return null;

                if (objectInfo.pos_r != null)
                {
                    lastTracked42Objects[listIndex].pos_r =
                        new Vector3d(objectInfo.pos_r.x, objectInfo.pos_r.y, objectInfo.pos_r.z);
                }
                objectInfo.pos_r = FindPointsAsVector3d(objectInfo.mnemonicPrefix + ".POS_R");
                yield return null;

                if (objectInfo.vel_r != null)
                {
                    lastTracked42Objects[listIndex].vel_r =
                        new Vector3d(objectInfo.vel_r.x, objectInfo.vel_r.y, objectInfo.vel_r.z);
                }
                objectInfo.vel_r = FindPointsAsVector3d(objectInfo.mnemonicPrefix + ".VEL_R");
                yield return null;

                //objectInfo.angvel = FindPointsAsVector3d(objectInfo.mnemonicPrefix + ".ANGVEL");
                if (objectInfo.qbn != null)
                {
                    lastTracked42Objects[listIndex].qbn =
                        new Quaterniond(objectInfo.qbn.x, objectInfo.qbn.y, objectInfo.qbn.z, objectInfo.qbn.w);
                }
                objectInfo.qbn = FindPointsAsQuaterniond(objectInfo.mnemonicPrefix + ".QBN");
                yield return null;
                if (objectInfo.sunvec != null)
                {
                    lastTracked42Objects[listIndex].sunvec =
                        new Vector3d(objectInfo.sunvec.x, objectInfo.sunvec.y, objectInfo.sunvec.z);
                }
                objectInfo.sunvec = FindPointsAsVector3d(objectInfo.mnemonicPrefix + ".SVB");
                yield return null;
                //objectInfo.magvec = FindPointsAsVector3d(objectInfo.mnemonicPrefix + ".MAGVEC");
                //objectInfo.angmom = FindPointsAsVector3d(objectInfo.mnemonicPrefix + ".ANGMOM");

                listIndex++;
            }
            reading = false;
        }

        yield return null;
    }

    private void Process42Data()
    {
        foreach (FortyTwoRawObjectInfo objectInfo in tracked42Objects)
        {
            // Orient the spacecraft.
            SavePointsFromVector3dAsFloats(objectInfo.mnemonicPrefix + ".REND.POS", objectInfo.pos_r * scaleFactor);
            SavePointsFromQuaterniondAsFloats(objectInfo.mnemonicPrefix + ".REND.ROT", objectInfo.qbn);

            // Orient the world.
            SavePointsFromVector3dAsFloats(objectInfo.mnemonicPrefix + ".REND.WORLDPOS", YUpLeftToZUpRight(objectInfo.orbpos_n) * -1 * scaleFactor);

            // Orient the sun.
            //SavePointsFromVector3dAsFloats(objectInfo.mnemonicPrefix + ".REND.SUNPOS",
            //    (YUpLeftToZUpRight(objectInfo.sunvec) * sunDistance));

            // Set up visual indicator data.

        }
    }

    private void Process42DataWithInterpolation()
    {
        int listIndex = 0;
        double updatePercentage = (double) (ticksSinceLastUpdate + 1) / updateDivisor;
        foreach (FortyTwoRawObjectInfo objectInfo in tracked42Objects)
        {
            // Orient the spacecraft.
            if (lastTracked42Objects[listIndex].pos_r != null)
            {
                SavePointsFromVector3dAsFloats(objectInfo.mnemonicPrefix + ".REND.POS",
                (((objectInfo.pos_r - lastTracked42Objects[listIndex].pos_r) * updatePercentage)
                + lastTracked42Objects[listIndex].pos_r) * scaleFactor);
            }
        
            if (lastTracked42Objects[listIndex].qbn != null)
            {
                SavePointsFromQuaterniondAsFloats(objectInfo.mnemonicPrefix + ".REND.ROT",
                ((objectInfo.qbn - lastTracked42Objects[listIndex].qbn) * updatePercentage)
                + lastTracked42Objects[listIndex].qbn);
            }

            // Orient the world.
            if (lastTracked42Objects[listIndex].orbpos_n != null)
            {
                SavePointsFromVector3dAsFloats(objectInfo.mnemonicPrefix + ".REND.WORLDPOS",
                ((YUpLeftToZUpRight(objectInfo.orbpos_n) -
                YUpLeftToZUpRight(lastTracked42Objects[listIndex].orbpos_n)) * updatePercentage
                + YUpLeftToZUpRight(lastTracked42Objects[listIndex].orbpos_n)) * -1 * scaleFactor);
            }

            // Orient the sun.
            if (lastTracked42Objects[listIndex].sunvec != null)
            {
                SavePointsFromVector3dAsFloats(objectInfo.mnemonicPrefix + ".REND.SUNPOS",
                ((YUpLeftToZUpRight(objectInfo.sunvec) -
                YUpLeftToZUpRight(lastTracked42Objects[listIndex].sunvec)) * updatePercentage
                + YUpLeftToZUpRight(lastTracked42Objects[listIndex].sunvec)) * sunDistance);
            }

            // Set up visual indicator data.

            listIndex++;
        }
    }

#region HELPERS
    private void SavePointsFromVector3d(string prefix, Vector3d points)
    {
        dataManager.SaveValue(prefix + ".X", points.x);
        dataManager.SaveValue(prefix + ".Y", points.y);
        dataManager.SaveValue(prefix + ".Z", points.z);
    }

    private void SavePointsFromVector3dAsFloats(string prefix, Vector3d points)
    {
        dataManager.SaveValue(prefix + ".X", (float) points.x);
        dataManager.SaveValue(prefix + ".Y", (float) points.y);
        dataManager.SaveValue(prefix + ".Z", (float) points.z);
    }

    private void SavePointsFromQuaterniond(string prefix, Quaterniond points)
    {
        dataManager.SaveValue(prefix + ".X", points.x);
        dataManager.SaveValue(prefix + ".Y", points.y);
        dataManager.SaveValue(prefix + ".Z", points.z);
        dataManager.SaveValue(prefix + ".W", points.w);
    }

    private void SavePointsFromQuaterniondAsFloats(string prefix, Quaterniond points)
    {
        dataManager.SaveValue(prefix + ".X", (float) points.x);
        dataManager.SaveValue(prefix + ".Y", (float) points.y);
        dataManager.SaveValue(prefix + ".Z", (float) points.z);
        dataManager.SaveValue(prefix + ".W", (float) points.w);
    }

    private Vector3d FindPointsAsVector3d(string prefix)
    {
        return new Vector3d(FindPointAsDouble(prefix + ".X"),
            FindPointAsDouble(prefix + ".Y"), FindPointAsDouble(prefix + ".Z"));
    }

    private Vector3 FindPointsAsVector3(string prefix)
    {
        return new Vector3(FindPointAsFloat(prefix + ".X"),
            FindPointAsFloat(prefix + ".Y"), FindPointAsFloat(prefix + ".Z"));
    }

    private Quaterniond FindPointsAsQuaterniond(string prefix)
    {
        return new Quaterniond(FindPointAsDouble(prefix + ".X"),
            FindPointAsDouble(prefix + ".Y"), FindPointAsDouble(prefix + ".Z"),
            FindPointAsDouble(prefix + ".W"));
    }

    private Quaternion FindPointsAsQuaternion(string prefix)
    {
        return new Quaternion(FindPointAsFloat(prefix + ".X"),
            FindPointAsFloat(prefix + ".Y"), FindPointAsFloat(prefix + ".Z"),
            FindPointAsFloat(prefix + ".W"));
    }

    private float FindPointAsFloat(string nameToFind)
    {
        object rawVal = dataManager.FindPoint(nameToFind);
        return (float) ((rawVal == null) ? 0f : rawVal);
    }

    private double FindPointAsDouble(string nameToFind)
    {
        object rawVal = dataManager.FindPoint(nameToFind);
        return (double) ((rawVal == null) ? 0f : rawVal);
    }

    private Vector3d YUpLeftToZUpRight(Vector3d leftCoordinate)
    {
        return new Vector3d(leftCoordinate.x, leftCoordinate.z, -leftCoordinate.y);
    }
#endregion
}