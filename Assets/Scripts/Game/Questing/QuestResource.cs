﻿using System;

namespace DaggerfallWorkshop.Game.Questing
{
    /// <summary>
    /// All quest resources (People, Place, Foe, Clock, etc.) must inherit from this base class.
    /// </summary>
    public abstract class QuestResource
    {
        Quest parentQuest = null;

        public Quest ParentQuest
        {
            get { return parentQuest; }
        }

        public QuestResource(Quest parentQuest)
        {
            this.parentQuest = parentQuest;
        }
    }
}