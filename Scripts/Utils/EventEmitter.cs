using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public delegate void CallbackMethod();
public class MonoEventEmitter : MonoBehaviour {
	public List<CallbackMethod> onceCallbacks = new List<CallbackMethod>();
	public List<CallbackMethod> callbacks = new List<CallbackMethod>();

	public void ExecuteCallbacks() {
		callbacks.ForEach((CallbackMethod evt)=>{
            evt.Invoke();
        });

        // 'Once' callbacks only fire... once.
        onceCallbacks.ForEach((CallbackMethod evt)=>{
            evt.Invoke();
        });
        onceCallbacks.Clear();
	}

	public void AddCallback(CallbackMethod cb) {
        callbacks.Add(cb);
    }

    public void AddCallbackOnce(CallbackMethod cb) {
        onceCallbacks.Add(cb);
    }

    public void RemoveCallback(CallbackMethod cb) {
        callbacks.Remove(cb);
        onceCallbacks.Remove(cb);
    }
}
