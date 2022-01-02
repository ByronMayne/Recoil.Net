﻿using RecoilNet.State;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace RecoilNet
{
	/// <summary>
	/// RecoilState is used to wrap an <see cref="Atom{T}"/> and
	/// the <see cref="IRecoilStore"/> instance that holds it's value.
	/// </summary>
	/// <typeparam name="T">The type of the value being held</typeparam>
	public sealed class RecoilState<T> : RecoilState
	{
		private T? m_value;
		private RecoilValue<T> m_recoilValue;

		/// <summary>
		/// Raised whenever the value of this state changes 
		/// </summary>
		public event EventHandler<T?>? ValueChanged; 

		/// <summary>
		/// Gets or sets the value of th recoil state. It should be noted that setting the value 
		/// happens in the background in an async operation so the value will not be accessable right away.
		/// </summary>
		public T? Value
		{
			get => m_value;
			set
			{
				m_value = value; // set it for now but it will be overriden later 
				m_recoilValue.SetValue(m_store, value);
				State = RecoilValueState.Loading;
			}
		}

		/// <summary>
		/// Gets if this recoil state currently has a value.
		/// </summary>
		public bool HasValue => m_value != null;

		/// <summary>
		/// Gets if the value is currently being calculated 
		/// </summary>
		public RecoilValueState State { get; private set; }

		/// <summary>
		/// Creates a new Atom Accessor with the getter and setter defined.
		/// </summary>
		/// <param name="get">A delegate to fetch the value</param>
		/// <param name="set">A delegate to set the value</param>
		public RecoilState(RecoilValue<T> recoilValue, IRecoilStore? store) : base(recoilValue, store)
		{
			m_recoilValue = recoilValue;

			// Load the default value 
			Task.Run(async () =>
			{
				State = RecoilValueState.Loading;
				try
				{
					m_value = await m_recoilValue.GetValueAsync(store);
				}
				catch (Exception)
				{
					State = RecoilValueState.Error;
				}
				finally
				{
					State = RecoilValueState.Loaded;
				}
				if (!EqualityComparer<T>.Default.Equals(m_value, default(T)))
				{
					RaisePropertyChanged(nameof(Value));
				}
			});
		}

		/// <inheritdoc cref="RecoilState"/>
		protected override void OnStoreSet(IRecoilStore? store)
		{
			if (m_store == store) return;

			if (m_store != null)
			{
				m_store.States.Remove(this);
			}

			m_store = store;

			if (m_store != null)
			{
				m_store.States.Add(this);
			}

			RaisePropertyChanged(nameof(Value));
			RaisePropertyChanged(nameof(HasValue));
		}

		/// <inheritdoc cref="RecoilState"/>
		protected override Task OnValueChangedAsync(IRecoilStore store, object? newValue)
		{
			m_value = (T?)newValue;
			RaisePropertyChanged(nameof(Value));
			RaisePropertyChanged(nameof(HasValue));
			ValueChanged?.Invoke(this, m_value);
			return Task.CompletedTask;
		}

		/// <inheritdoc cref="RecoilState"/>
		protected override async Task OnDependentChangedAsync(IRecoilStore store, RecoilValue dependentValue)
		{
			m_value = await m_recoilValue.GetValueAsync(m_store);
			RaisePropertyChanged(nameof(Value));
		}
	}
}
