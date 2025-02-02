﻿using Recoil.net.DevTools.Data;
using RecoilNet;
using RecoilNet.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Recoil.net.DevTools.State
{
	internal class ProcessState
	{
		public static readonly Atom<int> SelectedProcessIndex;
		public static readonly Atom<IReadOnlyList<ProcessTarget>> Processes;
		public static readonly Selector<ProcessTarget> SelectedProcess;
		public static readonly Selector<ProcessInsepector> ProcessInspector;

		static ProcessState()
		{
			SelectedProcessIndex = Atom.Create(() => SelectedProcessIndex, -1);
			Processes = Atom.Create(() => Processes);
			SelectedProcess = Selector.Create(() => SelectedProcess, GetSelectedProcess);
			ProcessInspector = Selector.Create(() => ProcessInspector, GetProcessInspector);
		}

		private static async Task<ProcessTarget?> GetSelectedProcess(IValueProvider builder)
		{
			int index = await builder.GetAsync(SelectedProcessIndex);
			IReadOnlyList<ProcessTarget>? targets = await builder.GetAsync(Processes);
			if (index < 0 || targets == null || index >= targets.Count)
			{
				return null;
			}

			return targets[index];
		}

		private static async Task<ProcessInsepector?> GetProcessInspector(IValueProvider provider)
		{
			ProcessTarget? target = await provider.GetAsync(SelectedProcess);
			if (target == null)
			{
				return null;
			}

			ProcessInsepector inspector = new ProcessInsepector(target.Id);
			return inspector;
		}
	}
}
