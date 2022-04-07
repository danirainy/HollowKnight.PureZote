using Satchel;
using Modding;


namespace PureZote
{
    public class Common
    {
        private readonly Mod mod_;
        public Common(Mod mod) => mod_ = mod;
        private void Log(object message) => mod_.Log(message);
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
    }
}
