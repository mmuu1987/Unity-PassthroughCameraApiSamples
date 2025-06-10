using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FireCubeBase
{
    public class WorldNetPlayer : BaseNetPlayer
    {
        public override void OnStartClient()
        {
            this.name = this.name + $" {this.netIdentity.netId} £º{this.netIdentity}";
        }

        protected override void GetDir()
        {

            RootHeadTransform.position = QuestHeadTransform.position;
            RootHeadTransform.rotation = QuestHeadTransform.rotation;

            RootQuestLeftTransform.position = QuestLeftTransform.position;
            RootQuestLeftTransform.rotation = QuestLeftTransform.rotation;

            RootQuestRightTransform.position = QuestRightTransform.position;
            RootQuestRightTransform.rotation = QuestRightTransform.rotation;

        }
    }
}
