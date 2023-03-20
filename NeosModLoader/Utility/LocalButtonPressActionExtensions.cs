using FrooxEngine.UIX;
using FrooxEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseX;

namespace NeosModLoader.Utility
{
	/// <summary>
	/// Contains extension methods to setup locally defined actions for <see cref="IButton"/>s which are triggerable by anyone.<br/>
	/// Due to their nature, they will only work while the <see cref="User"/> that creates them hasn't left the session.
	/// </summary>
	public static class LocalButtonPressActionExtensions
	{
		/// <summary>
		/// Creates a <see cref="Button"/> using the given <paramref name="text"/> and <paramref name="action"/>.
		/// </summary>
		/// <param name="builder">The builder to use for creating the button.</param>
		/// <param name="text">The text displayed on the button.</param>
		/// <param name="action">The action to run when pressed.</param>
		/// <returns>The button created by the <see cref="UIBuilder"/>.</returns>
		public static Button LocalActionButton(this UIBuilder builder, in LocaleString text, Action<IButton> action)
		{
			return builder.Button(text).SetupLocalAction(action);
		}

		/// <summary>
		/// Creates a <see cref="Button"/> using the given <paramref name="icon"/> and <paramref name="action"/>.
		/// </summary>
		/// <param name="builder">The builder to use for creating the button.</param>
		/// <param name="icon">The icon displayed on the button.</param>
		/// <param name="action">The action to run when pressed.</param>
		/// <returns>The button created by the <see cref="UIBuilder"/>.</returns>
		public static Button LocalActionButton(this UIBuilder builder, Uri icon, Action<IButton> action)
		{
			return builder.Button(icon).SetupLocalAction(action);
		}

		/// <summary>
		/// Creates a <see cref="Button"/> using the given <paramref name="text"/>,
		/// <paramref name="icon"/> and <paramref name="action"/>.
		/// </summary>
		/// <param name="builder">The builder to use for creating the button.</param>
		/// <param name="icon">The icon displayed on the button.</param>
		/// <param name="text">The text displayed on the button.</param>
		/// <param name="action">The action to run when pressed.</param>
		/// <returns>The button created by the <see cref="UIBuilder"/>.</returns>
		public static Button LocalActionButton(this UIBuilder builder, Uri icon, LocaleString text, Action<IButton> action)
		{
			return builder.Button(icon, text).SetupLocalAction(action);
		}

		/// <summary>
		/// Creates a <see cref="Button"/> using the given <paramref name="text"/>,
		/// <paramref name="icon"/>, tints and <paramref name="action"/>.
		/// </summary>
		/// <param name="builder">The builder to use for creating the button.</param>
		/// <param name="icon">The icon displayed on the button.</param>
		/// <param name="text">The text displayed on the button.</param>
		/// <param name="tint">The background color of the button.</param>
		/// <param name="spriteTint">The tint of the icon.</param>
		/// <param name="action">The action to run when pressed.</param>
		/// <returns>The button created by the <see cref="UIBuilder"/>.</returns>
		public static Button LocalActionButton(this UIBuilder builder, Uri icon, LocaleString text, in color tint, in color spriteTint, Action<IButton> action)
		{
			return builder.Button(icon, text, tint, spriteTint).SetupLocalAction(action);
		}

		/// <summary>
		/// Creates a <see cref="Button"/> using the given <paramref name="text"/> and <paramref name="action"/> with an extra <paramref name="argument"/>.
		/// </summary>
		/// <typeparam name="T">The type of the extra argument to pass to the action.</typeparam>
		/// <param name="builder">The builder to use for creating the button.</param>
		/// <param name="text">The text displayed on the button.</param>
		/// <param name="action">The action to run when pressed.</param>
		/// <param name="argument">The extra argument to pass to the action when this button is pressed.</param>
		/// <returns>The button created by the <see cref="UIBuilder"/>.</returns>
		public static Button LocalActionButton<T>(this UIBuilder builder, in LocaleString text, Action<IButton, T> action, T argument)
		{
			return builder.Button(text).SetupLocalAction(action, argument);
		}

