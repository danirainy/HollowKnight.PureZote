using Satchel;
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
                FsmUtil.InsertCustomAction(fsm, state.Name, () =>
                {
                    mod.Log("FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + " entering " + "state: " + state.Name + ".");
                }, 0);
            }
            mod.Log("Added Logging to FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
        }
    }
}
