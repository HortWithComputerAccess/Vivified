using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public static class TaskExtensions
{
	public static IEnumerator WaitTask(IAsyncResult task)
	{
		while (!task.IsCompleted)
		{
			yield return null;
		}
	}

	public static IEnumerator AsCoroutine(this Task task)
	{
		yield return WaitTask(task);
		AggregateException exception = task.Exception;
		if (exception == null)
		{
			yield break;
		}
		foreach (Exception innerException in exception.InnerExceptions)
		{
			LogInnerExceptions(innerException);
		}
		LogInnerExceptions(exception);
		throw exception;
	}

	private static void LogInnerExceptions(Exception e)
	{
		while (true)
		{
			Debug.LogException(e);
			if (e.InnerException != e && e.InnerException != null)
			{
				e = e.InnerException;
				continue;
			}
			break;
		}
	}
}