		/// <summary>
		/// Creates a <see cref="Button"/> using the given <paramref name="icon"/> and <paramref name="action"/> with an extra <paramref name="argument"/>.
		/// </summary>
		/// <typeparam name="T">The type of the extra argument to pass to the action.</typeparam>
		/// <param name="builder">The builder to use for creating the button.</param>
		/// <param name="icon">The icon displayed on the button.</param>
		/// <param name="action">The action to run when pressed.</param>
		/// <param name="argument">The extra argument to pass to the action when this button is pressed.</param>
		/// <returns>The button created by the <see cref="UIBuilder"/>.</returns>
		public static Button LocalActionButton<T>(this UIBuilder builder, Uri icon, Action<IButton, T> action, T argument)
		{
			return builder.Button(icon).SetupLocalAction(action, argument);
		}

		/// <summary>
		/// Creates a <see cref="Button"/> using the given <paramref name="text"/>,
		/// <paramref name="icon"/> and <paramref name="action"/> with an extra <paramref name="argument"/>.
		/// </summary>
		/// <typeparam name="T">The type of the extra argument to pass to the action.</typeparam>
		/// <param name="builder">The builder to use for creating the button.</param>
		/// <param name="icon">The icon displayed on the button.</param>
		/// <param name="text">The text displayed on the button.</param>
		/// <param name="action">The action to run when pressed.</param>
		/// <param name="argument">The extra argument to pass to the action when this button is pressed.</param>
		/// <returns>The button created by the <see cref="UIBuilder"/>.</returns>
		public static Button LocalActionButton<T>(this UIBuilder builder, Uri icon, LocaleString text, Action<IButton, T> action, T argument)
		{
			return builder.Button(icon, text).SetupLocalAction(action, argument);
		}

		/// <summary>
		/// Creates a <see cref="Button"/> using the given <paramref name="text"/>,
		/// <paramref name="icon"/>, tints and <paramref name="action"/> with an extra <paramref name="argument"/>.
		/// </summary>
		/// <typeparam name="T">The type of the extra argument to pass to the action.</typeparam>
		/// <param name="builder">The builder to use for creating the button.</param>
		/// <param name="icon">The icon displayed on the button.</param>
		/// <param name="text">The text displayed on the button.</param>
		/// <param name="tint">The background color of the button.</param>
		/// <param name="spriteTint">The tint of the icon.</param>
		/// <param name="action">The action to run when pressed.</param>
		/// <param name="argument">The extra argument to pass to the action when this button is pressed.</param>
		/// <returns>The button created by the <see cref="UIBuilder"/>.</returns>
		public static Button LocalActionButton<T>(this UIBuilder builder, Uri icon, LocaleString text, in color tint, in color spriteTint, Action<IButton, T> action, T argument)
		{
			return builder.Button(icon, text, tint, spriteTint).SetupLocalAction(action, argument);
		}

		/// <summary>
		/// Sets up an <see cref="IButton"/> with the given <paramref name="action"/>.
		/// </summary>
		/// <typeparam name="TButton">The specific type of the button.</typeparam>
		/// <param name="button">The button to set up with an action.</param>
		/// <param name="action">The action to run when pressed.</param>
		/// <returns>The unchanged button.</returns>
		public static TButton SetupLocalAction<TButton>(this TButton button, Action<IButton> action) where TButton : IButton
		{
			var valueField = button.Slot.AttachComponent<ValueField<bool>>().Value;
			valueField.OnValueChange += field => action(button);

			var toggle = button.Slot.AttachComponent<ButtonToggle>();
			toggle.TargetValue.Target = valueField;

			return button;
		}

		/// <summary>
		/// Sets up an <see cref="IButton"/> with the given <paramref name="action"/> and extra <paramref name="argument"/>.
		/// </summary>
		/// <typeparam name="TButton">The specific type of the button.</typeparam>
		/// <typeparam name="TArgument">The type of the extra argument to pass to the action.</typeparam>
		/// <param name="button">The button to set up with an action.</param>
		/// <param name="action">The action to run when pressed.</param>
		/// <param name="argument">The extra argument to pass to the action when this button is pressed.</param>
		/// <returns>The unchanged button.</returns>
		public static TButton SetupLocalAction<TButton, TArgument>(this TButton button, Action<IButton, TArgument> action, TArgument argument) where TButton : IButton
		{
			var valueField = button.Slot.AttachComponent<ValueField<bool>>().Value;
			valueField.OnValueChange += field => action(button, argument);

			var toggle = button.Slot.AttachComponent<ButtonToggle>();
			toggle.TargetValue.Target = valueField;

			return button;
		}
	}
}
