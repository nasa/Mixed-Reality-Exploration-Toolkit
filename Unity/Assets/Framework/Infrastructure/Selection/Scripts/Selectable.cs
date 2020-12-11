namespace GSFC.ARVR.MRET.Selection
{
    interface ISelectable
    {
        void Select(bool hierarchical = true);

        void Deselect(bool hierarchical = true);
    }
}