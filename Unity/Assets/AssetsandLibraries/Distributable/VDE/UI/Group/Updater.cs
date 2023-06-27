/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
namespace Assets.VDE.UI.Group
{
    internal class Updater : Assets.VDE.UI.Updater
    {
        internal Container owner;
        private void OnEnable()
        {
            if (owner is null)
            {
                gameObject.TryGetComponent<Container>(out owner);
            }
            if (!(owner is null))
            {
                owner.SetUpdaters(true, false);
            }
        }
        private void OnDisable()
        {
            if (!(owner is null))
            {
                owner.SetUpdaters(false, false);
            }
        }
    }
}