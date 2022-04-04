using Core.FsmUtil;
using Modding;


namespace PureZote
{
    public class Common
    {
        public static void LogFSM(Mod mod, PlayMakerFSM fsm)
        {
            mod.Log("Adding Logging to FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            foreach (var state in fsm.FsmStates)
            {
                FsmUtil.InsertMethod(fsm, state.Name, () => { mod.Log(state.Name); }, 0);
            }
            mod.Log("Added Logging to FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
        }
    }
}
