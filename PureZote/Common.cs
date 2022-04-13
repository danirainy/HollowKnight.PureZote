using Satchel;
using Modding;


namespace PureZote
{
    public class Common
    {
        private readonly Mod mod_;
        public Common(Mod mod) => mod_ = mod;
        private void Log(object message) => mod_.LogDebug(message);
        public void LogFSM(PlayMakerFSM fsm, System.Action function = null)
        {
            Log("Adding Logging to FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
            foreach (var state in fsm.FsmStates)
            {
                FsmUtil.InsertCustomAction(fsm, state.Name, () =>
                {
                    Log("FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + " entering " + "state: " + state.Name + ".");
                    if (function != null)
                        function();
                }, 0);
            }
            Log("Added Logging to FSM: " + fsm.gameObject.name + " - " + fsm.FsmName + ".");
        }
        public void LogFSMState(PlayMakerFSM fsm, string state, System.Action function = null)
        {
            Log("Adding Logging to State: " + fsm.FsmName + " - " + state + ".");
            for (int i = fsm.GetState(state).Actions.Length; i >= 0; i--)
            {
                FsmUtil.InsertCustomAction(fsm, state, () =>
                {
                    Log("FSM: " + fsm.FsmName + " - " + state + " entering " + "action: " + i.ToString() + ".");
                    if (function != null)
                        function();
                }, i);
            }
            Log("Added Logging to State: " + fsm.FsmName + " - " + state + ".");
        }
    }
}
