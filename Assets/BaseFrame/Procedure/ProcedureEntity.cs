using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace FireCubeBase
{
    public class ProcedureEntity : FsmState<ProcedureManagerBase>
    {

        protected ProcedureInfo _info;
        public ProcedureEntity(ProcedureInfo info)
        {
            _info = info;


        }

        public int GetState()
        {
            return _info.ProcedureState;
        }



    }

}
