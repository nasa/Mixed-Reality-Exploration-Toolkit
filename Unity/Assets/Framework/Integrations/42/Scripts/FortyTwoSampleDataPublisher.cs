// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.Framework;

public class FortyTwoSampleDataPublisher : MonoBehaviour
{
    public string mnemonicPrefix = "DEFAULT";
    public double orbpos_n_x, orbpos_n_y, orbpos_n_z;       // <PRE>.ORBPOS_N.<X/Y/Z>.
    public double orbvel_n_x, orbvel_n_y, orbvel_n_z;       // <PRE>.ORBVEL_N.<X/Y/Z>.
    public double pos_r_x, pos_r_y, pos_r_z;                // <PRE>.POS_R.<X/Y/Z>.
    public double vel_r_x, vel_r_y, vel_r_z;                // <PRE>.VEL_R.<X/Y/Z>.
    public double angvel_x, angvel_y, angvel_z;             // <PRE>.ANGVEL.<X/Y/Z>.
    public double qbn_x, qbn_y, qbn_z, qbn_w;               // <PRE>.QBN.<X/Y/Z/W>.
    public double sunvec_x, sunvec_y, sunvec_z;             // <PRE>.SUNVEC.<X/Y/Z>.
    public double magvec_x, magvec_y, magvec_z;             // <PRE>.MAGVEC.<X/Y/Z>.
    public double angmom_x, angmom_y, angmom_z;             // <PRE>.ANGMOM.<X/Y/Z>.
	
	void Update()
    {
        MRET.DataManager.SaveValue(mnemonicPrefix + ".ORBPOS_N.X", orbpos_n_x);
        MRET.DataManager.SaveValue(mnemonicPrefix + ".ORBPOS_N.Y", orbpos_n_y);
        MRET.DataManager.SaveValue(mnemonicPrefix + ".ORBPOS_N.Z", orbpos_n_z);

        MRET.DataManager.SaveValue(mnemonicPrefix + ".ORBVEL_N.X", orbvel_n_y);
        MRET.DataManager.SaveValue(mnemonicPrefix + ".ORBVEL_N.Y", orbvel_n_z);
        MRET.DataManager.SaveValue(mnemonicPrefix + ".ORBVEL_N.Z", orbvel_n_x);

        MRET.DataManager.SaveValue(mnemonicPrefix + ".POS_R.X", pos_r_x);
        MRET.DataManager.SaveValue(mnemonicPrefix + ".POS_R.Y", pos_r_y);
        MRET.DataManager.SaveValue(mnemonicPrefix + ".POS_R.Z", pos_r_z);

        MRET.DataManager.SaveValue(mnemonicPrefix + ".VEL_R.X", vel_r_x);
        MRET.DataManager.SaveValue(mnemonicPrefix + ".VEL_R.Y", vel_r_y);
        MRET.DataManager.SaveValue(mnemonicPrefix + ".VEL_R.Z", vel_r_z);

        MRET.DataManager.SaveValue(mnemonicPrefix + ".ANGVEL.X", angvel_x);
        MRET.DataManager.SaveValue(mnemonicPrefix + ".ANGVEL.Y", angvel_y);
        MRET.DataManager.SaveValue(mnemonicPrefix + ".ANGVEL.Z", angvel_z);

        MRET.DataManager.SaveValue(mnemonicPrefix + ".QBN.X", qbn_x);
        MRET.DataManager.SaveValue(mnemonicPrefix + ".QBN.Y", qbn_y);
        MRET.DataManager.SaveValue(mnemonicPrefix + ".QBN.Z", qbn_z);
        MRET.DataManager.SaveValue(mnemonicPrefix + ".QBN.W", qbn_w);

        MRET.DataManager.SaveValue(mnemonicPrefix + ".SUNVEC.X", sunvec_x);
        MRET.DataManager.SaveValue(mnemonicPrefix + ".SUNVEC.Y", sunvec_y);
        MRET.DataManager.SaveValue(mnemonicPrefix + ".SUNVEC.Z", sunvec_z);

        MRET.DataManager.SaveValue(mnemonicPrefix + ".MAGVEC.X", magvec_x);
        MRET.DataManager.SaveValue(mnemonicPrefix + ".MAGVEC.Y", magvec_y);
        MRET.DataManager.SaveValue(mnemonicPrefix + ".MAGVEC.Z", magvec_z);

        MRET.DataManager.SaveValue(mnemonicPrefix + ".ANGMOM.X", angmom_x);
        MRET.DataManager.SaveValue(mnemonicPrefix + ".ANGMOM.Y", angmom_y);
        MRET.DataManager.SaveValue(mnemonicPrefix + ".ANGMOM.Z", angmom_z);
    }
}