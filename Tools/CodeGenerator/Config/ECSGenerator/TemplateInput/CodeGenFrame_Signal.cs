public unsafe partial class Frame  {
    public unsafe partial class FrameSignals  {
#_ME_FOR #COLLISION; use(#DECLARE_PARAMS_LIST,#CALL_PARAMS_LIST); tag(1)
        public void #CLS_NAME(#DECLARE_PARAMS_LIST) {
            var array = _f._ISignal#CLS_NAMESystems;
            var systems = &(_f._globals->Systems);
            for (Int32 i = 0; i < array.Length; ++i) {
                var s = array[i];
                if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
                  s.#CLS_NAME(_f#CALL_PARAMS_LIST);
                }
            }
        }
#_ME_ENDFOR    
    }
}