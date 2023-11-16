using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineRunner : MonoBehaviour 
{    
	public Coroutine run(IEnumerator process)
	{
		return StartCoroutine(process);
	}

	public void stop(Coroutine coroutine)
	{
		if (coroutine == null)
        {
            return;
        }


        StopCoroutine(coroutine);
	}

    public Coroutine delayAction(System.Action action, float seconds)
    {
        return run(delayedActionEnumerator(action, seconds));
    }

    IEnumerator delayedActionEnumerator(System.Action action, float seconds)
    {
        yield return new WaitForSeconds(seconds);

        action();
    }
}
