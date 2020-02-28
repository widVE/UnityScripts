using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	[DefaultExecutionOrder(-1000)]
	public class CommandLineReader : MonoBehaviour
	{
		[SerializeField]
		List<CommandLineArgument> _arguments;
		List<CommandLineArgument> Arguments => _arguments ?? (_arguments = new List<CommandLineArgument>());

		void ApplyArguments()
		{
			foreach(CommandLineArgument cla in Arguments)
			{
				if(cla.Applied) continue;
				cla.Apply();
			}
		}

		void Awake()
		{
			//apply arguments before any other scripts run
			ApplyArguments();
		}

		void Start()
		{
			//try to apply more arguments during start
			ApplyArguments();
		}

		void Update()
		{
			//keep applying in update as long as there are still arguments remaining
			ApplyArguments();

			//check if all arguments have been applied
			bool allApplied = true;
			foreach(CommandLineArgument cla in Arguments)
			{
				allApplied &= cla.Applied;
			}

			if(allApplied)
			{
				//done applying - go to sleep
				enabled = false;
			}
		}
	}
}