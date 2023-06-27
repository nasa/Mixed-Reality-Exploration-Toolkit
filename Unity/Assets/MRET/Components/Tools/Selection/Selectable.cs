// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

namespace GOV.NASA.GSFC.XR.MRET.Tools.Selection
{
    public interface ISelectable
    {
        void Select(bool hierarchical = true);

        void Deselect(bool hierarchical = true);
    }
}