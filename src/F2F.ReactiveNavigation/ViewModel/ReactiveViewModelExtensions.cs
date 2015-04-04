using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace F2F.ReactiveNavigation.ViewModel
{
	public static class ReactiveViewModelExtensions
	{
		public static IObservable<INavigationParameters> WhenNavigatedTo(this ReactiveViewModel This)
		{
			return This._navigateTo.ObserveOn(RxApp.MainThreadScheduler);
		}

		public static IObservable<INavigationParameters> WhenNavigatedTo(
			this ReactiveViewModel This,
			Func<INavigationParameters, bool> filter)
		{
			return This.WhenNavigatedTo().Where(filter);
		}

		public static IDisposable WhenNavigatedTo(
			this ReactiveViewModel This,
			Func<INavigationParameters, bool> filter,
			Action<INavigationParameters> syncAction)
		{
			return
				This.WhenNavigatedTo(filter)
					.Do(syncAction)
					.Subscribe();
		}

		public static IObservable<INavigationParameters> WhenNavigatedToAsync(this ReactiveViewModel This)
		{
			return This._navigateTo.ObserveOn(RxApp.TaskpoolScheduler);
		}

		public static IObservable<INavigationParameters> WhenNavigatedToAsync(this ReactiveViewModel This, Func<INavigationParameters, bool> filter)
		{
			return This._navigateTo.ObserveOn(RxApp.TaskpoolScheduler).Where(filter);
		}

		public static IDisposable WhenNavigatedToAsync(
			this ReactiveViewModel This, 
			Func<INavigationParameters, bool> filter,
			Func<INavigationParameters, Task> asyncAction,
			Action<INavigationParameters> syncAction)
		{
			return
				This.WhenNavigatedToAsync(filter)
					.Do(_ => This._asyncNavigating.OnNext(true))
					.SelectMany(async p => 
						{
							await asyncAction(p);
							This._asyncNavigating.OnNext(false);
							return p;
						})
					.ObserveOn(RxApp.MainThreadScheduler)
					.Do(syncAction)
					.Subscribe();
		}

		public static IDisposable WhenNavigatedToAsync<T>(
			this ReactiveViewModel This,
			Func<INavigationParameters, bool> filter,
			Func<INavigationParameters, Task<T>> asyncAction,
			Action<INavigationParameters, T> syncAction)
		{
			return
				This.WhenNavigatedToAsync(filter)
					.SelectMany(async p =>
					{
						var result = await asyncAction(p);
						This._asyncNavigating.OnNext(false);
						return new
						{
							Result = result,
							Parameters = p
						};
					})
					.ObserveOn(RxApp.MainThreadScheduler)
					.Do(p => syncAction(p.Parameters, p.Result))
					.Subscribe();
		}

	}
}
