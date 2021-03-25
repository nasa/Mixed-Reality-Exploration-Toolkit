// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

namespace GSFC.ARVR.MRET.Selection
{
    interface ISelectable
    {
        void Select(bool hierarchical = true);

        void Deselect(bool hierarchical = true);
    }
}