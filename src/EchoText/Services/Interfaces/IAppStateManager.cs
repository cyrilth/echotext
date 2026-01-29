using System;
using EchoText.Models;

namespace EchoText.Services.Interfaces;

/// <summary>
/// Manages the application's state machine and state transitions
/// </summary>
public interface IAppStateManager
{
    /// <summary>
    /// Gets the current state of the application
    /// </summary>
    AppState CurrentState { get; }

    /// <summary>
    /// Fired when the application state changes
    /// </summary>
    event EventHandler<AppState>? StateChanged;

    /// <summary>
    /// Attempts to transition to a new state
    /// </summary>
    /// <param name="newState">The target state</param>
    /// <returns>True if the transition was successful, false if invalid</returns>
    bool TransitionTo(AppState newState);

    /// <summary>
    /// Checks if a transition to the specified state is valid from the current state
    /// </summary>
    /// <param name="newState">The target state</param>
    /// <returns>True if the transition is valid, false otherwise</returns>
    bool CanTransitionTo(AppState newState);
}
